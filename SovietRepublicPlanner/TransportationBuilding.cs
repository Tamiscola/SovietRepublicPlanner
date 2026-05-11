public enum TransportationType
{
    Bus,
    Trolley,
    Tram,
    Depot,
    Station,
    Refueling,
}
public class TransportationBuilding : Building
{
    // Identification
    public TransportationType Type;  // enum: Bus, Trolley, Tram, Depot, Station, Refueling, Maintenance

    // Utilities (same as other buildings)
    public double WaterConsumptionM3;
    public double HeatConsumptionMW;

    // Transport-specific
    public int? ParkingSpots;  // nullable - not all have this
    public double? FuelStorageCapacity;  // nullable - only gas stations/end stations
    public int? PassengerCapacity;  // nullable - only stops/platforms
}
