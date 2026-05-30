using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class City
{
    public void Display()
    {
        Console.WriteLine("\n┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓");
        Console.WriteLine("┃         CITY PLAN                      ┃");
        Console.WriteLine("┞━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┦");
        // Display Status section
        Console.WriteLine("├────────────────────────────────────────┤");
        Console.WriteLine("│ Utilities Status:                      │");
        Console.WriteLine("├────────────────────────────────────────┤");
        Console.WriteLine("│                   Needed    Produced  Balance");
        Console.WriteLine($"│ Workers    :    {totalWorkers,6}     {totalHousingCapacity,6}  {totalHousingCapacity - totalWorkers,7}");
        Console.WriteLine($"│ Citizens   :    {totalCitizen,6}     ");
        Console.WriteLine($"│ Power (MW) :    {totalPower,6:F2}    {UtilityProduction[GameData.PowerResource],7:F2} {UtilityProduction[GameData.PowerResource] - totalPower,8:F2}");
        Console.WriteLine($"│ Water (m³) :    {totalWater,6:F2}    {UtilityProduction[GameData.WaterResource],7:F2} {UtilityProduction[GameData.WaterResource] - totalWater,8:F2}");
        Console.WriteLine($"│ Sewage (m³):    {totalWater,6:F2}    {UtilityProduction[GameData.WasteWaterResource],7:F2} {UtilityProduction[GameData.WasteWaterResource] - totalWater,8:F2}");
        Console.WriteLine($"│ Heat (MW)  :    {totalHeat,6:F2}    {UtilityProduction[GameData.HeatResource],7:F2} {UtilityProduction[GameData.HeatResource] - totalHeat,8:F2}");
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
        }
        Console.WriteLine("├────────────────────────────────────────┤");
        Console.WriteLine("│ Planned Industry:                      │");
        Console.WriteLine("├────────────────────────────────────────┤");
        foreach (var plan in industryPlans)
            plan.DisplayAllBuildings(plan, 0);
        if (combinedSupBldgs.Count() > 0)
        {
            Console.WriteLine("├────────────────────────────────────────┤");
            Console.WriteLine("│ Support Infrastructures:               │");
            Console.WriteLine("├────────────────────────────────────────┤");

            var supportByCategory = combinedSupBldgs
                .GroupBy(kv => kv.Key.SupportCategory)
                .OrderBy(g => (int)g.Key);

            foreach (var supGroup in supportByCategory)
            {
                string categoryName = GetSupCategoryDisplayName(supGroup.Key);
                Console.WriteLine($"│ [ {categoryName} ]");

                foreach (var kv in supGroup.OrderBy(x => x.Key.Name))
                    Console.WriteLine($"│ · {kv.Value} × {kv.Key.Name}");
                Console.WriteLine("│");
            }
        }
        if (microDistricts.Any(p => p.ResidentialBuildings.Count > 0))
        {
            Console.WriteLine("├────────────────────────────────────────┤");
            Console.WriteLine("│ MicroDistricts:                 ");
            Console.WriteLine("├────────────────────────────────────────┤");

            foreach (var m in microDistricts)
            {
                Console.WriteLine($"│ [ {m.Name} ]:");
                foreach (var ri in m.ResidentialBuildings)
                    Console.WriteLine($"│ · {ri.Count} × {ri.Building.Name}");
            }
        }
        if (microDistricts.Any(p => p.AmenityBuildings.Count > 0))
        {
            Console.WriteLine("├────────────────────────────────────────┤");
            Console.WriteLine("│ Amenity Buildings:                     │");
            Console.WriteLine("├────────────────────────────────────────┤");

            // Group amenities by AmenityType enum
            var amenitiesByCategory = combinedAmeBldgs
                .GroupBy(kv => kv.Key.Type)
                .OrderBy(g => (int)g.Key); // Order by enum value

            foreach (var categoryGroup in amenitiesByCategory)
            {
                string categoryName = GetCategoryDisplayName(categoryGroup.Key);
                Console.WriteLine($"│ [ {categoryName} ]");

                foreach (var kv in categoryGroup.OrderBy(x => x.Key.Name))
                {
                    Console.WriteLine($"│  · {kv.Value} × {kv.Key.Name}");
                }

                // Add warnings for this category
                DisplayCategoryWarnings(categoryGroup.Key, categoryGroup.ToList(), totalWorkers, totalCitizen);

                Console.WriteLine("│");
            }
        }
        if (industryPlans.Any(p => p.TransportationBuildings.Count > 0) || microDistricts.Any(m => m.TransportBuildings.Count > 0))
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
        Console.WriteLine($"│ [ Industry Resources ]");
        foreach (var kv in net)
            if (kv.Value < 0)
            {
                if (kv.Key == GameData.PowerResource)
                    Console.WriteLine($"│ · {Math.Abs(kv.Value):F2} MWh/day x {kv.Key.Name}");
                else if (kv.Key == GameData.WaterResource)
                    Console.WriteLine($"│ · {Math.Abs(kv.Value):F2} ㎥/day x {kv.Key.Name}");
                else
                    Console.WriteLine($"│ · {Math.Abs(kv.Value):F2} t/day x {kv.Key.Name}");
            }
        Console.WriteLine($"\n│ [ Citizen Supply ]");
        foreach (var kv in combinedCitizenConsumption)
        {
            if (!net.ContainsKey(kv.Key))
                Console.WriteLine($"│ · {kv.Value} t/day x {kv.Key.Name}");
        }
        Console.WriteLine("├────────────────────────────────────────┤");
        Console.WriteLine("│ Residues:                              │");
        Console.WriteLine("├────────────────────────────────────────┤");
        foreach (var kv in net)
            if (kv.Value >= 0)
                if (kv.Key != GameData.PowerResource)
                    Console.WriteLine($"│ {kv.Value:F2} t/day x {kv.Key.Name}");
                else Console.WriteLine($"│ {kv.Value:F2} MWh/day x {kv.Key.Name}");
        if (industryPlans.Any(p => p.TotalConstructionMaterials.Count() > 0) || microDistricts.Any(m => m.ConstructionMaterials.Count > 0))
        {
            Console.WriteLine("├────────────────────────────────────────┤");
            Console.WriteLine("│ Total Construction Materials:          │");
            Console.WriteLine("├────────────────────────────────────────┤");
            foreach (var kv in combinedConstructionMaterials)
                Console.WriteLine($"│ · {kv.Value:F2} × {kv.Key.Name}");
        }
        Console.WriteLine("└────────────────────────────────────────┘");
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
}

