using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MicroDistrict
{
    public string Name { get; set; }
    public List<ResidentialInstance> ResidentialBuildings { get; set; } = new List<ResidentialInstance>();
    public int Population
    {
        get
        {
            int r = 0;
            for (int i = 0; i < ResidentialBuildings.Count; i++)
                r += ResidentialBuildings[i].Building.WorkerCapacity;
            return r;
        }
    }
    // Utilities
    public double PowerConsumption { get; set; }        // MW
    public double WaterConsumption { get; set; }        // ㎥/day
    public double SewageProduction { get; set; }        // ㎥/day
    public double SewageDisposalCapacity { get; set; } = 0;  // ㎥/day
    public double HeatConsumption { get; set; }         // Gcal/day
    public double GarbagePerWorker { get; set; }        // Garbage production per worker
    public double GarbageProduction => Population * GarbagePerWorker;  // tons/day
}
