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
        static List<IndustryPlan> allPlans = new List<IndustryPlan>();
        static List<MicroDistrict> allMicroDistricts = new List<MicroDistrict>();
        static City city = new City()
        {
            Name = "City 1",
            industryPlans = allPlans,
            microDistricts = allMicroDistricts
        };
        static List<City> allCities = new List<City>();
        static SavedFile savedFile = new SavedFile()
        {
            Name = "plans.json",
        };

        // Navigation pointer: Points to the current position in the active plan's tree
        // Used for commands that navigate/modify existing plans (expand, dive, back, cancel)
        // - At root level: currentResult == rootResult (the plan itself)
        // - When diving: currentResult points to a SubChain within the plan
        // - When expanding: operations are performed on currentResult's level

        static IndustryPlan currentResult;
        static int currentPlanIndex = -1;
        static IndustryPlan rootResult => (currentPlanIndex >= 0 && currentPlanIndex < allPlans.Count)
    ? allPlans[currentPlanIndex]
    : null;
        static Stack<IndustryPlan> navigationStack = new Stack<IndustryPlan>();
        static MicroDistrict microDistrict = new MicroDistrict();
        static string currentSaveFile = "plans.json";
        static string fileDirectory = Path.Combine(Directory.GetCurrentDirectory(), "plans");
        static string[] jsonFiles = Directory.Exists(fileDirectory)
            ? Directory.GetFiles(fileDirectory, "*.json")
            : new string[0];    // Empty array if folder doesn't exist

        static void Main(string[] args)
        {
            // Check if there's 'plans' folder
            if (!Directory.Exists(fileDirectory))
                Directory.CreateDirectory(fileDirectory);

            // Load existing plans if they exist
            if (jsonFiles.Length > 0)
            {
                Console.Write("Found saved plans. Load? [y/n]: ");
                if (Console.ReadKey().KeyChar == 'y')
                {
                    Console.WriteLine();
                    // Display all files with indices
                    for (int i = 0; i < jsonFiles.Length; i++)
                        Console.WriteLine($"[{i}] {Path.GetFileName(jsonFiles[i])}");

                    // Get user choice
                    int loadChoice;
                    while (true)
                    {
                        Console.Write("Choose file to load: ");
                        if (int.TryParse(Console.ReadLine(), out loadChoice) &&  loadChoice >= 0 && loadChoice < jsonFiles.Length)
                        {
                            allCities = LoadFile(Path.Combine(fileDirectory, jsonFiles[loadChoice]));
                            allPlans = allCities[0].industryPlans;
                            currentSaveFile = Path.GetFileName(jsonFiles[loadChoice]);
                        } else { Console.WriteLine("Invalid input"); continue; }
                        break;
                    }

                    // Display success message
                    if (allCities.Count > 0)
                    {
                        currentPlanIndex = 0;
                        currentResult = allPlans[0];
                        Console.WriteLine($"\n'{currentSaveFile}' Loaded. Type 'listplans' to view them.");
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
                        Console.Write("Which method you wanna save?: [0] Save (overwrite) [1] Save as ...(new)\n> ");
                        int saveChoice;
                        if (int.TryParse(Console.ReadLine(), out saveChoice) && saveChoice >= 0 && saveChoice <= 1)
                        {
                            if (saveChoice == 0) SaveFile(allCities, Path.Combine(fileDirectory, currentSaveFile));
                            else if (saveChoice == 1) 
                            {
                                // create a new filename
                                while (true)
                                {
                                    Console.Write($"Type the save file name: ");
                                    currentSaveFile = Console.ReadLine();
                                    if (!currentSaveFile.EndsWith(".json")) currentSaveFile += ".json";
                                    break;
                                }
                                SaveFile(allCities, Path.Combine(fileDirectory, currentSaveFile));
                            }
                        } else { Console.WriteLine("Invalid input."); continue; }
                        Console.WriteLine("\n✓ File saved!");
                    }
                    Console.WriteLine("\nGoodbye!");
                    break;
                }                           // Exit program   
                else { Console.WriteLine("Invalid command. Try 'newplan', 'buildplan', 'navigate' or 'done'."); }
            }
            return;
        }
        // Helper method
        static void ExpandUtility(IndustryPlan result, Resource utility, double totalNeeded)
        {
            Console.WriteLine($"\n{utility.Name}: {totalNeeded:F2} needed");

            // Find buildings that produce this utility
            List<ProductionBuilding> utilityBuildings = CalculationEngine.FindBuildingForResource(utility.Name);
            if (utilityBuildings.Count() == 0) { Console.WriteLine($"No buildings found that produce {utility.Name}!"); return; }

            // Show options and get user choice
            IndustryPlan expandedResult = new IndustryPlan();
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
        static void DisplayOptions(IndustryPlan result)
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
        static void SaveFile(List<City> cities, string filename)
        {
            try
            {
                savedFile.Name = filename;
                savedFile.Cities = cities.Select(c => City.ConvertToSavedCity(c)).ToList();  

                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(savedFile, options);
                File.WriteAllText(filename, jsonString);
                Console.WriteLine($"\n{cities.Count} Cities saved to {filename}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Error saving Cities: {ex.Message}");
            }
        }
        static List<City> LoadFile(string filename)
        {
            try
            {
                string jsonString = File.ReadAllText(filename);
                var saveFile = JsonSerializer.Deserialize<SavedFile>(jsonString);
                var cities = saveFile.Cities
                    .Select(c => City.ConvertFromSavedCity(c))
                    .ToList();
                Console.WriteLine($"✓ Loaded {cities.Count} city(s) from {filename}\n");

                foreach (var city in cities) 
                {
                    Console.WriteLine($"City Name : {city.Name}");
                    Console.WriteLine($"City Plans : {city.industryPlans.Count}");
                    Console.WriteLine($"City MicroDistricts : {city.microDistricts.Count}");
                }
                return cities;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error loading Cities: {ex.Message}\n");
                return new List<City>();
            }
        }
        static void CreateNewPlan()
        {
            IndustryPlan result = new IndustryPlan();
            double workerProductivity = 100.0;
            int choiceIndex;

            // Worker Productivity Choice : User Interaction
            while (true)
            {
                Console.Write("Type the Productivity of workers (30-150 default: 100 (100% productivity)\n: ");
                if (double.TryParse(Console.ReadLine(), out workerProductivity) && workerProductivity >= 30 && workerProductivity <= 150)
                {
                    CalculationSettings.WorkersProductivity = workerProductivity;
                    result.WorkersProductivity = workerProductivity;
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
            }
            // Default Industries
            if (result.TargetResource != GameData.CropsResource)
                DisplayOptions(result); 

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
            allCities.Add(city);
            currentResult = allPlans[currentPlanIndex];
            navigationStack.Clear();
            CommandLoop();
        }
        static void CreateBuildPlan()
        {
            double workerProductivity = 100;
            int choiceIndex;
            IndustryPlan result = new IndustryPlan();

            // Worker Productivity Choice : User Interaction
            while (true)
            {
                Console.Write("Type the Productivity of workers (30-150 default: 100 (100% productivity)\n: ");
                if (double.TryParse(Console.ReadLine(), out workerProductivity) && workerProductivity >= 30 && workerProductivity <= 150)
                    result.WorkersProductivity = workerProductivity;
                else { Console.Write("Invalid input. "); continue; }
                break;
            }

            // Building Choice : User Interaction
            List<ProductionBuilding> allBuildings = GameData.AllProductionBuildings;
            List<ProductionBuilding> allExtractions = allBuildings.Where(b => b.IsQualityDependent).ToList();
            List<ProductionBuilding> allProcessing = allBuildings.Where(b => !b.IsQualityDependent && !b.IsSeasonDependent && !b.IsUtilityBuilding).ToList();
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
                            result.ChosenBuilding.UseVehicles = true;  // Override to vehicles
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
            allCities.Add(city);
            currentResult = allPlans[currentPlanIndex];
            navigationStack.Clear();
            CommandLoop();
        }
        static void CommandLoop()
        {
            double workerProductivity = 100.0;
            int userInput;
            double inputAmount;
            string expandInput;
            int choiceIndex;
            List<Resource> allResources = GameData.AllResources;

            // Command loop
            while (true)
            {
                // Check if no plans exist
                if (allPlans.Count() == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\nNo plans exist. Returning to creation menu...");
                    Console.ResetColor();
                    break;  // Exit CommandLoop, returns to main menu
                }

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

                    // Option 3: Utility
                    List<Resource> utilNeeds = currentResult.TotalUtilityNeeds.Keys.ToList();

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
                    if (utilNeeds.Count() > 0)
                    {
                        Console.WriteLine("\n[Utility]");
                        foreach (var r in utilNeeds)
                            if (r == GameData.WasteWaterResource) { Console.WriteLine($"  · Sewage"); }
                            else Console.WriteLine($"  · {r.Name}");
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
                                utilityAmount = currentResult.TotalWaterNeeded + (currentResult.ChosenBuilding.RequiredResources.ContainsKey(GameData.WaterResource)
                                    ? currentResult.ChosenBuilding.RequiredResources[GameData.WaterResource] : 0);
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
                        IndustryPlan expandedResult = new IndustryPlan();
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
                                    Console.WriteLine("Invalid input. Choose the Option plan(number).");
                                else
                                {
                                    choiceIndex--;
                                    expandedResult.ChosenBuilding = expandedResult.Buildings[choiceIndex];

                                    // Check if building can use vehicles
                                    if (expandedResult.ChosenBuilding.Building.CanUseVehicles)
                                    {
                                        // Ask user: vehicles or workers?
                                        Console.WriteLine();
                                        Console.ForegroundColor = ConsoleColor.Cyan;
                                        Console.WriteLine("This building can be operated by vehicles or workers:");
                                        Console.ResetColor();
                                        Console.WriteLine("[1] Use vehicles");
                                        Console.WriteLine("[2] Use workers");
                                        Console.Write("Your choice: ");

                                        // Get user input and set the flag
                                        int userChoice;
                                        if (int.TryParse(Console.ReadLine(), out userChoice) && userChoice >= 1 && userChoice <= 2)
                                        {
                                            if (userChoice == 1)
                                            {
                                                expandedResult.ChosenBuilding.UseVehicles = true;
                                                Console.WriteLine("Vehicles chosen.");
                                            }
                                            else
                                            {
                                                expandedResult.ChosenBuilding.UseVehicles = false;
                                                Console.WriteLine("Workers chosen.");
                                            }
                                            break;  // Exit the loop after valid choice
                                        }
                                        else
                                        {
                                            Console.WriteLine("Invalid input. Choose 1 or 2:");
                                            continue;  // Ask again
                                        }
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
                                    expandedResult.ChosenBuilding = expandedResult.Buildings[choiceIndex];

                                    // Check if building can use vehicles
                                    if (expandedResult.ChosenBuilding.Building.CanUseVehicles)
                                    {
                                        // Ask user: vehicles or workers?
                                        Console.WriteLine();
                                        Console.ForegroundColor = ConsoleColor.Cyan;
                                        Console.WriteLine("This building can be operated by vehicles or workers:");
                                        Console.ResetColor();
                                        Console.WriteLine("[1] Use vehicles");
                                        Console.WriteLine("[2] Use workers");
                                        Console.Write("Your choice: ");

                                        // Get user input and set the flag
                                        int userChoice;
                                        if (int.TryParse(Console.ReadLine(), out userChoice) && userChoice >= 1 && userChoice <= 2)
                                        {
                                            if (userChoice == 1)
                                            {
                                                expandedResult.ChosenBuilding.Building.WorkersPerShift = 0;
                                                Console.WriteLine("Vehicles choosed.");
                                            }
                                            else { break; Console.WriteLine("Workers choosed."); }
                                        }
                                        else { continue; }
                                    }
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
                    Console.Write("[d] Delete a plan  [Enter] Return to commands\n> ");

                    // Plan deletion
                    if (Console.ReadKey().KeyChar == 'd')
                    {
                        Console.WriteLine();
                        int delChoice;
                        Console.Write($"Which plan to delete? [0-" + (allPlans.Count() - 1) + "]: ");
                        if (int.TryParse(Console.ReadLine(), out delChoice) && delChoice >= 0 && delChoice < allPlans.Count())
                        {
                            // Confirm deletion
                            Console.WriteLine($"Delete '{allPlans[delChoice].TargetResource.Name} ({allPlans[delChoice].TargetAmount}t/day)'? [y/n]");
                            if (Console.ReadKey().KeyChar == 'y')
                            {
                                Console.WriteLine();
                                allPlans.RemoveAt(delChoice);
                                if (delChoice < currentPlanIndex) currentPlanIndex--;   // Deleted before active plan → shift index down
                                else if (delChoice == currentPlanIndex)
                                {
                                    // Deleted the active plan → pick new active
                                    if (allPlans.Count > 0)
                                        currentPlanIndex = Math.Min(delChoice, allPlans.Count - 1);
                                    else
                                        currentPlanIndex = -1;  // No plans left
                                }   // else: deleted after active plan, no change needed

                                // Update currentResult
                                if (currentPlanIndex >= 0 && allPlans.Count > 0) currentResult = allPlans[currentPlanIndex];
                                else currentResult = null;

                                // Display confirmation
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("Plan deleted!");
                                Console.ResetColor();
                            }
                            else continue;
                        }
                        else { Console.WriteLine("Invalid input"); continue; }
                    }
                    else { Console.WriteLine("invalid input."); continue; } // Return to commands

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
                    city.industryPlans = allPlans;

                    if (city.industryPlans.Count == 0) { Console.WriteLine("No industry plans created yet."); continue; }
                    else { city.Display();}
                    continue;
                }
                else if (command == "support")
                {
                    //  Detect needed infrastructure types
                    //      Collect all resources that need infrastructure
                    HashSet<Resource> allIOResources = new HashSet<Resource>();
                    List<SupportBuilding> liquidInfra = GameData.AllSupportBuildings.Where(sb => sb.SupportCategory == SupportCategory.LiquidHandling).ToList();
                    List<SupportBuilding> bulkHandlingInfra = GameData.AllSupportBuildings.Where(sb => sb.SupportCategory == SupportCategory.BulkHandling).ToList();
                    List<SupportBuilding> dryBulkHandlingInfra = GameData.AllSupportBuildings.Where(sb => sb.SupportCategory == SupportCategory.DryBulkHandling).ToList();
                    List<SupportBuilding> solidHandlingInfra = GameData.AllSupportBuildings.Where(sb => sb.SupportCategory == SupportCategory.SolidHandling).ToList();
                    List<SupportBuilding> generalInfra = GameData.AllSupportBuildings.Where(sb => sb.SupportCategory == SupportCategory.GeneralDistribution).ToList();
                    List<SupportBuilding> waterInfra = GameData.AllSupportBuildings.Where(sb => sb.SupportCategory == SupportCategory.WaterHandling).ToList();
                    List<SupportBuilding> powerInfra = GameData.AllSupportBuildings.Where(sb => sb.SupportCategory == SupportCategory.PowerHandling).ToList();
                    List<SupportBuilding> sewageInfra = GameData.AllSupportBuildings.Where(sb => sb.SupportCategory == SupportCategory.SewageHandling).ToList();
                    List<SupportBuilding> heatInfra = GameData.AllSupportBuildings.Where(sb => sb.SupportCategory == SupportCategory.HeatHandling).ToList();
                    Dictionary<int, List<SupportBuilding>> categoryBuildings = new Dictionary<int, List<SupportBuilding>>();
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
                        if (r.RequiresDryBulkHandling)
                            dryBulkHandlingInfra = GameData.AllSupportBuildings.Where(sb => sb.SupportCategory == SupportCategory.DryBulkHandling).ToList();
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
                    if (dryBulkHandlingInfra.Count() > 0 && allIOResources.Any(r => r.RequiresDryBulkHandling))
                    {
                        categoryBuildings[catIndex] = dryBulkHandlingInfra;
                        Console.WriteLine("\n────────────────────────────────────────────────────");
                        Console.WriteLine("\nRelated Support Buildings (Dry-Bulk):");
                        for (int i = 0; i < dryBulkHandlingInfra.Count(); i++)
                            Console.WriteLine($"[{i + 1}]: {dryBulkHandlingInfra[i].Name}");
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
                    if (powerInfra.Count() > 0 && allIOResources.Any(r => r.RequiresPowerInfrastructure))
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
                    if (sewageInfra.Count() > 0 && allIOResources.Any(r => r.RequiresWaterInfrastructure))
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
                    Dictionary<SupportBuilding, int> chosenSupports = new Dictionary<SupportBuilding, int>();
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

                    // Add to SupportBuildings
                    foreach (var cb in chosenSupports)
                    {
                        SupportInstance addSupBuilding = new SupportInstance()
                        {
                            Building = cb.Key,
                            Count = cb.Value
                        };
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
                            AmenityType.Pub,           // 2
                            AmenityType.Healthcare,    // 3
                            AmenityType.Fireservice,   // 4
                            AmenityType.CityService,   // 5
                            AmenityType.Culture,       // 6
                            AmenityType.Sports,        // 7
                            AmenityType.Education,     // 8
                            AmenityType.CrimeJustice,  // 9
                            AmenityType.Fountain       // 10
                        };
                        AmenityType selectedType = typeMap[amenChoice - 1];
                        Console.WriteLine("┌─────────────────────────────────────────");
                        Console.WriteLine($"│ {selectedType} Buildings:");
                        Console.WriteLine("└─────────────────────────────────────────");

                        // Get current coverage
                        var coverage = microDistrict.GetAmenityCoverage();
                        int currentCapacity = coverage.ServiceCoverage[selectedType];

                        // Calculate needed capacity
                        var (neededCapacity, populationDesc, showCapacity) = CalculateCapacityNeeded(selectedType, rootResult);

                        // Display capacity info
                        if (showCapacity)
                        {
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"Population: {populationDesc}");
                            Console.ResetColor();

                            // Special handling for Education
                            if (selectedType == AmenityType.Education)
                            {
                                int kindergartenNeeded = (int)Math.Ceiling(rootResult.TotalPopulationNeeded * CalculationSettings.KindergartenAgePercent / 100);
                                int schoolNeeded = (int)Math.Ceiling(rootResult.TotalPopulationNeeded * CalculationSettings.SchoolAgePercent / 100);

                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"Kindergarten: {coverage.KindergartenCapacity}/{kindergartenNeeded}");

                                if (coverage.KindergartenCapacity < kindergartenNeeded)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"   Deficit: {kindergartenNeeded - coverage.KindergartenCapacity}");
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine($"   Surplus: {coverage.KindergartenCapacity - kindergartenNeeded}");
                                }

                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"School: {coverage.SchoolCapacity}/{schoolNeeded}");

                                if (coverage.SchoolCapacity < schoolNeeded)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"   Deficit: {schoolNeeded - coverage.SchoolCapacity}");
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine($"   Surplus: {coverage.SchoolCapacity - schoolNeeded}");
                                }

                                Console.ResetColor();
                            }
                            else
                            {
                                // Standard capacity display
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"Current capacity: {currentCapacity}");
                                Console.WriteLine($"Needed capacity: {neededCapacity}");

                                int difference = currentCapacity - neededCapacity;
                                if (difference < 0)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($" Deficit: {-difference}");
                                }
                                else if (difference > 0)
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine($" Surplus: {difference}");
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine(" Perfect match!");
                                }
                                Console.ResetColor();
                            }
                            Console.WriteLine();
                        }

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
                            if (selectedType == AmenityType.Education)
                            {
                                int kindergartenNeeded = (int)Math.Ceiling(rootResult.TotalPopulationNeeded * CalculationSettings.KindergartenAgePercent / 100);
                                int schoolNeeded = (int)Math.Ceiling(rootResult.TotalPopulationNeeded * CalculationSettings.SchoolAgePercent / 100);
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"Kindergarten: {coverage.KindergartenCapacity}/{kindergartenNeeded}");
                                Console.WriteLine($"School: {coverage.SchoolCapacity}/{schoolNeeded}");
                                Console.ResetColor();
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"{rootResult.TotalPopulationNeeded} citizens need to be served.");
                                Console.ResetColor();
                            }
                            Console.Write("How many?: ");
                            int count;

                            // Decide amount
                            if (int.TryParse(Console.ReadLine(), out count) && count >= 0)
                            {
                                AmenityInstance amenityInstance = new AmenityInstance();
                                amenityInstance.Building = buildingsOfType[buildChoice - 1];
                                amenityInstance.Count = count;
                                microDistrict.AmenityBuildings.Add(amenityInstance);
                                Console.WriteLine($"{buildingsOfType[buildChoice - 1].Name} x {count} has been added!");
                                Console.WriteLine($"Citizen Capacity: {buildingsOfType[buildChoice - 1].CitizenCapacity * count}");
                                Console.WriteLine($"Needed Workers: {buildingsOfType[buildChoice - 1].EffectiveWorkersPerShift}");
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

                        // Filter buildings by type from GameData.cs
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
                                    IndustryPlan subChainToRemove = currentResult.SubChains
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
                                    IndustryPlan subchainToCancel = currentResult.SubChains[undoChoice];
                                    currentResult.SubChains.Remove(subchainToCancel);
                                }
                                else { Console.WriteLine("Invalid input."); continue; }
                            }
                            else { Console.WriteLine("\nNo sub-chains to cancel!"); }
                        }
                        else if (buildChoice == 2)
                        {
                            if (currentResult.SupportBuildings.Count > 0)
                            {
                                Dictionary<int, SupportInstance> supIndex = new Dictionary<int, SupportInstance>();
                                Console.WriteLine("Which Support Building do you want to cancel?:");
                                int i = 0;
                                foreach (var si in currentResult.SupportBuildings)
                                {
                                    Console.WriteLine($"{i}: {si.Building.Name} x {si.Count}");
                                    supIndex.Add(i, si);
                                    i++;
                                }
                                Console.Write("Choose the number to cancel: ");
                                if (int.TryParse(Console.ReadLine(), out undoChoice)
                                    && undoChoice >= 0
                                    && undoChoice < currentResult.SupportBuildings.Count)
                                {
                                    var buildingToRemove = supIndex[undoChoice];

                                    // Recursively search and remove
                                    if (currentResult.SupportBuildings.Remove(buildingToRemove))
                                        Console.WriteLine($"{buildingToRemove.Building.Name} has been canceled.");
                                    else Console.WriteLine("Error: Building not found in tree.");
                                }
                                else { Console.WriteLine("Invalid input."); continue; }
                            }
                            else Console.WriteLine("\nNo Support Buildings to cancel!");
                        }
                        else if (buildChoice == 3)
                        {
                            if (microDistrict.ResidentialBuildings.Count() > 0)
                            {
                                Console.WriteLine("Which building do you want to cancel?: ");
                                int i = 0;
                                foreach (var ri in microDistrict.ResidentialBuildings)
                                {
                                    Console.WriteLine($"{i}: {ri.Building.Name}");
                                    i++;
                                }
                                Console.Write("Choose the number to cancel: ");
                                if (int.TryParse(Console.ReadLine(), out undoChoice) && undoChoice >= 0 && undoChoice <= microDistrict.ResidentialBuildings.Count())
                                {
                                    Console.WriteLine($"{microDistrict.ResidentialBuildings[undoChoice].Building.Name} has been canceled.");
                                    microDistrict.ResidentialBuildings.RemoveAt(undoChoice);
                                }
                                else { Console.WriteLine("Invalid input."); continue; }
                            }
                            else { Console.WriteLine("\nNo Residential Buildings to cancel!"); }
                        }
                        else if (buildChoice == 4)
                        {
                            if (microDistrict.AmenityBuildings.Count() > 0)
                            {
                                Console.WriteLine("Which building do you want to cancel?: ");
                                int i = 0;
                                foreach (var ri in microDistrict.AmenityBuildings)
                                {
                                    Console.WriteLine($"{i}: {ri.Building.Name}");
                                    i++;
                                }
                                Console.Write("Choose the number to cancel: ");
                                if (int.TryParse(Console.ReadLine(), out undoChoice) && undoChoice >= 0 && undoChoice <= microDistrict.AmenityBuildings.Count())
                                {
                                    Console.WriteLine($"{microDistrict.AmenityBuildings[undoChoice].Building.Name} has been canceled.");
                                    microDistrict.AmenityBuildings.RemoveAt(undoChoice);
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
                    // Select a district
                    if (allMicroDistricts.Count < 1)
                    {   // if none, create a district
                        microDistrict = new MicroDistrict();
                        string mdName;
                        Console.Write($"Write a name of the district: ");
                        mdName = Console.ReadLine();
                        microDistrict.Name = mdName;
                        allMicroDistricts.Add(microDistrict);
                    }
                    else                               
                    {
                        // MicroDistrict list
                        microDistrict = new MicroDistrict();
                        Console.WriteLine("Choose the District. If you want to create a new one, type '-1': ");
                        for (int i = 0; i < allMicroDistricts.Count; i++)  
                            Console.WriteLine($"[{i}]: {allMicroDistricts[i].Name}");
                        Console.Write(": ");

                        int userChoice;
                        if (int.TryParse(Console.ReadLine(), out userChoice) && userChoice >= 0 && userChoice < allMicroDistricts.Count)
                        {
                            microDistrict = allMicroDistricts[userChoice];
                            Console.WriteLine($"Chosen District: {microDistrict.Name}");

                            foreach (var ri in microDistrict.ResidentialBuildings) 
                                Console.WriteLine($"│ · {ri.Count} {ri.Building.Name}"); 
                        } else if (userChoice == -1)
                        {
                            microDistrict = new MicroDistrict();
                            string mdName;
                            Console.Write($"Write a name of the district: ");
                            mdName = Console.ReadLine();
                            microDistrict.Name = mdName;
                            allMicroDistricts.Add(microDistrict);
                        }
                    }

                    // Calculate total citizens from ALL plans
                    int totalWorkers = city.totalWorkers;
                    int totalHousingCapacity = allMicroDistricts.Sum(m => m.
                    TotalHousingCapacity);
                    Console.WriteLine($"\nYou need extra housing for {totalWorkers - totalHousingCapacity} workers.");
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

                                // Select the Amount
                                Console.WriteLine("\nSet the amount:");
                                foreach (var ri in resCount)
                                {
                                    Console.Write($"{ri.Building.Name}: ");
                                    if (int.TryParse(Console.ReadLine(), out count) && count >= 0)
                                        ri.Count += count;
                                }

                                // Add resCount to CalculationResult
                                microDistrict.ResidentialBuildings.AddRange(resCount);

                                // Show the Capacity
                                Console.WriteLine($"\nCurrent capacity: {city.totalHousingCapacity}" +
                                    $"\nExtra capcity needed: {((city.totalWorkers - city.totalHousingCapacity) >= 0
                                    ? (city.totalWorkers - city.totalHousingCapacity)
                                    : Math.Abs(city.totalWorkers - city.totalHousingCapacity))}");

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
                                microDistrict.ResidentialBuildings.AddRange(resCount);

                                // Show the Capacity
                                Console.WriteLine($"\nCurrent capacity: {city.totalHousingCapacity}" +
                                    $"\nExtra capcity needed: {((city.totalWorkers - city.totalHousingCapacity) >= 0
                                    ? (city.totalWorkers - city.totalHousingCapacity)
                                    : Math.Abs(city.totalWorkers - city.totalHousingCapacity))}");

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
                                microDistrict.ResidentialBuildings.AddRange(resCount);

                                // Show the Capacity
                                Console.WriteLine($"\nCurrent capacity: {city.totalHousingCapacity}" +
                                    $"\nExtra capcity needed: {((city.totalWorkers - city.totalHousingCapacity) >= 0
                                    ? (city.totalWorkers - city.totalHousingCapacity)
                                    : city.totalWorkers - city.totalHousingCapacity)}");

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
        private static string GetCategoryDisplayName(AmenityType type)
        {
            return type switch
            {
                AmenityType.Shopping => "Shopping",
                AmenityType.Pub => "Pub",
                AmenityType.Healthcare => "Healthcare",
                AmenityType.Fireservice => "Fire Service",
                AmenityType.Education => "Education",
                AmenityType.Culture => "Culture",
                AmenityType.Sports => "Sports",
                AmenityType.CrimeJustice => "Crime & Justice",
                AmenityType.CityService => "City Service",
                AmenityType.Fountain => "Fountain",
                _ => type.ToString()
            };
        }
        private static string GetSupCategoryDisplayName(SupportCategory type)
        {
            return type switch
            {
                SupportCategory.LiquidHandling => "Liquid",
                SupportCategory.BulkHandling => "Bulk",
                SupportCategory.DryBulkHandling => "Dry-Bulk",
                SupportCategory.SolidHandling => "Solid",
                SupportCategory.GeneralDistribution => "General",
                SupportCategory.PowerHandling => "Power",
                SupportCategory.HeatHandling => "Heat",
                SupportCategory.WaterHandling => "Water",
                SupportCategory.SewageHandling => "Sewage",
                _ => type.ToString()
            };
        }
        private static void DisplayCategoryWarnings(AmenityType type, List<KeyValuePair<AmenityBuilding, int>> amenities, int totalWorkers, int totalCitizens)
        {
            switch (type)
            {
                case AmenityType.Shopping:
                    int shoppingCapacity = amenities.Sum(kv => kv.Key.MaxVisitors * kv.Value * 15); // ×15 ratio
                    int shoppingNeeded = totalWorkers;

                    if (shoppingCapacity < shoppingNeeded)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"│            {shoppingNeeded - shoppingCapacity} workers underserved!");
                        Console.ResetColor();
                    }

                    bool hasAlcohol = amenities.Any(kv => kv.Key.ProductsOffered.Contains(GameData.AlcoholResource));
                    bool hasFood = amenities.Any(kv => kv.Key.ProductsOffered.Contains(GameData.FoodResource));
                    bool hasClothes = amenities.Any(kv => kv.Key.ProductsOffered.Contains(GameData.ClothesResource));
                    bool hasMeat = amenities.Any(kv => kv.Key.ProductsOffered.Contains(GameData.MeatResource));
                    bool hasElectronics = amenities.Any(kv => kv.Key.ProductsOffered.Contains(GameData.ElectronicsResource));
                    if (!hasAlcohol)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("│            Alcohol is not served!");
                        Console.ResetColor();
                    }
                    if (!hasFood)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("│            Food is not served!");
                        Console.ResetColor();
                    }
                    if (!hasClothes)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("│            Clothes are not served!");
                        Console.ResetColor();
                    }
                    if (!hasMeat)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("│            Meat is not served!");
                        Console.ResetColor();
                    }
                    if (!hasElectronics)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("│            Electronics are not served!");
                        Console.ResetColor();
                    }

                    break;

                case AmenityType.Pub:
                    int pubCapacity = amenities.Sum(kv => kv.Key.MaxVisitors * kv.Value * 100); // ×100 ratio
                    int pubNeeded = totalCitizens;

                    if (pubCapacity < pubNeeded)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"│            {pubNeeded - pubCapacity} citizens underserved!");
                        Console.ResetColor();
                    }
                    break;

                case AmenityType.Healthcare:
                    int healthcareCapacity = amenities.Sum(kv => kv.Key.MaxVisitors * kv.Value * 100); // ×100 ratio
                    int healthcareNeeded = totalCitizens;

                    if (healthcareCapacity < healthcareNeeded)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine($"│           {healthcareNeeded - healthcareCapacity} citizens uncovered (optional)");
                        Console.ResetColor();
                    }
                    break;

                case AmenityType.Culture:
                    int cultureCapacity = amenities.Sum(kv => kv.Key.MaxVisitors * kv.Value * 80); // ×80 ratio
                    int cultureNeeded = totalCitizens;

                    if (cultureCapacity < cultureNeeded)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"│            {cultureNeeded - cultureCapacity} citizens underserved!");
                        Console.ResetColor();
                    }
                    break;

                case AmenityType.Sports:
                    int sportsCapacity = amenities.Sum(kv => kv.Key.MaxVisitors * kv.Value * 80); // ×80 ratio
                    int sportsNeeded = totalCitizens;

                    if (sportsCapacity < sportsNeeded)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"│            {sportsNeeded - sportsCapacity} citizens underserved!");
                        Console.ResetColor();
                    }
                    break;

                case AmenityType.Education:
                    // Filter by EducationSubtype instead of name matching
                    var kindergartens = amenities.Where(kv => kv.Key.EducationLevel == EducationSubtype.Kindergarten).ToList();
                    var schools = amenities.Where(kv => kv.Key.EducationLevel == EducationSubtype.School).ToList();

                    // Use CalculationSettings percentages
                    int kindergartenNeeded = (int)Math.Ceiling(totalCitizens * CalculationSettings.KindergartenAgePercent / 100);
                    int schoolNeeded = (int)Math.Ceiling(totalCitizens * CalculationSettings.SchoolAgePercent / 100);

                    // Education buildings: MaxVisitors × 12 for kindergarten, × 20 for school
                    int kindergartenCapacity = kindergartens.Sum(kv => kv.Key.MaxVisitors * kv.Value * 12);
                    int schoolCapacity = schools.Sum(kv => kv.Key.MaxVisitors * kv.Value * 20);

                    if (kindergartenCapacity < kindergartenNeeded)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"│            Kindergarten: {kindergartenCapacity}/{kindergartenNeeded} ({kindergartenNeeded - kindergartenCapacity} underserved!)");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"│            Kindergarten: {kindergartenCapacity}/{kindergartenNeeded}");
                        Console.ResetColor();
                    }

                    if (schoolCapacity < schoolNeeded)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"│            School: {schoolCapacity}/{schoolNeeded} ({schoolNeeded - schoolCapacity} underserved!)");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"│            School: {schoolCapacity}/{schoolNeeded}");
                        Console.ResetColor();
                    }
                    break;

                case AmenityType.CrimeJustice:
                    int crimeCapacity = amenities.Sum(kv => kv.Key.MaxVisitors * kv.Value * 50); // ×50 estimate
                    int crimeNeeded = totalCitizens;

                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"│           {crimeCapacity}/{crimeNeeded} citizens covered (crime rate based - optional)");
                    Console.ResetColor();
                    break;

                case AmenityType.Fountain:
                case AmenityType.Fireservice:
                case AmenityType.CityService:
                    // No warnings needed - coverage-based, not capacity-based
                    break;
            }
        }
        // Returns (needed capacity, description of population)
        private static (int capacity, string description, bool showCapacity) CalculateCapacityNeeded(
    AmenityType type,
    IndustryPlan result)
        {
            int citizens = result.TotalPopulationNeeded;
            int workers = result.TotalWorkers;

            return type switch
            {
                AmenityType.Shopping =>
                    ((int)Math.Ceiling((double)workers / 15), $"{workers} workers", true),
                AmenityType.Pub =>
                    ((int)Math.Ceiling((double)citizens / 100), $"{citizens} citizens", true),
                AmenityType.Culture =>
                    ((int)Math.Ceiling((double)citizens / 80), $"{citizens} citizens", true),
                AmenityType.Sports =>
                    ((int)Math.Ceiling((double)citizens / 80), $"{citizens} citizens", true),
                AmenityType.Healthcare =>
                    ((int)Math.Ceiling((double)citizens / 100), $"{citizens} citizens (optional)", true),
                AmenityType.Education =>
                    (0, "See breakdown below", true),
                AmenityType.CrimeJustice =>
                    ((int)Math.Ceiling((double)citizens / 50), $"{citizens} citizens (estimate)", true),
                AmenityType.Fireservice =>
                    (0, "Coverage-based (not capacity)", false),
                AmenityType.CityService =>
                    (0, "Utility service (not capacity)", false),
                AmenityType.Fountain =>
                    ((int)Math.Ceiling((double)citizens / 200), $"{citizens} citizens (low priority)", true),
                _ => (0, "Unknown", false)
            };
        }
    }
}
