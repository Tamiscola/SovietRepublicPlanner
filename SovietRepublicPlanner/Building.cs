using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class Building
{
    public string Name { get; set; }
    public int WorkDays {  get; set; }
    public Dictionary<Resource, double> ConstructionMaterials { get; set; }
    public double PowerConsumptionMWh {  get; set; }
    public double ConsumptionWattageMW 
    {  
        get
        {
            return PowerConsumptionMWh / 60;
        } 
    }
}

