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
    public List<CalculationResult> plans = new List<CalculationResult>();

    // Initialize totals
    public int totalWorkers
    {
        get
        {
            int total = 0;
            foreach (var plan in plans) total += plan.TotalWorkers;
            return total;
        }
    }
    public int totalCitizen {
        get
        {
            int total = 0;
            foreach (var plan in plans) total += plan.TotalPopulationNeeded;
            return total;
        }
    }
    public double totalPower
    {
        get
        {
            double total = 0;
            foreach (var plan in plans) total += plan.TotalPowerNeeded;
            return total;
        }
    }
    public double totalWater
    {
        get
        {
            double total = 0;
            foreach (var plan in plans) total += plan.TotalWaterNeeded;
            return total;
        }
    }
    public double totalHeat
    {
        get
        {
            double total = 0;
            foreach (var plan in plans) total += plan.TotalHeatNeeded;
            return total;
        }
    }
    public double totalGarbage
    {
        get
        {
            double total = 0;
            foreach (var plan in plans) total += plan.TotalGarbageProduced;
            return total;
        }
    }
    public double totalPollution
    {
        get
        {
            double total = 0;
            foreach (var plan in plans) total+= plan.TotalEnvironmentPollution;
            return total;
        }
    }
    public int totalHousingCapacity
    {
        get
        {
            int total = 0;
            foreach (var plan in plans) total+= plan.TotalHousingCapacity;
            return total;
        }
    }
    public Dictionary<Resource, double> utilityProduction {
        get
        {
            Dictionary<Resource, double> r = new Dictionary<Resource, double>();
            foreach (var plan in plans)
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
            foreach (var plan in plans)
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
            foreach (var plan in plans)
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
            Dictionary<Resource, double> r = new Dictionary<Resource, double>();
            foreach (var plan in plans)
            {
                foreach (var kv in plan.TotalCitizenConsumption)
                {
                    if (r.ContainsKey(kv.Key)) { r[kv.Key] += kv.Value; }
                    else r[kv.Key] = kv.Value;
                }
            }
            return r;
        }
    }
    public Dictionary<Resource, double> net
    {
        get
        {
            Dictionary<Resource, double> r = new Dictionary<Resource, double>();
            foreach (var plan in plans)
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
    public Dictionary<ProductionBuilding, int> combinedSupBldgs
    {
        get
        {
            Dictionary<ProductionBuilding, int> r = new Dictionary<ProductionBuilding, int>();
            foreach (var plan in plans)
            {
                foreach (var kv in plan.AllSupportBuildings)
                {
                    if (r.ContainsKey(kv.Key)) r[kv.Key] += kv.Value;
                    else r.Add(kv.Key, kv.Value);
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
            foreach (var plan in plans)
            {
                foreach (var kv in plan.AmenityBuildings)
                {
                    if (r.ContainsKey(kv.Building)) r[kv.Building] += kv.Count;
                    else r.Add(kv.Building, kv.Count);
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
            foreach (var plan in plans)
            {
                foreach (var kv in plan.TransportationBuildings)
                {
                    if (r.ContainsKey(kv.Building)) r[kv.Building] += kv.Count;
                    else r.Add(kv.Building, kv.Count);
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
            foreach (var plan in plans)
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
            plans = city.plans
                .Select(p => SavedPlan.ConvertToSavedPlan(p))
                .ToList(),
        };
        return saved;
    }
    public static City ConvertFromSavedCity(SavedCity sc)
    {
        City c = new City();
        c.Name = sc.Name;
        c.plans = sc.plans
            .Select(p => SavedPlan.ConvertFromSavedPlan(p))
            .ToList();
        return c;
    }
}
