public class ProductionBuilding : Building
{
    // Identification
    public override BuildingCategory Category => BuildingCategory.Production;
    public ProductionBuilding() { }

    // Workers & Resources
    public List<ResourceAmount> Inputs { get; set; } = new List<ResourceAmount>();
    public List<ResourceAmount> Outputs { get; set; } = new List<ResourceAmount>();
    public int MaxWorkers { get; set; }
    public int CurNumEmp { get; set; }
    public double ProdPerWorker => (double) 100 / EffectiveWorkersPerShift;
    public double CurProductivity => (CurNumEmp * ProdPerWorker) > 100
        ? 100
        : CurNumEmp * ProdPerWorker;
    public int EffectiveWorkersPerShift => (int)Math.Ceiling(MaxWorkers / CalculationSettings.ProductivityMultiplier) > MaxWorkers
        ? MaxWorkers
        : (int)Math.Ceiling(MaxWorkers / CalculationSettings.ProductivityMultiplier); // Practical Number of workers that can reach 100% production

    // Utilities
    public double WaterConsumption { get; set; }        // ㎥/day
    public double SewageProduction { get; set; }        // ㎥/day
    public double SewageDisposalCapacity { get; set; } = 0;  // ㎥/day
    public double BaseGarbageProduction { get; set; } = 0;  // Building's base garbage (without workers)
    public double GarbagePerWorker { get; set; }        // Garbage production per worker
    public double GarbageProduction => BaseGarbageProduction + (MaxWorkers * GarbagePerWorker);  // tons/day
    public double EnvironmentPollution { get; set; }    // tons/day
    public override IEnumerable<UtilityType> GetActiveUtilities()
    {
        return new[] { UtilityType.Power, UtilityType.Water, UtilityType.Sewage, UtilityType.Garbage };
    }
    public override double GetUtilityValue(UtilityType type)
    {
        return type switch 
        {
            UtilityType.Water => this.WaterConsumption,
            UtilityType.Sewage => this.SewageProduction,
            _ => base.GetUtilityValue(type)
        };
    }

    // Variability
    public bool IsSeasonDependent { get; set; }
    public double SeasonalMultiplier { get; set; }      // For current season/average
    public bool IsQualityDependent { get; set; }
    public bool IsUtilityBuilding { get; set; } = false;
    public bool CanUseVehicles { get; set; } = false;
    public SupportCategory SupportCategory { get; set; } = SupportCategory.None;
}
