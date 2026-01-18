class BuildingInstance
{
    public ProductionBuilding Building { get; set; }
    public double ResourceAbundanceMultiplier { get; set; } = 1.0;  // quality %
    public Dictionary<Resource, double> StartupCosts { get; set; } = new Dictionary<Resource, double>();
    public double SeasonalMultiplier { get; set; }  // Fertility
    public Dictionary<Resource, double> ExpectedOutput
    {
        get
        {
            Dictionary<Resource, double> result = new Dictionary<Resource, double>();
            for (int i = 0; i < Building.Outputs.Count(); i++)
            {
                if (!result.ContainsKey(Building.Outputs[i].Resource))
                {
                    if (Building.Outputs[i].Resource.Name.Contains("Crops"))
                        result.Add(Building.Outputs[i].Resource, Building.Outputs[i].Amount * SeasonalMultiplier);
                    else
                        result.Add(Building.Outputs[i].Resource, Building.Outputs[i].Amount * ResourceAbundanceMultiplier);
                } else
                {
                    if (Building.Outputs[i].Resource.Name.Contains("Crops"))
                        result[Building.Outputs[i].Resource] += Building.Outputs[i].Amount * SeasonalMultiplier;
                    else
                        result[Building.Outputs[i].Resource] += Building.Outputs[i].Amount * ResourceAbundanceMultiplier;
                }
            }
            return result;
        }
    }
}