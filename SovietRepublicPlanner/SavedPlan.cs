public class SavedPlan
{
    public string ResourceName { get; set; }
    public string BuildingName { get; set; }
    public double Amount { get; set; }
    public double Loyalty { get; set; }
    public int ChosenBuildingIndex { get; set; }
    public bool IsBuildingBasedPlan { get; set; } = false;
    public int BuildingCount { get; set; }                 // For building-based plans
    public List<double> BuildingQualities { get; set; }     // For mines/quarries
    public bool UsesVehicles { get; set; } = false;
    public List<SavedPlan> SubChains { get; set; } = new List<SavedPlan>();
    public List<SavedBuildingInstance> SupportBuildings { get; set; }
    public class SavedBuildingInstance
    {
        public string BuildingName { get; set; }
        public int Count { get; set; }
    }
    public List<SavedResidentialInstance> ResidentialBuildings { get; set; }
    public class SavedResidentialInstance
    {
        public string BuildingName { get; set; }
        public int Count { get; set; }
    }
    public List<SavedAmenityInstance> AmenityBuildings { get; set; }
    public class SavedAmenityInstance
    {
        public string BuildingName { get; set; }
        public int Count { get; set; }
    }
    public List<SavedTransportationInstance> TransportationBuildings { get; set; }
    public class SavedTransportationInstance
    {
        public string BuildingName { get; set; }
        public int Count { get; set; }
    }

}