using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class MicroDistrict
{
    public string Name { get; set; }
    public City ParentCity { get; set; }
    public List<ResidentialInstance> ResidentialBuildings { get; set; } = new List<ResidentialInstance>();
    public List<AmenityInstance> AmenityBuildings { get; set; } = new List<AmenityInstance>();
    public List<TransportationInstance> TransportBuildings { get; set; } = new List<TransportationInstance>();
    public int TotalHousingCapacity
    {
        get
        {
            int r = 0;
            for (int i = 0; i < ResidentialBuildings.Count; i++)
                r += (ResidentialBuildings[i].Building.WorkerCapacity * ResidentialBuildings[i].Count);
            return r;
        }
    }
    public int TotalWorkers
    {
        get 
        {
            int thisLevel = ParentCity.industryPlans.Sum(i => i.ChosenBuilding.TotalWorkers);
            int subChainTotal = ParentCity.industryPlans.Sum(i => i.SubChains.Sum(sc => sc.TotalWorkers));

            // Calculate base population from production workers
            int baseProductionWorkers = thisLevel + subChainTotal;
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

            return fullCapacityAmenityWorkers + percentageBasedWorkers;
        }
    }

    // Utilities
    public double PowerConsumption
    {
        get
        {
            double r = 0;
            r += ResidentialBuildings.Sum(ri => ri.Building.PowerConsumptionMWh * ri.Count)
                + AmenityBuildings.Sum(ai => ai.Building.PowerConsumptionMWh * ai.Count)
                + TransportBuildings.Sum(ti => ti.Building.PowerConsumptionMWh * ti.Count);
            return r;
        }
    }
    public double WaterConsumption
    {
        get
        {
            double r = 0;
            r += ResidentialBuildings.Sum(ri => ri.Building.WaterPerDay * ri.Count)
                + AmenityBuildings.Sum(ai => ai.Building.WaterConsumptionM3 * ai.Count);
            return r;
        }
    }// ㎥/day
    public double SewageProduction
    {
        get
        {
            double r = 0;
            r += ResidentialBuildings.Sum(ri => ri.Building.WaterPerDay * ri.Count)
                + AmenityBuildings.Sum(ai => ai.Building.WaterConsumptionM3 * ai.Count);
            return r;
        }
    }// ㎥/day
    public double SewageDisposalCapacity { get; set; } = 0;  // ㎥/day
    public double HeatConsumption
    {
        get
        {
            double r = 0;
            r += ResidentialBuildings.Sum(ri => ri.Building.HeatTankM3 * ri.Count)
                + AmenityBuildings.Sum(ai => ai.Building.HeatConsumptionMW * ai.Count);
            return r;
        }
    }// Gcal/day
    public double GarbageProduction 
    {
        get
        {
            double r = 0;
            r += ResidentialBuildings.Sum(ri => ri.Building.GarbageProduction * ri.Count)
                + AmenityBuildings.Sum(ai => ai.Building.GarbageProduction * ai.Count);
            return r;
        }
    }  // tons/day

    // Construction Materials
    public Dictionary<Resource, double> ConstructionMaterials { get; set; } = new Dictionary<Resource, double>();
    
    public static SavedMicroDistrict ConvertToSavedMicroDistrict(MicroDistrict microDistrict)
    {
        var saved = new SavedMicroDistrict
        {
            Name = microDistrict.Name,
            ResidentialBuildings = microDistrict.ResidentialBuildings
                                        .Select(rb => new SavedMicroDistrict.SavedResidentialInstance
                                        {
                                            BuildingName = rb.Building.Name,
                                            Count = rb.Count,
                                        }).ToList(),
            AmenityBuildings = microDistrict.AmenityBuildings
                                        .Select(ab => new SavedMicroDistrict.SavedAmenityInstance
                                        {
                                            BuildingName = ab.Building.Name,
                                            Count = ab.Count,
                                        }).ToList(),
            TransportBuildings = microDistrict.TransportBuildings
                                        .Select(tb => new SavedMicroDistrict.SavedTransportationInstance
                                        {
                                            BuildingName = tb.Building.Name,
                                            Count = tb.Count,
                                        }).ToList(),
            ConstructionMaterials = microDistrict.ConstructionMaterials
                                        .Select(cm => new SavedMicroDistrict.SavedResourceInstance
                                        {
                                            Name = cm.Key.Name,
                                            Amount = cm.Value
                                        }).ToList(),

        };
        return saved;
    }
    public static MicroDistrict ConvertFromSavedMicroDistrict(SavedMicroDistrict savedMicroDistrict)
    {
        var md = new MicroDistrict
        {
            Name = savedMicroDistrict.Name,
            ResidentialBuildings = savedMicroDistrict.ResidentialBuildings
                                    .Select(sri => new ResidentialInstance
                                    {
                                        Building = GameData.AllResidentialBuildings
                                                    .FirstOrDefault(rb => rb.Name == sri.BuildingName),
                                        Count = sri.Count
                                    }).ToList(),
            AmenityBuildings = savedMicroDistrict.AmenityBuildings
                                    .Select(sai => new AmenityInstance
                                    {
                                        Building = GameData.AllAmenityBuildings
                                                    .FirstOrDefault(ab => ab.Name == sai.BuildingName),
                                        Count = sai.Count
                                    }).ToList(),
            TransportBuildings = savedMicroDistrict.TransportBuildings
                                    .Select(sti => new TransportationInstance
                                    {
                                        Building = GameData.AllTransportationBuildings
                                                    .FirstOrDefault(tb => tb.Name == sti.BuildingName),
                                        Count = sti.Count
                                    }).ToList(),
            ConstructionMaterials = savedMicroDistrict.ConstructionMaterials
                                    .ToDictionary(sri => GameData.AllResources
                                                            .FirstOrDefault(r => r.Name == sri.Name),
                                                  sri => sri.Amount)
        };
        return md;
    }
}
