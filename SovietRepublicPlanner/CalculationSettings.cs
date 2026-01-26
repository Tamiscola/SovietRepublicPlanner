class CalculationSettings
{
    public static double WorkersProductivity { get; set; } = 100.0;
    public bool AccountForSeasons { get; set; } = true;
    public TimePeriod DisplayUnit { get; set; } = TimePeriod.Day;

    // Import/Expand decisions
    public Dictionary<Resource, bool> ImportFlags { get; set; }

    // Multipliers
    public static double ProductivityMultiplier => WorkersProductivity / 100;
    public static double AmenityCoverageMultiplier { get; set; } = 30.0;    // (user-configurable)
    public static double KindergartenAgePercent { get; set; } = 18.0;
    public static double SchoolAgePercent { get; set; } = 32.0;
    public static double UniversityDemandPercent { get; set; } = 0;
}