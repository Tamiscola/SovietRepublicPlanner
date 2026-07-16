public class ResidentialBuilding : Building
{
    public int WorkerCapacity { get; set; }
    public int CurNumRes { get; set; }
    public double WaterPerDay { get; set; }  // m³/day
    public double HeatTankM3 { get; set; }  // m³ (hot water tank capacity)
    public int Quality { get; set; }  // Percentage (affects happiness)
    public double Area { get; set; }   // Distance between dots : 5.5m
    public double WorkersPerArea => WorkerCapacity / Area;
    public double GarbagePerCitizen { get; set; } = 0.0003;
    public double GarbageProduction => WorkerCapacity * GarbagePerCitizen;
}