using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static CalculationEngine;

namespace SovietRepublicPlanner
{
    internal class Program
    {
        static List<IndustryPlan> allPlans = new List<IndustryPlan>();
        static List<MicroDistrict> allMicroDistricts = new List<MicroDistrict>();
        static List<City> allCities = new List<City>();

        // Navigation pointer: Points to the current position in the active plan's tree
        // Used for commands that navigate/modify existing plans (expand, dive, back, cancel)
        // - At root level: currentResult == rootResult (the plan itself)
        // - When diving: currentResult points to a SubChain within the plan
        // - When expanding: operations are performed on currentResult's level

        static City city = new City()
        {
            industryPlans = allPlans,
            microDistricts = allMicroDistricts
        };
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
                            if (allCities.Count > 0)
                            {
                                city = allCities[0];
                                allPlans = city.industryPlans;
                                allMicroDistricts = city.microDistricts;
                                currentSaveFile = Path.GetFileName(jsonFiles[loadChoice]);
                            } else
                            {
                                Console.WriteLine($"No city is in this File.");
                            }
                        } else { Console.WriteLine("Invalid input"); continue; }
                        break;
                    }

                    // Display success message
                    if (allCities.Count > 0)
                    {
                        currentPlanIndex = 0;
                        currentResult = allPlans[0];
                        Console.WriteLine($"\n'{currentSaveFile}' Loaded. Type 'listplans' to view them.");

                        //debug
                        for (int i = 0; i < allCities.Count; i++) 
                        {
                            Console.WriteLine($"{i}: {allCities[i].Name}");
                        }
                    }
                }
            }
            Console.WriteLine("\n");
            // Initialize BoundsCaches
            GameData.InitializeUtilBounds(GameData.AllBuildings);
            Console.WriteLine("All Buildings Utilities Bounds cache have been Initialized");
            GameData.InitializeUtilPerWorkerBounds(GameData.AllBuildings);
            Console.WriteLine("All Buildings Utilities Bounds Per Worker cache have been Initialized");
            GameData.GeneralBoundsCache.Clear();
            GameData.InitializeGeneralBounds(GameData.AllBuildings, GeneralMetricType.ConstructionCost);
            Console.WriteLine("All Buildings ConstructionCost Bounds cache have been Initialized");
            GameData.InitializeGeneralBounds(GameData.AllBuildings, GeneralMetricType.WorkDays);
            Console.WriteLine("All Buildings WorkDays Bounds cache have been Initialized");
            GameData.InitializeGeneralBounds(GameData.AllBuildings, GeneralMetricType.WorkersPerArea);
            Console.WriteLine("All Buildings Density(WorkersPerArea) Bounds cache have been Initialized");
            GameData.InitializeGeneralBounds(GameData.AllBuildings, GeneralMetricType.CostPerWorker);
            Console.WriteLine("All Buildings CostPerWorker Bounds cache have been Initialized");
            GameData.InitializeGeneralBounds(GameData.AllBuildings, GeneralMetricType.WorkDaysPerWorker);
            Console.WriteLine("All Buildings WorkDaysPerWorker Bounds cache have been Initialized");

            // Main program loop
            while (true)
            {
                // If there are Cities -> Command Loop
                if (allCities.Count() > 0) { CommandLoop(); }

                // Creation menu
                Console.WriteLine("\n────────────────────────────────────────");
                Console.WriteLine("  'createcity'- Create a new city");
                Console.WriteLine("  'switchcity'- Switch City");
                Console.WriteLine("  'newplan'   - Resource-target mode (Default City)");
                Console.WriteLine("  'buildplan' - Building-count mode (Default City)");
                Console.WriteLine("  'navigate'  - View/modify existing plans");
                Console.WriteLine("  'done'      - Exit program");
                Console.Write("> ");
                List<string> planInputs = new List<string> { "createcity", "switchcity","newplan", "buildplan", "navigate", "view", "done"};
                string planInput = ReadLineWithCompletion(planInputs).ToLower().Trim();
                if (planInput == "createcity")
                {
                    City c = new City();
                    string cname = null;
                    while (cname == null)
                    {
                        Console.Write("Write a name of the city: ");
                        cname = Console.ReadLine().Trim();
                        if (cname.Replace(" ", "") == null) continue;
                    }
                    c.Name = cname;
                    city = c;
                    allPlans = c.industryPlans;
                    allMicroDistricts = c.microDistricts;
                    allCities.Add(city);
                }
                else if (planInput == "switchcity")
                {
                    if (allCities.Count <= 1) { Console.WriteLine("There's no city to switch"); continue; }
                    Console.WriteLine($"[ City List ]:");
                    for (int i = 0; i < allCities.Count; i++)
                    {
                        Console.WriteLine($"[{i}]: {allCities[i].Name}");
                    }
                    int choice;
                    if (int.TryParse(Console.ReadLine(), out choice) && choice >= 0 && choice < allCities.Count)
                    {
                        city = allCities[choice];
                        allPlans = city.industryPlans;
                        allMicroDistricts = city.microDistricts;
                    } else { Console.WriteLine("Invalid index"); continue; }
                }
                else if (planInput == "newplan") { CreateNewPlan(); }            // After creating, loop back (will enter CommandLoop next iteration)
                else if (planInput == "buildplan") { CreateBuildPlan(); }   // After creating, loop back (will enter CommandLoop next iteration)
                else if (planInput == "navigate" || planInput == "view")    // Go back into CommandLoop without creating a new plan
                {
                    if (allPlans.Count > 0) { continue; }   // Skip to next iteration, which enters CommandLoop
                    else { Console.WriteLine("No plans exist yet. Create one first!"); }
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
                            // debug
                            Console.WriteLine($"allCities : {allCities.Count}");

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
                Console.WriteLine($"Sewage produced: {result.Buildings[i].TotalSewageProduced:F2}");
                Console.WriteLine($"Garbage produced: {result.Buildings[i].TotalGarbageProduced:F6}");
                Console.WriteLine($"Pollution emitted: {result.Buildings[i].TotalEnvironmentPollution:F6}");
            }
        }
        static void SaveFile(List<City> cities, string filename)
        {
            // debug
            Console.WriteLine($"SaveFile() allCities : {cities.Count}");

            try
            {
                SavedFile savedFile = new SavedFile()
                {
                    Name = filename,
                    Cities = cities.Select(c => City.ConvertToSavedCity(c)).ToList(),
                    CurrentYear = CalculationSettings.CurrentYear,
                    UnlockedTech = CalculationSettings.UnlockedTech
                };

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
                CalculationSettings.CurrentYear = saveFile.CurrentYear;
                CalculationSettings.UnlockedTech = saveFile.UnlockedTech ?? new HashSet<string>();
                var cities = saveFile.Cities
                    .Select(c => City.ConvertFromSavedCity(c))
                    .ToList();
                Console.WriteLine($"✓ Loaded {cities.Count} city(s) from {filename}\n");
                return cities;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error loading Cities: {ex.Message}\n");
                return new List<City>();
            }
        }
        static IndustryPlan CreateIndustryPlan(string plantype)
        {
            IndustryPlan result = new IndustryPlan();

            if (plantype == "buildplan") {
                double workerProductivity = 100;
                int choiceIndex;

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
                List<ProductionBuilding> allProcessing = allBuildings.Where(b => !b.IsQualityDependent && !b.IsSeasonDependent).ToList();
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
                        // Set the Current Number of Employees
                        else
                        {
                            Console.Write($"How many workers? [0 - {result.ChosenBuilding.Building.MaxWorkers}]: ");
                            int curNumEmp;
                            if (int.TryParse(Console.ReadLine(), out curNumEmp) && curNumEmp >= 0 && curNumEmp <= result.ChosenBuilding.Building.MaxWorkers)
                            {
                                result.ChosenBuilding.Building.CurNumEmp = curNumEmp;
                                Console.WriteLine($"The number of workers for {result.ChosenBuilding.Building.Name}: {result.ChosenBuilding.Building.CurNumEmp}");
                            }
                            else { Console.WriteLine("The number of workers should be within the range of 0 ~ Max number of workers"); continue; }
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
                                    else { Console.Write("Wrong input. Try again."); break; }
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
                    for (int i = 0; i < result.ChosenBuilding.BuildingInstances.Count; i++)
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
                Console.WriteLine($"Sewage produced: {result.ChosenBuilding.TotalSewageProduced:F2}");
                Console.WriteLine($"Garbage produced: {result.ChosenBuilding.TotalGarbageProduced:F6}");
                Console.WriteLine($"Pollution emitted: {result.ChosenBuilding.TotalEnvironmentPollution:F6}");

                return result; 
            }
            else if (plantype == "newplan")
            {
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
                                    result.ChosenBuilding.Building.MaxWorkers = 0;  // Override to vehicles
                                }
                                // else keep the default 100 workers
                            }
                            // Set the Current Number of Employees
                            else
                            {
                                Console.Write($"How many workers? [0 - {result.ChosenBuilding.Building.MaxWorkers}]: ");
                                int curNumEmp;
                                if (int.TryParse(Console.ReadLine(), out curNumEmp) && curNumEmp >= 0 && curNumEmp <= result.ChosenBuilding.Building.MaxWorkers)
                                {
                                    result.ChosenBuilding.Building.CurNumEmp = curNumEmp;
                                    Console.WriteLine($"The number of workers for {result.ChosenBuilding.Building.Name}: {result.ChosenBuilding.Building.CurNumEmp}");
                                }
                                else { Console.WriteLine("The number of workers should be within the range of 0 ~ Max number of workers"); continue; }
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
                                    result.ChosenBuilding.Building.MaxWorkers = 0;  // Override to vehicles
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
            }
            return result;
        }
        static void CreateNewPlan()
        {
            IndustryPlan result = CreateIndustryPlan("newplan");

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
            IndustryPlan result = CreateIndustryPlan("buildplan");

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

                Console.Write("\nCommand (listcities/listplans/listdistrict/masterplan/switchcity/switchplan/switchdistrict/newcity/expand/utility/support/cancel/back/dive/summary/housing/amenity/transportation/research/done): ");
                List<string> commands = new List<string> { "listcities", "listplans", "masterplan", "switchplan", "expand", "utility","support", "cancel", "back", "dive", "summary",
                    "housing", "amenity", "transportation", "done", "newcity", "switchcity", "listdistrict", "switchdistrict", "research", "year" };
                string command = ReadLineWithCompletion(commands).ToLower().Trim();
                if (command == "expand")
                {
                    // Choose Resources to expand
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

                    bool allValid = false;
                    while (!allValid)
                    {
                        Console.Write($"\nType the resource to expand ('0' to back): ");
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

                            // Search for the resource
                            foreach (Resource r in currentResult.ChosenBuilding.RequiredResources.Keys)
                            {
                                if (r.Name.ToLower() == trimmedName)
                                {
                                    // Normal resource
                                    resourcesToExpand.Add(r);
                                    matchedResource = r;
                                    break;
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
                        if (rootResult.TotalInputs.ContainsKey(resourcesToExpand[j]))
                        {
                            expandedResult.TargetAmount = rootResult.TotalInputs[resourcesToExpand[j]];
                        }
                        else
                        {
                            // Fallback: shouldn't happen, but use current requirement
                            expandedResult.TargetAmount = productionInputs.Contains(resourcesToExpand[j])
                                ? currentResult.ChosenBuilding.RequiredResources[resourcesToExpand[j]]
                                : rootResult.TotalCitizenConsumption[resourcesToExpand[j]];
                        }
                        expandedResult = CalculationEngine.Calculate(expandedResult.TargetResource.Name, expandedResult.TargetAmount);

                        // Set Current Number of Employee
                        //Console.Write($"How many employees? [0 - {expandedResult.ChosenBuilding.Building.MaxWorkers}]: ");
                        //int curNumEmp;
                        //if (int.TryParse(Console.ReadLine(), out curNumEmp) && curNumEmp >= 0 && curNumEmp <= expandedResult.ChosenBuilding.Building.MaxWorkers)
                        //{
                        //    expandedResult.ChosenBuilding.Building.CurNumEmp = curNumEmp;
                        //}
                        //else { Console.WriteLine("Invalid number of employees."); break; }

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

                                                Console.Write("How many employees?: ");
                                                int curNumEmp2;
                                                if (int.TryParse(Console.ReadLine(), out curNumEmp2) && curNumEmp2 >= 0 && curNumEmp2 <= expandedResult.ChosenBuilding.Building.MaxWorkers)
                                                {
                                                    expandedResult.ChosenBuilding.Building.CurNumEmp = curNumEmp2;
                                                }
                                                else { Console.WriteLine("Invalid number of employees."); break; }
                                            }
                                            break;  // Exit the loop after valid choice
                                        }
                                        else
                                        {
                                            Console.WriteLine("Invalid input. Choose 1 or 2:");
                                            continue;  // Ask again
                                        }
                                    }
                                    // Set Current Number of Employees
                                    else
                                    {
                                        Console.Write($"How many employees? [0 - {expandedResult.ChosenBuilding.Building.MaxWorkers}]: ");
                                        int curNumEmp2;
                                        if (int.TryParse(Console.ReadLine(), out curNumEmp2) && curNumEmp2 >= 0 && curNumEmp2 <= expandedResult.ChosenBuilding.Building.MaxWorkers)
                                        {
                                            expandedResult.ChosenBuilding.Building.CurNumEmp = curNumEmp2;
                                        } else { Console.WriteLine("Invalid number of employees."); break; }
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

                                    // Set Current Number of Employee
                                    Console.Write($"How many employees? [0 - {expandedResult.ChosenBuilding.Building.MaxWorkers}]: ");
                                    int curNumEmp;
                                    if (int.TryParse(Console.ReadLine(), out curNumEmp) && curNumEmp >= 0 && curNumEmp <= expandedResult.ChosenBuilding.Building.MaxWorkers)
                                    {
                                        expandedResult.ChosenBuilding.Building.CurNumEmp = curNumEmp;
                                    }
                                    else { Console.WriteLine("Invalid number of employees."); break; }


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
                                                expandedResult.ChosenBuilding.Building.MaxWorkers = 0;
                                                Console.WriteLine("Vehicles choosed.");
                                            }
                                            else { 
                                                Console.WriteLine("Workers choosed.");

                                                // Set Current Number of Employee
                                                Console.Write($"How many employees? [0 - {expandedResult.ChosenBuilding.Building.MaxWorkers}]: ");
                                                int curNumEmp2;
                                                if (int.TryParse(Console.ReadLine(), out curNumEmp2) && curNumEmp2 >= 0 && curNumEmp2 <= expandedResult.ChosenBuilding.Building.MaxWorkers)
                                                {
                                                    expandedResult.ChosenBuilding.Building.CurNumEmp = curNumEmp2;
                                                }
                                                else { Console.WriteLine("Invalid number of employees."); break; }

                                                break;
                                            }
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
                else if (command == "utility")
                {
                    // Designate the City
                    City currentCity = allCities.FirstOrDefault(c => c.Name == city.Name);
                    if (currentCity == null)
                    {
                        Console.WriteLine($"There's no a City chosen.");
                        continue;
                    }

                    // List and Choose existing UtilityPlans
                    if (currentCity.UtilityPlans.Count > 0)
                    {
                        int pc;

                        // List the existing UtilityPlans in City
                        Console.WriteLine("Choose the UtilityPlan or Create a new one(Type -1): ");
                        for (int i = 0; i < currentCity.UtilityPlans.Count; i++)
                        {
                            Console.WriteLine($"[{i}]: {currentCity.UtilityPlans[i].Name}");
                        }

                        // Choose a UtilityPlan
                        if (int.TryParse(Console.ReadLine(), out pc) && pc >= 0 && pc < currentCity.UtilityPlans.Count)
                        {
                            Console.WriteLine("\n┌────────────────────────────────────────┐");
                            Console.WriteLine("│         UTILITY PLAN                   │");
                            Console.WriteLine("├────────────────────────────────────────┤");
                            Console.WriteLine($"│ Name: {currentCity.UtilityPlans[pc].Name} ");
                            Console.WriteLine("├────────────────────────────────────────┤");
                            Console.WriteLine("│ Utilities Status:");
                            Console.WriteLine("│                      Needed         Produced   Balance");
                            Console.WriteLine($"│ Total Workers:{currentCity.UtilityPlans[pc].TotalWorkers,13}");

                            // Power
                            double produced = currentCity.UtilityPlans[pc].TotalOutputs.Any(o => o.Key == GameData.PowerResource)
                                ? currentCity.UtilityPlans[pc].TotalOutputs[GameData.PowerResource]
                                : 0;
                            Console.WriteLine($"│ Power (MW):{currentCity.UtilityPlans[pc].TotalPowerConsumptionMWh,16:F2}{produced,17:F2}{produced - currentCity.UtilityPlans[pc].TotalPowerConsumptionMWh,10:F2}");
                            // Water
                            produced = currentCity.UtilityPlans[pc].TotalOutputs.Any(o => o.Key == GameData.WaterResource)
                                ? currentCity.UtilityPlans[pc].TotalOutputs[GameData.WaterResource]
                                : 0;
                            Console.WriteLine($"│ Water (t/day):{currentCity.UtilityPlans[pc].TotalWaterConsumptionM3,13:F2}{produced,17:F2}{produced - currentCity.UtilityPlans[pc].TotalWaterConsumptionM3,10}");
                            // Heat
                            produced = currentCity.UtilityPlans[pc].TotalOutputs.Any(o => o.Key == GameData.HeatResource)
                                ? currentCity.UtilityPlans[pc].TotalOutputs[GameData.HeatResource]
                                : 0;
                            Console.WriteLine($"│ Heat (MW):{0:F2}{produced,17:F2}{produced,10}");
                            // Sewage
                            produced = currentCity.UtilityPlans[pc].TotalSewageDisposalCapacity;
                            Console.WriteLine($"│ Sewage (t/day):{currentCity.UtilityPlans[pc].TotalWaterConsumptionM3,12:F2}{produced,17:F2}{0,10}");

                            // Garbage
                            Console.WriteLine($"│ Garbage (t/day):{currentCity.UtilityPlans[pc].TotalGarbageProduction,11:F2}{currentCity.UtilityPlans[pc].TotalGarbageProduction,27:F2}");

                            Console.WriteLine($"│ Pollution:{currentCity.UtilityPlans[pc].TotalEnvironmentPollution,17:F6}{currentCity.UtilityPlans[pc].TotalEnvironmentPollution,27:F6}");
                            Console.WriteLine("├────────────────────────────────────────┤");
                            Console.WriteLine("│ All Utility Buildings :                │");
                            Console.WriteLine("├────────────────────────────────────────┤");

                            // Display all buildings recursively
                            foreach (var ui in currentCity.UtilityPlans[pc].Buildings)
                            {
                                Console.WriteLine($"│ [ {currentCity.UtilityPlans[pc].Type} ]");
                                Console.WriteLine($"│   · {ui.Count} x {ui.Building.Name}");
                            }

                            var totalSupBldgs = currentCity.UtilityPlans[pc].SupportBuildings;

                            if (totalSupBldgs.Count > 0)
                            {
                                Console.WriteLine("├────────────────────────────────────────┤");
                                Console.WriteLine("│ Support Infrastructures:               │");
                                Console.WriteLine("├────────────────────────────────────────┤");
                                foreach (var kv in totalSupBldgs)
                                    Console.WriteLine($"│ · {kv.Count} × {kv.Building.Name}");
                            }
                            //if (TransportationBuildings.Count() > 0)
                            //{
                            //    Console.WriteLine("├────────────────────────────────────────┤");
                            //    Console.WriteLine("│ Transportation Buildings:");
                            //    Console.WriteLine("├────────────────────────────────────────┤");
                            //    foreach (var ti in TransportationBuildings)
                            //        Console.WriteLine($"│ · {ti.Count} × {ti.Building.Name}");
                            //}
                            Console.WriteLine("├────────────────────────────────────────┤");
                            Console.WriteLine("│ Importing Resources:");
                            Console.WriteLine("├────────────────────────────────────────┤");
                            foreach (var kv in currentCity.UtilityPlans[pc].TotalInputs)
                                Console.WriteLine($"│ ·{kv.Value,6:F2}t/day {kv.Key.Name,-20}");
                            if (currentCity.UtilityPlans[pc].ConstructionMaterials.Count() > 0)
                            {
                                Console.WriteLine("├────────────────────────────────────────┤");
                                Console.WriteLine("│ Total Construction Materials:");
                                Console.WriteLine("├────────────────────────────────────────┤");
                                foreach (var kv in currentCity.UtilityPlans[pc].ConstructionMaterials)
                                    Console.WriteLine($"│ · {kv.Value:F2} × {kv.Key.Name}");
                                Console.WriteLine("└────────────────────────────────────────┘");
                            }

                            // User Choice : Modify || Delete UtilityPlan
                            Console.Write($"Which action do you want to do on this plan? (m : modify | d : delete): ");
                            char actionChoice;

                            if (char.TryParse(Console.ReadLine(), out actionChoice) && (actionChoice == 'm' || actionChoice == 'd'))
                            {
                                // Deletion
                                if (actionChoice == 'd')
                                {
                                    Console.WriteLine($"Do you want to delete this UtilityPlan ({currentCity.UtilityPlans[pc].Name})? [y/n]");
                                    char deleteChoice;
                                    if (char.TryParse(Console.ReadLine(), out deleteChoice) && (deleteChoice == 'y' || deleteChoice == 'n'))
                                    {
                                        if (deleteChoice == 'y')
                                        {
                                            Console.WriteLine($"{currentCity.UtilityPlans[pc].Name} has been deleted.");
                                            currentCity.UtilityPlans.Remove(currentCity.UtilityPlans[pc]);
                                        }
                                        else
                                        {
                                            Console.WriteLine("Going back to CommandLoop");
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Invalid input. Going back to CommandLoop");
                                        continue;
                                    }
                                }
                                // Modification
                                else
                                {
                                    for (int i2 = 0; i2 < currentCity.UtilityPlans[pc].Buildings.Count; i2++)
                                    {
                                        Console.WriteLine($"[{i2}]: {currentCity.UtilityPlans[pc].Buildings[i2].Building.Name}");
                                    }
                                    Console.Write($"Which action do you want to do on these instances? (a : add | m : change number of employees | d : delete): ");

                                    if (char.TryParse(Console.ReadLine(), out actionChoice) && (actionChoice == 'a' || actionChoice == 'd' || actionChoice == 'm'))
                                    {
                                        // Addition
                                        if (actionChoice == 'a')
                                        {
                                            AddUtilityInstance(currentCity.UtilityPlans[pc]);
                                        }
                                        // Deletion
                                        else if (actionChoice == 'd')
                                        {
                                            Console.Write($"\nChoose the instance to delete: ");
                                            int deleteChoice;
                                            if (int.TryParse(Console.ReadLine(), out deleteChoice) && deleteChoice >= 0 && deleteChoice < currentCity.UtilityPlans[pc].Buildings.Count)
                                            {
                                                Console.WriteLine($"{currentCity.UtilityPlans[pc].Buildings[deleteChoice].Building.Name} has been deleted from {currentCity.UtilityPlans[pc].Name}");
                                                currentCity.UtilityPlans[pc].Buildings.RemoveAt(deleteChoice);
                                            }
                                            else
                                            {
                                                Console.WriteLine("Invalid input. Going back to Command Loop");
                                                continue;
                                            }
                                        }
                                        else if (actionChoice == 'm')
                                        {
                                            int insChoice;
                                            int curNumEmp;
                                            UtilityInstance chosenIns = new UtilityInstance();

                                            // Choose Instance
                                            Console.Write($"Choose the instance : ");
                                            if (int.TryParse(Console.ReadLine(), out insChoice) && insChoice >= 0 && insChoice <= currentCity.UtilityPlans[pc].Buildings.Count )
                                            {
                                                chosenIns = currentCity.UtilityPlans[pc].Buildings[insChoice];
                                                Console.WriteLine($"Chosen Instance : {chosenIns.Building.Name} ({chosenIns.CurNumEmp} / {chosenIns.Building.MaxWorkers})");
                                            } else { Console.WriteLine("Invalid UtilityInstance number."); continue; }

                                            Console.Write($"How many employees for {chosenIns.Building.Name}?: ");
                                            if (int.TryParse(Console.ReadLine(), out curNumEmp) && curNumEmp >= 0 && curNumEmp <= chosenIns.Building.MaxWorkers)
                                            {
                                                chosenIns.CurNumEmp = curNumEmp;
                                                Console.WriteLine($"{chosenIns.Building.Name} : {chosenIns.CurNumEmp} / {chosenIns.Building.MaxWorkers} set!");
                                            }
                                            else { Console.WriteLine("Invalid input"); continue; }
                                        }
                                    } else { Console.WriteLine("Invalid action choice. Going back to Command Loop"); continue; }
                                }
                            } else
                            {
                                Console.WriteLine("Invalid input. Going back to CommandLoop");
                                continue;
                            }
                        }
                        else if (pc == -1)
                        {
                            AddUtilityPlan(currentCity);
                        }
                        else { Console.WriteLine("Invalid index. Going back to the command menu"); continue; }
                    }
                    // Create a new UtilityPlan
                    else
                    {
                        AddUtilityPlan(currentCity);
                        continue;
                    }
                }
                else if (command == "listcities")
                {
                    if (allCities.Count() == 0) { Console.WriteLine("No city is created yet."); continue; }

                    Console.WriteLine("\n=== All Cities ===");
                    for (int i = 0; i < allCities.Count(); i++)
                    {
                        string marker = "← ACTIVE";
                        Console.WriteLine($"{i}. {allCities[i].Name}" +
                            $"{(allCities[i] == city ? marker : null)}");
                    }
                    Console.WriteLine();
                    Console.Write("[d] Delete a city  [Enter] Return to commands\n> ");

                    // Plan deletion
                    if (Console.ReadKey().KeyChar == 'd')
                    {
                        Console.WriteLine();
                        int delChoice;
                        Console.Write($"Which city to delete? [0-" + (allCities.Count() - 1) + "]: ");
                        if (int.TryParse(Console.ReadLine(), out delChoice) && delChoice >= 0 && delChoice < allCities.Count())
                        {
                            // Confirm deletion
                            Console.WriteLine($"Delete '{allCities[delChoice].Name}'? [y/n]");
                            if (Console.ReadKey().KeyChar == 'y')
                            {
                                Console.WriteLine();
                                allCities.RemoveAt(delChoice);
                                if (delChoice < currentPlanIndex) currentPlanIndex--;   // Deleted before active plan → shift index down
                                else if (delChoice == currentPlanIndex)
                                {
                                    // Deleted the active city → pick new active
                                    if (allCities.Count > 0)
                                        currentPlanIndex = Math.Min(delChoice, allCities.Count - 1);
                                    else
                                        currentPlanIndex = -1;  // No plans left
                                }   // else: deleted after active city, no change needed

                                // Update current city(city)
                                if (currentPlanIndex >= 0 && allCities.Count > 0)
                                {
                                    city = allCities[currentPlanIndex];
                                    allPlans = city.industryPlans;
                                    allMicroDistricts = city.microDistricts;
                                }
                                else city = null;

                                // Display confirmation
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("Plan deleted!");
                                Console.ResetColor();
                            }
                            else continue;
                        }
                        else { Console.WriteLine("Invalid input"); continue; }
                    }
                    else { Console.WriteLine("Returned to commands"); continue; } // Return to commands

                    continue;
                }
                else if (command == "listplans")
                {
                    City currentCity = city;
                    if (currentCity.industryPlans.Count() == 0) { Console.WriteLine("No plans created yet."); continue; }

                    Console.WriteLine("\n=== All Plans ===");
                    for (int i = 0; i < currentCity.industryPlans.Count(); i++)
                    {
                        string marker = "← ACTIVE";
                        Console.WriteLine($"{i}. {currentCity.industryPlans[i].TargetResource.Name} ({currentCity.industryPlans[i].TargetAmount} t/day) " +
                            $"{(currentCity.industryPlans[i] == currentResult ? marker : null)}");
                    }
                    Console.WriteLine();
                    Console.Write("[d] Delete a plan  [m] Modify plan  [a] Add IndustryPlan [Index] Display District Info  [Enter] Return to commands\n> ");
                    string input = Console.ReadLine()?.Trim().ToLower();
                    int dChoice;

                    // Plan deletion
                    if (input == "d")
                    {
                        Console.WriteLine();
                        int delChoice;
                        Console.Write($"Which plan to delete? [0-" + (currentCity.industryPlans.Count() - 1) + "]: ");
                        if (int.TryParse(Console.ReadLine(), out delChoice) && delChoice >= 0 && delChoice < currentCity.industryPlans.Count())
                        {
                            // Confirm deletion
                            Console.WriteLine($"Delete '{currentCity.industryPlans[delChoice].TargetResource.Name} ({currentCity.industryPlans[delChoice].TargetAmount}t/day)'? [y/n]");
                            if (Console.ReadKey().KeyChar == 'y')
                            {
                                Console.WriteLine();
                                currentCity.industryPlans.RemoveAt(delChoice);
                                if (delChoice < currentPlanIndex) currentPlanIndex--;   // Deleted before active plan → shift index down
                                else if (delChoice == currentPlanIndex)
                                {
                                    // Deleted the active plan → pick new active
                                    if (currentCity.industryPlans.Count > 0)
                                        currentPlanIndex = Math.Min(delChoice, currentCity.industryPlans.Count - 1);
                                    else
                                        currentPlanIndex = -1;  // No plans left
                                }   // else: deleted after active plan, no change needed

                                // Update currentResult
                                if (currentPlanIndex >= 0 && currentCity.industryPlans.Count > 0) currentResult = currentCity.industryPlans[currentPlanIndex];
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
                    // Display IndustryPlan
                    else if (int.TryParse(input, out dChoice) && dChoice >= 0 && dChoice < currentCity.industryPlans.Count)
                    {
                        currentResult = currentCity.industryPlans[dChoice];
                        Console.WriteLine($"Now You're in {currentResult.Name} of City {currentResult.ParentCity.Name}");
                        currentCity.industryPlans[dChoice].DisplayTotalReceipt();
                    }
                    // Modify Plan
                    else if (input == "m")
                    {
                        Console.WriteLine();
                        int modChoice;
                        Console.Write($"Which plan to modify? [0-" + (currentCity.industryPlans.Count() - 1) + "]: ");
                        if (int.TryParse(Console.ReadLine(), out modChoice) && modChoice >= 0 && modChoice < currentCity.industryPlans.Count())
                        {
                            currentResult = currentCity.industryPlans[modChoice];
                            Console.WriteLine($"Now You're in {currentResult.Name} of City {currentResult.ParentCity.Name}");
                            currentCity.industryPlans[modChoice].DisplayTotalReceipt();
                            Console.Write($"\nWhat do you want to modify? [0: Current Number of Employees || 1: Support Infrastructures]: ");
                            int modOption;
                            int curNumEmp;

                            if (int.TryParse(Console.ReadLine(), out modOption) && modOption >= 0 && modOption <= 1)
                            {
                                switch (modOption)
                                {
                                    case 0:
                                        Console.Write($"How many employees for {currentResult.ChosenBuilding.Building.Name}?: ");
                                        if (int.TryParse(Console.ReadLine(), out curNumEmp) && curNumEmp >= 0 && curNumEmp <= currentResult.ChosenBuilding.Building.MaxWorkers)
                                        {
                                            currentResult.ChosenBuilding.Building.CurNumEmp = curNumEmp;
                                        } else { Console.WriteLine("Invalid input"); continue; }
                                        foreach (var item in currentResult.SubChains)
                                        {
                                            if (item.ChosenBuilding != null)
                                            {
                                                Console.Write($"How many employees for {item.ChosenBuilding.Building.Name}?: ");
                                                if (int.TryParse(Console.ReadLine(), out curNumEmp) && curNumEmp >= 0 && curNumEmp <= item.ChosenBuilding.Building.MaxWorkers)
                                                {
                                                    item.ChosenBuilding.Building.CurNumEmp = curNumEmp;
                                                }
                                                else { Console.WriteLine("The Number of workers are either too big or minus."); continue; }
                                            }
                                        }
                                        break;
                                    case 1:
                                        Console.WriteLine("Not available at the moment.");
                                        break;
                                }
                            }
                            else { Console.WriteLine("Invalid input"); continue; }
                        }
                        else { Console.WriteLine("Invalid input"); continue; }
                    }
                    else if (input == "a")
                    {
                        Console.WriteLine("  'newplan'   - Resource-target mode (Default City)");
                        Console.WriteLine("  'buildplan' - Building-count mode (Default City)");
                        List<string> planInputs = new List<string> { "createcity", "switchcity", "newplan", "buildplan", "navigate", "view", "done" };
                        string planInput = ReadLineWithCompletion(planInputs).ToLower().Trim();

                        if (planInput == "newplan") { CreateNewPlan(); }
                        else if (planInput == "buildplan") { CreateBuildPlan(); }
                        else { Console.WriteLine("Invalid command. Try 'newplan' or 'buildplan'"); }
                    }
                    else { Console.WriteLine("invalid input."); continue; } // Return to commands

                    continue;
                }
                else if (command == "listdistrict")
                {
                    City currentCity = city;
                    if (currentCity.microDistricts.Count() == 0) { Console.WriteLine("No plans created yet."); continue; }

                    Console.WriteLine("\n=== All MicroDistricts ===");
                    for (int i = 0; i < currentCity.microDistricts.Count(); i++)
                    {
                        string marker = "← ACTIVE";
                        Console.WriteLine($"{i}. {currentCity.microDistricts[i].Name}" +
                            $"{(currentCity.microDistricts[i] == microDistrict ? marker : null)}");
                    }
                    Console.WriteLine();
                    Console.Write("[d] Delete a District  [Index] Display District Info  [Enter] Return to commands\n> ");
                    string input = Console.ReadLine()?.Trim().ToLower();
                    int dChoice;

                    // Plan deletion
                    if (input == "d")
                    {
                        Console.WriteLine();
                        int delChoice;
                        Console.Write($"Which District to delete? [0-" + (currentCity.microDistricts.Count() - 1) + "]: ");
                        if (int.TryParse(Console.ReadLine(), out delChoice) && delChoice >= 0 && delChoice < currentCity.microDistricts.Count())
                        {
                            // Confirm deletion
                            Console.WriteLine($"Delete '{currentCity.microDistricts[delChoice].Name}? [y/n]");
                            if (Console.ReadKey().KeyChar == 'y')
                            {
                                Console.WriteLine();
                                currentCity.microDistricts.RemoveAt(delChoice);
                                if (delChoice < currentPlanIndex) currentPlanIndex--;   // Deleted before active plan → shift index down
                                else if (delChoice == currentPlanIndex)
                                {
                                    // Deleted the active plan → pick new active
                                    if (currentCity.microDistricts.Count > 0)
                                        currentPlanIndex = Math.Min(delChoice, currentCity.microDistricts.Count - 1);
                                    else
                                        currentPlanIndex = -1;  // No plans left
                                }   // else: deleted after active plan, no change needed

                                // Update currentResult
                                if (currentPlanIndex >= 0 && currentCity.microDistricts.Count > 0) microDistrict = currentCity.microDistricts[currentPlanIndex];
                                else microDistrict = null;

                                // Display confirmation
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("MicroDistrict deleted!");
                                Console.ResetColor();
                            }
                            else continue;
                        }
                        else { Console.WriteLine("Invalid input"); continue; }
                    }
                    // Display District
                    else if (int.TryParse(input, out dChoice) && dChoice >= 0 && dChoice < currentCity.microDistricts.Count)
                    {
                        microDistrict = currentCity.microDistricts[dChoice];
                        Console.WriteLine($"Now You're in {microDistrict.Name} of City {microDistrict.ParentCity.Name}");
                        currentCity.microDistricts[dChoice].Display();
                    }
                    else { Console.WriteLine("invalid input."); continue; } // Return to commands
                    continue;
                }
                else if (command == "switchcity")
                {
                    if (allCities.Count <= 1) { Console.WriteLine("Only one City exists. Nothing to switch to."); continue; }

                    // Display plan list
                    Console.WriteLine("\n=== All Cities ===");
                    for (int i = 0; i < allCities.Count(); i++)
                    {
                        string marker = "← ACTIVE";
                        Console.WriteLine($"{i}. {allCities[i].Name}" +
                            $"{(allCities[i] == city ? marker : null)}");
                    }

                    // Choose Plan to switch
                    Console.Write("Choose the city: ");
                    int cityChoice;
                    if (int.TryParse(Console.ReadLine(), out cityChoice) && cityChoice >= 0 && cityChoice < allCities.Count())
                    {
                        city = allCities[cityChoice];
                        allPlans = city.industryPlans;
                        allMicroDistricts = city.microDistricts;
                        if (allPlans.Count < 1) currentResult = new IndustryPlan();
                        else currentResult = allPlans[0];
                        if (allMicroDistricts.Count < 1) microDistrict = new MicroDistrict();
                        else microDistrict = allMicroDistricts[0];
                        Console.WriteLine($"Switched to city '{city.Name}'!");
                    }
                    else { Console.Write("Invalid city choice. "); continue; }
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
                else if (command == "switchdistrict")
                {
                    if (allMicroDistricts.Count <= 1) { Console.WriteLine("Only one plan exists. Nothing to switch to."); continue; }

                    // Display plan list
                    Console.WriteLine("\n=== All MicroDistricts ===");
                    for (int i = 0; i < allMicroDistricts.Count(); i++)
                    {
                        string marker = "← ACTIVE";
                        Console.WriteLine($"{i}. {allMicroDistricts[i].Name}" +
                            $"{(allMicroDistricts[i] == microDistrict ? marker : null)}");
                    }

                    // Choose Plan to switch
                    Console.Write("Choose the MicroDistrict to switch: ");
                    int planChoice;
                    if (int.TryParse(Console.ReadLine(), out planChoice) && planChoice >= 0 && planChoice < allMicroDistricts.Count())
                    {
                        currentPlanIndex = planChoice;
                        microDistrict = allMicroDistricts[currentPlanIndex];
                        navigationStack.Clear();
                        Console.WriteLine($"Switched to: {microDistrict.Name}");
                    }
                    else { Console.Write("Invalid input. "); continue; }
                }
                else if (command == "masterplan")
                {
                    city.industryPlans = allPlans;

                    if (city.industryPlans.Count == 0) { Console.WriteLine("No industry plans created yet."); continue; }
                    else { city.Display(); }
                    continue;
                }
                else if (command == "newcity")
                {
                    City c = new City();
                    Console.Write($"Type a name of the City: ");
                    string cname = Console.ReadLine();
                    if (cname == null)
                    {
                        Console.WriteLine("Invalid City name.");
                        continue;
                    }
                    c.Name = cname;
                    city = c;
                    allPlans = city.industryPlans;
                    allMicroDistricts = city.microDistricts;
                    allCities.Add(c);
                    Console.WriteLine($"City created!\nName: {city.Name}\nPlans: {city.industryPlans.Count}\nResidentials: {city.microDistricts.Count}");
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

                    Console.Write($"On which level will you save Support Infrastructure?\n[0]: {city.Name}\n[1]: {currentResult.ChosenBuilding.Building.Name}\n:");
                    choiceIndex = int.TryParse(Console.ReadLine(), out choiceIndex) ? choiceIndex : -1;

                    // Add to City.SupportBuildings
                    if (choiceIndex == 0)
                    {
                        foreach (var cb in chosenSupports)
                        {
                            SupportInstance addSupBuilding = new SupportInstance()
                            {
                                Building = cb.Key,
                                Count = cb.Value
                            };
                            city.SupportBuildings.Add(addSupBuilding);
                            Console.WriteLine($"{cb.Key.Name}: {cb.Value} added to the {city.Name}!");
                        }
                    }
                    // Add to IndustryPlan.SupportBuildings
                    else if (choiceIndex == 1)
                    {
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
                    }
                    else { Console.WriteLine("Invalid input."); continue; }
                    continue;
                }
                else if (command == "amenity")
                {
                    // Designate the City
                    City currentCity = allCities.FirstOrDefault(c => c.Name == city.Name);
                    if (currentCity == null)
                    {
                        Console.WriteLine($"There's no City chosen.");
                        continue;
                    }

                    // Designate MicroDistrict 
                    if (currentCity.microDistricts.Count > 0)
                    {
                        // Display MicroDistricts
                        Console.WriteLine("Choose a District(Or press '-1' to create an amenity on City Level): ");
                        for (int i = 0; i < currentCity.microDistricts.Count; i++)
                        {
                            Console.WriteLine($"[{i}]: {currentCity.microDistricts[i].Name}");
                        }
                        Console.Write(": ");

                        // Choose District
                        int dChoice;
                        if (int.TryParse(Console.ReadLine(), out dChoice) && dChoice >= 0 && dChoice < currentCity.microDistricts.Count)
                        {
                            microDistrict = currentCity.microDistricts[dChoice];
                            Console.WriteLine($"Current District : {microDistrict.Name}");
                            Console.WriteLine($"Amenities: ");

                            // Display existing Amenities
                            if (microDistrict.AmenityBuildings.Count > 0)
                            {
                                for (int i2 = 0; i2 < microDistrict.AmenityBuildings.Count; i2++)
                                {
                                    Console.WriteLine($"[{i2}]: {microDistrict.AmenityBuildings[i2].Count} x {microDistrict.AmenityBuildings[i2].Building.Name}");
                                }
                                Console.WriteLine("Choose an action [d (delete) / m (modify) / b (back) / a (add)]: ");

                                InstanceActionChoices(currentCity, microDistrict.AmenityBuildings, dChoice);
                            }
                            // No AmenityInstances exist.
                            else 
                            { 
                                Console.WriteLine($"· None");
                                // Create a new AmenityInstance
                                AddAmenityInstance(currentCity, dChoice);
                                continue;
                            }
                        }
                        // Amenities on City Level
                        else if (dChoice == -1)
                        {
                            // Display existing City Amenities
                            Console.WriteLine($"Current City : {currentCity.Name}");
                            Console.WriteLine($"Amenities: ");

                            // Display existing Amenities
                            if (currentCity.CityAmenityBuildings.Count > 0)
                            {
                                for (int i2 = 0; i2 < currentCity.CityAmenityBuildings.Count; i2++)
                                {
                                    Console.WriteLine($"[{i2}]: {currentCity.CityAmenityBuildings[i2].Count} x {currentCity.CityAmenityBuildings[i2].Building.Name}");
                                }
                                Console.WriteLine("Choose an action [d (delete) / m (modify) / b (back) / a (add)]: ");

                                InstanceActionChoices(currentCity, currentCity.CityAmenityBuildings, -1);
                            }
                            // No AmenityInstances exist.
                            else
                            {
                                Console.WriteLine("No City Amenity exists.");
                                AddAmenityInstance(currentCity, dChoice);
                            }
                        }
                        else { Console.WriteLine("Invalid Input. Going back to Command Loop."); continue; }
                    }
                    // No MicroDistrict existing
                    else
                    {
                        Console.WriteLine("There's no existing MicroDistrict. Create a MicroDistrict First.");
                        continue;
                    }
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
                    Console.Write("Which one to dive into? (press 9 to go back): [1]SubChain: ");
                    if (int.TryParse(Console.ReadLine(), out buildChoice) && buildChoice >= 0 && buildChoice <= 1)
                    {
                        if (buildChoice == 1)
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
                    Console.Write("Which one to cancel? (press 9 to go back):\n[1]SubChain \n[2]Support Buildings \n[3]Residential Buildings \n[4]Amenity Buildings \n[5]Transportation Buildings: ");
                    if (int.TryParse(Console.ReadLine(), out buildChoice) && buildChoice >= 0 && buildChoice <= 5)
                    {
                        if (buildChoice == 1)
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
                else if (command == "research")
                {
                    Console.Write("Have you unlocked a new tech?: ");
                    List<string> techs = new List<string>() 
                    {
                        GameData.TechNames.ConcreteUtilizing,
                        GameData.TechNames.PrefabPanelConstruction,
                        GameData.TechNames.DurablePrefab,
                        GameData.TechNames.HighQualityLiving
                    };
                    string unlockedTech = ReadLineWithCompletion(techs);
                    TechNode matchedTech = GameData.AllTechNodes.FirstOrDefault(t => t.Name ==  unlockedTech);

                    if (matchedTech == null) { Console.WriteLine("No such Research Name exists in the AllTechNodes."); continue; }
                    else if (CalculationEngine.CanResearch(matchedTech, CalculationSettings.UnlockedTech, CalculationSettings.CurrentYear))
                    {
                        CalculationSettings.UnlockedTech.Add(matchedTech.Name);
                        Console.WriteLine($"Tech Unlocked! : {matchedTech.Name}");
                    }
                    else 
                    {
                        Console.WriteLine("Cannot research this yet. Check year and prerequisites.");
                        if (CalculationSettings.CurrentYear < matchedTech.UnlockYear)
                            Console.WriteLine($"  Requires year {matchedTech.UnlockYear} (current: {CalculationSettings.CurrentYear})");
                        var missing = matchedTech.Prerequisites.Where(p => !CalculationSettings.UnlockedTech.Contains(p));
                        if (missing.Any())
                            Console.WriteLine($"  Missing prerequisites: {string.Join(", ", missing)}");
                        continue;
                    }
                }
                else if (command == "year")
                {
                    Console.Write("Type the year to change: ");
                    int year;

                    if (int.TryParse(Console.ReadLine(), out year) && year >= 1920 && year <= 2500)
                    {
                        CalculationSettings.CurrentYear = year;
                        Console.WriteLine($"Current year Updated! : {year}");
                    } else { Console.WriteLine("Invalid Year figure. (1920 ~ 2500)"); continue; }
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
                    // Designate the City
                    City currentCity = allCities.FirstOrDefault(c => c.Name == city.Name);
                    if (currentCity == null)
                    {
                        Console.WriteLine($"There's no City chosen.");
                        continue;
                    }

                    // Select a district
                    if (currentCity.microDistricts.Count < 1)
                    {   // if none, create a district
                        microDistrict = new MicroDistrict();
                        string mdName;
                        Console.Write($"Write a name of the district: ");
                        mdName = Console.ReadLine();
                        microDistrict.Name = mdName;
                        microDistrict.ParentCity = currentCity;
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

                            // User Choice : Modify || Delete MicroDistrict
                            Console.Write($"Which action do you want to do on this Plan? (m : modify | d : delete): ");
                            char actionChoice;

                            if (char.TryParse(Console.ReadLine(), out actionChoice) && (actionChoice == 'm' || actionChoice == 'd'))
                            {
                                // Deletion
                                if (actionChoice == 'd')
                                {
                                    Console.WriteLine($"Do you want to delete this District Plan ({microDistrict.Name})? [y/n]");
                                    char deleteChoice;
                                    if (char.TryParse(Console.ReadLine(), out deleteChoice) && (deleteChoice == 'y' || deleteChoice == 'n'))
                                    {
                                        if (deleteChoice == 'y')
                                        {
                                            Console.WriteLine($"{microDistrict.Name} has been deleted.");
                                            allMicroDistricts.RemoveAt(allMicroDistricts.IndexOf(microDistrict));
                                            continue;
                                        }
                                        else
                                        {
                                            Console.WriteLine("Going back to CommandLoop");
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Invalid input. Going back to CommandLoop");
                                        continue;
                                    }
                                }
                                // Modification
                                else if (actionChoice == 'm')
                                {
                                    // User Choice : Add || Delete Instances
                                    Console.Write($"Modification : Which action do you want to do on this Instances? (a : add | d : delete): ");
                                    char actionChoice2;

                                    if (char.TryParse(Console.ReadLine(), out actionChoice2) && (actionChoice2 == 'a' || actionChoice2 == 'd'))
                                    {
                                        // Deletion
                                        if (actionChoice2 == 'd')
                                        {
                                            // Display List
                                            for (int i = 0; i < microDistrict.ResidentialBuildings.Count + microDistrict.AmenityBuildings.Count; i++)
                                            {
                                                // AmenityInstance List
                                                if (i == microDistrict.ResidentialBuildings.Count)
                                                {
                                                    Console.WriteLine($"[{i}]: {microDistrict.AmenityBuildings[i - microDistrict.ResidentialBuildings.Count].Count} " +
                                                        $"x {microDistrict.AmenityBuildings[i - microDistrict.ResidentialBuildings.Count].Building.Name}");
                                                }
                                                // ResidentailInstance List
                                                else
                                                {
                                                    Console.WriteLine($"[{i}]: {microDistrict.ResidentialBuildings[i].Count} x {microDistrict.ResidentialBuildings[i].Building.Name}");
                                                }
                                            }
                                            Console.Write(": ");

                                            // Choose Instance
                                            int instanceChoice;
                                            if (int.TryParse(Console.ReadLine(), out instanceChoice) && instanceChoice >= 0 && instanceChoice < microDistrict.ResidentialBuildings.Count + microDistrict.AmenityBuildings.Count)
                                            {
                                                // AmenityInstance
                                                if (instanceChoice == microDistrict.ResidentialBuildings.Count)
                                                {
                                                    // Confirm 
                                                    char deleteChoice2;
                                                    Console.WriteLine($"Do you want to delete this AmenityInstance ({microDistrict.AmenityBuildings[instanceChoice - microDistrict.ResidentialBuildings.Count].Building.Name})? [y/n]");
                                                    if (char.TryParse(Console.ReadLine(), out deleteChoice2) && (deleteChoice2 == 'y' || deleteChoice2 == 'n'))
                                                    {
                                                        if (deleteChoice2 == 'y')
                                                        {
                                                            Console.WriteLine($"{microDistrict.AmenityBuildings[instanceChoice - microDistrict.ResidentialBuildings.Count].Building.Name} has been deleted.");
                                                            microDistrict.AmenityBuildings.RemoveAt(instanceChoice - microDistrict.ResidentialBuildings.Count);
                                                            continue;
                                                        }
                                                        else
                                                        {
                                                            Console.WriteLine("Deletion Canceled. Going back to CommandLoop");
                                                            continue;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("Invalid input. Going back to CommandLoop");
                                                        continue;
                                                    }
                                                }
                                                // ResidentailInstance
                                                else
                                                {
                                                    // Confirm 
                                                    char deleteChoice2;
                                                    Console.WriteLine($"Do you want to delete this ResidentialInstance ({microDistrict.ResidentialBuildings[instanceChoice].Building.Name})? [y/n]");
                                                    if (char.TryParse(Console.ReadLine(), out deleteChoice2) && (deleteChoice2 == 'y' || deleteChoice2 == 'n'))
                                                    {
                                                        if (deleteChoice2 == 'y')
                                                        {
                                                            Console.WriteLine($"{microDistrict.ResidentialBuildings[instanceChoice].Building.Name} has been deleted.");
                                                            microDistrict.ResidentialBuildings.RemoveAt(instanceChoice);
                                                            continue;
                                                        }
                                                        else
                                                        {
                                                            Console.WriteLine("Deletion Canceled. Going back to CommandLoop");
                                                            continue;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("Invalid input. Going back to CommandLoop");
                                                        continue;
                                                    }
                                                }
                                            }
                                            else { Console.WriteLine("Invalid input. Going back to Command Loop."); continue; }
                                        }
                                        // Addition 
                                        else if (actionChoice2 == 'a')
                                        {
                                            AddResidentialInstance(city, microDistrict);
                                        }
                                    }
                                    continue;
                                }
                                else
                                {
                                    Console.WriteLine("Invalid input. Going back to CommandLoop");
                                    continue;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Invalid input. Going back to CommandLoop");
                                continue;
                            }
                        }
                        else if (userChoice == -1)
                        {
                            MicroDistrict md = new MicroDistrict();
                            string mdName;
                            Console.Write($"Write a name of the district: ");
                            mdName = Console.ReadLine();
                            microDistrict.Name = mdName;
                            microDistrict.ParentCity = currentCity;
                            allMicroDistricts.Add(microDistrict);
                        }
                    }
                    AddResidentialInstance(city, microDistrict);
                    continue;   // back to 'command'
                }
            }
        }
        static void AddResidentialInstance(City c, MicroDistrict md)
        {
            // Calculate total citizens from ALL plans
            int totalWorkers = c.totalWorkers;
            int totalHousingCapacity = c.microDistricts.Sum(m => m.TotalHousingCapacity);
            int neededHousing = totalWorkers - totalHousingCapacity;
            Console.WriteLine($"\nYou need extra housing for {neededHousing} workers.");
            bool allValid = false;
            while (!allValid)
            {
                // User choose Residential Size
                int sizeChoice;
                Console.Write($"\nChoose the Size of Residential ([3]: Optimization):\n[0]: Small\n[1]: Medium\n[2]: Large\n: ");
                if (int.TryParse(Console.ReadLine(), out sizeChoice) && sizeChoice >= 0 && sizeChoice <= 3)
                {
                    // Small
                    int userChoice;
                    int count;
                    if (sizeChoice == 0)
                    {
                        Dictionary<int, ResidentialBuilding> resList = new Dictionary<int, ResidentialBuilding>();

                        // Display Buildings
                        Console.WriteLine($"Small Residentials:");
                        List<ResidentialBuilding> availableBuildings = GameData.SmallResidentialBuildings.Where(b => CalculationEngine.CanBuildResidential(b)).OrderBy(b => b.Name).ToList();
                        for (int i = 0; i < availableBuildings.Count(); i++)
                        {
                            Console.WriteLine($"[{i + 1}]: {availableBuildings[i].WorkerCapacity}, {availableBuildings[i].Name}");
                            resList.Add(i + 1, availableBuildings[i]);
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
                        md.ResidentialBuildings.AddRange(resCount);

                        // Show the Capacity
                        Console.WriteLine($"\nCurrent capacity: {c.totalHousingCapacity}" +
                            $"\nExtra capcity needed: {((c.totalWorkers - c.totalHousingCapacity) >= 0
                            ? (c.totalWorkers - c.totalHousingCapacity)
                            : c.totalWorkers - c.totalHousingCapacity)}");

                        allValid = true;
                    }
                    // Medium
                    else if (sizeChoice == 1)
                    {
                        Dictionary<int, ResidentialBuilding> resList = new Dictionary<int, ResidentialBuilding>();

                        // Display Buildings
                        Console.WriteLine($"Medium Residentials:");
                        List<ResidentialBuilding> availableBuildings = GameData.MediumResidentialBuildings.Where(b => CalculationEngine.CanBuildResidential(b)).OrderBy(b => b.Name).ToList();
                        for (int i = 0; i < availableBuildings.Count(); i++)
                        {
                            Console.WriteLine($"[{i + 1}]: {availableBuildings[i].WorkerCapacity}, {availableBuildings[i].Name}");
                            resList.Add(i + 1, availableBuildings[i]);
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
                        md.ResidentialBuildings.AddRange(resCount);

                        // Show the Capacity
                        Console.WriteLine($"\nCurrent capacity: {c.totalHousingCapacity}" +
                            $"\nExtra capcity needed: {((c.totalWorkers - c.totalHousingCapacity) >= 0
                            ? (c.totalWorkers - c.totalHousingCapacity)
                            : Math.Abs(c.totalWorkers - c.totalHousingCapacity))}");

                        allValid = true;
                    }
                    // Large
                    else if (sizeChoice == 2)
                    {
                        Dictionary<int, ResidentialBuilding> resList = new Dictionary<int, ResidentialBuilding>();

                        // Display Buildings
                        Console.WriteLine($"Large Residentials:");
                        List<ResidentialBuilding> availableBuildings = GameData.LargeResidentialBuildings.Where(b => CalculationEngine.CanBuildResidential(b)).OrderBy(b => b.Name).ToList();
                        for (int i = 0; i < availableBuildings.Count(); i++)
                        {
                            Console.WriteLine($"[{i + 1}]: {availableBuildings[i].WorkerCapacity}, {availableBuildings[i].Name}");
                            resList.Add(i + 1, availableBuildings[i]);
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
                        md.ResidentialBuildings.AddRange(resCount);

                        // Show the Capacity
                        Console.WriteLine($"\nCurrent capacity: {c.totalHousingCapacity}" +
                            $"\nExtra capcity needed: {((c.totalWorkers - c.totalHousingCapacity) >= 0
                            ? (c.totalWorkers - c.totalHousingCapacity)
                            : c.totalWorkers - c.totalHousingCapacity)}");

                        allValid = true;
                    }
                    // [3]: Pareto-Front Algorithm(Optimization)
                    else
                    {
                        // Display
                        Func<ResidentialBuilding, double> priorityKey = null;
                        int priorChoice;
                        Console.WriteLine("\nChoose the priority to optimize: \n[0]: Balanced (utility cost)" +
                            "\n[1]: Density (WorkersPerArea)\n[2]: Quality\n[3]: Utility Per Worker\n[4]: Construction Cost\n[5]: Construction Period\n[6]: Composed Weights");
                        if (int.TryParse(Console.ReadLine(), out priorChoice) && priorChoice >= 0 && priorChoice <= 6)
                        {
                            switch (priorChoice)
                            {
                                case 0:
                                    priorityKey = b => 1 - CalculationEngine.UtilityCalculator.NormalizedTotalUtilityCostPerWorker(b);
                                    break;
                                case 1:
                                    priorityKey = b => CalculationEngine.NormalizedWorkersPerArea(b);
                                    break;
                                case 2:
                                    priorityKey = b => b.Quality / 100;
                                    break;
                                case 3:
                                    Console.WriteLine($"\nChoose the Utility to optimize: \n[0]: Power\n[1]: Water\n[2]: Heat\n[3]: Garbage");
                                    int utilChoice;
                                    if (int.TryParse(Console.ReadLine(), out utilChoice) && utilChoice >= 0 && utilChoice <= 3)
                                    {
                                        switch (utilChoice)
                                        {
                                            case 0:
                                                priorityKey = b => 1 - CalculationEngine.UtilityCalculator.NormalizedUtilityCostPerWorker(b, UtilityType.Power);
                                                break;
                                            case 1:
                                                priorityKey = b => 1 - CalculationEngine.UtilityCalculator.NormalizedUtilityCostPerWorker(b, UtilityType.Water);
                                                break;
                                            case 2:
                                                priorityKey = b => 1 - CalculationEngine.UtilityCalculator.NormalizedUtilityCostPerWorker(b, UtilityType.Heat);
                                                break;
                                            case 3:
                                                priorityKey = b => 1 - CalculationEngine.UtilityCalculator.NormalizedUtilityCostPerWorker(b, UtilityType.Garbage);
                                                break;
                                        }
                                    }
                                    break;
                                case 4:
                                    priorityKey = b => 1 - (CalculationEngine.NormalizedCostPerWorker(b));
                                    break;
                                case 5:
                                    priorityKey = b => 1 - (CalculationEngine.NormalizedWorkDaysPerWorker(b));
                                    break;
                                case 6: // Custom Composite
                                    Console.WriteLine("\nAvailable priority keys:");
                                    for (int i = 0; i < PriorityKeys.All.Count; i++)
                                        Console.WriteLine($"[{i}]: {PriorityKeys.All[i].Name}");

                                    Console.Write("\nSelect keys to combine (comma-separated indices): ");
                                    var selectedIndices = Console.ReadLine().Split(',').Select(s => int.Parse(s.Trim())).ToList();

                                    var components = new List<(Func<ResidentialBuilding, double> Key, double Weight)>();
                                    foreach (int idx in selectedIndices)
                                    {
                                        var (name, key) = PriorityKeys.All[idx];
                                        Console.Write($"Weight for {name} [default {1.0 / selectedIndices.Count:0.00}]: ");
                                        string input = Console.ReadLine();
                                        double weight = string.IsNullOrWhiteSpace(input)
                                            ? 1.0 / selectedIndices.Count
                                            : double.Parse(input);
                                        components.Add((key, weight));
                                    }

                                    priorityKey = CalculationEngine.ComposeWeighted(components.ToArray());
                                    break;
                            }
                        } else { Console.WriteLine("Invalid Priority option."); continue; }

                        List<ResidentialBuilding> availableBuildings = GameData.AllResidentialBuildings.Where(b => CalculationEngine.CanBuildResidential(b)).ToList();
                        List<ResidentialBuilding> paretoFront = CalculationEngine.GetParetoFrontResidential(availableBuildings);

                        // Debug
                        Console.WriteLine($"Pareto front size: {paretoFront.Count}");
                        foreach (var b in paretoFront)
                            Console.WriteLine($"  {b.Name}");

                        foreach (var b in paretoFront)
                        {
                            double d = CalculationEngine.NormalizedWorkersPerArea(b);
                            double co = 1 - CalculationEngine.NormalizedCostPerWorker(b);
                            double u = 1 - CalculationEngine.UtilityCalculator.NormalizedTotalUtilityCostPerWorker(b);
                            Console.WriteLine($"{b.Name}: Density={d:0.00}, Cost={co:0.00}, Utility={u:0.00}");
                        }

                        // Greedy Allocation
                        Dictionary<ResidentialBuilding, int> allocated = CalculationEngine.AllocateHousing(neededHousing, paretoFront, priorityKey);
                        Console.WriteLine($"\nOptimized Residential: ");
                        foreach (var b in  allocated)
                        {
                            Console.WriteLine($"[{b.Key.Name}]: {b.Value}");
                        }

                        allValid = true;
                    }
                }
                else { Console.Write("Invalid Input. "); continue; }
            }
        }
        static void AddAmenityInstance(City c, int index)
        {
            MicroDistrict md = null;

            if (index == -1) { }
            else { md = c.microDistricts[index]; }

            AmenityInstance amenityInstance = new AmenityInstance();

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
                if (amenChoice == 0) { return; }

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

                // Get current district coverage
                var coverage = md != null
                    ? md.GetAmenityCoverage()
                    : null;
                int currentCapacity = md != null
                    ? coverage.ServiceCoverage[selectedType]
                    : 0;

                // Get current city coverage
                var cityCoverage = c.GetAmenityCoverage();
                int cityCurrentCapacity = cityCoverage.ServiceCoverage[selectedType];

                // Calculate needed capacity
                var (neededCapacity, cityPopulationDesc, showCapacity) = CalculateCapacityNeeded(selectedType, c);

                // Display capacity info
                if (showCapacity)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"City Population: {cityPopulationDesc}");
                    if (index != -1) Console.WriteLine($"District Maximum Population: {md.TotalHousingCapacity}");
                    else { };
                    Console.ResetColor();

                    // Special handling for Education
                    if (selectedType == AmenityType.Education)
                    {
                        if (index != -1) EduAmenCapacityDisplay(c, coverage);
                        else EduAmenCapacityDisplay(c, cityCoverage);
                    }
                    // Standard capacity display
                    else
                    {
                        if (index != -1) AmenCapacityDisplay(currentCapacity, md.TotalHousingCapacity);
                        else AmenCapacityDisplay(cityCurrentCapacity, neededCapacity);
                    }
                    Console.WriteLine();
                }

                // Filter buildings by type
                var buildingsOfType = GameData.AllAmenityBuildings.Where(b => b.Type == selectedType).ToList();

                // Display buildings
                for (int i = 0; i < buildingsOfType.Count; i++)
                    Console.WriteLine($"[{i + 1}] (Max Coverage: {buildingsOfType[i].MaxCoverage}) {buildingsOfType[i].Name}");
                Console.Write("[0] Back\n: ");
                int buildChoice;

                // Choose Building
                if (int.TryParse(Console.ReadLine(), out buildChoice) && buildChoice >= 0 && buildChoice <= buildingsOfType.Count())
                {
                    if (buildChoice == 0) { return; }
                    if (selectedType == AmenityType.Education)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        if (index == -1)
                        {
                            int kindergartenNeeded = (int)Math.Ceiling(city.totalCitizen * CalculationSettings.KindergartenAgePercent / 100);
                            int schoolNeeded = (int)Math.Ceiling(city.totalCitizen * CalculationSettings.SchoolAgePercent / 100);
                            Console.WriteLine($"Kindergarten: {cityCoverage.KindergartenCapacity}/{kindergartenNeeded}");
                            Console.WriteLine($"School: {cityCoverage.SchoolCapacity}/{schoolNeeded}");
                        }
                        // This needs to be parceled out according to the District numbers.
                        else
                        {
                            int kindergartenNeeded = (int)Math.Ceiling(md.TotalCitizen * CalculationSettings.KindergartenAgePercent / 100);
                            int schoolNeeded = (int)Math.Ceiling(md.TotalCitizen * CalculationSettings.SchoolAgePercent / 100);
                            Console.WriteLine($"Kindergarten: {coverage.KindergartenCapacity}/{kindergartenNeeded}");
                            Console.WriteLine($"School: {coverage.SchoolCapacity}/{schoolNeeded}");
                        }
                        Console.ResetColor();
                    }
                    else if (selectedType == AmenityType.Healthcare)
                    {
                        if (index == -1)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"{city.totalCitizen} citizens need to be served.");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"{md.TotalCitizen} citizens need to be served.");
                            Console.ResetColor();
                        }
                    }
                    else
                    {
                        if (index == -1)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"{city.totalWorkers} workers need to be served.");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"{md.AmenityWorkers} workers need to be served.");
                            Console.ResetColor();
                        }
                    }

                    // Decide Current Number of Employees
                    Console.Write($"How many workers in the {buildingsOfType[buildChoice - 1].Name}(0 - {buildingsOfType[buildChoice - 1].MaxWorkers})?: ");
                    int curNumEmp;
                    if (int.TryParse(Console.ReadLine(), out curNumEmp) && curNumEmp >= 0 && curNumEmp <= buildingsOfType[buildChoice - 1].MaxWorkers)
                    {
                        amenityInstance.Building = buildingsOfType[buildChoice - 1];
                        amenityInstance.CurNumEmp = curNumEmp;
                        Console.WriteLine($"Current Number of Employees : {amenityInstance.CurNumEmp} [Capacity: {amenityInstance.CurCustomCapacity} / Coverage: {amenityInstance.CurCoverage}]");
                    } else { Console.WriteLine($"The number of Workers should be within the range Max workers of the building.");}

                    Console.Write("How many?: ");
                    int count;

                    // Decide amount of the Building
                    if (int.TryParse(Console.ReadLine(), out count) && count > 0)
                    {
                        amenityInstance.Count = count;

                        // Add AmenityInstance to District Level
                        if (index >= 0)
                        {
                            // (Auto) Name the Instance
                            if (md.AmenityBuildings.Any(ins => ins.Name == amenityInstance.Name))
                            {
                                int number = 1;
                                int sameBuildingCount = md.AmenityBuildings.Select(ins2 => ins2.Name == amenityInstance.Name).Count();
                                amenityInstance.Name = amenityInstance.Building.Name + $" ({number + sameBuildingCount})";
                            }
                            else amenityInstance.Name = amenityInstance.Building.Name;

                            md.AmenityBuildings.Add(amenityInstance);
                            Console.WriteLine($"{amenityInstance.Building.Name} x {count} has been added to District {md.Name} of City {md.ParentCity.Name}!");
                            Console.WriteLine($"Population Coverage: {amenityInstance.CurCoverage * count} / {amenityInstance.Building.MaxCoverage * count}");
                            Console.WriteLine($"Optimized Max Workers: {amenityInstance.Building.EffectiveWorkersPerShift}");
                        }
                        // Add AmenityInstance to City Level
                        else
                        {
                            // (Auto) Name the Instance
                            if (c.CityAmenityBuildings.Any(ins => ins.Building.Name == amenityInstance.Building.Name))
                            {
                                int number = 1;
                                int sameBuildingCount = c.CityAmenityBuildings.Where(ins2 => ins2.Name == amenityInstance.Name).Count();
                                amenityInstance.Name = amenityInstance.Building.Name + $" ({number + sameBuildingCount})";
                            }
                            else amenityInstance.Name = amenityInstance.Building.Name;

                            c.CityAmenityBuildings.Add(amenityInstance);
                            Console.WriteLine($"{amenityInstance.Building.Name} x {count} has been added to City {c.Name} directly!");
                            Console.WriteLine($"Population Coverage: {amenityInstance.CurCoverage * count} / {amenityInstance.Building.MaxCoverage * count}");
                            Console.WriteLine($"Optimized Max Workers: {buildingsOfType[buildChoice - 1].EffectiveWorkersPerShift}");
                        }
                    }
                    else { Console.Write("Invalid Input."); }
                }
                else { Console.Write("Invalid Input."); }
            }
            else { Console.Write("Invalid Input(Index)."); }
        }
        static void AddUtilityPlan(City c)
        {
            // Setting Utility Resources to expand
            List<Resource> ur = new List<Resource>();
            UtilityInstance ui = new UtilityInstance();
            UtilityPlan p = new UtilityPlan();

            // Naming UtilityPlan
            Console.Write("Write a name of the Utility Plan. (Defulat name: CityName - buildingName)\n: ");
            while (true)
            {
                string un = Console.ReadLine();

                if (string.IsNullOrEmpty(un)) { continue; }
                else
                {
                    p.Name = un;
                    Console.WriteLine($"Utility Plan: {un}"); break;
                }
            }
            AddUtilityInstance(p);
            c.UtilityPlans.Add(p);
            Console.WriteLine($"{p.Name} has been added to the City {c.Name}.");
        }
        static void AddUtilityInstance(UtilityPlan utilityPlan)
        {
            UtilityInstance ui = new UtilityInstance();

            Console.WriteLine("Type the utility to expand:\n[0]: Power\n[1]: Water\n[2]: Sewage\n[3]: Heat\n[4]: Garbage\n\n");
            List<UtilityBuilding> powerbuildings = GameData.AllUtilityBuildings
                .Where(ub => ub.Outputs.Any(ra => ra.Resource == GameData.PowerResource))
                .ToList();
            List<UtilityBuilding> waterbuildings = GameData.AllUtilityBuildings
                .Where(ub => ub.Outputs.Any(ra => ra.Resource == GameData.WaterResource || ra.Resource == GameData.RawWaterResource))
                .ToList();
            List<UtilityBuilding> sewagebuildings = GameData.AllUtilityBuildings
                .Where(ub => ub.Outputs.Any(ra => ra.Resource == GameData.WasteWaterResource))
                .ToList();
            List<UtilityBuilding> heatbuildings = GameData.AllUtilityBuildings
                .Where(ub => ub.Outputs.Any(ra => ra.Resource == GameData.HeatResource))
                .ToList();
            List<UtilityBuilding> garbagebuildings = GameData.AllUtilityBuildings
                .Where(ub => ub.Outputs.Any(ra => ra.Resource == GameData.MixedWaste))
                .ToList();
            List<List<UtilityBuilding>> utilityBuildings = new List<List<UtilityBuilding>>()
                        {
                            powerbuildings,
                            waterbuildings,
                            sewagebuildings,
                            heatbuildings,
                            garbagebuildings
                        };

            // User Selection & Display
            int utilchoice;
            int bldgchoice;

            if (int.TryParse(Console.ReadLine(), out utilchoice) && utilchoice >= 0 && utilchoice < 5)
            {
                // Display : util category choice
                Console.WriteLine();

                for (int i = 0; i < utilityBuildings[utilchoice].Count; i++)
                {
                    string inputs = string.Join("\n\t", utilityBuildings[utilchoice][i].Inputs.Select(ra => $"{ra.Amount} x {ra.Resource.Name}"));
                    string outputs = string.Join("\n\t", utilityBuildings[utilchoice][i].Outputs.Select(ra => $"{ra.Amount} x {ra.Resource.Name}"));
                    Console.WriteLine($"[{i}]: {utilityBuildings[utilchoice][i].Name} " +
                        $"\n- Max input: {inputs}" +
                        $"\n- Max output: {outputs}\n");
                }

                // Display : util building choice
                Console.Write(": ");

                if (int.TryParse(Console.ReadLine(), out bldgchoice) && bldgchoice >= 0 && bldgchoice < utilityBuildings[utilchoice].Count)
                {
                    ui.Building = utilityBuildings[utilchoice][bldgchoice];

                    Console.Write($"{utilityBuildings[utilchoice][bldgchoice].Name} has been selected.\nHow many workers? [0 - {ui.Building.MaxWorkers}]: ");
                    int curNumEmp;

                    if (int.TryParse(Console.ReadLine(), out curNumEmp) && curNumEmp >= 0 && curNumEmp <= utilityBuildings[utilchoice][bldgchoice].MaxWorkers)
                    {
                        ui.CurNumEmp = curNumEmp;
                    } else { Console.WriteLine($"Invalid Employee number."); return; }

                    Console.Write($"How many buildings?: ");
                    int amount;

                    if (int.TryParse(Console.ReadLine(), out amount) && amount > 0)
                    {
                        ui.Count = amount;
                        Console.WriteLine($"{amount} x {utilityBuildings[utilchoice][bldgchoice]} has been added to the UtilityInstance.");
                    }
                    else { Console.WriteLine("Invalid amount. Going back to the command loop."); return; }

                    utilityPlan.Buildings.Add(ui);
                    Console.WriteLine($"{amount} x {utilityBuildings[utilchoice][bldgchoice]} has been added to the {utilityPlan.Name}.");
                }
                else { Console.WriteLine("Invalid index. Going back to the command loop."); return; }
            }
            else { Console.WriteLine("Invalid index. Going back to the command loop."); return; }
        }
        static void InstanceActionChoices(City c, List<AmenityInstance> amenityInstances, int districtChoice)
        {
            // Amenity Deletion
            char aChoice;
            if (char.TryParse(Console.ReadLine(), out aChoice) && (aChoice == 'd' || aChoice == 'm' ||aChoice == 'b' || aChoice == 'a'))
            {
                if (aChoice == 'd')
                {
                    Console.Write("Choose the Instance to delete: ");
                    int delChoice;
                    if (int.TryParse(Console.ReadLine(), out delChoice) && delChoice >= 0 && delChoice < amenityInstances.Count)
                    {
                        Console.WriteLine($"{amenityInstances[delChoice].Count} x {amenityInstances[delChoice].Building.Name} " +
                            $"has been deleted from the City {c.Name}");
                        amenityInstances.RemoveAt(delChoice);
                        
                    }
                    else Console.WriteLine("Invalid action input. Going back to Command Loop."); 
                }
                else if (aChoice == 'm')
                {
                    Console.Write("Choose the Instance to modify: ");
                    int modChoice;
                    if (int.TryParse(Console.ReadLine(), out modChoice) && modChoice >= 0 && modChoice < amenityInstances.Count)
                    {
                        Console.WriteLine($"{amenityInstances[modChoice].Count} x {amenityInstances[modChoice].Building.Name}" +
                            $"[Coverage: {amenityInstances[modChoice].TotalCurCoverage} / {amenityInstances[modChoice].TotalMaxCoverage} " +
                            $"| Workers: {amenityInstances[modChoice].Building.CurNumEmp * amenityInstances[modChoice].Count}/{amenityInstances[modChoice].Building.MaxWorkers * amenityInstances[modChoice].Count}]");
                        Console.Write("\nHow many current number of employees?: ");
                        int curNumEmp;
                        if (int.TryParse(Console.ReadLine(), out curNumEmp) && curNumEmp >= 0 && curNumEmp <= amenityInstances[modChoice].Building.MaxWorkers)
                        {
                            amenityInstances[modChoice].Building.CurNumEmp = curNumEmp;
                            Console.WriteLine("Current Number of Employees is updated.");
                            Console.WriteLine($"{amenityInstances[modChoice].Count} x {amenityInstances[modChoice].Building.Name}" +
                                $"[Coverage: {amenityInstances[modChoice].TotalCurCoverage} / {amenityInstances[modChoice].TotalMaxCoverage} " +
                                $"| Workers: {amenityInstances[modChoice].Building.CurNumEmp * amenityInstances[modChoice].Count}/{amenityInstances[modChoice].Building.MaxWorkers * amenityInstances[modChoice].Count}]");
                        }
                        else { Console.WriteLine("The number of Employees should be within range of 0 - MaxWorkers.");  }
                    }
                    else { Console.WriteLine("Instance Index is out of range.");  }
                }
                // back
                else if (aChoice == 'b') { Console.WriteLine("Going back to Command Loop.");  }
                // Add new Amenity
                else if (aChoice == 'a') { AddAmenityInstance(c, districtChoice);  }
            }
            else { Console.WriteLine("Invalid action input. Going back to Command Loop.");  }
        }
        static void EduAmenCapacityDisplay(City c, MicroDistrict.AmenityCoverage amenityCoverage)
        {
            int kindergartenNeeded = (int)Math.Ceiling(c.totalCitizen * CalculationSettings.KindergartenAgePercent / 100);
            int schoolNeeded = (int)Math.Ceiling(c.totalCitizen * CalculationSettings.SchoolAgePercent / 100);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Kindergarten: {amenityCoverage.KindergartenCapacity}/{kindergartenNeeded}");

            if (amenityCoverage.KindergartenCapacity < kindergartenNeeded)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"   Deficit: {kindergartenNeeded - amenityCoverage.KindergartenCapacity}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"   Surplus: {amenityCoverage.KindergartenCapacity - kindergartenNeeded}");
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"School: {amenityCoverage.SchoolCapacity}/{schoolNeeded}");

            if (amenityCoverage.SchoolCapacity < schoolNeeded)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"   Deficit: {schoolNeeded - amenityCoverage.SchoolCapacity}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"   Surplus: {amenityCoverage.SchoolCapacity - schoolNeeded}");
            }

            Console.ResetColor();
        }
        static void AmenCapacityDisplay(int currentCapacity, int neededCapacity)
        {
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
    City c)
        {
            int citizens = c.totalCitizen;
            int workers = c.totalWorkers;

            return type switch
            {
                AmenityType.Shopping =>
                    ((int)Math.Ceiling((double)workers), $"{workers} workers", true),
                AmenityType.Pub =>
                    ((int)Math.Ceiling((double)workers), $"{workers} citizens", true),
                AmenityType.Culture =>
                    ((int)Math.Ceiling((double)workers), $"{workers} citizens", true),
                AmenityType.Sports =>
                    ((int)Math.Ceiling((double)workers), $"{workers} citizens", true),
                AmenityType.Healthcare =>
                    ((int)Math.Ceiling((double)citizens), $"{citizens} citizens (optional)", true),
                AmenityType.Education =>
                    (0, "See breakdown below", true),
                AmenityType.CrimeJustice =>
                    ((int)Math.Ceiling((double)workers), $"{workers} citizens (estimate)", true),
                AmenityType.Fireservice =>
                    (0, "Coverage-based (not capacity)", false),
                AmenityType.CityService =>
                    (0, "Utility service (not capacity)", false),
                AmenityType.Fountain =>
                    ((int)Math.Ceiling((double)citizens), $"{citizens} citizens (low priority)", true),
                _ => (0, "Unknown", false)
            };
        }
        static bool ConfirmContinue()
        {
            bool r = false;
            Console.Write($"Continue? [y/n]: ");
            char input = Console.ReadKey().KeyChar;

            if (input == 'y')
            {
                r = true;
            }
            return r;
        }
        static void CreateCity()
        {
            // Create IndustryPlan 
            Console.WriteLine("\n────────────────────────────────────────");
            Console.WriteLine("  'newplan'   - Resource-target mode");
            Console.WriteLine("  'buildplan' - Building-count mode");
            Console.WriteLine("  'done'      - Exit program");
            Console.Write("> ");
            List<string> planInputs = new List<string> {"newplan", "buildplan", "done" };
            string planInput = ReadLineWithCompletion(planInputs).ToLower().Trim();

            IndustryPlan ip = CreateIndustryPlan(planInput);

            // Step 1: Gate
            int industryWorkers = ip.TotalWorkers;
            if (!ConfirmContinue()) return;

            bool converged = false;
            List<AmenityInstance> cityAmenities = null;
            Dictionary<ResidentialBuilding, int> optimizedResidential = null;
            List<MicroDistrict> proposedDistricts = null;
            int loopPass = 0;

            while (!converged)
            {
                // Step 2: Inform
                cityAmenities = RunCityAmenityPass(industryWorkers);
                Console.WriteLine($"City amenity worker delta: {cityAmenities.Sum(a => a.CurNumEmp)}");

                // Step 3: Gate (only first pass), Inform (subsequent passes)
                optimizedResidential = RunHousingOptimization(industryWorkers, cityAmenities);
                DisplayOptimizedResidential(optimizedResidential);
                if (loopPass == 0 && !ConfirmContinue())
                {
                    optimizedResidential = ReenterWeightsAndRetry(); // loops back into weight prompt
                }

                // Step 4: Gate (only first pass)
                proposedDistricts = AllocateDistricts(optimizedResidential); // area-budget bin-packing
                DisplayDistrictPreview(proposedDistricts);
                if (loopPass == 0 && !ConfirmContinue()) return; // discard run entirely

                // Step 5: Inform
                int districtAmenityDelta = RunDistrictAmenityPass(proposedDistricts);
                Console.WriteLine($"District amenity worker delta: {districtAmenityDelta}");

                // Step 6: Inform / loop condition
                converged = CheckConvergence(districtAmenityDelta);
                loopPass++;
            }

            // Step 7: Gate — final commit
            Console.WriteLine("Final city plan ready. Commit? [y/n]");
            if (Console.ReadLine()?.ToLower() == "y")
            {
                CommitCity(proposedDistricts, cityAmenities);
            }
            // else: discard, nothing written
        }
    }
}
