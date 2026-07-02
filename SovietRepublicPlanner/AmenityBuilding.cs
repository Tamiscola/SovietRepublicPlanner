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
public enum PopulationType
{
    Workers,
    Children,
    Infants,
    YoungAdults,
    Citizen
}
public class AmenityBuilding : Building
{
    public AmenityType Type { get; set; }

    // Worker & Visitor info
    public int MaxWorkers { get; set; }
    public int CurNumEmp {  get; set; }
    public int MaxVisitors { get; set; }
    public int CurCoverage => Type switch
    {
        AmenityType.Education => EducationLevel switch
        {
            EducationSubtype.Kindergarten => (int)(CurCustomCapacity * 15),
            EducationSubtype.School => (int)(CurCustomCapacity * 20),
            EducationSubtype.University => (int)(CurCustomCapacity * 20),
            EducationSubtype.UniversityDorm => CurCustomCapacity,  // Direct housing
            _ => CurCustomCapacity  // Fallback: no multiplier
        },
        AmenityType.Shopping => (int)(CurCustomCapacity * 15),    
        AmenityType.Pub => (int)(CurCustomCapacity * 100),
        AmenityType.Healthcare => (int)(CurCustomCapacity * 100),
        AmenityType.Culture => (int)(CurCustomCapacity * 80),
        AmenityType.Sports => (int)(CurCustomCapacity * 80),
        AmenityType.CrimeJustice => (int)(CurCustomCapacity * 100),
        AmenityType.Fireservice => 0,
        AmenityType.CityService => 0,
        AmenityType.Fountain => 0,
        _ => (int)(CurCustomCapacity * 30)  // Fallback
    };
    public int MaxCoverage => Type switch
    {
        AmenityType.Education => EducationLevel switch
        {
            EducationSubtype.Kindergarten => (int)(MaxVisitors * 15),
            EducationSubtype.School => (int)(MaxVisitors * 20),
            EducationSubtype.University => (int)(MaxVisitors * 20),
            EducationSubtype.UniversityDorm => MaxVisitors,  // Direct housing
            _ => MaxVisitors  // Fallback: no multiplier
        },
        AmenityType.Shopping => (int)(MaxVisitors * 15),
        AmenityType.Pub => (int)(MaxVisitors * 100),
        AmenityType.Healthcare => (int)(MaxVisitors * 100),
        AmenityType.Culture => (int)(MaxVisitors * 80),
        AmenityType.Sports => (int)(MaxVisitors * 80),
        AmenityType.CrimeJustice => (int)(MaxVisitors * 100),
        AmenityType.Fireservice => 0,
        AmenityType.CityService => 0,
        AmenityType.Fountain => 0,
        _ => (int)(MaxVisitors * 30)  // Fallback
    };
    public int CurCustomCapacity => (CurNumEmp * CustomersPerWorker) > MaxVisitors
        ? MaxVisitors
        : CurNumEmp * CustomersPerWorker;
    public int CustomersPerWorker => (int)Math.Floor((double)MaxVisitors / EffectiveWorkersPerShift); // default 100% Productivity of Worker
    public int EffectiveWorkersPerShift => (int)Math.Ceiling(MaxWorkers / CalculationSettings.ProductivityMultiplier) > MaxWorkers
        ? MaxWorkers
        : (int)Math.Ceiling(MaxWorkers / CalculationSettings.ProductivityMultiplier); // Practical Number of workers that can reach 100% production
    public List<Resource> ProductsOffered { get; set; } = new List<Resource>();

    // Percentage-based demand
    public bool UsesPercentageBasedDemand { get; set; } = false;
    public double PopulationPercentageServed { get; set; } = 1.0; // 1.0 = 100%

    // Who does this building serve?
    public PopulationType ServesPopulationType { get; set; } = PopulationType.Workers;

    // Utilities
    public double WaterConsumptionM3 { get; set; }
    public double HotWaterTankM3 { get; set; }
    public double HeatConsumptionMW { get; set; }

    // Waste
    public double GarbagePerWorker { get; set; }
    public double GarbagePerCustomer { get; set; }
    public double GarbageProduction
    {
        get
        {
            double result = 0;
            result = GarbagePerWorker * MaxWorkers + GarbagePerCustomer * MaxVisitors;
            return result;
        }
    }

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

    // Weather requirement (for beach cafe)
    public double? MinTemperature { get; set; }
}

