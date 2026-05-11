using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class SupportBuilding : Building
{
    public SupportCategory SupportCategory { get; set; } = SupportCategory.None;
    public double WaterConsumption { get; set; } = 0;   // For LiveStockHall
    public double EnvironmentPollution { get; set; } = 0;   // For CoolingTowers
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

