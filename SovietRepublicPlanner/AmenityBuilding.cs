public enum AmenityType
{
    Shopping,
    Pub,
    Healthcare,
    Fireservice,
    CityService,
    Culture,
    Sports,
    Education,
    CrimeJustice,
    Fountain
}
public enum EducationSubtype
{
    None,
    Kindergarten,
    School,
    University,
    UniversityDorm
}
public class AmenityBuilding
{
    public string Name { get; set; }
    public AmenityType Type { get; set; }

    // Worker & Visitor info
    public int WorkersPerShift { get; set; }
    public int MaxVisitors { get; set; }
    public int CitizenCapacity => Type == AmenityType.Education
        ? MaxVisitors   // Education : MaxVisitors = CitizenCapacity
        : (int)(MaxVisitors * CalculationSettings.AmenityCoverageMultiplier);
    public int EffectiveWorkersPerShift => (int)Math.Ceiling(WorkersPerShift / CalculationSettings.ProductivityMultiplier);
    public List<Resource> ProductsOffered { get; set; } = new List<Resource>();

    // Utilities
    public double PowerConsumptionMWh { get; set; }
    public double WattageKW { get; set; }
    public double WaterConsumptionM3 { get; set; }
    public double HotWaterTankM3 { get; set; }
    public double HeatConsumptionMW { get; set; }

    // Waste
    public double GarbagePerWorker { get; set; }
    public double GarbagePerCustomer { get; set; }

    // Optional properties
    public double? AttractionScore { get; set; }
    public EducationSubtype EducationLevel { get; set; } = EducationSubtype.None;

    // Storage (for grocery stores, prisons, etc.)
    public Dictionary<Resource, double> WarehouseCapacity { get; set; }
    public Dictionary<Resource, double> ColdStorageCapacity = new Dictionary<Resource, double>(); 

    // Vehicle & Parking (for police, fire, etc.)
    public int VehicleStations { get; set; }
    public int ParkingSpots { get; set; } = 0;

    // Special properties
    public double? QualityOfFlats { get; set; }  //  (for prisons - housing prisoners)
    public double FuelImport { get; set; } = 0;  //  (oil tank for police vehicles)
    public string RequiresResearch { get; set; }  //  (e.g., "Secret Police")

    // Construction
    public int Workdays { get; set; }
    public Dictionary<Resource, double> ConstructionMaterials = new Dictionary<Resource, double>();

    // Weather requirement (for beach cafe)
    public double? MinTemperature { get; set; }
}

