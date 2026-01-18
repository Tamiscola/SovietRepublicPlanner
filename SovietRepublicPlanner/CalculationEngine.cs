class CalculationEngine
{
    public static CalculationResult Calculate(string resourceName, double targetAmount)
    {
        // Search TargetResource
        Resource targetResource = null;
        foreach (Resource r in GameData.AllResources)
        {
            if (r.Name == resourceName)
            {
                targetResource = r;
            }
        }

        // Construct CalculationResult instance
        CalculationResult result = new CalculationResult();
        BuildingRequirement targetBuildingRequirement = null;
        List<BuildingRequirement> targetBuildingRequirements = new List<BuildingRequirement>() { };

        // Find Buildings that produce the target Resource
        List<ProductionBuilding> targetProductionBuildings = new List<ProductionBuilding> { };
        targetProductionBuildings.AddRange(FindBuildingForResource(resourceName));

        // Calculate Buildings & Workers & Resources & Utilities
        // Put each recipe into targetBuildingRequirement(Individual Calculation Result)
        bool fieldProcessed = false;
        foreach (ProductionBuilding pb in targetProductionBuildings)
        {
            targetBuildingRequirement = new BuildingRequirement(pb);
            // Mines, Pumpjack
            if (pb.IsQualityDependent) 
            {
                while (true)
                {
                    Console.Write($"How many spots to build '{pb.Name}'?: ");
                    int buildingSpots;
                    if (int.TryParse(Console.ReadLine(), out buildingSpots)) 
                    {
                        List<double> qualities = new List<double>();
                        while (true) {
                            qualities.Clear();
                            Console.Write("What qualities? (comma-separated): ");
                            string[] qualityInputs = Console.ReadLine().Split(',');
                            foreach (string qualityInput in qualityInputs)
                            {
                                int quality;
                                if (int.TryParse(qualityInput.Trim(), out quality))
                                {
                                    double percentageQuality = Math.Round((double) quality / 100, 2);
                                    qualities.Add(percentageQuality);
                                } else {
                                    Console.Write("Wrong input. Try again.");
                                    break;
                                }
                            }
                            if (qualities.Count() != buildingSpots) { continue; }
                            break;
                        }
                        for (int i = 0; i < buildingSpots; i++)
                        {
                            BuildingInstance buildingInstance = new BuildingInstance();
                            buildingInstance.Building = pb;
                            buildingInstance.ResourceAbundanceMultiplier = qualities[i];
                            targetBuildingRequirement.BuildingInstances.Add(buildingInstance);
                        }
                        break;
                    }
                    else { Console.Write("Wrong input. "); continue; }
                }
                foreach (ResourceAmount ra in pb.Outputs)
                {
                    targetBuildingRequirement.Count = targetBuildingRequirement.BuildingInstances.Count();
                }
            }
            // Farming Buildings
            else if (pb.IsSeasonDependent && !fieldProcessed)
            {
                // Convert to annual: dailyAmount × 365
                double annualAmount = targetAmount * 365;

                // Ask fertilizer strategy
                while (true)
                {
                    Console.Write("Which fertilizer are you gonna use? ([0]:None / [1]:Solid / [2]:Liquid/ [3]:Both)\n:");
                    GameData.FertilizerType fertilizerType = GameData.FertilizerType.None;
                    int userChoice = -1;
                    double fertilityRate = 0;
                    BuildingInstance buildingInstance = new BuildingInstance();
                    if (int.TryParse(Console.ReadLine(), out userChoice) && userChoice >= 0 && userChoice <= 3)
                    {
                        // Determine fertility level from fertilizer
                        switch (userChoice)
                        {
                            case 0:
                                fertilizerType = GameData.FertilizerType.None;
                                Console.Write("What fertility will you maintain? (70-100%): ");
                                if (double.TryParse(Console.ReadLine(),out fertilityRate) && fertilityRate >= 70 && fertilityRate <= 100)
                                {
                                    buildingInstance.SeasonalMultiplier = Math.Round(fertilityRate / 100, 2);
                                    break;
                                } else { Console.Write("Invalid input. "); continue; }
                            case 1:
                                fertilizerType = GameData.FertilizerType.Solid;
                                buildingInstance.StartupCosts.Add(new Resource("Solid Fertilizer", 1, true, false), 5);
                                buildingInstance.SeasonalMultiplier = 1.5;
                                break;
                            case 2:
                                fertilizerType = GameData.FertilizerType.Liquid;
                                buildingInstance.StartupCosts.Add(new Resource("Liquid Fertilizer", 4, true, false), 3);
                                buildingInstance.SeasonalMultiplier = 1.5;
                                break;
                            case 3:
                                fertilizerType = GameData.FertilizerType.Both;
                                buildingInstance.StartupCosts.Add(new Resource("Solid Fertilizer", 1, true, false), 5);
                                buildingInstance.StartupCosts.Add(new Resource("Liquid Fertilizer", 4, true, false), 3);
                                buildingInstance.SeasonalMultiplier = 2.0;
                                break;
                        }
                    } else { Console.Write("Invalid input. "); continue; }

                    // Calculate hectares needed
                    double hectaresNeeded = Math.Round(annualAmount / buildingInstance.SeasonalMultiplier / 62, 2);

                    // Fields needed (optimized)
                    Dictionary<ProductionBuilding, int> fieldsNeeded = new Dictionary<ProductionBuilding, int>();
                    Dictionary<ProductionBuilding, int> farmsNeeded = new Dictionary<ProductionBuilding, int>();
                    int largeFields = 0;
                    int mediumFields = 0;
                    int smallFields = 0;
                    int largeFarms = 0;
                    int mediumFarms = 0;
                    int smallFarms = 0;
                    double remainder = 0;
                    const double smallFieldSize = 0.39;
                    const double mediumFieldSize = 1.57;
                    const double largeFieldSize = 4.81;
                    if ((int)(hectaresNeeded / largeFieldSize) >= 1)
                    {
                        largeFields = (int)(hectaresNeeded / largeFieldSize);
                        fieldsNeeded.Add(GameData.CreateLargeField(fertilizerType), largeFields);
                        if ((hectaresNeeded % largeFieldSize) >= mediumFieldSize)
                        {
                            mediumFields = (int)((hectaresNeeded % largeFieldSize) / mediumFieldSize);
                            fieldsNeeded.Add(GameData.CreateMediumField(fertilizerType), mediumFields);
                            if (((hectaresNeeded % largeFieldSize) % mediumFieldSize) >= smallFieldSize)
                            {
                                smallFields = (int)Math.Ceiling(((hectaresNeeded % largeFieldSize) % mediumFieldSize) / smallFieldSize);
                                fieldsNeeded.Add(GameData.CreateSmallField(fertilizerType), smallFields);
                            }
                            else
                            {
                                smallFields = 1;
                                fieldsNeeded.Add(GameData.CreateSmallField(fertilizerType), smallFields);
                            }
                        }
                        else
                        {
                            smallFields = (int)Math.Ceiling((hectaresNeeded % largeFieldSize) / smallFieldSize);
                            fieldsNeeded.Add(GameData.CreateSmallField(fertilizerType), smallFields);
                        }
                    }
                    else
                    {
                        mediumFields = (int)(hectaresNeeded / mediumFieldSize);
                        fieldsNeeded.Add(GameData.CreateMediumField(fertilizerType), mediumFields);
                        if ((hectaresNeeded % mediumFieldSize) >= smallFieldSize)
                        {
                            smallFields = (int)Math.Ceiling(((hectaresNeeded % mediumFieldSize) / smallFieldSize));
                            fieldsNeeded.Add(GameData.CreateSmallField(fertilizerType), smallFields);
                        }
                        else { 
                            smallFields = 1;
                            fieldsNeeded.Add(GameData.CreateSmallField(fertilizerType), smallFields);
                        }
                    }

                    // Calculate farm buildings needed
                    int handlingFields = 0;
                    foreach (var kv in fieldsNeeded)
                        handlingFields += kv.Value;
                    ProductionBuilding largeFarm = GameData.CreateLargeFarm();
                    ProductionBuilding mediumFarm = GameData.CreateMediumFarm();
                    ProductionBuilding smallFarm = GameData.CreateSmallFarm();
                    if (handlingFields >= 15)
                    {
                        largeFarms = handlingFields / 15;
                        farmsNeeded.Add(largeFarm,  largeFarms);
                        if ((handlingFields % 15) >= 6)
                        {
                            mediumFarms = (handlingFields % 15) / 6;
                            farmsNeeded.Add(mediumFarm, mediumFarms);
                            if (((handlingFields % 15) % 6) >= 3)
                            {
                                smallFarms = (int)Math.Ceiling((double)((handlingFields % 15) % 6) / 3);
                                farmsNeeded.Add(smallFarm, smallFarms);
                            } else
                            {
                                smallFarms = 1;
                                farmsNeeded.Add(smallFarm, smallFarms);
                            }
                        } else
                        {
                            smallFarms = (int)Math.Ceiling((double)(handlingFields % 15) / 3);
                            farmsNeeded.Add(smallFarm, smallFarms);
                        }
                    } else
                    {
                        mediumFarms = (handlingFields / 6);
                        farmsNeeded.Add(mediumFarm, mediumFarms);
                        if ((handlingFields % 6) >= 3)
                        {
                            smallFarms = (int)Math.Ceiling((((double)handlingFields % 6) / 3));
                            farmsNeeded.Add(smallFarm, smallFarms);
                        }
                        else
                        {
                            smallFarms = 1;
                            farmsNeeded.Add(smallFarm, smallFarms);
                        }
                    }

                    // Calculate total buildings needed
                    foreach (ProductionBuilding pb2 in fieldsNeeded.Keys)
                    {
                        for (int i = 0; i < fieldsNeeded[pb2]; i++)
                        {
                            BuildingInstance field = new BuildingInstance();
                            field.Building = pb2;
                            field.SeasonalMultiplier = buildingInstance.SeasonalMultiplier;
                            field.StartupCosts = buildingInstance.StartupCosts;
                            targetBuildingRequirement.BuildingInstances.Add(field);
                        }
                    }
                    foreach (ProductionBuilding pb2 in farmsNeeded.Keys)
                    {
                        for (int i = 0; i < farmsNeeded[pb2]; i++)
                        {
                            BuildingInstance farm = new BuildingInstance();
                            farm.Building = pb2;
                            targetBuildingRequirement.BuildingInstances.Add(farm);
                        }
                    }
                    targetBuildingRequirement.Count = targetBuildingRequirement.BuildingInstances.Count();
                    break;
                }
                fieldProcessed = true;
            }
            else if (pb.IsSeasonDependent && fieldProcessed) { continue; }
            // Sewage Treatment
            else if (pb.SewageDisposalCapacity > 0)
                targetBuildingRequirement.Count = (int)Math.Ceiling(targetAmount / pb.SewageDisposalCapacity);
            // Normal Production Buildings
            else
            {
                foreach (ResourceAmount ra in pb.Outputs)
                { if (ra.Resource == targetResource)
                        targetBuildingRequirement.Count = (int)Math.Ceiling(targetAmount / ra.Amount);
                }
            }
            targetBuildingRequirements.Add(targetBuildingRequirement);
        }
        result.TargetResource = targetResource;
        result.TargetAmount = targetAmount;
        result.Buildings.AddRange(targetBuildingRequirements);
        return result;
    }
    public static List<ProductionBuilding> FindBuildingForResource(string resourceName)
    {
        List<ProductionBuilding> result = new List<ProductionBuilding>();
        foreach (ProductionBuilding production_building in GameData.AllBuildings) {
            // Check if it's Sewage treatment
            if (production_building.SewageDisposalCapacity > 0 && production_building.Outputs.Any(ra => ra.Resource.Name == resourceName)) 
                result.Add(production_building);
            // Normal Production Buildings
            if (production_building.Outputs.Any(ra => ra.Resource.Name == resourceName) && production_building.SewageDisposalCapacity == 0)
                result.Add(production_building);
        }
        return result;
    }
}