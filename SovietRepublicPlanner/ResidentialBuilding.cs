public class ResidentialBuilding : Building, IHasWorkers
{
    // Identification
    public override BuildingCategory Category => BuildingCategory.Residential;

    public int MaxWorkers { get; set; }
    public double WorkersPerArea => MaxWorkers / Area;
    public int CurNumRes { get; set; }
    public int Quality { get; set; }  // Percentage (affects happiness)
    public override double GetGeneralValue(GeneralMetricType metricType)
    {
        return metricType switch
        {
            GeneralMetricType.WorkersPerArea => WorkersPerArea,
            GeneralMetricType.CostPerWorker => ConstructionCostRUB / MaxWorkers,
            GeneralMetricType.WorkDaysPerWorker => WorkDays / MaxWorkers,
            _ => base.GetGeneralValue(metricType)
        };
    }

    // Utility
    public double WaterPerDay { get; set; }  // m³/day
    public double HeatTankM3 { get; set; }  // m³ (hot water tank capacity)
    public double GarbagePerCitizen { get; set; } = 0.0003;
    public double GarbageProduction => MaxWorkers * GarbagePerCitizen;
    public override IEnumerable<UtilityType> GetActiveUtilities()
    {
        return new[] { UtilityType.Power, UtilityType.Water, UtilityType.Heat, UtilityType.Sewage, UtilityType.Garbage };
    }
    public override double GetUtilityValue(UtilityType type)
    {
        return type switch
        {
            UtilityType.Water => this.WaterPerDay,
            UtilityType.Sewage => this.WaterPerDay,
            UtilityType.Heat => this.HeatTankM3,
            _ => base.GetUtilityValue(type)
        };
    }

    // Unlock Prerequisites
    public int? UnlockYear { get; set; }    // null = always available
    public List<string> RequiresResearch { get; set; } = new List<string>();    // null = no research needed

    // Method
    public bool IsDominatedBy(ResidentialBuilding other)
    {
        bool atLeastAsGoodOnAll =
            other.WorkersPerArea >= this.WorkersPerArea &&
            other.Quality >= this.Quality &&
            other.ConstructionCostRUB <= this.ConstructionCostRUB &&
            other.WorkDays <= this.WorkDays &&
            CalculationEngine.UtilityCalculator.NormalizedTotalUtilityCostPerWorker(other) <= CalculationEngine.UtilityCalculator.NormalizedTotalUtilityCostPerWorker(this);

        bool strictlyBetterOnOne =
            other.WorkersPerArea > this.WorkersPerArea ||
            other.Quality > this.Quality ||
            other.ConstructionCostRUB < this.ConstructionCostRUB ||
            other.WorkDays < this.WorkDays ||
            CalculationEngine.UtilityCalculator.NormalizedTotalUtilityCostPerWorker(other) < CalculationEngine.UtilityCalculator.NormalizedTotalUtilityCostPerWorker(this);

        return atLeastAsGoodOnAll && strictlyBetterOnOne;
    }
}