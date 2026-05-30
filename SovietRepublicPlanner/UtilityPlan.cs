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
    public List<UtilityInstance> Buildings { get; set; }
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
            Dictionary<Resource, double> r= new Dictionary<Resource, double>();
            foreach (UtilityInstance instance in Buildings)
            {
                foreach (var ra in instance.Building.Inputs)
                {
                    if (r.ContainsKey(ra.Resource)) r[ra.Resource] += ra.Amount;
                    else r.Add(ra.Resource, ra.Amount);
                }
            }
            return r;
        }
    }
    public Dictionary<Resource, double> TotalOutputs
    {
        get
        {
            Dictionary<Resource, double> r = new Dictionary<Resource, double>();
            foreach (UtilityInstance instance in Buildings)
            {
                foreach (var ra in instance.Building.Inputs)
                {
                    if (r.ContainsKey(ra.Resource)) r[ra.Resource] += ra.Amount;
                    else r.Add(ra.Resource, ra.Amount);
                }
            }
            return r;
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

    // Transport-specific
    public int? TotalParkingSpots;  // nullable - not all have this
    public double? TotalFuelStorageCapacity;  // nullable - only gas stations/end stations
    public int? TotalPassengerCapacity;  // nullable - only stops/platforms
}
