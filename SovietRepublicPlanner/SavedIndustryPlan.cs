public class SavedIndustryPlan
{
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
    public List<SavedBuildingInstance> SupportBuildings { get; set; }
    public class SavedBuildingInstance
    {
        public string BuildingName { get; set; }
        public int Count { get; set; }
    }
    public List<SavedResidentialInstance> ResidentialBuildings { get; set; }
    public class SavedResidentialInstance
    {
        public string BuildingName { get; set; }
        public int Count { get; set; }
    }
    public List<SavedAmenityInstance> AmenityBuildings { get; set; }
    public class SavedAmenityInstance
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
    public static SavedIndustryPlan ConvertToSavedPlan(CalculationResult plan)
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
            SupportBuildings = plan.SupportBuildings.Select(br => new SavedIndustryPlan.SavedBuildingInstance
            {
                BuildingName = br.Building.Name,
                Count = br.Count
            }).ToList(),
            ResidentialBuildings = plan.ResidentialBuildings.Select(rb => new SavedIndustryPlan.SavedResidentialInstance
            {
                BuildingName = rb.Building.Name,
                Count = rb.Count
            }).ToList(),
            AmenityBuildings = plan.AmenityBuildings.Select(ab => new SavedIndustryPlan.SavedAmenityInstance
            {
                BuildingName = ab.Building.Name,
                Count = ab.Count
            }).ToList(),
            TransportationBuildings = plan.TransportationBuildings.Select(tb => new SavedIndustryPlan.SavedTransportationInstance
            {
                BuildingName = tb.Building.Name,
                Count = tb.Count
            }).ToList(),
        };

        return saved;
    }
    public static CalculationResult ConvertFromSavedPlan(SavedIndustryPlan savedPlan)
    {
        CalculationResult result;

        if (savedPlan.IsBuildingBasedPlan)
        {
            // Reconstruct building-based plan
            result = new CalculationResult();
            result.WorkersProductivity = savedPlan.Productivity;
            result.TargetAmount = 0;

            var building = GameData.AllBuildings
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
                        BuildingRequirement supportBuilding = new BuildingRequirement(GameData.AllSupportBuildings.FirstOrDefault(sb => sb.Name == savedbi.BuildingName));
                        supportBuilding.Count = savedbi.Count;
                        result.SupportBuildings.Add(supportBuilding);
                    }
                }

                // Reconstruct residential buildings
                if (savedPlan.ResidentialBuildings != null && savedPlan.ResidentialBuildings.Count() > 0)
                {
                    foreach (var savedRes in savedPlan.ResidentialBuildings)
                    {
                        ResidentialBuilding resBldg = GameData.AllResidentialBuildings.FirstOrDefault(rb => rb.Name == savedRes.BuildingName);
                        if (resBldg != null)
                        {
                            ResidentialInstance resInstance = new ResidentialInstance
                            {
                                Building = resBldg,
                                Count = savedRes.Count
                            };
                            result.ResidentialBuildings.Add(resInstance);
                        }
                    }
                }

                // Reconstruct amenity buildings
                if (savedPlan.AmenityBuildings != null && savedPlan.AmenityBuildings.Count() > 0)
                {
                    foreach (var savedbi in savedPlan.AmenityBuildings)
                    {
                        AmenityInstance amenityBuilding = new AmenityInstance();
                        amenityBuilding.Building = GameData.AllAmenityBuildings.FirstOrDefault(ab => ab.Name == savedbi.BuildingName);
                        amenityBuilding.Count = savedbi.Count;
                        result.AmenityBuildings.Add(amenityBuilding);
                    }
                }

                // Reconstruct transportation buildings
                if (savedPlan.TransportationBuildings != null && savedPlan.TransportationBuildings.Count() > 0)
                {
                    foreach (var savedbi in savedPlan.TransportationBuildings)
                    {
                        TransportationInstance transportationInstance = new TransportationInstance();
                        transportationInstance.Building = GameData.TransportationBuildings.FirstOrDefault(ti => ti.Name == savedbi.BuildingName);
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
            result = new CalculationResult();
            result.TargetResource = GameData.AllResources.FirstOrDefault(r => r.Name == savedPlan.ResourceName);
            result.TargetAmount = savedPlan.Amount;
            result.WorkersProductivity = savedPlan.Productivity;

            // Find the saved building by name
            var building = GameData.AllBuildings.FirstOrDefault(b => b.Name == savedPlan.BuildingName);

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
                        var supportBuilding = GameData.AllSupportBuildings
                            .FirstOrDefault(sb => sb.Name == savedbi.BuildingName);

                        if (supportBuilding != null)
                        {
                            BuildingRequirement br2 = new BuildingRequirement(supportBuilding);
                            br2.Count = savedbi.Count;
                            result.SupportBuildings.Add(br2);
                        }
                    }
                }

                // Reconstruct residential buildings
                if (savedPlan.ResidentialBuildings != null && savedPlan.ResidentialBuildings.Count > 0)
                {
                    foreach (var savedRes in savedPlan.ResidentialBuildings)
                    {
                        ResidentialBuilding resBldg = GameData.AllResidentialBuildings
                            .FirstOrDefault(rb => rb.Name == savedRes.BuildingName);
                        if (resBldg != null)
                        {
                            ResidentialInstance resInstance = new ResidentialInstance
                            {
                                Building = resBldg,
                                Count = savedRes.Count
                            };
                            result.ResidentialBuildings.Add(resInstance);
                        }
                    }
                }

                // Reconstruct amenity buildings
                if (savedPlan.AmenityBuildings != null && savedPlan.AmenityBuildings.Count > 0)
                {
                    foreach (var savedbi in savedPlan.AmenityBuildings)
                    {
                        AmenityInstance amenityBuilding = new AmenityInstance();
                        amenityBuilding.Building = GameData.AllAmenityBuildings
                            .FirstOrDefault(ab => ab.Name == savedbi.BuildingName);
                        amenityBuilding.Count = savedbi.Count;
                        result.AmenityBuildings.Add(amenityBuilding);
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