using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class Building
{
    public string Name { get; set; }
    public int WorkDays {  get; set; }
    public double Area { get; set; }   // Distance between dots : 5.5m
    public Dictionary<Resource, double> ConstructionMaterials { get; set; }
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
    public double PowerConsumptionMWh {  get; set; }
    public double ConsumptionWattageMW 
    {  
        get
        {
            return PowerConsumptionMWh / 60;
        } 
    }
}

