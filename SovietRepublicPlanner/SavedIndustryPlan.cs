using System;

public class SavedIndustryPlan
{
    public SavedCity ParentCity { get; set; }
    public string ResourceName { get; set; }
    public string BuildingName { get; set; }
    public double Amount { get; set; }
    public double Productivity { get; set; }
    public int ChosenBuildingIndex { get; set; }
    public bool IsBuildingBasedPlan { get; set; } = false;
    public int BuildingCount { get; set; }                 // For building-based plans
    public List<double> BuildingQualities { get; set; }     // For mines/quarries
    public bool UsesVehicles { get; set; } = false;
    public List<SavedIndustryPlan> SubChains { get; set; } = new List<SavedIndustryPlan>();
    public List<SavedSupportInstance> SupportBuildings { get; set; }
    public class SavedSupportInstance
    {
        public string BuildingName { get; set; }
        public int Count { get; set; }
    }
    public List<SavedTransportationInstance> TransportationBuildings { get; set; }
    public class SavedTransportationInstance
    {
        public string BuildingName { get; set; }
        public int Count { get; set; }
    }
    public List<SavedResourceInstance> ConstructionMaterials { get; set; }
    public class SavedResourceInstance
    {
        public string Name { get; set; }
        public double Amount { get; set; }
    }
    public static SavedIndustryPlan ConvertToSavedPlan(IndustryPlan plan)
    {
        // Find chosen building index
        int chosenIndex = -1;
        if (plan.ChosenBuilding != null)
        {
            chosenIndex = plan.Buildings.IndexOf(plan.ChosenBuilding);
        }

        var saved = new SavedIndustryPlan
        {
            ResourceName = plan.TargetResource.Name,
            BuildingName = plan.ChosenBuilding?.Building.Name,
            Amount = plan.TargetAmount,
            Productivity = plan.WorkersProductivity,
            ChosenBuildingIndex = chosenIndex,
            IsBuildingBasedPlan = (plan.TargetAmount == 0),
            BuildingCount = plan.ChosenBuilding?.Count ?? 0,
            BuildingQualities = plan.ChosenBuilding?.BuildingInstances
                .Select(bi => bi.ResourceAbundanceMultiplier)
                .ToList(),
            UsesVehicles = (plan.ChosenBuilding?.Building.WorkersPerShift == 0),
            SubChains = plan.SubChains
                .Select(sc => ConvertToSavedPlan(sc))  // Calls itself!
                .ToList(),
            SupportBuildings = plan.SupportBuildings.Select(br => new SavedIndustryPlan.SavedSupportInstance
            {
                BuildingName = br.Building.Name,
                Count = br.Count
            }).ToList(),
            TransportationBuildings = plan.TransportationBuildings.Select(tb => new SavedIndustryPlan.SavedTransportationInstance
            {
                BuildingName = tb.Building.Name,
                Count = tb.Count
            }).ToList(),
        };

        return saved;
    }
    public static IndustryPlan ConvertFromSavedPlan(SavedIndustryPlan savedPlan)
    {
        IndustryPlan result;

        if (savedPlan.IsBuildingBasedPlan)
        {
            // Reconstruct building-based plan
            result = new IndustryPlan();
            result.WorkersProductivity = savedPlan.Productivity;
            result.TargetAmount = 0;

            var building = GameData.AllProductionBuildings
                .FirstOrDefault(b => b.Name == savedPlan.BuildingName);

            if (building != null)
            {
                // Set vehicle mode if needed
                if (savedPlan.UsesVehicles && building.CanUseVehicles)
                {
                    building.WorkersPerShift = 0;
                }

                BuildingRequirement br = new BuildingRequirement(building);
                br.Count = savedPlan.BuildingCount;
                br.WorkersProductivity = savedPlan.Productivity;

                // Reconstruct BuildingInstances with qualities
                if (savedPlan.BuildingQualities != null && savedPlan.BuildingQualities.Count > 0)
                {
                    foreach (var quality in savedPlan.BuildingQualities)
                    {
                        BuildingInstance instance = new BuildingInstance
                        {
                            Building = building,
                            ResourceAbundanceMultiplier = quality
                        };
                        br.BuildingInstances.Add(instance);
                    }
                }

                // Reconstruct support buildings
                if (savedPlan.SupportBuildings != null && savedPlan.SupportBuildings.Count() > 0)
                {
                    foreach (var savedbi in savedPlan.SupportBuildings)
                    {
                        SupportInstance supportInstance = new SupportInstance();
                        supportInstance.Building = GameData.AllSupportBuildings.FirstOrDefault(b => b.Name == savedbi.BuildingName);
                        if (supportInstance.Building == null)
                            throw new InvalidOperationException($"Support Infrastructure '{savedbi.BuildingName}' not found in GameData.");
                        supportInstance.Count = savedbi.Count;
                        result.SupportBuildings.Add(supportInstance);
                    }
                }

                // Reconstruct transportation buildings
                if (savedPlan.TransportationBuildings != null && savedPlan.TransportationBuildings.Count() > 0)
                {
                    foreach (var savedbi in savedPlan.TransportationBuildings)
                    {
                        TransportationInstance transportationInstance = new TransportationInstance();
                        transportationInstance.Building = GameData.TransportationBuildings.FirstOrDefault(ti => ti.Name == savedbi.BuildingName);
                        if (transportationInstance.Building == null)
                            throw new InvalidOperationException($"Transportation building '{savedbi.BuildingName}' not found in GameData.");
                        transportationInstance.Count = savedbi.Count;
                        result.TransportationBuildings.Add(transportationInstance);
                    }
                }

                result.ChosenBuilding = br;
                result.Buildings.Add(br);

                var firstOutput = br.ExpectedOutput.FirstOrDefault();
                result.TargetResource = firstOutput.Key;
            }
        }
        else
        {
            // Reconstruct resource-target plan WITHOUT re-calculating
            result = new IndustryPlan();
            result.TargetResource = GameData.AllResources.FirstOrDefault(r => r.Name == savedPlan.ResourceName);
            result.TargetAmount = savedPlan.Amount;
            result.WorkersProductivity = savedPlan.Productivity;

            // Find the saved building by name
            var building = GameData.AllProductionBuildings.FirstOrDefault(b => b.Name == savedPlan.BuildingName);

            if (building != null)
            {
                // Set vehicle mode if needed
                if (savedPlan.UsesVehicles && building.CanUseVehicles)
                {
                    building.WorkersPerShift = 0;
                }

                BuildingRequirement br = new BuildingRequirement(building);
                br.Count = savedPlan.BuildingCount;
                br.WorkersProductivity = savedPlan.Productivity;

                // Reconstruct BuildingInstances with qualities
                if (savedPlan.BuildingQualities != null && savedPlan.BuildingQualities.Count > 0)
                {
                    foreach (var quality in savedPlan.BuildingQualities)
                    {
                        BuildingInstance instance = new BuildingInstance
                        {
                            Building = building,
                            ResourceAbundanceMultiplier = quality
                        };
                        br.BuildingInstances.Add(instance);
                    }
                }

                // Reconstruct support buildings
                if (savedPlan.SupportBuildings != null && savedPlan.SupportBuildings.Count > 0)
                {
                    foreach (var savedbi in savedPlan.SupportBuildings)
                    {
                        SupportInstance supportInstance = new SupportInstance();
                        supportInstance.Building = GameData.AllSupportBuildings
                            .FirstOrDefault(ti => ti.Name == savedbi.BuildingName);
                        if (supportInstance.Building == null)
                            throw new InvalidOperationException($"Support building '{savedbi.BuildingName}' not found in GameData.");
                        supportInstance.Count = savedbi.Count;
                        result.SupportBuildings.Add(supportInstance);
                    }
                }

                // Reconstruct transportation buildings
                if (savedPlan.TransportationBuildings != null && savedPlan.TransportationBuildings.Count > 0)
                {
                    foreach (var savedbi in savedPlan.TransportationBuildings)
                    {
                        TransportationInstance transportationInstance = new TransportationInstance();
                        transportationInstance.Building = GameData.TransportationBuildings
                            .FirstOrDefault(ti => ti.Name == savedbi.BuildingName);
                        if (transportationInstance.Building == null)
                            throw new InvalidOperationException($"Transportation building '{savedbi.BuildingName}' not found in GameData.");
                        transportationInstance.Count = savedbi.Count;
                        result.TransportationBuildings.Add(transportationInstance);
                    }
                }

                result.ChosenBuilding = br;
                result.Buildings.Add(br);
            }
        }

        // restore all subchains
        foreach (var savedSubChain in savedPlan.SubChains)
        {
            var subChain = ConvertFromSavedPlan(savedSubChain);  // Calls itself!
            result.SubChains.Add(subChain);
        }

        return result;
    }
}