using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class UtilityBuilding : Building
{
    // Identification
    public override BuildingCategory Category => BuildingCategory.Utility;
    public UtilityType Type;
    public SupportCategory SupportCategory;

    // Resources & Workers
    public int MaxWorkers;
    public int TotalWorkers => MaxWorkers * 3;
    public List<ResourceAmount> Inputs { get; set; } = new List<ResourceAmount>();
    public List<ResourceAmount> Outputs { get; set; } = new List<ResourceAmount>();
    public double ProdPerWorker => (double)100 / EffectiveWorkersPerShift;
    public int EffectiveWorkersPerShift => (int)Math.Ceiling(MaxWorkers / CalculationSettings.ProductivityMultiplier) > MaxWorkers
    ? MaxWorkers
    : (int)Math.Ceiling(MaxWorkers / CalculationSettings.ProductivityMultiplier); // Practical Number of workers that can reach 100% production

    // Utilities (same as other buildings)
    public double WaterConsumptionM3;
    public double SewageProductionM3 => WaterConsumptionM3;
    public double SewageDisposalCapacity { get; set; } = 0;
    public double GarbagePerWorker;
    public double BaseGarbageProduction { get; set; } = 0;
    public double EnvironmentPollution;
    public double GarbageProduction => BaseGarbageProduction + (MaxWorkers * GarbagePerWorker);  // tons/day
    public override IEnumerable<UtilityType> GetActiveUtilities()
    {
        return new[] { UtilityType.Power, UtilityType.Water, UtilityType.Sewage, UtilityType.Garbage };
    }
    public override double GetUtilityValue(UtilityType type)
    {
        return type switch 
        {
            UtilityType.Water => this.WaterConsumptionM3,
            UtilityType.Sewage => this.SewageProductionM3,
            _ => base.GetUtilityValue(type)
        };
    }

    // Transport-specific
    public int? ParkingSpots;  // nullable - not all have this
    public double? FuelStorageCapacity;  // nullable - only gas stations/end stations
    public int? PassengerCapacity;  // nullable - only stops/platforms
}
