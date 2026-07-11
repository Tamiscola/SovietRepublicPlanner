using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class IndustryPlan
{
    // Resources
    public HashSet<Resource> ExpandedResources
    {
        get
        {
            HashSet<Resource> result = new HashSet<Resource>();
            return FindExpandedResources(result, this);
        }
    }
    public Dictionary<Resource, double> TotalInputs
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
    public List<IndustryPlan> SubChains { get; set; } = new List<IndustryPlan>();
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
                    double consumption = TotalWorkers * r.PerCapitalConsumption;
                    if (result.ContainsKey(r))
                        result[r] += consumption;
                    else
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

    public void CalculateTotalImports(Dictionary<Resource, double> DictImport, HashSet<Resource> expandedResources, IndustryPlan cr)
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
                if (!DictImport.ContainsKey(r)) { DictImport.Add(r, ChosenBuilding.RequiredResources[r] * ChosenBuilding.Building.CurProductivity / 100); }
                else { DictImport[r] += ChosenBuilding.RequiredResources[r] * ChosenBuilding.Building.CurProductivity / 100; }
            }
        }

        // Recursively collect from SubChains
        foreach (IndustryPlan subchain in SubChains)
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

    public Dictionary<Resource, double> CalculateTotalOutput(Dictionary<Resource, double> result, IndustryPlan cr)
    {
        foreach (var kv in cr.ChosenBuilding.ExpectedOutput)
        {
            if (!result.ContainsKey(kv.Key)) result.Add(kv.Key, kv.Value * cr.ChosenBuilding.Building.CurProductivity / 100);
            else result[kv.Key] += kv.Value * cr.ChosenBuilding.Building.CurProductivity / 100;
        }
        foreach (var sub in cr.SubChains)
            CalculateTotalOutput(result, sub);
        return result;
    }
    public HashSet<Resource> FindExpandedResources(HashSet<Resource> result, IndustryPlan cr)
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

    public Dictionary<Resource, double> CalculateTotalInternallySourced(Dictionary<Resource, double> result, IndustryPlan cr)
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

    public Dictionary<Resource, double> CalculateTotalConstructionMaterials(Dictionary<Resource, double> result, IndustryPlan cr)
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
            foreach (var kv2 in kv.Building.ConstructionMaterials)
            {
                if (result.ContainsKey(kv2.Key)) result[kv2.Key] += kv2.Value;
                else result.Add(kv2.Key, kv2.Value);
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
}

