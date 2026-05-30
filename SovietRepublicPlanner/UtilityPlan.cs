using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class UtilityPlan
{
    // Identification
    public string Name { get; set; }
    public City ParentCity { get; set; }
    public List<UtilityInstance> Buildings { get; set; }
    public UtilityType Type;

    // Resources & Workers
    public int TotalWorkers {
        get 
        {
            int r = 0;
            foreach (UtilityInstance instance in Buildings)
            {
                r += instance.Building.TotalWorkers * instance.Count;
            }
            return r;
        }
    }
    public List<ResourceAmount> TotalInputs { get; set; } = new List<ResourceAmount>();
    public List<ResourceAmount> TotalOutputs { get; set; } = new List<ResourceAmount>();

    // Utilities 
    public double TotalWaterConsumptionM3;
    public double TotalSewageProductionM3 => TotalWaterConsumptionM3;
    public double TotalSewageDisposalCapacity { get; set; } = 0;
    public double TotalHeatConsumptionM3;
    public double TotalEnvironmentPollution;
    public double TotalGarbageProduction 
    {
        get
        {
            double r = 0;
            foreach (UtilityInstance instance in Buildings)
            {
                r += instance.Building.GarbageProduction * instance.Count;
            }
            return r;
        }
    }

    // Transport-specific
    public int? TotalParkingSpots;  // nullable - not all have this
    public double? TotalFuelStorageCapacity;  // nullable - only gas stations/end stations
    public int? TotalPassengerCapacity;  // nullable - only stops/platforms
}
