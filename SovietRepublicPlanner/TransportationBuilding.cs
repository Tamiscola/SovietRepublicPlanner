public enum TransportationType
{
    Bus,
    Trolley,
    Tram,
    Depot,
    Station,
    Refueling,
}
class TransportationBuilding
{
    // Identification
    public string Name;
    public TransportationType Type;  // enum: Bus, Trolley, Tram, Depot, Station, Refueling, Maintenance

    // Construction
    public int Workdays { get; set; }
    public Dictionary<Resource, double> ConstructionMaterials = new Dictionary<Resource, double>();

    // Utilities (same as other buildings)
    public double PowerConsumptionMWh;
    public double WattageKW;
    public double WaterConsumptionM3;
    public double HeatConsumptionMW;

    // Transport-specific
    public int? ParkingSpots;  // nullable - not all have this
    public double? FuelStorageCapacity;  // nullable - only gas stations/end stations
    public int? PassengerCapacity;  // nullable - only stops/platforms
}
