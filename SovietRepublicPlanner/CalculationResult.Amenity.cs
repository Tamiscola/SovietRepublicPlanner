using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
partial class CalculationResult
{
    public List<AmenityInstance> AmenityBuildings = new List<AmenityInstance>();

    // Amenity Rough coverage (guideline only)
    public Dictionary<AmenityType, int> GetAmenCoverage()
    {
        var coverage = new Dictionary<AmenityType, int>();

        foreach (var type in Enum.GetValues(typeof(AmenityType)))
        {
            var buildings = AmenityBuildings.Where(a => a.Building.Type == (AmenityType)type);
            int totalCapacity = buildings.Sum(a => a.TotalCapacity);
            coverage[(AmenityType)type] = totalCapacity;
        }
        return coverage;
    }

    public class AmenityCoverage
    {
        public Dictionary<Resource, int> ProductCoverage { get; set; } = new Dictionary<Resource, int>();
        public Dictionary<AmenityType, int> ServiceCoverage { get; set; } = new Dictionary<AmenityType, int>();
        public int KindergartenCapacity { get; set; }
        public int SchoolCapacity { get; set; }
        public int UniversityCapacity { get; set; }
    }
    public AmenityCoverage GetAmenityCoverage()
    {
        var coverage = new AmenityCoverage();

        // Initialize consumable products
        var consumables = new[] { GameData.FoodResource, GameData.MeatResource,
                              GameData.ClothesResource, GameData.AlcoholResource,
                              GameData.ElectronicsResource };
        foreach (var resource in consumables)
            coverage.ProductCoverage[resource] = 0;

        // Initialize service types
        foreach (AmenityType type in Enum.GetValues(typeof(AmenityType)))
            coverage.ServiceCoverage[type] = 0;


        // Calculate coverage
        foreach (var amenity in AmenityBuildings)
        {
            // Product-based coverage (Shopping, Pub)
            if (amenity.Building.ProductsOffered.Count > 0)
            {
                foreach (var product in amenity.Building.ProductsOffered)
                {
                    if (coverage.ProductCoverage.ContainsKey(product))
                        coverage.ProductCoverage[product] += amenity.TotalCapacity;
                }
            }

            // Service-based coverage (all amenity types)
            coverage.ServiceCoverage[amenity.Building.Type] += amenity.TotalCapacity;

            // Education subtype tracking
            if (amenity.Building.Type == AmenityType.Education)
            {
                switch (amenity.Building.EducationLevel)
                {
                    case EducationSubtype.Kindergarten:
                        coverage.KindergartenCapacity += amenity.TotalCapacity;
                        break;
                    case EducationSubtype.School:
                        coverage.SchoolCapacity += amenity.TotalCapacity;
                        break;
                    case EducationSubtype.University:
                        coverage.UniversityCapacity += amenity.TotalCapacity;
                        break;
                        // UniversityDorm doesn't count - it's housing, not essential service
                }
            }
        }
        return coverage;
    }
    private int CalculateWorkersForPercentageAmenity(AmenityBuilding building, int totalWorkers, int totalCitizens)
    {
        if (!building.UsesPercentageBasedDemand)
        {
            // Should not be called, but fallback
            return building.EffectiveWorkersPerShift * 3;
        }

        // Determine population served based on building type
        int populationServed = building.ServesPopulationType switch
        {
            PopulationType.Workers => totalWorkers,
            PopulationType.Citizen => totalCitizens,
            PopulationType.Children => (int)(totalCitizens * 0.165),  // 16.5% from your data
            PopulationType.YoungAdults => (int)(totalCitizens * 0.065), // 6.5% from your data
            _ => totalCitizens
        };

        // Calculate actual demand
        double actualDemand = populationServed * building.PopulationPercentageServed;

        // How many buildings needed to serve this demand?
        double buildingsNeeded = actualDemand / (building.MaxVisitors > 0 ? building.MaxVisitors : 1);

        // Workers needed = buildings needed × workers per building × 3 shifts
        int workersPerBuilding = building.EffectiveWorkersPerShift * 3;

        return (int)Math.Ceiling(buildingsNeeded * workersPerBuilding);
    }
}
