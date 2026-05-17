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
    public List<IndustryPlan> industryPlans = new List<IndustryPlan>();
    public List<MicroDistrict> microDistricts = new List<MicroDistrict>();

    // Initialize totals
    public int totalWorkers
    {
        get
        {
            int total = 0;
            foreach (var plan in industryPlans) total += plan.TotalWorkers;
            return total;
        }
    }
    public int totalCitizen {
        get
        {
            int total = 0;
            foreach (var plan in industryPlans) total += plan.TotalPopulationNeeded;
            return total;
        }
    }
    public double totalPower
    {
        get
        {
            double total = 0;
            foreach (var plan in industryPlans) total += plan.TotalPowerNeeded;
            return total;
        }
    }
    public double totalWater
    {
        get
        {
            double total = 0;
            foreach (var plan in industryPlans) total += plan.TotalWaterNeeded;
            return total;
        }
    }
    public double totalHeat
    {
        get
        {
            double total = 0;
            foreach (var plan in industryPlans) total += plan.TotalHeatNeeded;
            return total;
        }
    }
    public double totalGarbage
    {
        get
        {
            double total = 0;
            foreach (var plan in industryPlans) total += plan.TotalGarbageProduced;
            return total;
        }
    }
    public double totalPollution
    {
        get
        {
            double total = 0;
            foreach (var plan in industryPlans) total+= plan.TotalEnvironmentPollution;
            return total;
        }
    }
    public int totalHousingCapacity
    {
        get
        {
            int total = 0;
            foreach (var d in microDistricts) total+= d.TotalHousingCapacity;
            return total;
        }
    }
    public Dictionary<Resource, double> utilityProduction {
        get
        {
            Dictionary<Resource, double> r = new Dictionary<Resource, double>()
            {
                {GameData.PowerResource, 0},
                {GameData.WaterResource, 0},
                {GameData.WasteWaterResource, 0},
                {GameData.HeatResource, 0},
            };
            foreach (var plan in industryPlans)
            {
                foreach (var kv in plan.ExpandedUtilities)
                {
                    foreach (var kv2 in kv.Value.ExpectedOutput)
                    {
                        if (r.ContainsKey(kv2.Key))
                            r[kv2.Key] += kv2.Value;
                        else
                            r[kv2.Key] = kv2.Value;
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
            foreach (var plan in industryPlans)
            {
                foreach(var kv in plan.TotalImports)
                {
                    if (r.ContainsKey(kv.Key)) r[kv.Key] += kv.Value;
                    else r[kv.Key] = kv.Value;
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
            foreach (var plan in industryPlans)
            {
                foreach (var kv in plan.TotalResidues)
                {
                    if (r.ContainsKey(kv.Key)) r[kv.Key] += kv.Value;
                    else r[kv.Key] = kv.Value;
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
            return result;
        }
    }
    public Dictionary<Resource, double> net
    {
        get
        {
            Dictionary<Resource, double> r = new Dictionary<Resource, double>();
            foreach (var plan in industryPlans)
            {
                foreach (var kv in plan.TotalImports)
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
            return r;
        }
    }
    public Dictionary<SupportBuilding, int> combinedSupBldgs
    {
        get
        {
            Dictionary<SupportBuilding, int> r = new Dictionary<SupportBuilding, int>();
            foreach (var plan in industryPlans)
            {
                foreach (var kv in plan.SupportBuildings)
                {
                    if (r.ContainsKey(kv.Building)) r[kv.Building] += kv.Count;
                    else r.Add(kv.Building, kv.Count);
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
            foreach (var m in microDistricts)
            {
                foreach (var ai in m.AmenityBuildings)
                {
                    if (r.ContainsKey(ai.Building)) r[ai.Building] += ai.Count;
                    else r.Add(ai.Building, ai.Count);
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
            foreach (var plan in industryPlans)
            {
                foreach (var kv in plan.TransportationBuildings)
                {
                    if (r.ContainsKey(kv.Building)) r[kv.Building] += kv.Count;
                    else r.Add(kv.Building, kv.Count);
                }
            }
            foreach (var m in microDistricts)
            {
                foreach (var ti in m.TransportBuildings)
                {
                    if (r.ContainsKey(ti.Building)) r[ti.Building] += ti.Count;
                    else r.Add(ti.Building, ti.Count);
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
            foreach (var plan in industryPlans)
            {
                foreach(var kv in plan.TotalConstructionMaterials)
                {
                    if (r.ContainsKey(kv.Key)) r[kv.Key] += kv.Value;
                    else r.Add(kv.Key, kv.Value);
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
        };
        return saved;
    }
    public static City ConvertFromSavedCity(SavedCity sc)
    {
        City c = new City();
        c.Name = sc.Name;
        c.industryPlans = sc.savedIndustryPlans
            .Select(p => SavedIndustryPlan.ConvertFromSavedPlan(p))
            .ToList();
        return c;
    }
}
