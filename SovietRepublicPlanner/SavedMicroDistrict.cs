using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SavedMicroDistrict
{
    public string Name { get; set; }
    public SavedCity ParentCity { get; set; }
    public List<SavedResidentialInstance> ResidentialBuildings { get; set; } = new List<SavedResidentialInstance>();
    public List<SavedAmenityInstance> AmenityBuildings { get; set; } = new List<SavedAmenityInstance>();
    public List<SavedTransportationInstance> TransportBuildings { get; set; } = new List<SavedTransportationInstance>();
    public List<SavedSupportInstance> SupportBuildings { get; set; } = new List<SavedSupportInstance>();
    public int TotalHousingCapacity { get; set; }
    public double PowerConsumption { get; set; }        // MW
    public double WaterConsumption { get; set; }        // ㎥/day
    public double SewageProduction { get; set; }        // ㎥/day
    public double SewageDisposalCapacity { get; set; } = 0;  // ㎥/day
    public double HeatConsumption { get; set; }         // Gcal/day
    public double GarbagePerWorker { get; set; }        // Garbage production per worker
    public double GarbageProduction { get; set; }  // tons/day
    public List<SavedResourceInstance> ConstructionMaterials { get; set; }
    public class SavedResidentialInstance
    {
        public string BuildingName { get; set; }
        public int Count { get; set; }
    }
    public class SavedAmenityInstance
    {
        public string Name { get; set; }
        public string BuildingName { get; set; }
        public int CurNumEmp { get; set; }
        public int Count { get; set; }
    }
    public class SavedSupportInstance
    {
        public string BuildingName { get; set; }
        public int Count { get; set; }
    }
    public class SavedTransportationInstance
    {
        public string BuildingName { get; set; }
        public int Count { get; set; }
    }
    public class SavedResourceInstance
    {
        public string Name { get; set; }
        public double Amount { get; set; }
    }
}

