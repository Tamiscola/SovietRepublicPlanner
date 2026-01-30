class ResidentialBuilding
{
    public string Name { get; set; }
    public int WorkerCapacity { get; set; }
    public double PowerMW { get; set; }  // MW (converted from MWh/day ÷ 24)
    public double WaterPerDay { get; set; }  // m³/day
    public double HeatTankM3 { get; set; }  // m³ (hot water tank capacity)
    public int Quality { get; set; }  // Percentage (affects happiness)
    public double GarbagePerCitizen { get; set; } = 0.0003;
    public double GarbageProduction => WorkerCapacity * GarbagePerCitizen;
                                      
    // Construction
    public int Workdays { get; set; }
    public Dictionary<Resource, double> ConstructionMaterials = new Dictionary<Resource, double>();

}