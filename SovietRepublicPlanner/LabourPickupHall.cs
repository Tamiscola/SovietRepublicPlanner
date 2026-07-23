using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class LabourPickupHall : Building
{
    // Identification
    public override BuildingCategory Category => BuildingCategory.Transport;
    public TransportationType Type;

    // Utility
    public double WaterConsumptionM3 { get; set; }
    public override IEnumerable<UtilityType> GetActiveUtilities()
    {
        return new[] { UtilityType.Power, UtilityType.Water, UtilityType.Sewage };
    }
    public override double GetUtilityValue(UtilityType type)
    {
        return type switch
        {
            UtilityType.Water => this.WaterConsumptionM3,
            UtilityType.Sewage => this.WaterConsumptionM3,
            _ => base.GetUtilityValue(type),
        };
    }

    // Transport-specific
    public int? ParkingSpots;  // nullable - not all have this
    public double? FuelStorageCapacity;  // nullable - only gas stations/end stations
    public int? PassengerCapacity;  // nullable - only stops/platforms
}

