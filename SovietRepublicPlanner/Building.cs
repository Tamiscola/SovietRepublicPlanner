using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public enum BuildingCategory
{
    Residential,
    Production,
    Amenity,
    Utility,
    Recycling,
    Support,
    Transport
}
public enum GeneralMetricType
{
    ConstructionCost,
    WorkDays,
    WorkersPerArea,
}
public enum UtilityType
{
    Power,
    Water,
    Sewage,
    Heat,
    Garbage
}
public abstract class Building
{
    // Identification
    public abstract BuildingCategory Category { get; }
    public string Name { get; set; }

    // Area
    public double Area { get; set; }   // Distance between dots : 5.5m

    // Construction
    public int WorkDays {  get; set; }
    public Dictionary<Resource, double> ConstructionMaterials { get; set; } = new Dictionary<Resource, double>();
    public double ConstructionCostRUB 
    { 
        get
        {
            double sum = 0;
            foreach (var r in ConstructionMaterials)
            {
                sum += (r.Key.ImportPriceRUB * r.Value);
            }
            return sum;
        } 
    }
    public double ConstructionCostUSD
    {
        get
        {
            double sum = 0;
            foreach (var r in ConstructionMaterials)
            {
                sum += (r.Key.ImportPriceUSD * r.Value);
            }
            return sum;
        }
    }
    public virtual double GetGeneralValue(GeneralMetricType metricType)
    {
        return metricType switch
        {
            GeneralMetricType.ConstructionCost => this.ConstructionCostRUB,
            GeneralMetricType.WorkDays => this.WorkDays,
            _ => 0
        };
    }

    // Utility
    public double PowerConsumptionMWh {  get; set; }
    public double ConsumptionWattageMW 
    {  
        get
        {
            return PowerConsumptionMWh / 60;
        } 
    }
    public virtual double GetUtilityValue(UtilityType type)
    {
        return type switch
        {
            UtilityType.Power => this.PowerConsumptionMWh,
            _ => 0
        };
    }
    public abstract IEnumerable<UtilityType> GetActiveUtilities();
}

