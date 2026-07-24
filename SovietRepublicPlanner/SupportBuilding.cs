using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SupportBuilding : Building
{
    // Identification
    public override BuildingCategory Category => BuildingCategory.Support;
    public SupportCategory SupportCategory { get; set; } = SupportCategory.None;

    // Utilities
    public double WaterConsumptionM3 { get; set; } = 0;     // For LiveStockHall
    public double EnvironmentPollution { get; set; } = 0;   // For CoolingTowers
    public override IEnumerable<UtilityType> GetActiveUtilities()
    {
        var utilities = new List<UtilityType> { UtilityType.Power };
        if (this.WaterConsumptionM3 > 0) 
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
}
public enum SupportCategory
{
    None,
    LiquidHandling,
    BulkHandling,
    DryBulkHandling,
    SolidHandling,
    GeneralDistribution, // Always available
    Refrigeration,
    PowerHandling,
    WaterHandling,
    HeatHandling,
    SewageHandling,
}

