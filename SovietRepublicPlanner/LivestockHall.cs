using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class LiveStockHall : Building
{
    // Identification
    public override BuildingCategory Category => BuildingCategory.Support;
    public SupportCategory SupportCategory { get; set; } = SupportCategory.None;

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
}

