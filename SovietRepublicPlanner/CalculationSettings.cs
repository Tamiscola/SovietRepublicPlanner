class CalculationSettings
{
    public static double WorkersProductivity { get; set; } = 100.0;
    public bool AccountForSeasons { get; set; } = true;
    public TimePeriod DisplayUnit { get; set; } = TimePeriod.Day;

    // Import/Expand decisions
    public Dictionary<Resource, bool> ImportFlags { get; set; }
    public static double ProductivityMultiplier => WorkersProductivity / 100;
}