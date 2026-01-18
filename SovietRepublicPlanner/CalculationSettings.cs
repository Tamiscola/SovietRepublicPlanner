class CalculationSettings
{
    public static double WorkerLoyalty { get; set; } = 50.0;
    public bool AccountForSeasons { get; set; } = true;
    public TimePeriod DisplayUnit { get; set; } = TimePeriod.Day;

    // Import/Expand decisions
    public Dictionary<Resource, bool> ImportFlags { get; set; }
    public static double ProductivityMultiplier => AverageLoyaltyPercent(WorkerLoyalty);

    public static double AverageLoyaltyPercent(double loyalty)
    {
        double mult = 0;
        if (loyalty >= 0 && loyalty <= 50) mult = 0.3 + 0.014 * loyalty;
        else if (loyalty > 50 && loyalty <= 100) mult = 1.0 + 0.01 * (loyalty - 50.0);
        else { Console.WriteLine("Invalid Loyalty number"); mult = 1.0; }
        return mult;
    }
}