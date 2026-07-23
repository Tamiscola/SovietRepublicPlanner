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
    public override BuildingCategory Category => BuildingCategory.Transport;
    public TransportationType Type;  // enum: Bus, Trolley, Tram, Depot, Station, Refueling, Maintenance

    // Utilities (same as other buildings)
    public override IEnumerable<UtilityType> GetActiveUtilities()
    {
        return new[] { UtilityType.Power };
    }

    // Transport-specific
    public int? ParkingSpots;  // nullable - not all have this
    public double? FuelStorageCapacity;  // nullable - only gas stations/end stations
    public int? PassengerCapacity;  // nullable - only stops/platforms
}
