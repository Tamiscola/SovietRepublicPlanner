using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum UtilityType
{
    Power,
    Water,
    Sewage,
    Heat,
    Garbage
}
public class UtilityBuilding : Building
{
    // Identification
    public UtilityType Type;
    public SupportCategory SupportCategory;

    public int WorkersPerShift;
    public List<ResourceAmount> Inputs { get; set; } = new List<ResourceAmount>();
    public List<ResourceAmount> Outputs { get; set; } = new List<ResourceAmount>();

    // Utilities (same as other buildings)
    public double WaterConsumptionM3;
    public double SewageProductionM3 => WaterConsumptionM3;
    public double HeatConsumptionM3;
    public double GarbagePerWorker;
    public double BaseGarbageProduction { get; set; } = 0;
    public double EnvironmentPollution;
    public double GarbageProduction => BaseGarbageProduction + (WorkersPerShift * GarbagePerWorker);  // tons/day

    // Transport-specific
    public int? ParkingSpots;  // nullable - not all have this
    public double? FuelStorageCapacity;  // nullable - only gas stations/end stations
    public int? PassengerCapacity;  // nullable - only stops/platforms
}
