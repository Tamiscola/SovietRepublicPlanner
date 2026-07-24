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
    public double WaterConsumptionM3 { get; set; } = 0;     // LabourPickupHall
    public override IEnumerable<UtilityType> GetActiveUtilities()
    {
        var utilities = new List<UtilityType>() { UtilityType.Power };
        if (WaterConsumptionM3 > 0)
        {
            utilities.Add(UtilityType.Water);
            utilities.Add(UtilityType.Sewage);
        }
        return utilities;
    }
    public override double GetUtilityValue(UtilityType type)
    {
        return type switch
        {
            UtilityType.Water => WaterConsumptionM3,
            UtilityType.Sewage => WaterConsumptionM3,
            _ => base.GetUtilityValue(type)
        };
    }

    // Transport-specific
    public int? ParkingSpots;  // nullable - not all have this
    public double? FuelStorageCapacity;  // nullable - only gas stations/end stations
    public int? PassengerCapacity;  // nullable - only stops/platforms
}
