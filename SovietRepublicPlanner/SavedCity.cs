using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SavedCity
{
    public string Name { get; set; }
    public List<SavedIndustryPlan>? savedIndustryPlans { get; set; }
    public List<SavedMicroDistrict>? savedMicroDistricts { get; set; }
    public List<SavedUtilityPlan>? savedUtilityPlans { get; set; }
    public List<SavedSupportInstance>? savedSupportBuildings { get; set; }
    public class SavedSupportInstance
    {
        public string BuildingName { get; set; }
        public int Count { get; set; }
    }
    public List<SavedTransportationInstance> savedTransportationBuildings { get; set; }
    public class SavedTransportationInstance
    {
        public string BuildingName { get; set; }
        public int Count { get; set; }
    }
    public List<SavedResidentialInstance> savedResidentialBuildings { get; set; }
    public class SavedResidentialInstance
    {
        public string BuildingName { get; set; }
        public int Count { get; set; }
    }
    public List<SavedAmenityInstance> savedAmenityBuildings { get; set; }
    public class SavedAmenityInstance
    {
        public string BuildingName { get; set; }
        public int Count { get; set; }
    }
    public List<SavedResourceInstance> savedConstructionMaterials { get; set; }
    public class SavedResourceInstance
    {
        public string Name { get; set; }
        public double Amount { get; set; }
    }
}