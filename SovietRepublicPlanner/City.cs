using SovietRepublicPlanner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

public partial class City
{
    public string Name { get; set; }
    public List<IndustryPlan> industryPlans { get; set; } = new List<IndustryPlan>();
    public List<MicroDistrict> microDistricts { get; set; } = new List<MicroDistrict>();
    public List<UtilityPlan> UtilityPlans { get; set; } = new List<UtilityPlan>();
    public List<SupportInstance> SupportBuildings { get; set; } = new List<SupportInstance>();
    public List<TransportationInstance> TransportationBuildings { get; set; } = new List<TransportationInstance>();

    // Initialize totals
    public int totalWorkers
    {
        get
        {
            int total = 0;
            if (industryPlans.Count > 0)
            {
                foreach (var plan in industryPlans) total += plan.TotalWorkers;
            }
            if (microDistricts.Count > 0)
            {
                foreach (var m in microDistricts) total += m.TotalWorkers;
            }
            if (UtilityPlans.Count > 0)
            {
                foreach (var p in UtilityPlans) total += p.TotalWorkers;
            }
            return total;
        }
    }
    public int totalCitizen {
        get
        {
            int total = 0;
            if (industryPlans.Count > 0)
            {
                foreach (var plan in industryPlans) total += plan.TotalPopulationNeeded;
            }
            return total;
        }
    }
    public double totalPower
    {
        get
        {
            double total = 0;
            if (industryPlans.Count > 0)
            {
                foreach (var plan in industryPlans) total += plan.TotalPowerNeeded;
            }
            if (microDistricts.Count > 0)
            {
                foreach (var m in microDistricts) total += m.PowerConsumption;
            }
            if (UtilityPlans.Count > 0)
            {
                foreach (var p in UtilityPlans) total += p.TotalPowerConsumptionMWh;
            }
            return total;
        }
    }
    public double totalWater
    {
        get
        {
            double total = 0;

            if (industryPlans.Count > 0)
            {
                foreach (var plan in industryPlans) total += plan.TotalWaterNeeded;
            }
            if (microDistricts.Count > 0)
            {
                foreach (var m in microDistricts) total += m.WaterConsumption;
            }
            if (UtilityPlans.Count > 0)
            {
                foreach (var p in UtilityPlans) total += p.TotalWaterConsumptionM3;
            }
            return total;
        }
    }
    public double totalHeat
    {
        get
        {
            double total = 0;
            if (industryPlans.Count > 0)
            {
                foreach (var plan in industryPlans) total += plan.TotalHeatNeeded;
            }
            if (microDistricts.Count > 0)
            {
                foreach (var m in microDistricts) total += m.HeatConsumption;
            }
            if (UtilityPlans.Count > 0)
            {
                foreach (var p in UtilityPlans) total += p.TotalHeatConsumptionM3;
            }
            return total;
        }
    }
    public double totalGarbage
    {
        get
        {
            double total = 0;
            if (industryPlans.Count > 0)
            {
                foreach (var plan in industryPlans) total += plan.TotalGarbageProduced;
            }
            if (microDistricts.Count > 0)
            {
                foreach (var m in microDistricts) total += m.GarbageProduction;
            }
            if (UtilityPlans.Count > 0)
            {
                foreach (var p in UtilityPlans) total += p.TotalGarbageProduction;
            }
            return total;
        }
    }
    public double totalPollution
    {
        get
        {
            double total = 0;
            if (industryPlans.Count > 0)
            {
                foreach (var plan in industryPlans) total += plan.TotalEnvironmentPollution;
            }
            if (UtilityPlans.Count > 0)
            {
                foreach (var p in UtilityPlans) total += p.TotalEnvironmentPollution;
            }
            return total;
        }
    }
    public int totalHousingCapacity
    {
        get
        {
            int total = 0;
            if (microDistricts.Count > 0)
            {
                foreach (var d in microDistricts) total += d.TotalHousingCapacity;
            }
            return total;
        }
    }
    public Dictionary<Resource, double> UtilityProduction {
        get
        {
            Dictionary<Resource, double> r = new Dictionary<Resource, double>()
            {
                {GameData.PowerResource, 0},
                {GameData.WaterResource, 0},
                {GameData.WasteWaterResource, 0},
                {GameData.HeatResource, 0},
            };
            if (UtilityPlans.Count > 0)
            {
                foreach (var plan in UtilityPlans)
                {
                    foreach (var kv in plan.TotalOutputs)
                    {
                        if (r.ContainsKey(kv.Key)) r[kv.Key] += kv.Value;
                        else r.Add(kv.Key, kv.Value);
                    }
                }
            }
            return r;
        }
    }
    public Dictionary<Resource, double> combinedImports
    {
        get 
        {
            Dictionary<Resource, double> r = new Dictionary<Resource, double>();
            if (industryPlans.Count > 0)
            {
                foreach (var plan in industryPlans)
                {
                    foreach (var kv in plan.TotalInputs)
                    {
                        if (r.ContainsKey(kv.Key)) r[kv.Key] += kv.Value;
                        else r[kv.Key] = kv.Value;
                    }
                }
            }
            if (UtilityPlans.Count > 0)
            {
                foreach (var plan in UtilityPlans)
                {
                    foreach (var kv in plan.TotalInputs)
                    {
                        if (r.ContainsKey(kv.Key)) r[kv.Key] += kv.Value;
                        else r[kv.Key] = kv.Value;
                    }
                }
            }
            return r;
        }
    }
    public Dictionary<Resource, double> combinedResidues
    {
        get
        {
            Dictionary<Resource, double> r = new Dictionary<Resource, double>();
            if (industryPlans.Count > 0)
            {
                foreach (var plan in industryPlans)
                {
                    foreach (var kv in plan.TotalResidues)
                    {
                        if (r.ContainsKey(kv.Key)) r[kv.Key] += kv.Value;
                        else r[kv.Key] = kv.Value;
                    }
                }
            }
            return r;
        }
    }
    public Dictionary<Resource, double> combinedCitizenConsumption
    {
        get 
        {
            Dictionary<Resource, double> result = new Dictionary<Resource, double>();
            foreach (var r in GameData.AllResources)
            {
                if (r.IsConsumable && r.PerCapitalConsumption > 0)
                {
                    double consumption = totalWorkers * r.PerCapitalConsumption;
                    if (result.ContainsKey(r))
                        result[r] += consumption;
                    else
                        result.Add(r, consumption);
                }
            }
            return result;
        }
    }
    public Dictionary<Resource, double> ConsumptionBalance
    {
        get
        {
            Dictionary<Resource, double> result = new Dictionary<Resource, double>();
            foreach (var kv in combinedCitizenConsumption)
                if (industryPlans.Count > 0)
                {
                    foreach (var ip in industryPlans)
                    {
                        if (ip.TotalOutputs.ContainsKey(kv.Key))
                        {
                            double produced = ip.TotalOutputs[kv.Key];
                            double consumed = kv.Value;
                            double balance = produced - consumed;
                            result.Add(kv.Key, balance);
                        }
                    }
                }
            return result;
        }
    }
    public Dictionary<Resource, double> net
    {
        get
        {
            Dictionary<Resource, double> r = new Dictionary<Resource, double>();
            if (industryPlans.Count > 0)
            {
                foreach (var plan in industryPlans)
                {
                    foreach (var kv in plan.TotalInputs)
                    {
                        if (r.ContainsKey(kv.Key)) r[kv.Key] -= kv.Value;
                        else r[kv.Key] = -kv.Value;
                    }
                    foreach (var kv in plan.TotalResidues)
                    {
                        if (r.ContainsKey(kv.Key)) r[kv.Key] += kv.Value;
                        else r[kv.Key] = kv.Value;
                    }
                }
            }
            return r;
        }
    }
    public Dictionary<SupportBuilding, int> combinedSupBldgs
    {
        get
        {
            Dictionary<SupportBuilding, int> r = new Dictionary<SupportBuilding, int>();
            if (SupportBuildings.Count > 0)
            {
                foreach (var kv in SupportBuildings)
                {
                    if (r.ContainsKey(kv.Building)) r[kv.Building] += kv.Count;
                    else r.Add(kv.Building, kv.Count);
                }
            }
            if (industryPlans.Count > 0)
            {
                foreach (var plan in industryPlans)
                {
                    foreach (var kv in plan.SupportBuildings)
                    {
                        if (r.ContainsKey(kv.Building)) r[kv.Building] += kv.Count;
                        else r.Add(kv.Building, kv.Count);
                    }
                }
            }
            return r;
        }
    }
    public Dictionary<AmenityBuilding, int> combinedAmeBldgs
    {
        get
        {
            Dictionary<AmenityBuilding, int> r = new Dictionary<AmenityBuilding, int>();
            if (microDistricts.Count > 0)
            {
                foreach (var m in microDistricts)
                {
                    foreach (var ai in m.AmenityBuildings)
                    {
                        if (r.ContainsKey(ai.Building)) r[ai.Building] += ai.Count;
                        else r.Add(ai.Building, ai.Count);
                    }
                }
            }
            return r;
        }
    }
    public Dictionary<TransportationBuilding, int> combinedTransBldgs
    {
        get
        {
            Dictionary<TransportationBuilding, int> r = new Dictionary<TransportationBuilding, int>();
            if (industryPlans.Count > 0)
            {
                foreach (var plan in industryPlans)
                {
                    foreach (var kv in plan.TransportationBuildings)
                    {
                        if (r.ContainsKey(kv.Building)) r[kv.Building] += kv.Count;
                        else r.Add(kv.Building, kv.Count);
                    }
                }
            }
            if (microDistricts.Count > 0)
            {
                foreach (var m in microDistricts)
                {
                    foreach (var ti in m.TransportBuildings)
                    {
                        if (r.ContainsKey(ti.Building)) r[ti.Building] += ti.Count;
                        else r.Add(ti.Building, ti.Count);
                    }
                }
            }
            return r;
        }
    }
    public Dictionary<Resource, double> combinedConstructionMaterials
    {
        get
        {
            Dictionary<Resource, double> r = new Dictionary<Resource, double>();
            if (industryPlans.Count > 0)
            {
                foreach (var plan in industryPlans)
                {
                    foreach (var kv in plan.TotalConstructionMaterials)
                    {
                        if (r.ContainsKey(kv.Key)) r[kv.Key] += kv.Value;
                        else r.Add(kv.Key, kv.Value);
                    }
                }
            }
            if (microDistricts.Count > 0)
            {
                foreach (var m in microDistricts)
                {
                    foreach (var kv in m.ConstructionMaterials)
                    {
                        if (r.ContainsKey(kv.Key)) r[kv.Key] += kv.Value;
                        else r.Add(kv.Key, kv.Value);
                    }
                }
            }
            if (UtilityPlans.Count > 0)
            {
                foreach (var p in UtilityPlans)
                {
                    foreach (var kv in p.ConstructionMaterials)
                    {
                        if (r.ContainsKey(kv.Key)) r[kv.Key] += kv.Value;
                        else r.Add(kv.Key, kv.Value);
                    }
                }
            }
            return r;
        }
    }
    public static SavedCity ConvertToSavedCity(City city)
    {
        var saved = new SavedCity
        {
            Name = city.Name,
            savedIndustryPlans = city.industryPlans
                .Select(p => SavedIndustryPlan.ConvertToSavedPlan(p))
                .ToList(),
            savedMicroDistricts = city.microDistricts
                .Select(m => MicroDistrict.ConvertToSavedMicroDistrict(m))
                .ToList(),
            savedUtilityPlans = city.UtilityPlans
                .Select(m => UtilityPlan.ConvertToSavedUtilityPlan(m))
                .ToList(),
            savedSupportBuildings = city.combinedSupBldgs
                .Select(i => new SavedCity.SavedSupportInstance
                {
                    BuildingName = i.Key.Name,
                    Count = i.Value
                }).ToList(),
            savedTransportationBuildings = city.combinedTransBldgs
                .Select(i => new SavedCity.SavedTransportationInstance
                {
                    BuildingName = i.Key.Name,
                    Count = i.Value
                }).ToList(),
        };
        return saved;
    }
    public static City ConvertFromSavedCity(SavedCity sc)
    {
        City c = new City();
        c.Name = sc.Name;
        c.industryPlans = sc.savedIndustryPlans?
            .Select(p => SavedIndustryPlan.ConvertFromSavedPlan(p))
            .ToList() ?? new List<IndustryPlan>();
        c.microDistricts = sc.savedMicroDistricts?
            .Select(p => MicroDistrict.ConvertFromSavedMicroDistrict(p))
            .ToList() ?? new List<MicroDistrict>();
        c.UtilityPlans = sc.savedUtilityPlans?
            .Select(p => SavedUtilityPlan.ConvertToUtilityPlan(p))
            .ToList() ?? new List<UtilityPlan>();
        c.SupportBuildings = sc.savedSupportBuildings?.Select(i => new SupportInstance
            {
                Building = GameData.AllSupportBuildings
                            .FirstOrDefault(sb => sb.Name == i.BuildingName),
                Count = i.Count
            }).ToList() ?? new List<SupportInstance>();
        c.TransportationBuildings = sc.savedTransportationBuildings?
            .Select(i => new TransportationInstance
            {
                Building = GameData.TransportationBuildings
                            .FirstOrDefault(tb => tb.Name == i.BuildingName),
                Count = i.Count
            }).ToList() ?? new List<TransportationInstance>();
        return c;
    }
}
