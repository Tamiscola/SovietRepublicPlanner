// Calculation Result for one type of building that produces the target resource
class BuildingRequirement
{
    public BuildingRequirement(ProductionBuilding productionBuilding)
    {
        Building = productionBuilding;
    }
    public ProductionBuilding Building { get; set; }

    // Buildings Count
    public List<BuildingInstance> BuildingInstances { get; set; } = new List<BuildingInstance>();
    public int Count { get; set; }

    // Workers
    public double WorkerLoyalty { get; set; } = 50;
    public int baseTotalWorkers => Count * Building.WorkersPerShift * 3;
    public int TotalWorkers 
    {  get
        {
            if (Building.WorkersPerShift == 0) return 0;
            double multiplier = CalculationSettings.AverageLoyaltyPercent(WorkerLoyalty);
            return (int)Math.Ceiling(baseTotalWorkers / multiplier);
        }
    }

    // Resource
    public Dictionary<Resource, double> RequiredResources => CalculateInputResources();
    public Dictionary<Resource, double> ExpectedOutput => CalculateExpectedOutput();

    // Utilities
    public double TotalPowerNeeded => Count * Building.PowerConsumption;
    public double TotalWaterNeeded => Count * Building.WaterConsumption;
    public double TotalSewageProduced => Count * Building.SewageProduction;
    public double TotalSewageDisposalCapacity => Count * Building.SewageDisposalCapacity;
    public double TotalHeatNeeded => Count * Building.HeatConsumption;
    public double TotalGarbageProduced => Count * Building.GarbageProduction;
    public double TotalEnvironmentPollution => Count * Building.EnvironmentPollution;

    // Helper Methods
    public Dictionary<Resource, double> CalculateInputResources()
    {
        Dictionary<Resource, double> result = new Dictionary<Resource, double>();
        for (int i = 0; i < Building.Inputs.Count; i++)
        {
            if (!result.ContainsKey(Building.Inputs[i].Resource))
            {
                result.Add(Building.Inputs[i].Resource, Building.Inputs[i].Amount * Count);
            } else { result[Building.Inputs[i].Resource] += Building.Inputs[i].Amount * Count; }
        }

        return result;
    } 
    public Dictionary<Resource, double> CalculateExpectedOutput()
    {
        Dictionary<Resource, double> result = new Dictionary<Resource, double>();
        // Mines, Oils
        if (Building.IsQualityDependent)
        {
            foreach (BuildingInstance bi in BuildingInstances)
            {
                foreach (var kvp in bi.ExpectedOutput)
                {
                    if (!result.ContainsKey(kvp.Key))
                        result.Add(kvp.Key, kvp.Value);
                    else
                        result[kvp.Key] += kvp.Value;
                }
            }
        }
        // Crops
        else if (Building.IsSeasonDependent) 
        { 
            foreach (BuildingInstance bi in BuildingInstances)
            {
                foreach (var kvp in bi.ExpectedOutput)
                {
                    if (!result.ContainsKey(kvp.Key))
                        result.Add(kvp.Key, kvp.Value);
                    else
                        result[kvp.Key] += kvp.Value;
                }
            }
        }
        // Default
        else
        {
            for (int i = 0; i < Building.Outputs.Count; i++)
            {
                result.Add(Building.Outputs[i].Resource, Building.Outputs[i].Amount * Count);
            }
        }
        return result;
    }
}