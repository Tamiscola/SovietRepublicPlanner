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
    // Identification
    public override BuildingCategory Category => BuildingCategory.Amenity;
    public AmenityType Type { get; set; }

    // Worker & Visitor info
    public int MaxWorkers { get; set; }
    public int CurNumEmp {  get; set; }
    public int MaxVisitors { get; set; }
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
    public double CustomersPerWorker => ((double)MaxVisitors / (MaxWorkers / CalculationSettings.ProductivityMultiplier)); // default 100% Productivity of Worker
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
    public double HeatConsumptionMW { get; set; }
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
    public override IEnumerable<UtilityType> GetActiveUtilities()
    {
        return new[] { UtilityType.Power, UtilityType.Water, UtilityType.Sewage, UtilityType.Heat, UtilityType.Garbage };
    }
    public override double GetUtilityValue(UtilityType type)
    {
        return type switch 
        { 
            UtilityType.Water => this.WaterConsumptionM3,
            UtilityType.Sewage => this.WaterConsumptionM3,
            UtilityType.Heat => this.HeatConsumptionMW,
            _ => base.GetUtilityValue(type)
        };
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

    // Method
    public bool IsDominatedBy(AmenityBuilding other)
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

