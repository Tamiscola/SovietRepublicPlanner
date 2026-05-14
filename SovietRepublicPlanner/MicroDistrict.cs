using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MicroDistrict
{
    public string Name { get; set; }
    public List<ResidentialInstance> ResidentialBuildings { get; set; } = new List<ResidentialInstance>();
    public List<AmenityInstance> AmenityBuildings { get; set; } = new List<AmenityInstance>();
    public List<TransportationInstance> TransportBuildings { get; set; } = new List<TransportationInstance>();
    public int TotalHousingCapacity
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
    public double GarbageProduction => TotalHousingCapacity * GarbagePerWorker;  // tons/day

    // Construction Materials
    public Dictionary<Resource, double> ConstructionMaterials { get; set; } = new Dictionary<Resource, double>();
    
    public static SavedMicroDistrict ConvertToSavedMicroDistrict(MicroDistrict microDistrict)
    {
        var saved = new SavedMicroDistrict
        {
            Name = microDistrict.Name,
            ResidentialBuildings = microDistrict.ResidentialBuildings
                                        .Select(rb => new SavedMicroDistrict.SavedResidentialInstance
                                        {
                                            BuildingName = rb.Building.Name,
                                            Count = rb.Count,
                                        }).ToList(),
            AmenityBuildings = microDistrict.AmenityBuildings
                                        .Select(ab => new SavedMicroDistrict.SavedAmenityInstance
                                        {
                                            BuildingName = ab.Building.Name,
                                            Count = ab.Count,
                                        }).ToList(),
            TransportBuildings = microDistrict.TransportBuildings
                                        .Select(tb => new SavedMicroDistrict.SavedTransportationInstance
                                        {
                                            BuildingName = tb.Building.Name,
                                            Count = tb.Count,
                                        }).ToList(),
            TotalHousingCapacity = microDistrict.TotalHousingCapacity,
            PowerConsumption = microDistrict.PowerConsumption,
            WaterConsumption = microDistrict.WaterConsumption,
            SewageProduction = microDistrict.SewageProduction,
            SewageDisposalCapacity = microDistrict.TotalHousingCapacity,
            HeatConsumption = microDistrict.HeatConsumption,
            GarbagePerWorker = microDistrict.GarbagePerWorker,
            GarbageProduction = microDistrict.GarbageProduction,
            ConstructionMaterials = microDistrict.ConstructionMaterials
                                        .Select(cm => new SavedMicroDistrict.SavedResourceInstance
                                        {
                                            Name = cm.Key.Name,
                                            Amount = cm.Value
                                        }).ToList(),

        };
        return saved;
    }
    public static MicroDistrict ConvertFromSavedMicroDistrict(SavedMicroDistrict savedMicroDistrict)
    {
        var md = new MicroDistrict
        {
            Name = savedMicroDistrict.Name,
            ResidentialBuildings = savedMicroDistrict.ResidentialBuildings
                                    .Select(sri => new ResidentialInstance
                                    {
                                        Building = GameData.AllResidentialBuildings
                                                    .FirstOrDefault(rb => rb.Name == sri.BuildingName),
                                        Count = sri.Count
                                    }).ToList(),
            AmenityBuildings = savedMicroDistrict.AmenityBuildings
                                    .Select(sai => new AmenityInstance
                                    {
                                        Building = GameData.AllAmenityBuildings
                                                    .FirstOrDefault(ab => ab.Name == sai.BuildingName),
                                        Count = sai.Count
                                    }).ToList(),
            TransportBuildings = savedMicroDistrict.TransportBuildings
                                    .Select(sti => new TransportationInstance
                                    {
                                        Building = GameData.AllTransportationBuildings
                                                    .FirstOrDefault(tb => tb.Name == sti.BuildingName),
                                        Count = sti.Count
                                    }).ToList(),
            PowerConsumption = savedMicroDistrict.PowerConsumption,
            WaterConsumption = savedMicroDistrict.WaterConsumption,
            SewageProduction = savedMicroDistrict.SewageProduction,
            SewageDisposalCapacity = savedMicroDistrict.SewageDisposalCapacity,
            HeatConsumption = savedMicroDistrict.HeatConsumption,
            GarbagePerWorker = savedMicroDistrict.GarbagePerWorker,
            ConstructionMaterials = savedMicroDistrict.ConstructionMaterials
                                    .ToDictionary(sri => GameData.AllResources
                                                            .FirstOrDefault(r => r.Name == sri.Name),
                                                  sri => sri.Amount)
        };
        return md;
    }
}
