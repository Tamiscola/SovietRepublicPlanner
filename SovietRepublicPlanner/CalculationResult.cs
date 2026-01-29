using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

class CalculationResult
{
    // Target
    public Resource TargetResource { get; set; }
    public double TargetAmount { get; set; }

    // Settings
    public double WorkersProductivity { get; set; } = 100.0;

    // Buildings
    public List<BuildingRequirement> Buildings { get; set; } = new List<BuildingRequirement>();
    public List<BuildingRequirement> SupportBuildings { get; set; } = new List<BuildingRequirement>();
    public Dictionary<ProductionBuilding, int> AllSupportBuildings
    {
        get
        {
            Dictionary<ProductionBuilding, int> all = new Dictionary<ProductionBuilding, int>();

            // Add this level's support buildings
            foreach (var br in SupportBuildings)
            {
                if (all.ContainsKey(br.Building))
                    all[br.Building] += br.Count;
                else
                    all.Add(br.Building, br.Count);
            }

            // Recursively add from SubChains
            foreach (var sub in SubChains)
            {
                foreach (var kv in sub.AllSupportBuildings)
                {
                    if (all.ContainsKey(kv.Key))
                        all[kv.Key] += kv.Value;
                    else
                        all.Add(kv.Key, kv.Value);
                }
            }

            return all;
        }
    }
    public List<ResidentialInstance> ResidentialBuildings { get; set; } = new List<ResidentialInstance>();
    public List<AmenityInstance> AmenityBuildings = new List<AmenityInstance>();
    public List<TransportationInstance> TransportationBuildings = new List<TransportationInstance>();
    public BuildingRequirement ChosenBuilding { get; set; }
    public int TotalBuildings { get; set; }

    // Workers
    public int BaseWorkers => ChosenBuilding.baseTotalWorkers;
    public int TotalWorkers
    {
        get
        {
            int thisLevel = ChosenBuilding.TotalWorkers;
            int subChainTotal = SubChains.Sum(sc => sc.TotalWorkers);
            int supportTotal = SupportBuildings.Sum(sc => sc.TotalWorkers);

            // Calculate base population from production workers
            int baseProductionWorkers = thisLevel + subChainTotal + supportTotal;
            int baseCitizens = (int)(baseProductionWorkers * 1.82); // Citizens = workers × 1.82

            // Add full-capacity amenity workers (non-percentage-based)
            int fullCapacityAmenityWorkers = AmenityBuildings
                .Where(a => !a.Building.UsesPercentageBasedDemand)
                .Sum(a => a.Building.EffectiveWorkersPerShift * 3 * a.Count);

            // Add percentage-based amenity workers
            int percentageBasedWorkers = 0;
            foreach (var amenity in AmenityBuildings.Where(a => a.Building.UsesPercentageBasedDemand))
            {
                percentageBasedWorkers += CalculateWorkersForPercentageAmenity(
                    amenity.Building,
                    baseProductionWorkers,
                    baseCitizens
                ) * amenity.Count;
            }

            return baseProductionWorkers + fullCapacityAmenityWorkers + percentageBasedWorkers;
        }
    }
    public double WorkerToPopulationRatio { get; set; } = 0.55;     // 55% are workers
    public int TotalPopulationNeeded => (int)Math.Ceiling(TotalWorkers / WorkerToPopulationRatio);
    public int TotalHousingCapacity => ResidentialBuildings.Sum(rb => rb.Building.WorkerCapacity * rb.Count);

    // Amenity Rough coverage (guideline only)
    public Dictionary<AmenityType, int> GetAmenCoverage()
    {
        var coverage = new Dictionary<AmenityType, int>();

        foreach (var type in Enum.GetValues(typeof(AmenityType)))
        {
            var buildings = AmenityBuildings.Where(a => a.Building.Type == (AmenityType)type);
            int totalCapacity = buildings.Sum(a => a.TotalCapacity);
            coverage[(AmenityType)type] = totalCapacity;
        }
        return coverage;
    }

    // Resources
    public HashSet<Resource> ExpandedResources
    {
        get
        {
            HashSet<Resource> result = new HashSet<Resource>();
            return FindExpandedResources(result, this);
        }
    }
    public Dictionary<Resource, double> TotalImports
    {
        get
        {
            Dictionary<Resource, double> result = new Dictionary<Resource, double>();
            HashSet<Resource> expandedResources = ExpandedResources;
            CalculateTotalImports(result, expandedResources, this);
            return result;
        }
    }
    public Dictionary<Resource, double> TotalOutputs
    {
        get
        {
            Dictionary<Resource, double> result = new Dictionary<Resource, double>();
            return CalculateTotalOutput(result, this);
        }
    }
    public Dictionary<Resource, double> TotalResidues
    {
        get
        {
            return CalculateTotalResidue();
        }
    }
    public List<CalculationResult> SubChains { get; set; } = new List<CalculationResult>();
    public Dictionary<Resource, double> InternallySourcedResources
    {
        get
        {
            Dictionary<Resource, double> result = new Dictionary<Resource, double>();

            // Add all required inputs from this node's chosen building
            foreach (var kv in ChosenBuilding.RequiredResources)
            {
                // Skip Power and Water (they're handled separately in CalculateTotalResidue)
                if (kv.Key == GameData.PowerResource) continue;
                if (kv.Key == GameData.WaterResource) continue;
                if (kv.Key == GameData.HeatResource) continue;

                // Don't include target resource consumption here 
                // (it's already subtracted via TargetAmount in CalculateTotalResidue)
                if (kv.Key == this.TargetResource) continue;

                result[kv.Key] = kv.Value;
            }

            return result;
        }
    }
    public Dictionary<Resource, double> TotalInternallySourced
    {
        get
        {
            return CalculateTotalInternallySourced(new Dictionary<Resource, double>(), this);
        }
    }
    public Dictionary<Resource, double> TotalConstructionMaterials 
    {
        get
        {
            Dictionary<Resource, double> result = new Dictionary<Resource, double>();
            return CalculateTotalConstructionMaterials(result, this);
        }
    }

    // Resources - Citizen Consumption
    public Dictionary<Resource, double> TotalCitizenConsumption
    {
        get
        {
            Dictionary<Resource, double> result = new Dictionary<Resource, double>();
            foreach (var r in GameData.AllResources) 
            {
                if (r.IsConsumable && r.PerCapitalConsumption > 0)
                {
                    double consumption = TotalPopulationNeeded * r.PerCapitalConsumption;
                    result.Add(r, consumption);
                }
            }
            return result;
        }
    }
    public Dictionary<Resource, double> ConsumptionBalance
    {
        get
        { 
            Dictionary<Resource, double> result = new Dictionary<Resource, double>();
            foreach (var kv in TotalCitizenConsumption)
                if (TotalOutputs.ContainsKey(kv.Key))
                {
                    double produced = TotalOutputs[kv.Key];
                    double consumed = kv.Value;
                    double balance = produced - consumed;
                    result.Add(kv.Key, balance);
                }
            return result;
        }
    }

    // Utilities - Requirement
    public double TotalPowerNeeded
    {
        get
        {
            double thisLevel = (ChosenBuilding.BuildingInstances.Count() > 0 ? ChosenBuilding.BuildingInstances.Sum(bi => bi.Building.PowerConsumption) : ChosenBuilding.TotalPowerNeeded);
            double subChainTotal = SubChains.Sum(sc => sc.TotalPowerNeeded);
            double supportTotal = SupportBuildings.Sum(sc => sc.TotalPowerNeeded);
            double residentialTotal = ResidentialBuildings.Sum(rb => rb.Building.PowerMW * rb.Count);
            double amenTotal = AmenityBuildings.Sum(a => a.Building.PowerConsumptionMWh * a.Count);
            double transTotal = TransportationBuildings.Sum(t => t.Building.PowerConsumptionMWh * t.Count);
            return thisLevel + subChainTotal + supportTotal + residentialTotal + amenTotal + transTotal;
        }
    }
    public double TotalWaterNeeded
    {
        get
        {
            double thisLevel = ChosenBuilding.TotalWaterNeeded;
            double subChainTotal = SubChains.Sum(sc => sc.TotalWaterNeeded);
            double supportTotal = SupportBuildings.Sum(sc => sc.TotalWaterNeeded);
            double residentialTotal = ResidentialBuildings.Sum(rb => rb.Building.WaterPerDay * rb.Count);
            double amenTotal = AmenityBuildings.Sum(a => a.Building.WaterConsumptionM3 * a.Count);
            return thisLevel + subChainTotal + supportTotal + residentialTotal + amenTotal;
        }
    }
    public double TotalSewageProduced
    {
        get
        {
            double thisLevel = ChosenBuilding.TotalSewageProduced;
            double subChainTotal = SubChains.Sum(sc => sc.TotalSewageProduced);
            double supportTotal = SupportBuildings.Sum(sc => sc.TotalSewageProduced);
            double residentialTotal = ResidentialBuildings.Sum(rb => rb.Building.WaterPerDay * rb.Count);
            double amenTotal = AmenityBuildings.Sum(a => a.Building.WaterConsumptionM3 * a.Count);
            return thisLevel + subChainTotal + supportTotal + residentialTotal;
        }
    }
    public double TotalHeatNeeded
    {
        get
        {
            double thisLevel = ChosenBuilding.TotalHeatNeeded;
            double subChainTotal = SubChains.Sum(sc => sc.TotalHeatNeeded);
            double supportTotal = SupportBuildings.Sum(sc => sc.TotalHeatNeeded);
            double residentialTotal = ResidentialBuildings.Sum(rb => rb.Building.HeatTankM3 * rb.Count);
            double amenTotal = AmenityBuildings.Sum(a => a.Building.HotWaterTankM3 * a.Count);
            return thisLevel + subChainTotal + supportTotal + residentialTotal + amenTotal;
        }
    }
    public double TotalGarbageProduced
    {
        get
        {
            double thisLevel = ChosenBuilding.TotalGarbageProduced;
            double subChainTotal = SubChains.Sum(sc => sc.TotalGarbageProduced);
            double supportTotal = SupportBuildings.Sum(sc => sc.TotalGarbageProduced);
            return thisLevel + subChainTotal + supportTotal;
        }
    }
    public double TotalEnvironmentPollution
    {
        get
        {
            double thisLevel = ChosenBuilding.TotalEnvironmentPollution;
            double subChainTotal = SubChains.Sum(sc => sc.TotalEnvironmentPollution);
            double supportTotal = SupportBuildings.Sum(sc => sc.TotalEnvironmentPollution);
            return thisLevel + subChainTotal + supportTotal;
        }
    }
    public Dictionary<Resource, double> TotalUtilityNeeds    // Aggregate total utility needs (input + consumption for water)
    {
        get
        {
            Dictionary<Resource, double> result = new Dictionary<Resource, double>();
            result.Add(GameData.PowerResource, 0);
            result.Add(GameData.WaterResource, 0);
            result.Add(GameData.WasteWaterResource, 0);
            result.Add(GameData.HeatResource, 0);

            // Add utility consumption (power/water consumed to run buildings)
            if (TotalPowerNeeded > 0)
            {
                result[GameData.PowerResource] += TotalPowerNeeded;
            }
            if (TotalWaterNeeded > 0)
            {
                result[GameData.WaterResource] += TotalWaterNeeded;
            }
            // Add Sewage produced
            if (TotalSewageProduced > 0)
            {
                result[GameData.WasteWaterResource] += TotalSewageProduced;
            }
            if (TotalHeatProduced > 0)
            {
                result[GameData.HeatResource] += TotalHeatNeeded;
            }
            // Add utility as input resource (power/water as production inputs)
            foreach (var kv in ChosenBuilding.RequiredResources)
                if (kv.Key.IsUtility && kv.Key == GameData.WaterResource)   // Only Water as input resource
                {
                    if (result.ContainsKey(kv.Key))
                        result[kv.Key] += kv.Value;
                    else
                        result.Add(kv.Key, kv.Value);
                }
            // Recursively aggregate from SubChains
            foreach (var sc in SubChains)
            {
                CollectInputWater(result, sc);
            }
            return result;
        }
    }

    // Utilities - Residential
    public double TotalResidentialPower => ResidentialBuildings.Sum(rb => rb.Building.PowerMW * rb.Count);
    public double TotalResidentialWater => ResidentialBuildings.Sum(rb => rb.Building.WaterPerDay * rb.Count);
    public double TotalResidentialSewage => ResidentialBuildings.Sum(rb => rb.Building.WaterPerDay * rb.Count);
    public double TotalResidentialHeat => ResidentialBuildings.Sum(rb => rb.Building.HeatTankM3 * rb.Count);

    // Utilities - Produced
    public double TotalPowerProduced
    {
        get
        {
            double result = 0;
            return CalculateTotalPowerProduced(result, this);
        }
    }
    public double TotalWaterProduced
    {
        get
        {
            double result = 0;
            return CalculateTotalWaterProduced(result, this);
        }
    }
    public double TotalHeatProduced
    {
        get
        {
            double result = 0;
            return CalculateTotalHeatProduced(result, this);
        }
    }

    public Dictionary<Resource, BuildingRequirement> ExpandedUtilities
    {
        get
        {
            Dictionary<Resource, BuildingRequirement> result = new Dictionary<Resource, BuildingRequirement>();
            return CalculateExpandedUtility(result, this);
        }
    }
    // Economics
    public double EstimatedRevenue { get; set; }
    public void CalculateTotalImports(Dictionary<Resource, double> DictImport, HashSet<Resource> expandedResources, CalculationResult cr)
    {
        // Add this level's non-expanded resources
        if (TargetResource.Name == "Crops")
        {
            foreach (var br in Buildings)
                foreach (var bi in br.BuildingInstances)
                    foreach (var ra in bi.Building.Inputs)
                        if (DictImport.ContainsKey(ra.Resource)) DictImport[ra.Resource] += ra.Amount;
                        else DictImport.Add(ra.Resource, ra.Amount);
        }
        else
        {
            // Add TotalPowerNeeded to the Imports (if non-expanded)
            if (!expandedResources.Contains(GameData.PowerResource) && cr == this)  // Only add root TotalPowerNeeded
            {
                if (!DictImport.ContainsKey(GameData.PowerResource))
                    DictImport.Add(GameData.PowerResource, this.TotalPowerNeeded);
            }
            // Add TotalWaterNeeded to the Imports (if non-expanded)
            if (!expandedResources.Contains(GameData.WaterResource) && cr == this)
            {
                if (!DictImport.ContainsKey(GameData.WaterResource))
                {
                    DictImport.Add(GameData.WaterResource, this.TotalUtilityNeeds.ContainsKey(GameData.WaterResource)
                        ? this.TotalUtilityNeeds[GameData.WaterResource]
                        : 0);
                }
            }
            // Add Resources to import
            foreach (Resource r in ChosenBuilding.RequiredResources.Keys)
            {
                // Input Power is already included in TotalPowerNeeded
                if (r == GameData.PowerResource) { continue; }
                // Input Water is already included in TotalUtilityNeeds[Water]
                if (r == GameData.WaterResource) { continue; }
                // If the resource is expanded (automatically calculate the right amount)
                if (expandedResources.Contains(r)) { continue; }
                // Add the resource to import if it's non-expanded or not locally sourced
                if (!DictImport.ContainsKey(r)) { DictImport.Add(r, ChosenBuilding.RequiredResources[r]); }
                else { DictImport[r] += ChosenBuilding.RequiredResources[r]; }
            }
        }

        // Recursively collect from SubChains
        foreach (CalculationResult subchain in SubChains)
        {
            subchain.CalculateTotalImports(DictImport, expandedResources, subchain);
        }

        // Add citizen consumption to imports (if not produced locally)
        //if (cr == this) // Only at root level
        //{
        //    foreach (var kv in this.TotalCitizenConsumption)
        //    {
        //        // Skip if we're already producing this resource
        //        if (expandedResources.Contains(kv.Key)) { continue; }
        //        // Skip if it's the target resource (we're producing it)
        //        if (kv.Key == this.TargetResource) { continue; }
        //        // Add to imports
        //        if (!DictImport.ContainsKey(kv.Key)) DictImport.Add(kv.Key, kv.Value);
        //        else DictImport[kv.Key] += kv.Value;
        //    }
        //}
    }
    public void DisplayTotalReceipt()
    {
        var amenityCoverage = this.GetAmenCoverage();

        if (ChosenBuilding == null) { Console.WriteLine("There's no saved plan!"); }
        else
        {
            Console.WriteLine("\n┌────────────────────────────────────────┐");
            Console.WriteLine("│         TOTAL RECEIPT                  │");
            Console.WriteLine("├────────────────────────────────────────┤");
            Console.WriteLine($"│ Target: {TargetResource.Name} {TargetAmount} t/day");
            Console.WriteLine("├────────────────────────────────────────┤");
            Console.WriteLine("│ Utilities Status:");
            Console.WriteLine("│                      Needed         Produced   Balance");
            Console.WriteLine($"│ Total Workers:      {TotalWorkers,8}");
            Console.WriteLine($"│ Total Citizens:      {TotalPopulationNeeded,7}{TotalHousingCapacity,15}{TotalHousingCapacity - TotalPopulationNeeded,10}");
            // Power
            string powerProduced = TotalPowerProduced > 0 ? $"{TotalPowerProduced,10:F2}" : "         —";
            double powerBalance = TotalPowerProduced > 0
                ? TotalPowerProduced - TotalPowerNeeded
                : -TotalPowerNeeded;
            string powerBalanceStr = TotalPowerProduced > 0
                ? $"{(powerBalance >= 0 ? "+" : "")}{powerBalance:F2}"
                : $"-{TotalPowerNeeded:F2}";
            Console.WriteLine($"│ Power (MW): {TotalPowerNeeded,16:F2}{powerProduced}  {powerBalanceStr,13}");
            // Water
            double waterInput = TotalUtilityNeeds.ContainsKey(GameData.WaterResource)
                ? TotalUtilityNeeds[GameData.WaterResource] - TotalWaterNeeded
                : 0;
            string waterLabel = waterInput > 0
                ? $"{TotalUtilityNeeds[GameData.WaterResource]:F2} ({TotalWaterNeeded:F2}+{waterInput:F2})"
                : $"{TotalWaterNeeded:F2}";
            string waterProduced = TotalWaterProduced > 0 ? $"{TotalWaterProduced,10:F2}" : "         —";
            double waterBalance = TotalWaterProduced > 0
                ? TotalWaterProduced - TotalUtilityNeeds[GameData.WaterResource]
                : -TotalWaterNeeded;
            string waterBalanceStr = TotalWaterProduced > 0
                ? $"{(waterBalance >= 0 ? "+" : "")}{waterBalance:F2}"
                : (TotalUtilityNeeds.ContainsKey(GameData.WaterResource)) ? $"-{TotalUtilityNeeds[GameData.WaterResource]:F2}" : "0.00";
            Console.WriteLine($"│ Water (t/day): {waterLabel,13}{waterProduced}  {waterBalanceStr,13}");
            // Heat
            string heatProduced = TotalHeatProduced > 0 ? $"{TotalHeatProduced,10:F2}" : "         —";
            double heatBalance = TotalHeatProduced > 0
                ? TotalHeatProduced - TotalHeatNeeded
                : -TotalHeatNeeded;
            string heatBalanceStr = TotalHeatProduced > 0
                ? $"{(heatBalance >= 0 ? "+" : "")}{heatBalance:F2}"
                : $"-{TotalHeatNeeded:F2}";
            Console.WriteLine($"│ Heat (MW):        {TotalHeatNeeded,10:F2}{heatProduced}  {heatBalanceStr,13}");
            // Sewage
            double sewageTreated = TotalUtilityNeeds.ContainsKey(GameData.WasteWaterResource)
                ? (ExpandedUtilities.Any(eu => eu.Key == GameData.WasteWaterResource) ? ExpandedUtilities[GameData.WasteWaterResource].TotalSewageDisposalCapacity : 0)
                : 0;
            string sewageProduced = TotalSewageProduced > 0 ? $"{TotalSewageProduced,10:F2}" : "         —";
            double sewageBalance = TotalSewageProduced > 0
                ? sewageTreated - TotalSewageProduced
                : -TotalSewageProduced;
            string sewageBalanceStr = TotalSewageProduced > 0
                ? $"{(sewageBalance >= 0 ? "+" : "")}{sewageBalance:F2}"
                : $"{TotalSewageProduced:F2}";
            Console.WriteLine($"│ Sewage (t/day):   {sewageProduced}{sewageTreated,10}  {sewageBalanceStr,13}");
            Console.WriteLine($"│ Garbage (t/day):  {TotalGarbageProduced,10:F2}        —  {TotalGarbageProduced,14:F2}");
            Console.WriteLine($"│ Pollution:        {TotalEnvironmentPollution,10:F6}        —  {TotalEnvironmentPollution,14:F6}");
            Console.WriteLine("├────────────────────────────────────────┤");
            Console.WriteLine("│ All Buildings in Chain:                │");
            Console.WriteLine("├────────────────────────────────────────┤");

            // Display all buildings recursively
            DisplayAllBuildings(this, 0);

            var totalSupBldgs = AllSupportBuildings;

            if (totalSupBldgs.Count > 0)
            {
                Console.WriteLine("├────────────────────────────────────────┤");
                Console.WriteLine("│ Support Infrastructures:               │");
                Console.WriteLine("├────────────────────────────────────────┤");
                foreach (var kv in totalSupBldgs)
                    Console.WriteLine($"│ · {kv.Value} × {kv.Key.Name}");
            }
            Console.WriteLine("├────────────────────────────────────────┤");
            Console.WriteLine("│ Residential Buildings:");
            Console.WriteLine("├────────────────────────────────────────┤");
            if (TotalPopulationNeeded > TotalHousingCapacity)
            {
                int deficit = TotalPopulationNeeded - TotalHousingCapacity;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"WARNING: Housing deficit! Need {deficit} more housing capacity");
                Console.WriteLine($"   (Total needed: {TotalPopulationNeeded}, Current: {TotalHousingCapacity})");
                Console.ResetColor();
            }
            foreach (var ri in ResidentialBuildings)
                Console.WriteLine($"│ · {ri.Count} × {ri.Building.Name}");
            // In summary command - Amenity section
            Console.WriteLine("├────────────────────────────────────────┤");
            Console.WriteLine("│ Amenity Buildings:");
            Console.WriteLine("├────────────────────────────────────────┤");

            if (AmenityBuildings.Count == 0)
            {
                Console.WriteLine("│ (none)");
            }
            else
            {
                // Group by AmenityType
                var amenitiesByType = AmenityBuildings
                    .GroupBy(a => a.Building.Type)
                    .OrderBy(g => g.Key);

                int totalCitizens = TotalPopulationNeeded;
                var coverage = this.GetAmenityCoverage();

                foreach (var typeGroup in amenitiesByType)
                {
                    Console.WriteLine($"│ [ {typeGroup.Key} ]");

                    // Group buildings by name within this type
                    var buildingGroups = typeGroup
                        .GroupBy(a => a.Building.Name)
                        .Select(g => new
                        {
                            Name = g.Key,
                            TotalCount = g.Sum(a => a.Count),
                            Building = g.First().Building
                        });

                    foreach (var building in buildingGroups)
                    {
                        Console.WriteLine($"│  · {building.TotalCount} × {building.Name}");
                    }

                    // Service capacity warning (for all types)
                    int typeCapacity = coverage.ServiceCoverage[typeGroup.Key];
                    int deficit = totalCitizens - typeCapacity;
                    if (deficit > 0 && (typeGroup.Key == AmenityType.Shopping
                                     || typeGroup.Key == AmenityType.Pub
                                     || typeGroup.Key == AmenityType.Culture
                                     || typeGroup.Key == AmenityType.Healthcare
                                     || typeGroup.Key == AmenityType.Sports
                                     || typeGroup.Key == AmenityType.Education
                                     || typeGroup.Key == AmenityType.CrimeJustice))
                    {
                        if (typeGroup.Key == AmenityType.Education)
                        {
                            int kindergartenNeeded = (int)Math.Ceiling(totalCitizens * CalculationSettings.KindergartenAgePercent / 100);
                            int schoolNeeded = (int)Math.Ceiling(totalCitizens * CalculationSettings.SchoolAgePercent / 100);
                            int kindergartenDeficit = kindergartenNeeded - coverage.KindergartenCapacity;
                            int schoolDeficit = schoolNeeded - coverage.SchoolCapacity;
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"│          Kindergarten: {coverage.KindergartenCapacity}/{kindergartenNeeded}" +
                                             (kindergartenDeficit > 0 ? $" ({kindergartenDeficit} underserved!)" : "  "));
                            Console.WriteLine($"│          School: {coverage.SchoolCapacity}/{schoolNeeded}" +
                                             (schoolDeficit > 0 ? $" ({schoolDeficit} underserved!)" : "  "));
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"│          {deficit} citizens underserved!");
                            Console.ResetColor();
                        }
                    }

                    // Product coverage warnings (ONLY for Shopping/Pub)
                    if (typeGroup.Key == AmenityType.Shopping || typeGroup.Key == AmenityType.Pub)
                    {
                        var missingProducts = coverage.ProductCoverage
                            .Where(kvp => kvp.Value == 0)  // Not served at all
                            .Select(kvp => kvp.Key.Name);

                        foreach (var product in missingProducts)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"│          {product} is not served!");
                            Console.ResetColor();
                        }
                    }

                    Console.WriteLine("│");
                }
            }
            if (TransportationBuildings.Count() > 0)
            {
                Console.WriteLine("├────────────────────────────────────────┤");
                Console.WriteLine("│ Transportation Buildings:");
                Console.WriteLine("├────────────────────────────────────────┤");
                foreach (var ti in TransportationBuildings)
                    Console.WriteLine($"│ · {ti.Count} × {ti.Building.Name}");
            }
            Console.WriteLine("├────────────────────────────────────────┤");
            Console.WriteLine($"│ Citizen Consumption ({TotalPopulationNeeded} pop):");
            Console.WriteLine("├────────────────────────────────────────┤");
            foreach (var kv in TotalCitizenConsumption)
            {
                // Check if this resource is produced locally
                bool isProducedLocally = false;
                string statusSymbol = "○";

                // Check if it's the target resource
                if (kv.Key == this.TargetResource)
                {
                    isProducedLocally = true;
                    statusSymbol = "●";
                }
                // Check if it's in expanded resources (has production chain)
                else if (this.ExpandedResources.Contains(kv.Key))
                {
                    isProducedLocally = true;
                    statusSymbol = "●";
                }

                // Display with status indicator
                string statusText = isProducedLocally ? "(local)" : "(import)";
                Console.WriteLine($"│ {statusSymbol} {kv.Value,5:F2}t/day {kv.Key.Name,-15} {statusText}");
            }

            Console.WriteLine("├────────────────────────────────────────┤");
            Console.WriteLine("│ Importing Resources:");
            foreach (var kv in TotalImports)
                Console.WriteLine($"│ ·{kv.Value,6:F2}t/day {kv.Key.Name,-20}");
            Console.WriteLine("├────────────────────────────────────────┤");
            Console.WriteLine("│ Resource Residues:");
            foreach (var kv in TotalResidues)
            {
                double displayResidue = kv.Value;

                // If this is the target resource and we have a target, show excess only
                if (kv.Key == this.TargetResource && this.TargetAmount > 0)
                    displayResidue -= this.TargetAmount;

                if (kv.Key != GameData.CropsResource)
                    Console.WriteLine($"│ · {displayResidue,6:F2}t/day {kv.Key.Name,-20}");
                else
                    Console.WriteLine($"│ · {displayResidue,6:F2}t/yr {kv.Key.Name,-20}");
            }
            if (TotalConstructionMaterials.Count() > 0)
            {
                Console.WriteLine("├────────────────────────────────────────┤");
                Console.WriteLine("│ Total Construction Materials:");
                Console.WriteLine("├────────────────────────────────────────┤");
                foreach (var kv in TotalConstructionMaterials)
                    Console.WriteLine($"│ · {kv.Value:F2} × {kv.Key.Name}");
            }
        }
    }

    // Helper method to recursively list buildings
    public void DisplayAllBuildings(CalculationResult result, int depth)
    {
        string indent = new string(' ', depth * 2);
        string supportIndent = indent + "  └";
        if (result.TargetResource.Name == "Crops")
        {
            Dictionary<ProductionBuilding, int> fieldsNeeded = new Dictionary<ProductionBuilding, int>();
            Dictionary<ProductionBuilding, int> farmsNeeded = new Dictionary<ProductionBuilding, int>();
            for (int i = 0; i < result.Buildings.Count(); i++)
            {
                foreach (var bi in result.Buildings[i].BuildingInstances)
                {
                    if (bi.Building.Name.Contains("Field"))
                        if (fieldsNeeded.ContainsKey(bi.Building))
                            fieldsNeeded[bi.Building]++;
                        else fieldsNeeded.Add(bi.Building, 1);
                    else if (bi.Building.Name.Contains("Farm"))
                        if (farmsNeeded.ContainsKey(bi.Building))
                            farmsNeeded[bi.Building]++;
                        else farmsNeeded.Add(bi.Building, 1);
                }
            }
            foreach (var k in fieldsNeeded)
                Console.WriteLine($"│ {indent}· {k.Value} × {k.Key.Name}");
            foreach (var k in farmsNeeded)
                Console.WriteLine($"│ {indent}· {k.Value} × {k.Key.Name}");
            foreach (var subChain in result.SubChains)
            {
                DisplayAllBuildings(subChain, depth + 1);
            }
        }
        else
        {
            // Display THIS level's building
            if (result.ChosenBuilding != null)
                Console.WriteLine($"│ {indent}· {result.ChosenBuilding.Count} × {result.ChosenBuilding.Building.Name}");

            // Aggregate SubChains at the NEXT level
            Dictionary<ProductionBuilding, int> subChainBuildings = new Dictionary<ProductionBuilding, int>();

            foreach (var subChain in result.SubChains)
            {
                if (subChain.ChosenBuilding != null)
                {
                    var building = subChain.ChosenBuilding.Building;
                    if (subChainBuildings.ContainsKey(building))
                        subChainBuildings[building] += subChain.ChosenBuilding.Count;
                    else
                        subChainBuildings.Add(building, subChain.ChosenBuilding.Count);
                }
            }

            // Display aggregated SubChain buildings
            string subIndent = new string(' ', (depth + 1) * 2);
            foreach (var kv in subChainBuildings)
                Console.WriteLine($"│ {subIndent}· {kv.Value} × {kv.Key.Name}");

            // Now recurse into SubChains' SubChains (depth + 2)
            foreach (var subChain in result.SubChains)
            {
                foreach (var subSubChain in subChain.SubChains)
                {
                    DisplayAllBuildings(subSubChain, depth + 2);
                }
            }
        }
    }
    // 1) Helper to accumulate I/O
    void AccumulateIO(Dictionary<Resource, double> outputs,
                      Dictionary<Resource, double> inputs,
                      CalculationResult cr)
    {
        // Add this node's chosen building outputs
        foreach (var kv in cr.ChosenBuilding.ExpectedOutput)
            outputs[kv.Key] = outputs.GetValueOrDefault(kv.Key) + kv.Value;

        // Add this node's chosen building inputs
        foreach (var kv in cr.ChosenBuilding.RequiredResources)
            inputs[kv.Key] = inputs.GetValueOrDefault(kv.Key) + kv.Value;

        // Recurse into all subchains
        foreach (var sub in cr.SubChains)
            AccumulateIO(outputs, inputs, sub);
    }

    // 2) At the root, compute residue
    public Dictionary<Resource, double> CalculateTotalResidue()
    {
        Dictionary<Resource, double> result = new Dictionary<Resource, double>();
        foreach (var kv in this.TotalOutputs)
        {
            double residue = kv.Value;
            // Subtract target
            //if (kv.Key == this.TargetResource)
            //    residue -= this.TargetAmount;
            // Subtract internal sourcing
            if (this.TotalInternallySourced.ContainsKey(kv.Key))
                residue -= this.TotalInternallySourced[kv.Key];
            // Subtract utility consumption for utility resources
            if (kv.Key.Name == "Power")
                residue -= this.TotalPowerNeeded;
            else if (kv.Key.Name == "Water")
                residue -= (this.TotalUtilityNeeds.ContainsKey(kv.Key)) ? this.TotalUtilityNeeds[kv.Key] : this.TotalWaterNeeded;
            else if (kv.Key.Name == "Heat")
                residue -= this.TotalHeatNeeded;
            // Subtract citizen consumption
            if (TotalCitizenConsumption.ContainsKey(kv.Key))
                residue -= TotalCitizenConsumption[kv.Key];
            result.Add(kv.Key, residue);
        }
        return result;
    }
    // Helper methods
    public Dictionary<Resource, double> CalculateTotalOutput(Dictionary<Resource, double> result, CalculationResult cr)
    {
        foreach (var kv in cr.ChosenBuilding.ExpectedOutput)
        {
            if (!result.ContainsKey(kv.Key)) result.Add(kv.Key, kv.Value);
            else result[kv.Key] += kv.Value;
        }
        foreach (var sub in cr.SubChains)
            CalculateTotalOutput(result, sub);
        return result;
    }
    public Dictionary<Resource, BuildingRequirement> CalculateExpandedUtility(Dictionary<Resource, BuildingRequirement> result, CalculationResult cr)
    {
        if (cr.ChosenBuilding.Building.IsUtilityBuilding && cr.ChosenBuilding.ExpectedOutput.Any(eo => eo.Key.IsUtility))
        {
            foreach (var kv in cr.ChosenBuilding.ExpectedOutput)
                if (kv.Key == GameData.PowerResource)
                {
                    if (!result.ContainsKey(GameData.PowerResource))
                        result.Add(GameData.PowerResource, cr.ChosenBuilding);
                    else Console.WriteLine($"There's already existing utility building {cr.ChosenBuilding.Building.Name} for Power.");
                }
                else if (kv.Key == GameData.WaterResource)
                {
                    if (!result.ContainsKey(GameData.WaterResource))
                        result.Add(GameData.WaterResource, cr.ChosenBuilding);
                    else Console.WriteLine($"There's already existing utility building {cr.ChosenBuilding.Building.Name} for Water.");
                }
                else if (kv.Key == GameData.WasteWaterResource)
                {
                    if (!result.ContainsKey(GameData.WasteWaterResource))
                        result.Add(GameData.WasteWaterResource, cr.ChosenBuilding);
                    else Console.WriteLine($"There's already existing utility building {cr.ChosenBuilding.Building.Name} for Waste water.");
                }
        }
        foreach (var sub in cr.SubChains)
            CalculateExpandedUtility(result, sub);
        return result;
    }
    public double CalculateTotalPowerProduced(double d, CalculationResult cr)
    {
        if (cr.ChosenBuilding.ExpectedOutput.Any(eo => eo.Key == GameData.PowerResource))
            d += cr.ChosenBuilding.ExpectedOutput[GameData.PowerResource];
        foreach (var sub in cr.SubChains)
            d = CalculateTotalPowerProduced(d, sub);
        return d;
    }
    public double CalculateTotalWaterProduced(double d, CalculationResult cr)
    {
        if (cr.ChosenBuilding.ExpectedOutput.Any(eo => eo.Key == GameData.WaterResource))
            d += cr.ChosenBuilding.ExpectedOutput[GameData.WaterResource];
        foreach (var sub in cr.SubChains)
            d = CalculateTotalWaterProduced(d, sub);
        return d;
    }
    public double CalculateTotalHeatProduced(double d, CalculationResult cr)
    {
        if (cr.ChosenBuilding.ExpectedOutput.Any(eo => eo.Key == GameData.HeatResource))
            d += cr.ChosenBuilding.ExpectedOutput[GameData.HeatResource];
        foreach (var sub in cr.SubChains)
            d = CalculateTotalHeatProduced(d, sub);
        return d;
    }
    public HashSet<Resource> FindExpandedResources(HashSet<Resource>result ,CalculationResult cr)
    {
        // Crops
        if (cr.TargetResource == GameData.CropsResource)
        {
            foreach (var bi in cr.ChosenBuilding.BuildingInstances)
            {
                foreach (var output in bi.ExpectedOutput.Keys)
                    if (!result.Contains(output)) result.Add(output);
                    else continue;
            }
        }
        foreach (var k in cr.ChosenBuilding.ExpectedOutput.Keys)
            if (!result.Contains(k)) result.Add(k);
            else continue;
        foreach (var sub in cr.SubChains)
            FindExpandedResources(result, sub);
        return result;
    }
    public Dictionary<Resource, double> CalculateTotalInternallySourced(Dictionary<Resource, double> result, CalculationResult cr)
    {
        // Add this node's internally sourced resources
        foreach (var kv in cr.InternallySourcedResources)
        {
            if (result.ContainsKey(kv.Key))
                result[kv.Key] += kv.Value;  // Accumulate
            else
                result.Add(kv.Key, kv.Value);
        }

        // Recurse through all subchains
        foreach (var sub in cr.SubChains)
        {
            CalculateTotalInternallySourced(result, sub);
        }

        return result;
    }
    private void CollectInputWater(Dictionary<Resource, double> result, CalculationResult cr)
    {
        // Add this level's input water
        foreach (var kv in cr.ChosenBuilding.RequiredResources)
        {
            if (kv.Key.IsUtility && kv.Key == GameData.WaterResource)
            {
                if (result.ContainsKey(kv.Key))
                    result[kv.Key] += kv.Value;
                else
                    result.Add(kv.Key, kv.Value);
            }
        }

        // Recurse to subchains
        foreach (var sc in cr.SubChains)
        {
            CollectInputWater(result, sc);
        }
    }
    public Dictionary<Resource, double> CalculateTotalConstructionMaterials(Dictionary<Resource, double> result, CalculationResult cr)
    {
        // Industrial Buildings
        foreach (var kv in cr.ChosenBuilding.ConstructionMaterials)
        {
            if (result.ContainsKey(kv.Key)) result[kv.Key] += kv.Value;
            else result.Add(kv.Key, kv.Value);
        }

        // Support Buildings
        foreach (var kv in cr.SupportBuildings)
        {
            foreach (var kv2 in kv.ConstructionMaterials)
            {
                if (result.ContainsKey(kv2.Key)) result[kv2.Key] += kv2.Value;
                else result.Add(kv2.Key, kv2.Value);
            }
        }

        // Amenity Buildings
        foreach (var kv in cr.AmenityBuildings)
        {
            foreach (var k in kv.Building.ConstructionMaterials.Keys)
            {
                if (result.ContainsKey(k)) result[k] += kv.Building.ConstructionMaterials[k] * kv.Count;
                else result.Add(k, kv.Building.ConstructionMaterials[k] * kv.Count);
            }
        }

        // Residential Buildings
        foreach (var kv in cr.ResidentialBuildings)
        {
            foreach (var k in kv.Building.ConstructionMaterials.Keys)
            {
                if (result.ContainsKey(k)) result[k] += kv.Building.ConstructionMaterials[k] * kv.Count;
                else result.Add(k, kv.Building.ConstructionMaterials[k] * kv.Count);
            }
        }

        // Transportation Buildings
        foreach (var kv in cr.TransportationBuildings)
        {
            foreach (var k in kv.Building.ConstructionMaterials.Keys)
            {
                if (result.ContainsKey(k)) result[k] += kv.Building.ConstructionMaterials[k] * kv.Count;
                else result.Add(k, kv.Building.ConstructionMaterials[k] * kv.Count);
            }
        }

        foreach (var sub in cr.SubChains)
            CalculateTotalConstructionMaterials(result, sub);

        return result;
    }
    public class AmenityCoverage
    {
        public Dictionary<Resource, int> ProductCoverage { get; set; } = new Dictionary<Resource, int>();
        public Dictionary<AmenityType, int> ServiceCoverage { get; set; } = new Dictionary<AmenityType, int>();
        public int KindergartenCapacity { get; set; }
        public int SchoolCapacity { get; set; }
        public int UniversityCapacity { get; set; }
    }
    public AmenityCoverage GetAmenityCoverage()
    {
        var coverage = new AmenityCoverage();

        // Initialize consumable products
        var consumables = new[] { GameData.FoodResource, GameData.MeatResource,
                              GameData.ClothesResource, GameData.AlcoholResource,
                              GameData.ElectronicsResource };
        foreach (var resource in consumables)
            coverage.ProductCoverage[resource] = 0;

        // Initialize service types
        foreach (AmenityType type in Enum.GetValues(typeof(AmenityType)))
            coverage.ServiceCoverage[type] = 0; 


        // Calculate coverage
        foreach (var amenity in AmenityBuildings)
        {
            // Product-based coverage (Shopping, Pub)
            if (amenity.Building.ProductsOffered.Count > 0)
            {
                foreach (var product in amenity.Building.ProductsOffered)
                {
                    if (coverage.ProductCoverage.ContainsKey(product))
                        coverage.ProductCoverage[product] += amenity.TotalCapacity;
                }
            }

            // Service-based coverage (all amenity types)
            coverage.ServiceCoverage[amenity.Building.Type] += amenity.TotalCapacity;

            // Education subtype tracking
            if (amenity.Building.Type == AmenityType.Education)
            {
                switch (amenity.Building.EducationLevel)
                {
                    case EducationSubtype.Kindergarten:
                        coverage.KindergartenCapacity += amenity.TotalCapacity;
                        break;
                    case EducationSubtype.School:
                        coverage.SchoolCapacity += amenity.TotalCapacity;
                        break;
                    case EducationSubtype.University:
                        coverage.UniversityCapacity += amenity.TotalCapacity;
                        break;
                    // UniversityDorm doesn't count - it's housing, not essential service
                }
            }
        }
        return coverage;
    }
    private int CalculateWorkersForPercentageAmenity(AmenityBuilding building, int totalWorkers, int totalCitizens)
    {
        if (!building.UsesPercentageBasedDemand)
        {
            // Should not be called, but fallback
            return building.EffectiveWorkersPerShift * 3;
        }

        // Determine population served based on building type
        int populationServed = building.ServesPopulationType switch
        {
            PopulationType.Workers => totalWorkers,
            PopulationType.Citizen => totalCitizens,
            PopulationType.Children => (int)(totalCitizens * 0.165),  // 16.5% from your data
            PopulationType.YoungAdults => (int)(totalCitizens * 0.065), // 6.5% from your data
            _ => totalCitizens
        };

        // Calculate actual demand
        double actualDemand = populationServed * building.PopulationPercentageServed;

        // How many buildings needed to serve this demand?
        double buildingsNeeded = actualDemand / (building.MaxVisitors > 0 ? building.MaxVisitors : 1);

        // Workers needed = buildings needed × workers per building × 3 shifts
        int workersPerBuilding = building.EffectiveWorkersPerShift * 3;

        return (int)Math.Ceiling(buildingsNeeded * workersPerBuilding);
    }
    public bool RemoveSupportBuilding(ProductionBuilding building)
    {
        // Remove ALL instances from this level
        int removedCount = SupportBuildings.RemoveAll(br => br.Building == building);

        // ALSO search all SubChains (don't stop early!)
        bool foundInSubChain = false;
        foreach (var subChain in SubChains)
        {
            if (subChain.RemoveSupportBuilding(building))
                foundInSubChain = true;
        }

        // Return true if we found it at ANY level
        return (removedCount > 0 || foundInSubChain);
    }
}