using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.ComponentModel.Design;

namespace SovietRepublicPlanner
{
    internal class Program
    {
        static List<CalculationResult> allPlans = new List<CalculationResult>();
        // Navigation pointer: Points to the current position in the active plan's tree
        // Used for commands that navigate/modify existing plans (expand, dive, back, cancel)
        // - At root level: currentResult == rootResult (the plan itself)
        // - When diving: currentResult points to a SubChain within the plan
        // - When expanding: operations are performed on currentResult's level
        // DO NOT use this when creating NEW plans - create a local CalculationResult instead
        static CalculationResult currentResult;
        static int currentPlanIndex = -1;
        static CalculationResult rootResult => (currentPlanIndex >= 0 && currentPlanIndex < allPlans.Count)
    ? allPlans[currentPlanIndex]
    : null;
        static Stack<CalculationResult> navigationStack = new Stack<CalculationResult>();
        static void Main(string[] args)
        {
            // Load existing plans if they exist
            if (File.Exists("plans.json"))
            {
                Console.WriteLine("Found saved plans. Load? [y/n]:");
                if (Console.ReadKey().KeyChar == 'y')
                {
                    // Load plans from file
                    allPlans = LoadPlans("plans.json");
                    if (allPlans.Count > 0)
                    {
                        currentPlanIndex = 0;
                        currentResult = allPlans[0];
                        Console.WriteLine("\nLoaded plans. Type 'listplans' to view them.");
                    }
                }
            }
            Console.WriteLine("\n");

            // Main program loop
            while (true)
            {
                // If there are plans -> Command Loop
                if (allPlans.Count() > 0) { CommandLoop(); }

                // Creation menu
                Console.WriteLine("\n────────────────────────────────────────");
                Console.WriteLine("  'newplan'   - Resource-target mode");
                Console.WriteLine("  'buildplan' - Building-count mode");
                Console.WriteLine("  'navigate'  - View/modify existing plans");
                Console.WriteLine("  'done'      - Exit program");
                Console.Write("> ");
                List<string> planInputs = new List<string> { "newplan", "buildplan", "navigate", "view", "done"};
                string planInput = ReadLineWithCompletion(planInputs).ToLower().Trim();
                if (planInput == "newplan") { CreateNewPlan(); }            // After creating, loop back (will enter CommandLoop next iteration)
                else if (planInput == "buildplan") { CreateBuildPlan(); }   // After creating, loop back (will enter CommandLoop next iteration)
                else if (planInput == "navigate" || planInput == "view")    // Go back into CommandLoop without creating a new plan
                {
                    if (allPlans.Count > 0) { continue; }   // Skip to next iteration, which enters CommandLoop
                    else {Console.WriteLine("No plans exist yet. Create one first!");}
                }
                else if (planInput == "done")                               
                { 
                    Console.Write("Save plans before exiting? [y/n]: ");
                    if (Console.ReadKey().KeyChar == 'y')
                    {
                        Console.WriteLine($"Current directory: {Directory.GetCurrentDirectory()}");
                        SavePlans(allPlans, "plans.json");
                        Console.WriteLine("\n✓ Plans saved!");
                    }
                    Console.WriteLine("\nGoodbye!");
                    break;
                }                           // Exit program   
                else { Console.WriteLine("Invalid command. Try 'newplan', 'buildplan', 'navigate' or 'done'."); }
            }
            return;
        }
        // Helper method
        static void ExpandUtility(CalculationResult result, Resource utility, double totalNeeded)
        {
            Console.WriteLine($"\n{utility.Name}: {totalNeeded:F2} needed");

            // Find buildings that produce this utility
            List<ProductionBuilding> utilityBuildings = CalculationEngine.FindBuildingForResource(utility.Name);
            if (utilityBuildings.Count() == 0) { Console.WriteLine($"No buildings found that produce {utility.Name}!"); return; }

            // Show options and get user choice
            CalculationResult expandedResult = new CalculationResult();
            expandedResult.TargetResource = utility;
            expandedResult.TargetAmount = totalNeeded;
            int choiceIndex;
            expandedResult = CalculationEngine.Calculate(utility.Name, totalNeeded);

            // Display Options
            Console.WriteLine($"{expandedResult.TargetResource.Name} {expandedResult.TargetAmount} has been expanded.");
            // Display Sewage Treatment Options
            if (utility == GameData.WasteWaterResource)
            {
                for (int i = 0; i < expandedResult.Buildings.Count(); i++)
                {
                    Console.WriteLine("\n====================================================================");
                    Console.WriteLine($"Required number of building: {expandedResult.Buildings[i].Count} {expandedResult.Buildings[i].Building.Name}\n" +
                        $"Total Workers: {expandedResult.Buildings[i].TotalWorkers}");
                    Console.Write("Sewage Disposal Capacity: ");
                    foreach (Resource r in expandedResult.Buildings[i].ExpectedOutput.Keys)
                    {
                        Console.Write($"{expandedResult.Buildings[i].ExpectedOutput[r]}㎥/day {r.Name} ");
                    }
                    Console.WriteLine("\n\nRequired Input Resources: ");
                    foreach (Resource r in expandedResult.Buildings[i].RequiredResources.Keys)
                    {
                        Console.WriteLine($"- {expandedResult.Buildings[i].RequiredResources[r]} {r.Name}");
                    }
                    Console.WriteLine($"Power consumption: {expandedResult.Buildings[i].TotalPowerNeeded}");
                    Console.WriteLine($"Water consumption: {expandedResult.Buildings[i].TotalWaterNeeded}");
                    Console.WriteLine($"Heat consumption: {expandedResult.Buildings[i].TotalHeatNeeded}");
                    Console.WriteLine($"Sewage produced: {expandedResult.Buildings[i].TotalSewageProduced}");
                    Console.WriteLine($"Garbage produced: {expandedResult.Buildings[i].TotalGarbageProduced}");
                    Console.WriteLine($"Pollution emitted: {expandedResult.Buildings[i].TotalEnvironmentPollution}");
                }
            }
            // Display Power/Water Options
            else { DisplayOptions(expandedResult); }

            // User chooses a BuildingRequirement (if there's more than one option)
            if (expandedResult.Buildings.Count > 1)
            {
                Console.Write($"Choose the Option plan(number): ");
                while (true)
                {
                    if (!int.TryParse(Console.ReadLine(), out choiceIndex) || choiceIndex < 0 || choiceIndex > expandedResult.Buildings.Count())
                    {
                        Console.WriteLine("Invalid input. Choose the Option plan(number).");
                    }
                    else
                    {
                        choiceIndex--;
                        expandedResult.ChosenBuilding = expandedResult.Buildings[choiceIndex];
                        break;
                    }
                }
            }
            // When there's only one option
            else
            {
                choiceIndex = 0;
                Console.Write("Do you want to add this plan? (y/n): ");
                char addInput;
                while (true)
                {
                    if (!char.TryParse(Console.ReadLine(), out addInput))
                    {
                        Console.Write("Invalid input. Do you want to add this plan? (y/n):");
                        break;
                    }
                    else if (addInput == 'y')
                    {
                        expandedResult.ChosenBuilding = expandedResult.Buildings[choiceIndex];
                        break;
                    }
                    else if (addInput == 'n') { break; }
                }
            }
            result.SubChains.Add(expandedResult);
            Console.WriteLine($"{utility.Name} has been expanded!");
        }
        static void DisplayOptions(CalculationResult result)
        {
            for (int i = 0; i < result.Buildings.Count(); i++)
            {
                Console.WriteLine("\n====================================================================");
                Console.WriteLine($"Required number of building: {result.Buildings[i].Count} {result.Buildings[i].Building.Name}\n" +
                    $"Total Workers: {result.Buildings[i].TotalWorkers}");
                Console.Write("Expected Output: ");
                foreach (Resource r in result.Buildings[i].ExpectedOutput.Keys)
                {
                    Console.Write($"{result.Buildings[i].ExpectedOutput[r]:F2}t/day {r.Name} ");
                }
                Console.WriteLine("\n\nRequired Input Resources: ");
                foreach (Resource r in result.Buildings[i].RequiredResources.Keys)
                {
                    Console.WriteLine($"· {result.Buildings[i].RequiredResources[r]:F2} {r.Name}");
                }
                Console.WriteLine($"Power consumption: {result.Buildings[i].TotalPowerNeeded:F2}");
                Console.WriteLine($"Water consumption: {result.Buildings[i].TotalWaterNeeded:F2}");
                Console.WriteLine($"Heat consumption: {result.Buildings[i].TotalHeatNeeded:F2}");
                Console.WriteLine($"Sewage produced: {result.Buildings[i].TotalSewageProduced:F2}");
                Console.WriteLine($"Garbage produced: {result.Buildings[i].TotalGarbageProduced:F6}");
                Console.WriteLine($"Pollution emitted: {result.Buildings[i].TotalEnvironmentPollution:F6}");
            }
        }
        static void SavePlans(List<CalculationResult> plans, string filename)
        {
            try
            {
                var saveFile = new SaveFile
                {
                    Plans = plans.Select(p => ConvertToSavedPlan(p)).ToList()
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(saveFile, options);
                File.WriteAllText(filename, jsonString);
                Console.WriteLine($"\n✓ Plans saved to {filename}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Error saving plans: {ex.Message}");
            }
        }
        static List<CalculationResult> LoadPlans(string filename)
        {
            try
            {
                string jsonString = File.ReadAllText(filename);
                var saveFile = JsonSerializer.Deserialize<SaveFile>(jsonString);
                var plans = saveFile.Plans
                    .Select(sp => ConvertFromSavedPlan(sp))
                    .ToList();
                Console.WriteLine($"✓ Loaded {plans.Count} plan(s) from {filename}\n");
                return plans;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error loading plans: {ex.Message}\n");
                return new List<CalculationResult>();
            }
        }
        private static SavedPlan ConvertToSavedPlan(CalculationResult plan)
        {
            // Find chosen building index
            int chosenIndex = -1;
            if (plan.ChosenBuilding != null)
            {
                chosenIndex = plan.Buildings.IndexOf(plan.ChosenBuilding);
            }

            var saved = new SavedPlan
            {
                ResourceName = plan.TargetResource.Name,
                BuildingName = plan.ChosenBuilding?.Building.Name,
                Amount = plan.TargetAmount,
                Loyalty = plan.WorkerLoyalty,
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
                SupportBuildings = plan.SupportBuildings.Select(br => new SavedPlan.SavedBuildingInstance
                {
                    BuildingName = br.Building.Name,
                    Count = br.Count
                }).ToList(),
                ResidentialBuildings = plan.ResidentialBuildings.Select(rb => new SavedPlan.SavedResidentialInstance
                {
                    BuildingName = rb.Building.Name,
                    Count = rb.Count
                }).ToList(),
                AmenityBuildings = plan.AmenityBuildings.Select(ab => new SavedPlan.SavedAmenityInstance
                {
                    BuildingName = ab.Building.Name,
                    Count = ab.Count
                }).ToList(),
                TransportationBuildings = plan.TransportationBuildings.Select(tb => new SavedPlan.SavedTransportationInstance
                {
                    BuildingName = tb.Building.Name,
                    Count = tb.Count
                }).ToList(),
            };

            return saved;
        }
        private static CalculationResult ConvertFromSavedPlan(SavedPlan savedPlan)
        {
            CalculationResult result;

            if (savedPlan.IsBuildingBasedPlan)
            {
                // Reconstruct building-based plan
                result = new CalculationResult();
                result.WorkerLoyalty = savedPlan.Loyalty;
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
                    br.WorkerLoyalty = savedPlan.Loyalty;

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
                result.WorkerLoyalty = savedPlan.Loyalty;

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
                    br.WorkerLoyalty = savedPlan.Loyalty;

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

                    result.ChosenBuilding = br;
                    result.Buildings.Add(br);
                }
            }

            // ✅ RECURSIVE PART - restore all subchains
            foreach (var savedSubChain in savedPlan.SubChains)
            {
                var subChain = ConvertFromSavedPlan(savedSubChain);  // Calls itself!
                result.SubChains.Add(subChain);
            }

            return result;
        }
        static void CreateNewPlan()
        {
            CalculationResult result = new CalculationResult();
            double workerLoyalty = 50;
            int choiceIndex;

            // Worker Loyalty Choice : User Interaction
            while (true)
            {
                Console.Write("Type the loyalty of workers (0-100 default: 50 (100% productivity)\n: ");
                if (double.TryParse(Console.ReadLine(), out workerLoyalty) && workerLoyalty >= 0 && workerLoyalty <= 100)
                {
                    CalculationSettings.WorkerLoyalty = workerLoyalty;
                    result.WorkerLoyalty = workerLoyalty;
                }
                else { Console.Write("Invalid input. "); continue; }
                break;
            }

            // Resource Choice : User Interaction
            List<Resource> allResources = GameData.AllResources;
            int userInput = 0;
            double inputAmount = 0;
            Console.WriteLine("Resources:\n");
            for (int i = 0; i < allResources.Count; i++) { Console.WriteLine($"{i}: {allResources[i].Name}"); }
            while (true)
            {
                Console.Write("Choose a Resource: ");
                if (!int.TryParse(Console.ReadLine(), out userInput) || userInput < 0 || userInput >= allResources.Count)
                {
                    Console.WriteLine("Invalid input");
                    continue;
                }
                break;
            }
            while (true)
            {
                Console.Write("Choose the Amount (t/day): ");
                if (!double.TryParse(Console.ReadLine(), out inputAmount) || inputAmount < 0)
                {
                    Console.WriteLine("Invalid input");
                    continue;
                }
                break;
            }

            // Calculate
            result = CalculationEngine.Calculate(allResources[userInput].Name, inputAmount);
            // Display
            Console.WriteLine($"\nTarget Resource: {result.TargetResource.Name} {result.TargetAmount}t/day");
            for (int i = 0; i < result.Buildings.Count; i++)
            {
                if (result.TargetResource.Name == "Crops")
                {
                    Dictionary<ProductionBuilding, int> fieldsNeeded = new Dictionary<ProductionBuilding, int>();
                    Dictionary<ProductionBuilding, int> farmsNeeded = new Dictionary<ProductionBuilding, int>();
                    double expectedOutput = 0;
                    Dictionary<Resource, double> fertilizers = new Dictionary<Resource, double>();
                    double totalGarbage = 0;
                    foreach (var bi in result.Buildings[i].BuildingInstances)
                    {
                        if (bi.Building.Name.Contains("Field"))
                        {
                            if (fieldsNeeded.ContainsKey(bi.Building))
                                fieldsNeeded[bi.Building]++;
                            else
                                fieldsNeeded.Add(bi.Building, 1);
                        }
                        else if (bi.Building.Name.Contains("Farm"))
                        {
                            if (farmsNeeded.ContainsKey(bi.Building)) farmsNeeded[bi.Building]++;
                            else farmsNeeded.Add(bi.Building, 1);
                        }
                    }
                    Console.WriteLine("\n====================================================================");
                    Console.Write($"Required number of fields: ");
                    foreach (var key in fieldsNeeded.Keys)
                        Console.Write($"{fieldsNeeded[key]} x {key.Name} ");
                    Console.Write($"\nRequired number of farms: ");
                    foreach (var key in farmsNeeded.Keys)
                        Console.Write($"{farmsNeeded[key]} x {key.Name}");
                    Console.Write("\nExpected Output: ");
                    foreach (var key in result.Buildings[i].BuildingInstances)
                        foreach (var val in key.ExpectedOutput.Values)
                            expectedOutput += val;
                    Console.WriteLine($"{expectedOutput:F2}t/year");
                    Console.WriteLine("\nRequired Input Resources: ");
                    foreach (var key in result.Buildings[i].BuildingInstances)
                        foreach (var fer in key.Building.Inputs)
                            if (fertilizers.ContainsKey(fer.Resource)) fertilizers[fer.Resource] += fer.Amount;
                            else fertilizers.Add(fer.Resource, fer.Amount);
                    foreach (var key in fertilizers)
                        Console.WriteLine($"- {key.Key.Name} {key.Value}");
                    Console.WriteLine($"\nPower consumption: {result.Buildings[i].TotalPowerNeeded}");
                    Console.WriteLine($"Water consumption: {result.Buildings[i].TotalWaterNeeded}");
                    Console.WriteLine($"Heat consumption: {result.Buildings[i].TotalHeatNeeded}");
                    Console.WriteLine($"Sewage produced: {result.Buildings[i].TotalSewageProduced}");
                    foreach (var bi in result.Buildings[i].BuildingInstances)
                        totalGarbage += bi.Building.GarbageProduction;
                    Console.WriteLine($"Garbage produced: {totalGarbage}");
                    Console.WriteLine($"Pollution emitted: {result.Buildings[i].TotalEnvironmentPollution}");
                }
                // Default Industries
                else { DisplayOptions(result); }
            }

            // User chooses a BuildingRequirement (if there's more than one option)
            if (result.Buildings.Count > 1)
            {
                Console.Write($"Choose the Option plan(number): ");
                while (true)
                {
                    if (!int.TryParse(Console.ReadLine(), out choiceIndex))
                    {
                        Console.WriteLine("Invalid input. Choose the Option plan(number).");
                    }
                    else
                    {
                        choiceIndex--;
                        result.ChosenBuilding = result.Buildings[choiceIndex];
                        // if the building can use vehicles instead of workers
                        if (result.ChosenBuilding.Building.CanUseVehicles)
                        {
                            Console.Write("Use vehicles instead of workers? (y/n, default: n): ");
                            char vehicleChoice = Console.ReadKey().KeyChar;
                            Console.WriteLine();

                            if (vehicleChoice == 'y')
                            {
                                result.ChosenBuilding.Building.WorkersPerShift = 0;  // Override to vehicles
                            }
                            // else keep the default 100 workers
                        }
                        break;
                    }
                }
            }
            // When there's only one option
            else
            {
                choiceIndex = 0;
                Console.Write("Do you want to add this plan? (y/n): ");
                char addInput;
                while (true)
                {
                    if (!char.TryParse(Console.ReadLine(), out addInput))
                    {
                        Console.Write("Invalid input. Do you want to add this plan? (y/n):");
                        break;
                    }
                    else if (addInput == 'y')
                    {
                        result.ChosenBuilding = result.Buildings[choiceIndex];
                        // if the building can use vehicles instead of workers
                        if (result.ChosenBuilding.Building.CanUseVehicles)
                        {
                            Console.Write("Use vehicles instead of workers? (y/n, default: n): ");
                            char vehicleChoice = Console.ReadKey().KeyChar;
                            Console.WriteLine();

                            if (vehicleChoice == 'y')
                            {
                                result.ChosenBuilding.Building.WorkersPerShift = 0;  // Override to vehicles
                            }
                            // else keep the default 100 workers
                        }
                        break;
                    }
                    else if (addInput == 'n') { break; }
                    else
                    {
                        Console.Write("Invalid input. Do you want to add this plan? (y/n):");
                        continue;
                    }
                }
            }

            // Add to list and set as current
            allPlans.Add(result);
            currentPlanIndex = allPlans.Count - 1;
            currentResult = allPlans[currentPlanIndex];
            navigationStack.Clear();
            CommandLoop();
        }
        static void CreateBuildPlan()
        {
            double workerLoyalty = 50;
            int choiceIndex;
            CalculationResult result = new CalculationResult();

            // Worker Loyalty Choice : User Interaction
            while (true)
            {
                Console.Write("Type the loyalty of workers (0-100 default: 50 (100% productivity)\n: ");
                if (double.TryParse(Console.ReadLine(), out workerLoyalty) && workerLoyalty >= 0 && workerLoyalty <= 100)
                    result.WorkerLoyalty = workerLoyalty;
                else { Console.Write("Invalid input. "); continue; }
                break;
            }

            // Building Choice : User Interaction
            List<ProductionBuilding> allBuildings = GameData.AllBuildings;
            List<ProductionBuilding> allExtractions = allBuildings.Where(b => b.IsQualityDependent).ToList();
            List<ProductionBuilding> allProcessing = allBuildings.Where(b => !b.IsQualityDependent && !b.IsSeasonDependent && !b.IsSupportBuildings && !b.IsUtilityBuilding).ToList();
            List<ProductionBuilding> selectedCategory = new List<ProductionBuilding>();
            int userInput = 0;
            double inputAmount = 0;
            string[] inputStrings;
            Console.WriteLine("Select a building category:\n");
            Console.WriteLine("1. Extraction (mines, quarries, pumpjacks, woodcutters)");
            Console.WriteLine("2. Processing (factories, plants)");
            Console.Write("Type number:\n> ");
            
            //    1) Display Category/building Option
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out userInput) && userInput > 0 && userInput <= 2)
                {
                    if (userInput == 1)
                    {
                        selectedCategory = allExtractions;
                        for (int i = 0; i < selectedCategory.Count(); i++)
                            Console.WriteLine($"[{i}]: {selectedCategory[i].Name}");
                    }
                    else if (userInput == 2)
                    {
                        selectedCategory = allProcessing;
                        for (int i = 0; i < selectedCategory.Count(); i++)
                            Console.WriteLine($"[{i}]: {selectedCategory[i].Name}");
                    }
                }
                else { Console.Write("invalid input:"); continue; }
                break;
            }

            //    2) User selects building
            while (true)
            {
                Console.Write("Select building:\n> ");
                if (int.TryParse(Console.ReadLine(), out userInput) && userInput >= 0 && userInput < selectedCategory.Count())
                {
                    BuildingRequirement br = new BuildingRequirement(selectedCategory[userInput]);
                    result.ChosenBuilding = br;
                    // if the building can use vehicles instead of workers
                    if (result.ChosenBuilding.Building.CanUseVehicles)
                    {
                        Console.Write("Use vehicles instead of workers? (y/n, default: n): ");
                        char vehicleChoice = Console.ReadKey().KeyChar;
                        Console.WriteLine();

                        if (vehicleChoice == 'y')
                        {
                            result.ChosenBuilding.Building.WorkersPerShift = 0;  // Override to vehicles
                        }
                        // else keep the default 100 workers
                    }
                }
                else { Console.Write("invalid input: "); continue; }
                break;
            }

            //      3) Amount of buildings
            bool fieldProcessed = false;
            while (true)
            {
                Console.Write($"How many {result.ChosenBuilding.Building.Name}?\n> ");
                if (int.TryParse(Console.ReadLine(), out userInput) && userInput >= 0)
                {
                    // Extraction
                    if (result.ChosenBuilding.Building.IsQualityDependent)
                    {
                        result.ChosenBuilding.Count = userInput;
                        List<double> qualities = new List<double>();
                        while (true)
                        {
                            qualities.Clear();
                            Console.Write("Enter qualities for each spot (comma-separated, e.g. 80,75,90):\r\n>");
                            inputStrings = Console.ReadLine()?.ToLower().Trim().Split(',');
                            foreach (string qualityInput in inputStrings)
                            {
                                int quality;
                                if (int.TryParse(qualityInput.Trim(), out quality))
                                {
                                    double percentageQuality = Math.Round((double)quality / 100, 2);
                                    qualities.Add(percentageQuality);
                                }
                                else {Console.Write("Wrong input. Try again.");break;}
                            }
                            if (qualities.Count() != userInput) { continue; }
                            break;
                        }
                        for (int i = 0; i < userInput; i++)
                        {
                            BuildingInstance buildingInstance = new BuildingInstance();
                            buildingInstance.Building = result.ChosenBuilding.Building;
                            buildingInstance.ResourceAbundanceMultiplier = qualities[i];
                            result.ChosenBuilding.BuildingInstances.Add(buildingInstance);
                        }
                        break;
                    }
                    // Factories, Plants
                    else result.ChosenBuilding.Count = userInput;
                    break;
                }
                else { Console.Write("invalid input: "); continue; }
            }

            // Calculate & Display Output
            Console.WriteLine("\n====================================================================");
            Console.WriteLine($"Required number of building: {result.ChosenBuilding.Count} {result.ChosenBuilding.Building.Name}\n" +
                $"Total Workers: {result.ChosenBuilding.TotalWorkers}");
            //      1) If Mines/Pumpjacks/Woodcutting Posts
            if (result.ChosenBuilding.Building.IsQualityDependent)
            {
                Console.Write($"Qualities: ");
                for (int i = 0; i <  result.ChosenBuilding.BuildingInstances.Count; i ++)
                    Console.Write($"{result.ChosenBuilding.BuildingInstances[i].ResourceAbundanceMultiplier * 100}% ");
            }
            Console.Write("\nExpected Output: ");
            foreach (Resource r in result.ChosenBuilding.ExpectedOutput.Keys)
            {
                Console.Write($"{result.ChosenBuilding.ExpectedOutput[r]:F2}t/day {r.Name} ");
            }
            Console.WriteLine("\n\nRequired Input Resources: ");
            foreach (Resource r in result.ChosenBuilding.RequiredResources.Keys)
            {
                Console.WriteLine($"· {result.ChosenBuilding.RequiredResources[r]:F2} {r.Name}");
            }
            Console.WriteLine($"Power consumption: {result.ChosenBuilding.TotalPowerNeeded:F2}");
            Console.WriteLine($"Water consumption: {result.ChosenBuilding.TotalWaterNeeded:F2}");
            Console.WriteLine($"Heat consumption: {result.ChosenBuilding.TotalHeatNeeded:F2}");
            Console.WriteLine($"Sewage produced: {result.ChosenBuilding.TotalSewageProduced:F2}");
            Console.WriteLine($"Garbage produced: {result.ChosenBuilding.TotalGarbageProduced:F6}");
            Console.WriteLine($"Pollution emitted: {result.ChosenBuilding.TotalEnvironmentPollution:F6}");

            // User chooses a BuildingRequirement
            Console.Write("Do you want to add this plan? (y/n): ");
            char addInput;
            while (true)
            {
                if (!char.TryParse(Console.ReadLine(), out addInput))
                {
                    Console.Write("Invalid input. Do you want to add this plan? (y/n):");
                    break;
                }
                else if (addInput == 'y')
                {
                    result.TargetResource = result.ChosenBuilding.ExpectedOutput.Keys.FirstOrDefault();
                    result.TargetAmount = 0;
                    allPlans.Add(result);
                    Console.WriteLine($"The plan 'BB-{result.ChosenBuilding.Building.Name} x {result.ChosenBuilding.Count}' has been added!");
                    break;
                }
                else if (addInput == 'n') { result.ChosenBuilding = null; break; }
                else
                {
                    Console.Write("Invalid input. Do you want to add this plan? (y/n):");
                    continue;
                }
            }

            // Add to list and set as current
            currentPlanIndex = allPlans.Count - 1;
            currentResult = allPlans[currentPlanIndex];
            navigationStack.Clear();
            CommandLoop();
        }
        static void CommandLoop()
        {
            double workerLoyalty = 50;
            int userInput;
            double inputAmount;
            string expandInput;
            int choiceIndex;
            List<Resource> allResources = GameData.AllResources;

            // Command loop
            while (true)
            {
                Console.Write("\nCommand (listplans/masterplan/switchplan/create/expand/support/cancel/back/dive/summary/housing/amenity/transportation/done): ");
                List<string> commands = new List<string> { "listplans", "masterplan", "switchplan", "create", "expand", "support", "cancel", "back", "dive", "summary", "housing", "amenity", "transportation", "done" };
                string command = ReadLineWithCompletion(commands).ToLower().Trim(); if (command == "expand")
                {
                    // Choose Resources or Utility to expand
                    List<Resource> resourcesToExpand = new List<Resource>();

                    // Option 1: Production Inputs
                    List<Resource> productionInputs = currentResult.ChosenBuilding.RequiredResources.Keys
                        .Where(r => !r.IsUtility)
                        .ToList();

                    // Option 2: Citizen consumption (only at root)
                    List<Resource> citizenNeeds = new List<Resource>();
                    if (currentResult == rootResult && rootResult.TotalCitizenConsumption.Count() > 0)
                    {
                        foreach (var kv in rootResult.TotalCitizenConsumption)
                        {
                            // Skip if it's the target resource
                            if (kv.Key == rootResult.TargetResource) continue;
                            citizenNeeds.Add(kv.Key);
                        }
                    }

                    // Display Options grouped
                    Console.WriteLine("Available resources to expand:");
                    if (productionInputs.Count() > 0)
                    {
                        Console.WriteLine("\n[Production Inputs]");
                        foreach (var r in productionInputs)
                            Console.WriteLine($"  · {r.Name}");
                    }
                    if (citizenNeeds.Count > 0)
                    {
                        Console.WriteLine("\n[Citizen Consumption]");
                        foreach (var r in citizenNeeds)
                            Console.WriteLine($"  · {r.Name}");
                    }

                    bool isUtility = false;
                    Resource utilityResource = null;
                    double utilityAmount = 0;
                    bool allValid = false;
                    while (!allValid)
                    {
                        Console.Write($"\nType the resource or utility to expand ('0' to back): ");
                        expandInput = Console.ReadLine();
                        string[] inputNames = expandInput.Split(',');

                        // Clear previous attempt
                        resourcesToExpand.Clear();
                        allValid = true;

                        // Validate each input name
                        foreach (string name in inputNames)
                        {
                            string trimmedName = name.Trim();
                            Resource matchedResource = null;

                            if (trimmedName == "0") { break; }

                            // check if it's Utility
                            if (trimmedName == "power")
                            {
                                isUtility = true;
                                utilityResource = GameData.PowerResource;
                                utilityAmount = currentResult.TotalPowerNeeded + (currentResult.ChosenBuilding.RequiredResources.ContainsKey(GameData.PowerResource)
                                    ? currentResult.ChosenBuilding.RequiredResources[GameData.PowerResource] : 0);
                                ExpandUtility(currentResult, utilityResource, utilityAmount);
                                continue;
                            }
                            if (trimmedName == "water")
                            {
                                isUtility = true;
                                utilityResource = GameData.WaterResource;
                                utilityAmount = currentResult.TotalWaterNeeded + (currentResult.ChosenBuilding.RequiredResources.ContainsKey(GameData.WaterResource)
                                    ? currentResult.ChosenBuilding.RequiredResources[GameData.WaterResource] : 0);
                                ExpandUtility(currentResult, utilityResource, utilityAmount);
                                continue;
                            }
                            if (trimmedName == "sewage")
                            {
                                isUtility = true;
                                utilityResource = GameData.WasteWaterResource;
                                utilityAmount = currentResult.TotalSewageProduced;
                                ExpandUtility(currentResult, utilityResource, utilityAmount);
                            }
                            if (trimmedName == "heat")
                            {
                                isUtility = true;
                                utilityResource = GameData.HeatResource;
                                utilityAmount = currentResult.TotalHeatNeeded;
                                ExpandUtility(currentResult, utilityResource, utilityAmount);
                            }

                            // Search for the resource
                            foreach (Resource r in currentResult.ChosenBuilding.RequiredResources.Keys)
                            {
                                if (r.Name.ToLower() == trimmedName)
                                {
                                    if (r.IsUtility)
                                    {
                                        Console.WriteLine($"Unexpected utility resource: {r.Name}");
                                        allValid = false;
                                        break;
                                    }
                                    // Normal resource
                                    else
                                    {
                                        resourcesToExpand.Add(r);
                                        matchedResource = r;
                                        break;
                                    }
                                }
                            }

                            // Search for the citizen consumption
                            foreach (Resource r in citizenNeeds)
                            {
                                if (r.Name.ToLower() == trimmedName)
                                {
                                    if (!resourcesToExpand.Contains(r))
                                    {
                                        resourcesToExpand.Add(r);
                                        matchedResource = r;
                                    }
                                    else continue;
                                }
                            }

                            if (matchedResource == null && name.ToLower() == "sewage") { allValid = true; break; }
                            if (matchedResource == null && name.ToLower() == "heat") { allValid = true; break; }
                            if (matchedResource == null)
                            {
                                Console.WriteLine($"{name} does not exist in the requirement.");
                                allValid = false;
                                break;
                            }
                        }
                    }
                    // See if the resource to expand is already sourced by previously established Production Buildlings
                    // 1) Get the redisue of previously produced Resources : rootResult.TotalResidues
                    // 2) If the user's input is in rootResult.TotalResidues, subtract the value from rootResult.TotalResidues
                    for (int i = resourcesToExpand.Count - 1; i >= 0; i--)
                    {
                        Resource r = resourcesToExpand[i];
                        // If there's residue, and it's enough to source it:
                        if (rootResult.TotalResidues.Any(er => er.Key == r) && rootResult.TotalResidues[r] >= currentResult.ChosenBuilding.RequiredResources[r])
                        {
                            Console.Write($"The resource '{r.Name}' is already locally produced. Do you want to use them? [y/n]:");
                            char addInput;
                            while (true)
                            {
                                if (!char.TryParse(Console.ReadLine(), out addInput))
                                {
                                    Console.Write("Invalid input. Do you want to use the existing resource? [y/n]:");
                                    break;
                                }
                                else if (addInput == 'y')
                                {
                                    // Use residue for the required resource 
                                    if (currentResult.InternallySourcedResources.ContainsKey(r))
                                        currentResult.InternallySourcedResources[r] += currentResult.ChosenBuilding.RequiredResources[r];
                                    else
                                        currentResult.InternallySourcedResources.Add(r, currentResult.ChosenBuilding.RequiredResources[r]);
                                    resourcesToExpand.RemoveAt(i);
                                    break;
                                }
                                else if (addInput == 'n') { break; }
                            }
                        }
                    }

                    // If there's no resources to expand, go back to the command screen
                    if (resourcesToExpand.Count() < 1) { continue; }

                    // Calculate expansion
                    for (int j = 0; j < resourcesToExpand.Count(); j++)
                    {
                        CalculationResult expandedResult = new CalculationResult();
                        expandedResult.TargetResource = resourcesToExpand[j];
                        // Use total imports if available (represents all unfulfilled demand)
                        if (rootResult.TotalImports.ContainsKey(resourcesToExpand[j]))
                        {
                            expandedResult.TargetAmount = rootResult.TotalImports[resourcesToExpand[j]];
                        }
                        else
                        {
                            // Fallback: shouldn't happen, but use current requirement
                            expandedResult.TargetAmount = productionInputs.Contains(resourcesToExpand[j])
                                ? currentResult.ChosenBuilding.RequiredResources[resourcesToExpand[j]]
                                : rootResult.TotalCitizenConsumption[resourcesToExpand[j]];
                        }
                        expandedResult = CalculationEngine.Calculate(expandedResult.TargetResource.Name, expandedResult.TargetAmount);
                        currentResult.SubChains.Add(expandedResult);
                        Console.WriteLine($"{expandedResult.TargetResource.Name} {expandedResult.TargetAmount} has been expanded.");
                        for (int i = 0; i < expandedResult.Buildings.Count; i++)
                        {
                            // Expand Crops
                            if (expandedResult.TargetResource.Name == "Crops")
                            {
                                Dictionary<ProductionBuilding, int> fieldsNeeded = new Dictionary<ProductionBuilding, int>();
                                Dictionary<ProductionBuilding, int> farmsNeeded = new Dictionary<ProductionBuilding, int>();
                                double expectedOutput = 0;
                                Dictionary<Resource, double> fertilizers = new Dictionary<Resource, double>();
                                double totalGarbage = 0;
                                foreach (var bi in expandedResult.Buildings[i].BuildingInstances)
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
                                Console.WriteLine("\n====================================================================");
                                Console.Write($"Required number of fields: ");
                                foreach (var key in fieldsNeeded.Keys)
                                    Console.Write($"{fieldsNeeded[key]} x {key.Name} ");
                                Console.Write($"\nRequired number of farms: ");
                                foreach (var key in farmsNeeded.Keys)
                                    Console.Write($"{farmsNeeded[key]} x {key.Name} ");
                                Console.Write("\nExpected Output: ");
                                foreach (var key in expandedResult.Buildings[i].BuildingInstances)
                                    foreach (var val in key.ExpectedOutput.Values)
                                        expectedOutput += val;
                                Console.WriteLine($"{expectedOutput:F2}t/year");
                                Console.WriteLine("\nRequired Input Resources: ");
                                foreach (var key in expandedResult.Buildings[i].BuildingInstances)
                                    foreach (var fer in key.Building.Inputs)
                                        if (fertilizers.ContainsKey(fer.Resource)) fertilizers[fer.Resource] += fer.Amount;
                                        else fertilizers.Add(fer.Resource, fer.Amount);
                                foreach (var key in fertilizers)
                                    Console.WriteLine($"- {key.Key.Name} {key.Value}");
                                Console.WriteLine($"\nPower consumption: {expandedResult.Buildings[i].TotalPowerNeeded}");
                                Console.WriteLine($"Water consumption: {expandedResult.Buildings[i].TotalWaterNeeded}");
                                Console.WriteLine($"Heat consumption: {expandedResult.Buildings[i].TotalHeatNeeded}");
                                Console.WriteLine($"Sewage produced: {expandedResult.Buildings[i].TotalSewageProduced}");
                                foreach (var bi in expandedResult.Buildings[i].BuildingInstances)
                                    totalGarbage += bi.Building.GarbageProduction;
                                Console.WriteLine($"Garbage produced: {totalGarbage}");
                                Console.WriteLine($"Pollution emitted: {expandedResult.Buildings[i].TotalEnvironmentPollution}");
                            }
                            break;
                        }
                        // Expand Default Industries
                        // Display options
                        if (expandedResult.TargetResource != GameData.CropsResource) { DisplayOptions(expandedResult); }

                        // User chooses a BuildingRequirement (if there's more than one option)
                        if (expandedResult.Buildings.Count > 1)
                        {
                            Console.Write($"Choose the Option plan(number): ");
                            while (true)
                            {
                                if (!int.TryParse(Console.ReadLine(), out choiceIndex))
                                {
                                    Console.WriteLine("Invalid input. Choose the Option plan(number).");
                                }
                                else
                                {
                                    choiceIndex--;
                                    expandedResult.ChosenBuilding = expandedResult.Buildings[choiceIndex];
                                    break;
                                }
                            }
                        }
                        // When there's only one option
                        else
                        {
                            choiceIndex = 0;
                            Console.Write("Do you want to add this plan? (y/n): ");
                            char addInput;
                            while (true)
                            {
                                if (!char.TryParse(Console.ReadLine(), out addInput))
                                {
                                    Console.Write("Invalid input. Do you want to add this plan? (y/n):");
                                    break;
                                }
                                else if (addInput == 'y')
                                {
                                    expandedResult.ChosenBuilding = expandedResult.Buildings[choiceIndex];
                                    break;
                                }
                                else if (addInput == 'n') { break; }
                            }
                        }
                    }
                    navigationStack.Push(currentResult);    // Remember where user was
                    currentResult = (currentResult.SubChains.Count - 1 < 0) ? currentResult : currentResult.SubChains[currentResult.SubChains.Count - 1];
                    continue;
                }
                else if (command == "create")
                {

                }
                else if (command == "listplans")
                {
                    if (allPlans.Count() == 0) { Console.WriteLine("No plans created yet."); continue; }

                    Console.WriteLine("\n=== All Plans ===");
                    for (int i = 0; i < allPlans.Count(); i++)
                    {
                        string marker = "← ACTIVE";
                        Console.WriteLine($"{i}. {allPlans[i].TargetResource.Name} ({allPlans[i].TargetAmount} t/day) " +
                            $"{(allPlans[i] == rootResult ? marker : null)}");
                    }
                    Console.WriteLine();
                    continue;
                }
                else if (command == "switchplan")
                {
                    if (allPlans.Count <= 1) { Console.WriteLine("Only one plan exists. Nothing to switch to."); continue; }

                    // Display plan list
                    Console.WriteLine("\n=== All Plans ===");
                    for (int i = 0; i < allPlans.Count(); i++)
                    {
                        string marker = "← ACTIVE";
                        Console.WriteLine($"{i}. {allPlans[i].TargetResource.Name} ({allPlans[i].TargetAmount} t/day) " +
                            $"{(allPlans[i] == rootResult ? marker : null)}");
                    }

                    // Choose Plan to switch
                    Console.Write("Choose the plan: ");
                    int planChoice;
                    if (int.TryParse(Console.ReadLine(), out planChoice) && planChoice >= 0 && planChoice < allPlans.Count())
                    {
                        currentPlanIndex = planChoice;
                        currentResult = allPlans[currentPlanIndex];
                        navigationStack.Clear();
                        Console.WriteLine($"Switched to: {rootResult.TargetResource.Name} ({rootResult.TargetAmount} t/day)");
                    }
                    else { Console.Write("Invalid input. "); continue; }
                }
                else if (command == "masterplan")
                {
                    if (allPlans.Count == 0) { Console.WriteLine("No plans created yet."); continue; }

                    Console.WriteLine("\n┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓");
                    Console.WriteLine("┃         MASTER PLAN                    ┃");
                    Console.WriteLine("┞━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┦");

                    // Initialize totals
                    int totalWorkers = 0;
                    int totalCitizen = 0;
                    double totalPower = 0;
                    double totalWater = 0;
                    double totalHeat = 0;
                    double totalGarbage = 0;
                    double totalPollution = 0;
                    int totalHousingCapacity = 0;

                    Dictionary<Resource, double> utilityProduction = new Dictionary<Resource, double>();
                    utilityProduction.Add(GameData.PowerResource, 0);
                    utilityProduction.Add(GameData.WaterResource, 0);
                    utilityProduction.Add(GameData.HeatResource, 0);
                    Dictionary<Resource, double> combinedImports = new Dictionary<Resource, double>();
                    Dictionary<Resource, double> combinedResidues = new Dictionary<Resource, double>();
                    Dictionary<Resource, double> combinedCitizenConsumption = new Dictionary<Resource, double>();
                    Dictionary<Resource, double> net = new Dictionary<Resource, double>();
                    Dictionary<ProductionBuilding, int> combinedSupBldgs = new Dictionary<ProductionBuilding, int>();
                    Dictionary<AmenityBuilding, int> combinedAmeBldgs = new Dictionary<AmenityBuilding, int>();
                    Dictionary<TransportationBuilding, int> combinedTransBldgs = new Dictionary<TransportationBuilding, int>();
                    Dictionary<Resource, double> combinedConstructionMaterials = new Dictionary<Resource, double>();

                    // Loop through allPlans and sum everything
                    foreach (var plan in allPlans)
                    {
                        totalWorkers += plan.TotalWorkers;
                        totalCitizen += plan.TotalPopulationNeeded;
                        totalPower += plan.TotalPowerNeeded;
                        totalWater += plan.TotalUtilityNeeds[GameData.WaterResource];
                        totalHeat += plan.TotalHeatNeeded;
                        totalGarbage += plan.TotalGarbageProduced;
                        totalPollution += plan.TotalEnvironmentPollution;
                        totalHousingCapacity += plan.TotalHousingCapacity;

                        // Track utility production from ExpandedUtilities
                        foreach (var kv in plan.ExpandedUtilities)
                        {
                            foreach (var output in kv.Value.ExpectedOutput)
                            {
                                if (utilityProduction.ContainsKey(output.Key))
                                    utilityProduction[output.Key] += output.Value;
                                else
                                    utilityProduction[output.Key] = output.Value;
                            }
                        }

                        // Combine citizen consumption
                        foreach (var kv in plan.TotalCitizenConsumption)
                        {
                            if (combinedCitizenConsumption.ContainsKey(kv.Key)) { combinedCitizenConsumption[kv.Key] += kv.Value; }
                            else combinedCitizenConsumption[kv.Key] = kv.Value;
                        }

                        // Combine imports (lacking resources that have to be imported)
                        foreach (var kv in plan.TotalImports)
                        {
                            if (combinedImports.ContainsKey(kv.Key)) combinedImports[kv.Key] += kv.Value;
                            else combinedImports[kv.Key] = kv.Value;
                            if (net.ContainsKey(kv.Key)) { net[kv.Key] -= kv.Value; }
                            else net[kv.Key] = -kv.Value;
                        }

                        // Combine residues
                        foreach (var kv in plan.TotalResidues)
                        {
                            if (combinedResidues.ContainsKey(kv.Key)) combinedResidues[kv.Key] += kv.Value;
                            else combinedResidues[kv.Key] = kv.Value;
                            if (net.ContainsKey(kv.Key)) { net[kv.Key] += kv.Value; }
                            else net[kv.Key] = kv.Value;
                        }

                        // Combine Support Buildings
                        foreach (var br in plan.SupportBuildings)
                        {
                            if (combinedSupBldgs.ContainsKey(br.Building)) combinedSupBldgs[br.Building] += br.Count;
                            else combinedSupBldgs.Add(br.Building, br.Count);
                        }

                        // Combine Amenity Buildings
                        foreach (var ai in plan.AmenityBuildings)
                        {
                            if (combinedAmeBldgs.ContainsKey(ai.Building)) combinedAmeBldgs[ai.Building] += ai.Count;
                            else combinedAmeBldgs.Add(ai.Building, ai.Count);
                        }

                        // Combine Transportation Buildings
                        foreach (var ti in plan.TransportationBuildings)
                        {
                            if (combinedTransBldgs.ContainsKey(ti.Building)) combinedTransBldgs[ti.Building] += ti.Count;
                            else combinedTransBldgs.Add(ti.Building, ti.Count);
                        }

                        // Combine Construction Materials
                        foreach (var kv in plan.TotalConstructionMaterials)
                        {
                            if (combinedConstructionMaterials.ContainsKey(kv.Key)) combinedConstructionMaterials[kv.Key] += kv.Value;
                            else combinedConstructionMaterials.Add(kv.Key,kv.Value);
                        }
                    }

                    // Display Status section
                    Console.WriteLine("├────────────────────────────────────────┤");
                    Console.WriteLine("│ Utilities Status:                      │");
                    Console.WriteLine("├────────────────────────────────────────┤");
                    Console.WriteLine("│                   Needed    Produced  Balance");
                    Console.WriteLine($"│ Workers    :    {totalWorkers,6}                        ");
                    Console.WriteLine($"│ Citizens   :    {totalCitizen,6}      {totalHousingCapacity,6}  {totalHousingCapacity - totalCitizen,7}");
                    Console.WriteLine($"│ Power (MW) :    {totalPower,6:F2}    {utilityProduction[GameData.PowerResource],7:F2} {utilityProduction[GameData.PowerResource] - totalPower,8:F2}");
                    Console.WriteLine($"│ Water (m³) :    {totalWater,6:F2}    {utilityProduction[GameData.WaterResource],7:F2} {utilityProduction[GameData.WaterResource] - totalWater,8:F2}");
                    Console.WriteLine($"│ Sewage (m³):    {totalWater,6:F2}    {utilityProduction[GameData.WaterResource],7:F2} {utilityProduction[GameData.WaterResource] - totalWater,8:F2}");
                    Console.WriteLine($"│ Heat (MW)  :    {totalHeat,6:F2}    {utilityProduction[GameData.HeatResource],7:F2} {utilityProduction[GameData.HeatResource] - totalHeat,8:F2}");
                    Console.WriteLine($"│ Garbage    :    {totalGarbage,6:F6} t/day              ");
                    Console.WriteLine($"│ Pollution  :    {totalPollution,6:F6} t/day            ");
                    Console.WriteLine("├────────────────────────────────────────┤");
                    Console.WriteLine("│ Citizen Consumption:                   │");
                    Console.WriteLine("├────────────────────────────────────────┤");
                    foreach (var kv in combinedCitizenConsumption)
                    {
                        foreach (var kv2 in net)
                        {
                            if (kv.Key == kv2.Key && (kv2.Value - kv.Value) < 0)
                                Console.WriteLine($"│ ○ {kv.Value - kv2.Value:F2} t/day {kv.Key.Name}  (Import)");
                            else if (kv.Key == kv2.Key && (kv2.Value - kv.Value) > 0)
                                Console.WriteLine($"│ ● {kv.Value:F2} t/day {kv.Key.Name}  (Local)");
                        }
                        if (!net.ContainsKey(kv.Key))
                            Console.WriteLine($"│ ○ {kv.Value:F2} t/day {kv.Key.Name}  (Import)");
                    }
                    Console.WriteLine("├────────────────────────────────────────┤");
                    Console.WriteLine("│ Planned Buildings:                     │");
                    Console.WriteLine("├────────────────────────────────────────┤");
                    foreach (var plan in allPlans)
                        plan.DisplayAllBuildings(plan, 0);
                    if (allPlans.Any(p => p.SupportBuildings.Count > 0))
                    {
                        Console.WriteLine("├────────────────────────────────────────┤");
                        Console.WriteLine("│ Support Infrastructures:               │");
                        Console.WriteLine("├────────────────────────────────────────┤");
                        foreach (var kv in combinedSupBldgs)
                            Console.WriteLine($"│ · {kv.Value} × {kv.Key.Name}");
                    }
                    if (allPlans.Any(p => p.ResidentialBuildings.Count > 0))
                    {
                        Dictionary<ResidentialBuilding, int> resBuildings = new Dictionary<ResidentialBuilding, int>();
                        Console.WriteLine("├────────────────────────────────────────┤");
                        Console.WriteLine("│ Residential Buildings:                 │");
                        Console.WriteLine("├────────────────────────────────────────┤");
                        foreach (var plan in allPlans)
                        {
                            foreach (var resins in plan.ResidentialBuildings)
                            {
                                if (resBuildings.ContainsKey(resins.Building)) resBuildings[resins.Building] += resins.Count;
                                else resBuildings.Add(resins.Building, resins.Count);
                            }
                        }
                        foreach (var kv in resBuildings)
                            Console.WriteLine($"│ · {kv.Value} × {kv.Key.Name}");
                    }
                    if (allPlans.Any(p => p.AmenityBuildings.Count > 0))
                    {
                        Console.WriteLine("├────────────────────────────────────────┤");
                        Console.WriteLine("│ Amenity Buildings:                     │");
                        Console.WriteLine("├────────────────────────────────────────┤");
                        foreach (var kv in combinedAmeBldgs)
                            Console.WriteLine($"│ · {kv.Value} × {kv.Key.Name}");
                    }
                    if (allPlans.Any(p => p.TransportationBuildings.Count > 0))
                    {
                        Console.WriteLine("├────────────────────────────────────────┤");
                        Console.WriteLine("│ Transportation Buildings:              │");
                        Console.WriteLine("├────────────────────────────────────────┤");
                        foreach (var kv in combinedTransBldgs)
                            Console.WriteLine($"│ · {kv.Value} × {kv.Key.Name}");
                    }
                    Console.WriteLine("├────────────────────────────────────────┤");
                    Console.WriteLine("│ Import Needed:                         │");
                    Console.WriteLine("├────────────────────────────────────────┤");
                    foreach (var kv in net)
                        if (kv.Value < 0)
                            Console.WriteLine($"│ {Math.Abs(kv.Value):F2} t/day x {kv.Key.Name}");
                    Console.WriteLine("├────────────────────────────────────────┤");
                    Console.WriteLine("│ Residues:                              │");
                    Console.WriteLine("├────────────────────────────────────────┤");
                    foreach (var kv in net)
                        if (kv.Value >= 0)
                            if (kv.Key != GameData.PowerResource)
                                Console.WriteLine($"│ {kv.Value:F2} t/day x {kv.Key.Name}");
                            else Console.WriteLine($"│ {kv.Value:F2} MWh/day x {kv.Key.Name}");
                    if (allPlans.Any(p => p.TotalConstructionMaterials.Count() > 0))
                    {
                        Console.WriteLine("├────────────────────────────────────────┤");
                        Console.WriteLine("│ Total Construction Materials:          │");       
                        Console.WriteLine("├────────────────────────────────────────┤");
                        foreach (var kv in combinedConstructionMaterials)
                            Console.WriteLine($"│ · {kv.Value:F2} × {kv.Key.Name}");
                    }
                    Console.WriteLine("└────────────────────────────────────────┘");
                    continue;
                }
                else if (command == "support")
                {
                    //  Detect needed infrastructure types
                    //      Collect all resources that need infrastructure
                    HashSet<Resource> allIOResources = new HashSet<Resource>();
                    List<ProductionBuilding> liquidInfra = GameData.AllSupportBuildings.Where(sb => sb.SupportCategory == SupportCategory.LiquidHandling).ToList();
                    List<ProductionBuilding> bulkHandlingInfra = GameData.AllSupportBuildings.Where(sb => sb.SupportCategory == SupportCategory.BulkHandling).ToList();
                    List<ProductionBuilding> solidHandlingInfra = GameData.AllSupportBuildings.Where(sb => sb.SupportCategory == SupportCategory.SolidHandling).ToList();
                    List<ProductionBuilding> generalInfra = GameData.AllSupportBuildings.Where(sb => sb.SupportCategory == SupportCategory.GeneralDistribution).ToList();
                    List<ProductionBuilding> waterInfra = GameData.AllSupportBuildings.Where(sb => sb.SupportCategory == SupportCategory.WaterHandling).ToList();
                    List<ProductionBuilding> powerInfra = GameData.AllSupportBuildings.Where(sb => sb.SupportCategory == SupportCategory.PowerHandling).ToList();
                    List<ProductionBuilding> sewageInfra = GameData.AllSupportBuildings.Where(sb => sb.SupportCategory == SupportCategory.SewageHandling).ToList();
                    List<ProductionBuilding> heatInfra = GameData.AllSupportBuildings.Where(sb => sb.SupportCategory == SupportCategory.HeatHandling).ToList();
                    Dictionary<int, List<ProductionBuilding>> categoryBuildings = new Dictionary<int, List<ProductionBuilding>>();
                    int catIndex = 0;

                    //      Add outputs
                    foreach (Resource r in currentResult.ChosenBuilding.ExpectedOutput.Keys) allIOResources.Add(r);
                    //      Add inputs
                    foreach (Resource r in currentResult.ChosenBuilding.RequiredResources.Keys) allIOResources.Add(r);

                    //      Filter available support buildings
                    foreach (Resource r in allIOResources)
                    {
                        if (r.RequiresLiquidInfrastructure)
                            liquidInfra = GameData.AllSupportBuildings.Where(sb => sb.SupportCategory == SupportCategory.LiquidHandling).ToList();
                        if (r.RequiresBulkHandling)
                            bulkHandlingInfra = GameData.AllSupportBuildings.Where(sb => sb.SupportCategory == SupportCategory.BulkHandling).ToList();
                        if (r.RequiresSolidHandling)
                            solidHandlingInfra = GameData.AllSupportBuildings.Where(sb => sb.SupportCategory == SupportCategory.SolidHandling).ToList();
                        if (r.RequiresWaterInfrastructure)
                            powerInfra = GameData.AllSupportBuildings.Where(sb => sb.SupportCategory == SupportCategory.PowerHandling).ToList();
                        if (r.RequiresWaterInfrastructure)
                            waterInfra = GameData.AllSupportBuildings.Where(sb => sb.SupportCategory == SupportCategory.WaterHandling).ToList();
                        if (r.RequiresWaterInfrastructure)
                            sewageInfra = GameData.AllSupportBuildings.Where(sb => sb.SupportCategory == SupportCategory.SewageHandling).ToList();
                        if (r.RequiresWaterInfrastructure)
                            heatInfra = GameData.AllSupportBuildings.Where(sb => sb.SupportCategory == SupportCategory.HeatHandling).ToList();
                    }
                    //      GeneralDistribution is ALWAYS available (outside loop)
                    generalInfra = GameData.AllSupportBuildings.Where(sb => sb.SupportCategory == SupportCategory.GeneralDistribution).ToList();

                    //      Display grouped by category
                    if (liquidInfra.Count() > 0 && allIOResources.Any(r => r.RequiresLiquidInfrastructure))
                    {
                        categoryBuildings[catIndex] = liquidInfra;
                        Console.WriteLine("\n────────────────────────────────────────────────────");
                        Console.WriteLine("\nRelated Support Buildings (Liquid):");
                        for (int i = 0; i < liquidInfra.Count(); i++)
                            Console.WriteLine($"[{i + 1}]: {liquidInfra[i].Name}");
                        catIndex++;
                    }
                    if (bulkHandlingInfra.Count() > 0 && allIOResources.Any(r => r.RequiresBulkHandling))
                    {
                        categoryBuildings[catIndex] = bulkHandlingInfra;
                        Console.WriteLine("\n────────────────────────────────────────────────────");
                        Console.WriteLine("\nRelated Support Buildings (Bulk):");
                        for (int i = 0; i < bulkHandlingInfra.Count(); i++)
                            Console.WriteLine($"[{i + 1}]: {bulkHandlingInfra[i].Name}");
                        catIndex++;
                    }
                    if (solidHandlingInfra.Count() > 0 && allIOResources.Any(r => r.RequiresSolidHandling))
                    {
                        categoryBuildings[catIndex] = solidHandlingInfra;
                        Console.WriteLine("\n────────────────────────────────────────────────────");
                        Console.WriteLine("\nRelated Support Buildings (Solid):");
                        for (int i = 0; i < solidHandlingInfra.Count(); i++)
                            Console.WriteLine($"[{i + 1}]: {solidHandlingInfra[i].Name}");
                        catIndex++;
                    }
                    if (generalInfra.Count() > 0 && allIOResources.Any(r => r.RequiresGeneralDistribution))
                    {
                        categoryBuildings[catIndex] = generalInfra;
                        Console.WriteLine("\n────────────────────────────────────────────────────");
                        Console.WriteLine("Related Support Buildings (General):");
                        for (int i = 0; i < generalInfra.Count(); i++)
                            Console.WriteLine($"[{i + 1}]: {generalInfra[i].Name}");
                        catIndex++;
                    }
                    if (powerInfra.Count() > 0 && allIOResources.Any(r => r.RequiresElectricalInfrastructure))
                    {
                        categoryBuildings[catIndex] = powerInfra;
                        Console.WriteLine("\n────────────────────────────────────────────────────");
                        Console.WriteLine("\nRelated Support Buildings (Power):");
                        for (int i = 0; i < powerInfra.Count(); i++)
                            Console.WriteLine($"[{i + 1}]: {powerInfra[i].Name}");
                        catIndex++;
                    }
                    if (waterInfra.Count() > 0 && allIOResources.Any(r => r.RequiresWaterInfrastructure))
                    {
                        categoryBuildings[catIndex] = waterInfra;
                        Console.WriteLine("\n────────────────────────────────────────────────────");
                        Console.WriteLine("\nRelated Support Buildings (Water):");
                        for (int i = 0; i < waterInfra.Count(); i++)
                            Console.WriteLine($"[{i + 1}]: {waterInfra[i].Name}");
                        catIndex++;
                    }
                    if (sewageInfra.Count() > 0 && allIOResources.Any(r => r.RequiresSewageInfrastructure))
                    {
                        categoryBuildings[catIndex] = sewageInfra;
                        Console.WriteLine("\n────────────────────────────────────────────────────");
                        Console.WriteLine("\nRelated Support Buildings (Sewage):");
                        for (int i = 0; i < sewageInfra.Count(); i++)
                            Console.WriteLine($"[{i + 1}]: {sewageInfra[i].Name}");
                        catIndex++;
                    }
                    if (heatInfra.Count() > 0 && allIOResources.Any(r => r.RequiresHeatInfrastructure))
                    {
                        categoryBuildings[catIndex] = heatInfra;
                        Console.WriteLine("\n────────────────────────────────────────────────────");
                        Console.WriteLine("\nRelated Support Buildings (Heat):");
                        for (int i = 0; i < heatInfra.Count(); i++)
                            Console.WriteLine($"[{i + 1}]: {heatInfra[i].Name}");
                        catIndex++;
                    }

                    // User selection - Buildings, Count
                    Dictionary<ProductionBuilding, int> chosenSupports = new Dictionary<ProductionBuilding, int>();
                    string inputStrings;
                    while (true)
                    {
                        Console.Write("Choose the building types (comma-separated, '0' to skip): ");
                        inputStrings = Console.ReadLine();

                        // Only split by comma - all indices are from the same category
                        string[] indices = inputStrings.Trim().Split(',');

                        bool validInput = true;
                        for (int categoryIndex = 0; categoryIndex < indices.Length; categoryIndex++)
                        {
                            string trimmed = indices[categoryIndex].Trim();
                            if (string.IsNullOrEmpty(trimmed)) continue; // Skip empty strings from extra commas

                            if (int.TryParse(trimmed, out int buildingIndex))
                            {
                                if (buildingIndex == 0) { continue; }

                                // Check if categoryIndex is valid
                                if (categoryIndex >= categoryBuildings.Count)
                                {
                                    Console.WriteLine($"Too many inputs! Only {categoryBuildings.Count} categories available.");
                                    validInput = false;
                                    break;
                                }

                                // Use categoryIndex to access the correct category
                                var currentCategory = categoryBuildings[categoryIndex];

                                // Only one category (categoryIndex = 0), so use categoryBuildings[0]
                                if (buildingIndex < 1 || buildingIndex > categoryBuildings[0].Count)
                                {
                                    Console.WriteLine($"Invalid building index: {buildingIndex}");
                                    validInput = false;
                                    break;
                                }

                                var selectedBuilding = currentCategory[buildingIndex - 1];
                                Console.Write($"How many '{selectedBuilding.Name}'?: ");
                                int buildingCount = int.TryParse(Console.ReadLine(), out buildingCount) ? buildingCount : 0;

                                if (chosenSupports.ContainsKey(selectedBuilding))
                                    Console.WriteLine("Already existing Infrastructure!");
                                else chosenSupports.Add(selectedBuilding, buildingCount);
                            }
                            else
                            {
                                Console.WriteLine($"Invalid Input: '{trimmed}'");
                                validInput = false;
                                break;
                            }
                        }
                        if (validInput) break;
                    }

                    // Create BuildingRequirement and add to SupportBuildings
                    foreach (var cb in chosenSupports)
                    {
                        BuildingRequirement addSupBuilding = new BuildingRequirement(cb.Key);
                        addSupBuilding.Count = cb.Value;
                        currentResult.SupportBuildings.Add(addSupBuilding);
                        Console.WriteLine($"{cb.Key.Name}: {cb.Value} added to the {currentResult.ChosenBuilding.Building.Name}!");
                    }
                    Console.WriteLine("Check");
                    continue;
                }
                else if (command == "amenity")
                {
                    // Flat selection menu
                    Console.WriteLine("Select Amenity Type:");
                    Console.WriteLine("[1] Shopping");
                    Console.WriteLine("[2] Pub");
                    Console.WriteLine("[3] Healthcare");
                    Console.WriteLine("[4] Fireservice");
                    Console.WriteLine("[5] CityService");
                    Console.WriteLine("[6] Culture");
                    Console.WriteLine("[7] Sports");
                    Console.WriteLine("[8] Education");
                    Console.WriteLine("[9] Crime & Justice");
                    Console.WriteLine("[10] Fountain");
                    Console.WriteLine("[0] Back");
                    Console.Write("\nYour Choice: ");
                    int amenChoice;

                    if (int.TryParse(Console.ReadLine(), out amenChoice) && amenChoice >= 0 && amenChoice <= 9)
                    {
                        if (amenChoice == 0) { continue; }

                        // Map choice to enum
                        AmenityType[] typeMap = new AmenityType[]
                        {
                                AmenityType.Shopping,      // 1
                                AmenityType.Pub,           // 2 (Pub)
                                AmenityType.Healthcare,    // 3
                                AmenityType.Fireservice,   // 4
                                AmenityType.CityService,
                                AmenityType.Culture,       // 5
                                AmenityType.Sports,        // 6
                                AmenityType.Education,     // 7
                                AmenityType.CrimeJustice,  // 8
                                AmenityType.Fountain       // 10
                        };
                        AmenityType selectedType = typeMap[amenChoice - 1];
                        Console.WriteLine("┌─────────────────────────────────────────");
                        Console.WriteLine($"│ {selectedType} Buildings:");
                        Console.WriteLine("└─────────────────────────────────────────");

                        // Filter buildings by type
                        var buildingsOfType = GameData.AllAmenityBuildings.Where(b => b.Type == selectedType).ToList();

                        // Display buildings
                        for (int i = 0; i < buildingsOfType.Count; i++)
                            Console.WriteLine($"[{i + 1}] {buildingsOfType[i].Name}");
                        Console.Write("[0] Back\n: ");
                        int buildChoice;

                        // Choose Building
                        if (int.TryParse(Console.ReadLine(), out buildChoice) && buildChoice >= 0 && buildChoice <= buildingsOfType.Count())
                        {
                            if (buildChoice == 0) { continue; }
                            Console.Write("How many?: ");
                            int count;

                            // Decide amount
                            if (int.TryParse(Console.ReadLine(), out count) && count >= 0)
                            {
                                AmenityInstance amenityInstance = new AmenityInstance();
                                amenityInstance.Building = buildingsOfType[buildChoice - 1];
                                amenityInstance.Count = count;
                                rootResult.AmenityBuildings.Add(amenityInstance);
                            }
                            else { Console.Write("Invalid Input."); }
                        }
                        else { Console.Write("Invalid Input."); }
                    }
                    else { Console.Write("Invalid Input(Index)."); }
                    continue;
                }
                else if (command == "transportation")
                {
                    // Flat selection menu
                    Console.WriteLine("Select Transportation Type:");
                    Console.WriteLine("[1] Bus");
                    Console.WriteLine("[2] Trolley");
                    Console.WriteLine("[3] Tram");
                    Console.WriteLine("[4] Depot");
                    Console.WriteLine("[5] Station");
                    Console.WriteLine("[6] Refueling");
                    Console.WriteLine("[0] Back");
                    Console.Write("\nYour Choice: ");
                    int tranChoice;

                    if (int.TryParse(Console.ReadLine(), out tranChoice) && tranChoice >= 0 && tranChoice <= 9)
                    {
                        if (tranChoice == 0) { continue; }

                        // Map choice to enum
                        TransportationType[] typeMap = new TransportationType[]
                        {
                            TransportationType.Bus,
                            TransportationType.Trolley,
                            TransportationType.Tram,
                            TransportationType.Depot,
                            TransportationType.Station,
                            TransportationType.Refueling,
                        };
                        TransportationType selectedType = typeMap[tranChoice - 1];
                        Console.WriteLine("┌─────────────────────────────────────────");
                        Console.WriteLine($"│ {selectedType} Buildings:");
                        Console.WriteLine("└─────────────────────────────────────────");

                        // Filter buildings by type
                        var buildingsOfType = GameData.TransportationBuildings.Where(b => b.Type == selectedType).ToList();

                        // Display buildings
                        for (int i = 0; i < buildingsOfType.Count; i++)
                            Console.WriteLine($"[{i + 1}] {buildingsOfType[i].Name}");
                        Console.Write("[0] Back\n: ");
                        int buildChoice;

                        // Choose Building
                        if (int.TryParse(Console.ReadLine(), out buildChoice) && buildChoice >= 0 && buildChoice <= buildingsOfType.Count())
                        {
                            if (buildChoice == 0) { continue; }
                            Console.Write("How many?: ");
                            int count;

                            // Decide amount
                            if (int.TryParse(Console.ReadLine(), out count) && count >= 0)
                            {
                                TransportationInstance transportationInstance = new TransportationInstance();
                                transportationInstance.Building = buildingsOfType[buildChoice - 1];
                                transportationInstance.Count = count;
                                rootResult.TransportationBuildings.Add(transportationInstance);
                            }
                            else { Console.Write("Invalid Input."); }
                        }
                        else { Console.Write("Invalid Input."); }
                    }
                    else { Console.Write("Invalid Input(Index)."); }
                    continue;

                }
                else if (command == "dive")
                {
                    int buildChoice;
                    int diveChoice;
                    Console.Write("Which one to dive into? (press 9 to go back): [0]Utility, [1]SubChain: ");
                    if (int.TryParse(Console.ReadLine(), out buildChoice) && buildChoice >= 0 && buildChoice <= 1)
                    {
                        if (buildChoice == 0)
                        {
                            if (currentResult.ExpandedUtilities.Count() > 0)
                            {
                                Dictionary<int, Resource> utilIndex = new Dictionary<int, Resource>();
                                Console.WriteLine("Which utility do you want to dive into?:");
                                int i = 0;
                                foreach (var kv in currentResult.ExpandedUtilities)
                                {
                                    Console.WriteLine($"{i}: {kv.Key.Name}");
                                    utilIndex.Add(i, kv.Key);
                                    i++;
                                }
                                Console.Write("Choose the number to dive: ");
                                if (int.TryParse(Console.ReadLine(), out diveChoice) && diveChoice >= 0 && diveChoice <= currentResult.ExpandedUtilities.Count())
                                {
                                    Console.WriteLine($"Dived to {currentResult.ExpandedUtilities[utilIndex[diveChoice]].Building.Name}.");
                                    // Display the current level again
                                    if (utilIndex[diveChoice] == GameData.PowerResource)
                                        Console.WriteLine($"\nTarget Resource: {utilIndex[diveChoice].Name} {currentResult.ChosenBuilding.TotalPowerNeeded} MWh/day");
                                    else if (utilIndex[diveChoice] == GameData.WaterResource)
                                        Console.WriteLine($"\nTarget Resource: {utilIndex[diveChoice].Name} {currentResult.ChosenBuilding.TotalWaterNeeded} ㎥/day");
                                    Console.WriteLine("\n====================================================================");
                                    Console.WriteLine($"Required number of building: {currentResult.ExpandedUtilities[utilIndex[diveChoice]].Count} {currentResult.ExpandedUtilities[utilIndex[diveChoice]].Building.Name}\n" +
                                        $"Total Workers: {currentResult.ExpandedUtilities[utilIndex[diveChoice]].TotalWorkers}");
                                    Console.Write("Expected Output: ");
                                    foreach (Resource r in currentResult.ExpandedUtilities[utilIndex[diveChoice]].ExpectedOutput.Keys)
                                    {
                                        if (utilIndex[diveChoice] == GameData.PowerResource)
                                            Console.Write($"{currentResult.ExpandedUtilities[utilIndex[diveChoice]].ExpectedOutput[r]} MWh/day {r.Name} ");
                                        else if (utilIndex[diveChoice] == GameData.WaterResource)
                                            Console.Write($"{currentResult.ExpandedUtilities[utilIndex[diveChoice]].ExpectedOutput[r]} ㎥/day {r.Name} ");
                                    }
                                    Console.WriteLine("\n\nRequired Input Resources: ");
                                    foreach (Resource r in currentResult.ExpandedUtilities[utilIndex[diveChoice]].RequiredResources.Keys)
                                    {
                                        Console.WriteLine($"- {currentResult.ExpandedUtilities[utilIndex[diveChoice]].RequiredResources[r]} {r.Name}");
                                    }
                                    Console.WriteLine($"Power consumption: {currentResult.ExpandedUtilities[utilIndex[diveChoice]].TotalPowerNeeded}");
                                    Console.WriteLine($"Water consumption: {currentResult.ExpandedUtilities[utilIndex[diveChoice]].TotalWaterNeeded}");
                                    Console.WriteLine($"Heat consumption: {currentResult.ExpandedUtilities[utilIndex[diveChoice]].TotalHeatNeeded}");
                                    Console.WriteLine($"Sewage produced: {currentResult.ExpandedUtilities[utilIndex[diveChoice]].TotalSewageProduced}");
                                    Console.WriteLine($"Garbage produced: {currentResult.ExpandedUtilities[utilIndex[diveChoice]].TotalGarbageProduced}");
                                    Console.WriteLine($"Pollution emitted: {currentResult.ExpandedUtilities[utilIndex[diveChoice]].TotalEnvironmentPollution}");
                                }
                                else { Console.WriteLine("Invalid input."); continue; }
                            }
                            else { Console.WriteLine("\nNo Utility to dive into!"); }
                        }
                        else if (buildChoice == 1)
                        {
                            if (currentResult.SubChains.Count > 0)
                            {
                                Console.WriteLine("\nAvailable sub-chains:");
                                for (int i = 0; i < currentResult.SubChains.Count; i++)
                                {
                                    Console.WriteLine($"{i}: {currentResult.SubChains[i].TargetResource.Name}");
                                }

                                Console.Write("Choose which to dive into: ");
                                if (int.TryParse(Console.ReadLine(), out diveChoice) &&
                                    diveChoice >= 0 && diveChoice < currentResult.SubChains.Count)
                                {
                                    navigationStack.Push(currentResult);
                                    currentResult = currentResult.SubChains[diveChoice];
                                    Console.WriteLine($"\nDived into: {currentResult.TargetResource.Name}\n");

                                    // Display the current level again
                                    Console.WriteLine($"\nTarget Resource: {currentResult.TargetResource.Name} {currentResult.TargetAmount}t/day");
                                    Console.WriteLine("\n====================================================================");
                                    Console.WriteLine($"Required number of building: {currentResult.ChosenBuilding.Count} {currentResult.ChosenBuilding.Building.Name}\n" +
                                        $"Total Workers: {currentResult.ChosenBuilding.TotalWorkers}");
                                    Console.Write("Expected Output: ");
                                    foreach (Resource r in currentResult.ChosenBuilding.ExpectedOutput.Keys)
                                    {
                                        Console.Write($"{currentResult.ChosenBuilding.ExpectedOutput[r]}t/day {r.Name} ");
                                    }
                                    Console.WriteLine("\n\nRequired Input Resources: ");
                                    foreach (Resource r in currentResult.ChosenBuilding.RequiredResources.Keys)
                                    {
                                        Console.WriteLine($"- {currentResult.ChosenBuilding.RequiredResources[r]} {r.Name}");
                                    }
                                    Console.WriteLine($"Power consumption: {currentResult.ChosenBuilding.TotalPowerNeeded}");
                                    Console.WriteLine($"Water consumption: {currentResult.ChosenBuilding.TotalWaterNeeded}");
                                    Console.WriteLine($"Heat consumption: {currentResult.ChosenBuilding.TotalHeatNeeded}");
                                    Console.WriteLine($"Sewage produced: {currentResult.ChosenBuilding.TotalSewageProduced}");
                                    Console.WriteLine($"Garbage produced: {currentResult.ChosenBuilding.TotalGarbageProduced}");
                                    Console.WriteLine($"Pollution emitted: {currentResult.ChosenBuilding.TotalEnvironmentPollution}");
                                }
                            }
                            else { Console.WriteLine("\nNo sub-chains to dive into!"); }
                        }
                    }
                    else continue;
                    continue;
                }
                else if (command == "cancel")
                {
                    int buildChoice;
                    int undoChoice;
                    Console.Write("Which one to cancel? (press 9 to go back): \n[0]Utility \n[1]SubChain \n[2]Support Buildings \n[3]Residential Buildings \n[4]Amenity Buildings \n[5]Transportation Buildings: ");
                    if (int.TryParse(Console.ReadLine(), out buildChoice) && buildChoice >= 0 && buildChoice <= 5)
                    {
                        if (buildChoice == 0)
                        {
                            if (currentResult.ExpandedUtilities.Count() > 0)
                            {
                                Dictionary<int, Resource> utilIndex = new Dictionary<int, Resource>();
                                Console.WriteLine("Which utility do you want to cancel?:");
                                int i = 0;
                                foreach (var kv in currentResult.ExpandedUtilities)
                                {
                                    Console.WriteLine($"{i}: {kv.Key.Name}");
                                    utilIndex.Add(i, kv.Key);
                                    i++;
                                }
                                Console.Write("Choose the number to cancel: ");
                                if (int.TryParse(Console.ReadLine(), out undoChoice) && undoChoice >= 0 && undoChoice < currentResult.ExpandedUtilities.Count())
                                {
                                    Resource utilityToCancel = utilIndex[undoChoice];

                                    // Get the building name BEFORE we remove anything
                                    string buildingName = currentResult.ExpandedUtilities.ContainsKey(utilityToCancel)
                                        ? currentResult.ExpandedUtilities[utilityToCancel].Building.Name
                                        : "Unknown building";

                                    // Find and remove the SubChain that produces this utility
                                    CalculationResult subChainToRemove = currentResult.SubChains
                                        .FirstOrDefault(sc => sc.TargetResource == utilityToCancel);

                                    if (subChainToRemove != null)
                                        currentResult.SubChains.Remove(subChainToRemove);

                                    // Remove from ExpandedUtilities tracking
                                    if (currentResult.ExpandedUtilities.ContainsKey(utilityToCancel))
                                        currentResult.ExpandedUtilities.Remove(utilityToCancel);

                                    Console.WriteLine($"{buildingName} has been canceled.");
                                }
                                else { Console.WriteLine("Invalid input."); continue; }
                            }
                            else { Console.WriteLine("\nNo Utility to cancel!"); }
                        }
                        else if (buildChoice == 1)
                        {
                            if (currentResult.SubChains.Count() > 0)
                            {
                                Console.WriteLine("Which subchain do you want to cancel?:");
                                for (int i = 0; i < currentResult.SubChains.Count; i++)
                                {
                                    Console.WriteLine($"{i}: {currentResult.SubChains[i].TargetResource.Name}");
                                }
                                Console.Write("Choose the number to cancel: ");
                                if (int.TryParse(Console.ReadLine(), out undoChoice) &&
                                    undoChoice >= 0 && undoChoice < currentResult.SubChains.Count)
                                {
                                    CalculationResult subchainToCancel = currentResult.SubChains[undoChoice];
                                    currentResult.SubChains.Remove(subchainToCancel);
                                }
                                else { Console.WriteLine("Invalid input."); continue; }
                            }
                            else { Console.WriteLine("\nNo sub-chains to cancel!"); }
                        }
                        else if (buildChoice == 2)
                        {
                            if (currentResult.SupportBuildings.Count() > 0)
                            {
                                Dictionary<int, BuildingRequirement> supIndex = new Dictionary<int, BuildingRequirement>();
                                Console.WriteLine("Which Support Building do you want to cancel?:");
                                int i = 0;
                                foreach (var k in currentResult.SupportBuildings)
                                {
                                    Console.WriteLine($"{i}: {k.Building.Name}");
                                    supIndex.Add(i, k);
                                    i++;
                                }
                                Console.Write("Choose the number to cancel: ");
                                if (int.TryParse(Console.ReadLine(), out undoChoice) && undoChoice >= 0 && undoChoice <= currentResult.SupportBuildings.Count())
                                {
                                    Console.WriteLine($"{currentResult.SupportBuildings[undoChoice].Building.Name} has been canceled.");
                                    currentResult.SupportBuildings.RemoveAt(undoChoice);

                                }
                                else { Console.WriteLine("Invalid input."); continue; }
                            }
                            else { Console.WriteLine("\nNo Support Buildings to cancel!"); }
                        }
                        else if (buildChoice == 3)
                        {
                            if (rootResult.ResidentialBuildings.Count() > 0)
                            {
                                Console.WriteLine("Which building do you want to cancel?: ");
                                int i = 0;
                                foreach (var ri in rootResult.ResidentialBuildings)
                                {
                                    Console.WriteLine($"{i}: {ri.Building.Name}");
                                    i++;
                                }
                                Console.Write("Choose the number to cancel: ");
                                if (int.TryParse(Console.ReadLine(), out undoChoice) && undoChoice >= 0 && undoChoice <= rootResult.ResidentialBuildings.Count())
                                {
                                    Console.WriteLine($"{rootResult.ResidentialBuildings[undoChoice].Building.Name} has been canceled.");
                                    rootResult.ResidentialBuildings.RemoveAt(undoChoice);
                                }
                                else { Console.WriteLine("Invalid input."); continue; }
                            }
                            else { Console.WriteLine("\nNo Residential Buildings to cancel!"); }
                        }
                        else if (buildChoice == 4)
                        {
                            if (rootResult.AmenityBuildings.Count() > 0)
                            {
                                Console.WriteLine("Which building do you want to cancel?: ");
                                int i = 0;
                                foreach (var ri in rootResult.AmenityBuildings)
                                {
                                    Console.WriteLine($"{i}: {ri.Building.Name}");
                                    i++;
                                }
                                Console.Write("Choose the number to cancel: ");
                                if (int.TryParse(Console.ReadLine(), out undoChoice) && undoChoice >= 0 && undoChoice <= rootResult.AmenityBuildings.Count())
                                {
                                    Console.WriteLine($"{rootResult.AmenityBuildings[undoChoice].Building.Name} has been canceled.");
                                    rootResult.AmenityBuildings.RemoveAt(undoChoice);
                                }
                                else { Console.WriteLine("Invalid input."); continue; }
                            }
                            else { Console.WriteLine("\nNo Amenity Buildings to cancel!"); }
                        }
                        else if (buildChoice == 5)
                        {
                            if (rootResult.TransportationBuildings.Count() > 0)
                            {
                                Console.WriteLine("Which building do you want to cancel?: ");
                                int i = 0;
                                foreach (var ri in rootResult.TransportationBuildings)
                                {
                                    Console.WriteLine($"{i}: {ri.Building.Name}");
                                    i++;
                                }
                                Console.Write("Choose the number to cancel: ");
                                if (int.TryParse(Console.ReadLine(), out undoChoice) && undoChoice >= 0 && undoChoice <= rootResult.TransportationBuildings.Count())
                                {
                                    Console.WriteLine($"{rootResult.TransportationBuildings[undoChoice].Building.Name} has been canceled.");
                                    rootResult.TransportationBuildings.RemoveAt(undoChoice);
                                }
                                else { Console.WriteLine("Invalid input."); continue; }
                            }
                            else { Console.WriteLine("\nNo Transportation Buildings to cancel!"); }
                        }
                    }
                    else continue;
                    continue;
                }
                else if (command == "done")
                {
                    //allPlans.Clear();
                    navigationStack.Clear();
                    break;  // End Command Loop
                }
                else if (command == "back")
                {
                    if (navigationStack.Count > 0)
                    {
                        currentResult = navigationStack.Pop();  // Go back
                        Console.WriteLine($"\nReturned to: {currentResult.TargetResource.Name} {currentResult.TargetAmount}t/day");
                    }
                    // Display the current level again
                    Console.WriteLine($"\nTarget Resource: {currentResult.TargetResource.Name} {currentResult.TargetAmount}t/day");
                    Console.WriteLine("\n====================================================================");
                    Console.WriteLine($"Required number of building: {currentResult.ChosenBuilding.Count} {currentResult.ChosenBuilding.Building.Name}\n" +
                        $"Total Workers: {currentResult.ChosenBuilding.TotalWorkers}");
                    Console.Write("Expected Output: ");
                    foreach (Resource r in currentResult.ChosenBuilding.ExpectedOutput.Keys)
                    {
                        Console.Write($"{currentResult.ChosenBuilding.ExpectedOutput[r]}t/day {r.Name} ");
                    }
                    Console.WriteLine("\n\nRequired Input Resources: ");
                    foreach (Resource r in currentResult.ChosenBuilding.RequiredResources.Keys)
                    {
                        Console.WriteLine($"- {currentResult.ChosenBuilding.RequiredResources[r]} {r.Name}");
                    }
                    Console.WriteLine($"Power consumption: {currentResult.ChosenBuilding.TotalPowerNeeded}");
                    Console.WriteLine($"Water consumption: {currentResult.ChosenBuilding.TotalWaterNeeded}");
                    Console.WriteLine($"Heat consumption: {currentResult.ChosenBuilding.TotalHeatNeeded}");
                    Console.WriteLine($"Sewage produced: {currentResult.ChosenBuilding.TotalSewageProduced}");
                    Console.WriteLine($"Garbage produced: {currentResult.ChosenBuilding.TotalGarbageProduced}");
                    Console.WriteLine($"Pollution emitted: {currentResult.ChosenBuilding.TotalEnvironmentPollution}");
                    continue;
                }
                else if (command == "summary")
                {
                    rootResult.DisplayTotalReceipt();
                    continue;
                }
                else if (command == "housing")
                {
                    // Calculate total citizens from ALL plans
                    int totalWorkers = allPlans.Sum(p => p.TotalWorkers);
                    int totalCitizens = allPlans.Sum(p => p.TotalPopulationNeeded);
                    int totalHousingCapacity = allPlans.Sum(p => p.TotalHousingCapacity);
                    Console.WriteLine($"\nYou need housing for {totalCitizens - totalHousingCapacity} citizens ({totalWorkers} workers + dependents)");
                    bool allValid = false;
                    while (!allValid)
                    {
                        // User choose Residential Size
                        int sizeChoice;
                        Console.Write($"\nChoose the Size of Residential:\n[0]: Small\n[1]: Medium\n[2]: Large\n: ");
                        if (int.TryParse(Console.ReadLine(), out sizeChoice) && sizeChoice >= 0 && sizeChoice <= 2)
                        {
                            // Small
                            int userChoice;
                            int count;
                            if (sizeChoice == 0)
                            {
                                Dictionary<int, ResidentialBuilding> resList = new Dictionary<int, ResidentialBuilding>();

                                // Display Buildings
                                Console.WriteLine($"Small Residentials:");
                                for (int i = 0; i < GameData.SmallResidentialBuildings.Count(); i++)
                                {
                                    Console.WriteLine($"[{i + 1}]: {GameData.SmallResidentialBuildings[i].WorkerCapacity}, {GameData.SmallResidentialBuildings[i].Name}");
                                    resList.Add(i + 1, GameData.SmallResidentialBuildings[i]);
                                }

                                // Select Building type
                                Console.Write($"\nSelect the types ('type, type, type ...'): ");
                                List<ResidentialInstance> resCount = new List<ResidentialInstance>();
                                while (true)
                                {
                                    string input = Console.ReadLine();
                                    string[] parsed = input.Split(',');
                                    ResidentialInstance addRL = new ResidentialInstance();
                                    foreach (var s in parsed)
                                    {
                                        foreach (var rl in resList)
                                            if ((int.TryParse(s.Trim(), out userChoice) ? userChoice : 0) == rl.Key)
                                            {
                                                if (userChoice == 0) { continue; }
                                                if (resCount.Any(ri => ri.Building == resList[userChoice])) { continue; }
                                                addRL.Building = resList[userChoice];
                                                resCount.Add(addRL);
                                            }
                                    }
                                    break;
                                }

                                // Select the Count
                                Console.WriteLine("\nSet the amount:");
                                foreach (var ri in resCount)
                                {
                                    Console.Write($"{ri.Building.Name}: ");
                                    if (int.TryParse(Console.ReadLine(), out count) && count >= 0)
                                        ri.Count += count;
                                }

                                // Add resCount to CalculationResult
                                rootResult.ResidentialBuildings.AddRange(resCount);

                                // Show the Capacity
                                Console.WriteLine($"\nCurrent capacity: {rootResult.TotalHousingCapacity}" +
                                    $"\nExtra capcity needed: {((rootResult.TotalPopulationNeeded - rootResult.TotalHousingCapacity) >= 0
                                    ? (rootResult.TotalPopulationNeeded - rootResult.TotalHousingCapacity)
                                    : Math.Abs(rootResult.TotalPopulationNeeded - rootResult.TotalHousingCapacity))}");

                                allValid = true;
                            }
                            // Medium
                            else if (sizeChoice == 1)
                            {
                                Dictionary<int, ResidentialBuilding> resList = new Dictionary<int, ResidentialBuilding>();

                                // Display Buildings
                                Console.WriteLine($"Medium Residentials:");
                                for (int i = 0; i < GameData.MediumResidentialBuildings.Count(); i++)
                                {
                                    Console.WriteLine($"[{i + 1}]: {GameData.MediumResidentialBuildings[i].WorkerCapacity}, {GameData.MediumResidentialBuildings[i].Name}");
                                    resList.Add(i + 1, GameData.MediumResidentialBuildings[i]);
                                }

                                // Select Building type
                                Console.Write($"\nSelect the types ('type, type, type ...'): ");
                                List<ResidentialInstance> resCount = new List<ResidentialInstance>();
                                while (true)
                                {
                                    string input = Console.ReadLine();
                                    string[] parsed = input.Split(',');
                                    ResidentialInstance addRL = new ResidentialInstance();
                                    foreach (var s in parsed)
                                    {
                                        foreach (var rl in resList)
                                            if ((int.TryParse(s.Trim(), out userChoice) ? userChoice : 0) == rl.Key)
                                            {
                                                if (userChoice == 0) { continue; }
                                                if (resCount.Any(ri => ri.Building == resList[userChoice])) { continue; }
                                                addRL.Building = resList[userChoice];
                                                resCount.Add(addRL);
                                            }
                                    }
                                    break;
                                }

                                // Select the Count
                                Console.WriteLine("\nSet the amount:");
                                foreach (var ri in resCount)
                                {
                                    Console.Write($"{ri.Building.Name}: ");
                                    if (int.TryParse(Console.ReadLine(), out count) && count >= 0)
                                        ri.Count += count;
                                }

                                // Add resCount to CalculationResult
                                rootResult.ResidentialBuildings.AddRange(resCount);

                                // Show the Capacity
                                Console.WriteLine($"\nCurrent capacity: {rootResult.TotalHousingCapacity}" +
                                    $"\nExtra capcity needed: {((rootResult.TotalPopulationNeeded - rootResult.TotalHousingCapacity) >= 0
                                    ? (rootResult.TotalPopulationNeeded - rootResult.TotalHousingCapacity)
                                    : Math.Abs(rootResult.TotalPopulationNeeded - rootResult.TotalHousingCapacity))}");

                                allValid = true;
                            }
                            // Large
                            else
                            {
                                Dictionary<int, ResidentialBuilding> resList = new Dictionary<int, ResidentialBuilding>();

                                // Display Buildings
                                Console.WriteLine($"Large Residentials:");
                                for (int i = 0; i < GameData.LargeResidentialBuildings.Count(); i++)
                                {
                                    Console.WriteLine($"[{i + 1}]: {GameData.LargeResidentialBuildings[i].WorkerCapacity}, {GameData.LargeResidentialBuildings[i].Name}");
                                    resList.Add(i + 1, GameData.LargeResidentialBuildings[i]);
                                }

                                // Select Building type
                                Console.Write($"\nSelect the types ('type, type, type ...'): ");
                                List<ResidentialInstance> resCount = new List<ResidentialInstance>();
                                while (true)
                                {
                                    string input = Console.ReadLine();
                                    string[] parsed = input.Split(',');
                                    ResidentialInstance addRL = new ResidentialInstance();
                                    foreach (var s in parsed)
                                    {
                                        foreach (var rl in resList)
                                            if ((int.TryParse(s.Trim(), out userChoice) ? userChoice : 0) == rl.Key)
                                            {
                                                if (userChoice == 0) { continue; }
                                                if (resCount.Any(ri => ri.Building == resList[userChoice])) { continue; }
                                                addRL.Building = resList[userChoice];
                                                resCount.Add(addRL);
                                            }
                                    }
                                    break;
                                }

                                // Select the Count
                                Console.WriteLine("\nSet the amount:");
                                foreach (var ri in resCount)
                                {
                                    Console.Write($"{ri.Building.Name}: ");
                                    if (int.TryParse(Console.ReadLine(), out count) && count >= 0)
                                        ri.Count += count;
                                }

                                // Add resCount to CalculationResult
                                rootResult.ResidentialBuildings.AddRange(resCount);

                                // Show the Capacity
                                Console.WriteLine($"\nCurrent capacity: {rootResult.TotalHousingCapacity}" +
                                    $"\nExtra capcity needed: {((rootResult.TotalPopulationNeeded - rootResult.TotalHousingCapacity) >= 0
                                    ? (rootResult.TotalPopulationNeeded - rootResult.TotalHousingCapacity)
                                    : rootResult.TotalPopulationNeeded - rootResult.TotalHousingCapacity)}");

                                allValid = true;
                            }
                        }
                        else { Console.Write("Invalid Input. "); continue; }
                    }
                    continue;   // back to 'command'
                }
            }
        }
        static string ReadLineWithCompletion(List<string> availableCommands)
        {
            string input = "";
            int currentMatchIndex = -1; // Track which match we're showing
            List<string> lastMatches = new List<string>(); // Remember the matches

            while (true)
            {
                var key = Console.ReadKey(intercept: true); // Don't show the key yet

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return input;
                }
                else if (key.Key == ConsoleKey.Tab)
                {
                    // Find matches
                    var matches = availableCommands.Where(cmd => cmd.StartsWith(input, StringComparison.OrdinalIgnoreCase)).ToList();

                    if (matches.Count == 1)
                    {
                        ClearCurrentLine(input.Length);
                        input = matches[0];
                        Console.Write(input);
                    }
                    else if (matches.Count > 1)
                    {
                        // Multiple matches - cycle through them
                        currentMatchIndex = (currentMatchIndex + 1) % matches.Count; // Cycle: 0, 1, 2, ... back to 0

                        ClearCurrentLine(input.Length);
                        input = matches[currentMatchIndex];
                        Console.Write(input);

                        lastMatches = matches; // Remember for next Tab press
                    }
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    // User is editing - reset match cycling
                    if (input.Length > 0)
                    {
                        input = input.Substring(0, input.Length - 1);
                        Console.Write("\b \b"); // Move back, write space (erase char), move back again
                        currentMatchIndex = -1; // Reset cycling
                    }
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    // Normal character
                    input += key.KeyChar;
                    Console.Write(key.KeyChar);
                    currentMatchIndex = -1; // Reset cycling when user types
                }
            }
        }
        static void ClearCurrentLine(int length)
        {
            // Move cursor back, overwrite with spaces, move back again
            Console.Write(new string('\b', length)); // Move back
            Console.Write(new string(' ', length));  // Erase
            Console.Write(new string('\b', length)); // Move back to start
        }
    }
}
