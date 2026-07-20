public class ResidentialBuilding : Building
{
    public int WorkerCapacity { get; set; }
    public int CurNumRes { get; set; }
    public double WaterPerDay { get; set; }  // m³/day
    public double HeatTankM3 { get; set; }  // m³ (hot water tank capacity)
    public int Quality { get; set; }  // Percentage (affects happiness)
    public double WorkersPerArea => WorkerCapacity / Area;
    public double GarbagePerCitizen { get; set; } = 0.0003;
    public double GarbageProduction => WorkerCapacity * GarbagePerCitizen;
    public bool IsDominatedBy(ResidentialBuilding other)
    {
        bool atLeastAsGoodOnAll =
            other.WorkersPerArea >= this.WorkersPerArea &&
            other.Quality >= this.Quality &&
            other.ConstructionCostRUB <= this.ConstructionCostRUB &&
            CalculateUtilityCost(other) <= CalculateUtilityCost(this);

        bool strictlyBetterOnOne =
            other.WorkersPerArea > this.WorkersPerArea ||
            other.Quality > this.Quality ||
            other.ConstructionCostRUB < this.ConstructionCostRUB ||
            CalculateUtilityCost(other) < CalculateUtilityCost(this);

        return atLeastAsGoodOnAll && strictlyBetterOnOne;
    }
    static double CalculateUtilityCost(ResidentialBuilding building)
    {
        // Power
        double Denom = GameData.MaxResPower - GameData.MinResPower;
        double powerNorm;
        if (Denom == 0) powerNorm = 0;
        else powerNorm = (building.PowerConsumptionMWh - GameData.MinResPower) / Denom;

        // Water
        Denom = GameData.MaxResWater - GameData.MinResWater;
        double waterNorm;
        if (Denom == 0) waterNorm = 0;
        else waterNorm = (building.WaterPerDay - GameData.MinResWater) / Denom;

        // Heat
        Denom = GameData.MaxResHeat - GameData.MinResHeat;
        double heatNorm;
        if (Denom == 0) heatNorm = 0;
        else heatNorm = (building.HeatTankM3 - GameData.MinResHeat) / Denom;

        return powerNorm + waterNorm + heatNorm;
    }
}