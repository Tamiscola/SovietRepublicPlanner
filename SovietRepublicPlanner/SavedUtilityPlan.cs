using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SavedUtilityPlan
{
    public string Name { get; set; }
    public SavedCity ParentCity { get; set; }
    public List<SavedUtilityInstance> Buildings { get; set; }
    public List<SavedIndustryPlan.SavedSupportInstance> SavedSupportBuildings { get; set; }
    public UtilityType Type { get; set; }
    public int TotalWorkers { get; set; }
    public double PowerConsumption { get; set; }        // MW
    public double WaterConsumption { get; set; }        // ㎥/day
    public double SewageProduction { get; set; }        // ㎥/day
    public double SewageDisposalCapacity { get; set; } = 0;  // ㎥/day
    public double HeatConsumption { get; set; }         // Gcal/day
    public double GarbageProduction { get; set; }  // tons/day
    public List<SavedResourceInstance> ConstructionMaterials { get; set; }
    public class SavedUtilityInstance
    {
        public string BuildingName { get; set; }
        public int Count { get; set; }
    }
    public class SavedResourceInstance
    {
        public string Name { get; set; }
        public double Amount { get; set; }
    }
    public static UtilityPlan ConvertToUtilityPlan(SavedUtilityPlan plan)
    {
        if (plan == null) { Console.WriteLine("No plan to convert."); return null; }
        var p = new UtilityPlan()
        {
            Name = plan.Name,
            ParentCity = City.ConvertFromSavedCity(plan.ParentCity),
            Buildings = plan.Buildings.Select(su => new UtilityInstance()
            {
                Building = (UtilityBuilding)GameData.AllUtilityBuildings.Where(u => u.Name == su.BuildingName),
                Count = su.Count
            }).ToList(),
            SupportBuildings = plan.SavedSupportBuildings.Select(ssb => new SupportInstance()
            {
                Building = (SupportBuilding)GameData.AllSupportBuildings.Where(sb => sb.Name == ssb.BuildingName),
                Count = ssb.Count
            }).ToList(),
            Type = plan.Type,
        };
        return p;
    }
}
