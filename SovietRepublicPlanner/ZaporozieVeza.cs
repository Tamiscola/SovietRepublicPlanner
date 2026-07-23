using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ZaporozieVeza : Building
{
    // Identification
    public override BuildingCategory Category => BuildingCategory.Support;
    public SupportCategory SupportCategory { get; set; } = SupportCategory.None;

    // Utility
    public double EnvironmentPollution { get; set; }
    public override IEnumerable<UtilityType> GetActiveUtilities()
    {
        return new[] { UtilityType.Power };
    }
}

