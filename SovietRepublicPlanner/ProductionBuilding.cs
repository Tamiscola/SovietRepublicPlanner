class ProductionBuilding
{
    public ProductionBuilding() { }
    public string Name { get; set; }
    public List<ResourceAmount> Inputs { get; set; } = new List<ResourceAmount>();
    public List<ResourceAmount> Outputs { get; set; } = new List<ResourceAmount>();
    public int WorkersPerShift { get; set; }

    // Utilities
    public double PowerConsumption { get; set; }        // MW
    public double WaterConsumption { get; set; }        // ㎥/day
    public double SewageProduction { get; set; }        // ㎥/day
    public double SewageDisposalCapacity { get; set; } = 0;  // ㎥/day
    public double HeatConsumption { get; set; }         // Gcal/day
    public double BaseGarbageProduction { get; set; } = 0;  // Building's base garbage (without workers)
    public double GarbagePerWorker { get; set; }        // Garbage production per worker
    public double GarbageProduction => BaseGarbageProduction + (WorkersPerShift * GarbagePerWorker);  // tons/day
    public double EnvironmentPollution { get; set; }    // tons/day

    // Variability
    public bool IsSeasonDependent { get; set; }
    public double SeasonalMultiplier { get; set; }      // For current season/average
    public bool IsQualityDependent { get; set; }
    public bool IsUtilityBuilding { get; set; } = false;
    public bool IsSupportBuildings { get; set; } = false;
    public bool CanUseVehicles { get; set; } = false;
    public SupportCategory SupportCategory { get; set; } = SupportCategory.None;

    // Construction
    public int Workdays { get; set; }
    public Dictionary<Resource, double> ConstructionMaterials = new Dictionary<Resource, double>();

}
public enum SupportCategory
{
    None,
    LiquidHandling,
    BulkHandling,
    SolidHandling,
    WaterHandling,
    PowerHandling,
    HeatHandling,
    SewageHandling,
    GeneralDistribution, // Always available
    DryBulkHandling,
}
