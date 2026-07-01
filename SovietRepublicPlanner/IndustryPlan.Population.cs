using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class IndustryPlan
{
    // Workers
    public int TotalWorkers
    {
        get
        {
            int thisLevel = ChosenBuilding.TotalWorkers;
            int subChainTotal = SubChains.Sum(sc => sc.TotalWorkers);
            int baseProductionWorkers = thisLevel + subChainTotal;

            return baseProductionWorkers;
        }
    }
    public double WorkerToPopulationRatio { get; set; } = 0.70;     // 70% are workers
    public int TotalPopulationNeeded => (int)Math.Ceiling(TotalWorkers / WorkerToPopulationRatio);
}
