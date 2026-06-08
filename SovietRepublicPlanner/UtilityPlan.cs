using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class UtilityPlan
{
    // Identification
    public string Name { get; set; }
    public City ParentCity { get; set; }
    public List<UtilityInstance> Buildings { get; set; } = new List<UtilityInstance>();
    public List<SupportInstance> SupportBuildings { get; set; } = new List<SupportInstance>();
    public UtilityType Type;

    // Workers & Resources
    public int TotalWorkers {
        get 
        {
            int r = 0;
            foreach (UtilityInstance instance in Buildings)
            {
                r += instance.Building.TotalWorkers * instance.Count;
            }
            return r;
        }
    }
    public Dictionary<Resource, double> TotalInputs
    {
        get
        {
            Dictionary<Resource, double> result = new Dictionary<Resource, double>();
            foreach (UtilityInstance instance in Buildings)
            {
                foreach (var o in instance.Building.Inputs)
                {
                    if (result.ContainsKey(o.Resource)) result[o.Resource] += o.Amount;
                    else result.Add(o.Resource, o.Amount);
                }
            }
            return result;
        }
    }
    public Dictionary<Resource, double> TotalOutputs
    {
        get
        {
            Dictionary<Resource, double> result = new Dictionary<Resource, double>();
            foreach (UtilityInstance instance in Buildings)
            {
                foreach (var o in instance.Building.Outputs)
                {
                    if (result.ContainsKey(o.Resource)) result[o.Resource] += o.Amount;
                    else result.Add(o.Resource, o.Amount);
                }
            }
            return result;
        }
    }

    // Utilities 
    public double TotalPowerConsumptionMWh
    {
        get
        {
            double r = 0;
            foreach (UtilityInstance instance in Buildings)
            {
                r += instance.Building.PowerConsumptionMWh * instance.Count;
            }
            return r;
        }
    }
    public double TotalWaterConsumptionM3
    {
        get
        {
            double r = 0;
            foreach (UtilityInstance instance in Buildings)
            {
                r += instance.Building.WaterConsumptionM3 * instance.Count;
            }
            return r;
        }
    }
    public double TotalSewageProductionM3 => TotalWaterConsumptionM3;
    public double TotalSewageDisposalCapacity
    {
        get
        {
            double r = 0;
            foreach (UtilityInstance instance in Buildings)
            {
                r += instance.Building.SewageDisposalCapacity * instance.Count;
            }
            return r;
        }
    }
    public double TotalHeatConsumptionM3
    {
        get
        {
            double r = 0;
            foreach (UtilityInstance instance in Buildings)
            {
                r += instance.Building.HeatConsumptionM3 * instance.Count;
            }
            return r;
        }
    }
    public double TotalEnvironmentPollution
    {
        get
        {
            double r = 0;
            foreach (UtilityInstance instance in Buildings)
            {
                r += instance.Building.EnvironmentPollution * instance.Count;
            }
            return r;
        }
    }
    public double TotalGarbageProduction 
    {
        get
        {
            double r = 0;
            foreach (UtilityInstance instance in Buildings)
            {
                r += instance.Building.GarbageProduction * instance.Count;
            }
            return r;
        }
    }

    // Construction
    public Dictionary<Resource, double> ConstructionMaterials { get; set; } = new Dictionary<Resource, double>();

    // Transport-specific
    public int? TotalParkingSpots;  // nullable - not all have this
    public double? TotalFuelStorageCapacity;  // nullable - only gas stations/end stations
    public int? TotalPassengerCapacity;  // nullable - only stops/platforms

    public static SavedUtilityPlan ConvertToSavedUtilityPlan(UtilityPlan plan)
    {
        if (plan == null) { Console.WriteLine("No plan to convert."); return null; }
        var s = new SavedUtilityPlan()
        {
            Buildings = plan.Buildings.Select(b => new SavedUtilityPlan.SavedUtilityInstance()
            {
                BuildingName = b.Building.Name,
                Count = b.Count,
            }).ToList(),
            SavedSupportBuildings = plan.SupportBuildings.Select(s => new SavedIndustryPlan.SavedSupportInstance()
            {
                BuildingName = s.Building.Name,
                Count = s.Count,
            }).ToList(),
            Type = plan.Type,
            TotalWorkers = plan.TotalWorkers,
            PowerConsumption = plan.TotalPowerConsumptionMWh,
            WaterConsumption = plan.TotalWaterConsumptionM3,
            SewageProduction = plan.TotalSewageProductionM3,
            SewageDisposalCapacity = plan.TotalSewageDisposalCapacity,
            HeatConsumption = plan.TotalHeatConsumptionM3,
            GarbageProduction = plan.TotalGarbageProduction,
            ConstructionMaterials = plan.ConstructionMaterials.Select(c =>  new SavedUtilityPlan.SavedResourceInstance()
            {
                Name = c.Key.Name,
                Amount = c.Value,
            }).ToList(),
        };
        return s;
    }
}
