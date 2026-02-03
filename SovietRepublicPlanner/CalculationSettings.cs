class CalculationSettings
{
    public static double WorkersProductivity { get; set; } = 100.0;
    public bool AccountForSeasons { get; set; } = true;
    public TimePeriod DisplayUnit { get; set; } = TimePeriod.Day;

    // Import/Expand decisions
    public Dictionary<Resource, bool> ImportFlags { get; set; }

    // Multipliers
    public static double ProductivityMultiplier => WorkersProductivity / 100;
    public static Dictionary<AmenityType, double> AmenityCoverageMultiplier = new()
    {
        { AmenityType.Shopping, 15.0 },
        { AmenityType.Pub, 100.0 },
        { AmenityType.Healthcare, 100.0 },
        { AmenityType.Culture, 80.0 },
        { AmenityType.Sports, 80.0 },
        { AmenityType.Education, 1.0 },  // Special: uses shifts
        { AmenityType.CrimeJustice, 100.0 },
        { AmenityType.Fireservice, 0.0 },  // Not visitor-based
        { AmenityType.CityService, 0.0 },
        { AmenityType.Fountain, 0.0 }
    };
    public static double KindergartenAgePercent { get; set; } = 18.0;
    public static double SchoolAgePercent { get; set; } = 32.0;
    public static double UniversityDemandPercent { get; set; } = 0;
}