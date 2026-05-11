using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class IndustryPlan
{
    // Workers
    public int BaseWorkers => ChosenBuilding.baseTotalWorkers;
    public int TotalWorkers
    {
        get
        {
            int thisLevel = ChosenBuilding.TotalWorkers;
            int subChainTotal = SubChains.Sum(sc => sc.TotalWorkers);
            int supportTotal = SupportBuildings.Sum(sc => sc.TotalWorkers);

            // Calculate base population from production workers
            int baseProductionWorkers = thisLevel + subChainTotal + supportTotal;
            int baseCitizens = (int)(baseProductionWorkers * 1.82); // Citizens = workers × 1.82

            // Add full-capacity amenity workers (non-percentage-based)
            int fullCapacityAmenityWorkers = AmenityBuildings
                .Where(a => !a.Building.UsesPercentageBasedDemand)
                .Sum(a => a.Building.EffectiveWorkersPerShift * 3 * a.Count);

            // Add percentage-based amenity workers
            int percentageBasedWorkers = 0;
            foreach (var amenity in AmenityBuildings.Where(a => a.Building.UsesPercentageBasedDemand))
            {
                percentageBasedWorkers += CalculateWorkersForPercentageAmenity(
                    amenity.Building,
                    baseProductionWorkers,
                    baseCitizens
                ) * amenity.Count;
            }

            return baseProductionWorkers + fullCapacityAmenityWorkers + percentageBasedWorkers;
        }
    }
    public double WorkerToPopulationRatio { get; set; } = 0.55;     // 55% are workers
    public int TotalPopulationNeeded => (int)Math.Ceiling(TotalWorkers / WorkerToPopulationRatio);
    public int TotalHousingCapacity => ResidentialBuildings.Sum(rb => rb.Building.WorkerCapacity * rb.Count);

}
