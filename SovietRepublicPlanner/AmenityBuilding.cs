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

public class AmenityBuilding
{
    public string Name { get; set; }
    public AmenityType Type { get; set; }

    // Worker & Visitor info
    public int WorkersPerShift { get; set; }
    public int MaxVisitors { get; set; }

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
    public int Workdays { get; set; }

    // Storage (for grocery stores, prisons, etc.)
    public Dictionary<string, double> WarehouseCapacity { get; set; }
    public Dictionary<string, double> ColdStorageCapacity = new Dictionary<string, double>();  // NEW!

    // Vehicle & Parking (for police, fire, etc.)
    public int VehicleStations { get; set; }
    public int ParkingSpots { get; set; } = 0;  // NEW!

    // Special properties
    public double? QualityOfFlats { get; set; }  // NEW! (for prisons - housing prisoners)
    public double FuelImport { get; set; } = 0;  // NEW! (oil tank for police vehicles)
    public string RequiresResearch { get; set; }  // NEW! (e.g., "Secret Police")

    // Construction
    public Dictionary<string, double> ConstructionMaterials { get; set; }

    // Weather requirement (for beach cafe)
    public double? MinTemperature { get; set; }
}

