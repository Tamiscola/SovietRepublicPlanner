using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
public partial class IndustryPlan
{
    // Target
    public Resource TargetResource { get; set; }
    public double TargetAmount { get; set; }

    // Settings
    public double WorkersProductivity { get; set; } = 100.0;

    // Buildings
    
    // Buildings(candidates) that produce the target resource:
    // User selects an option from this list.
    public List<BuildingRequirement> Buildings { get; set; } = new List<BuildingRequirement>();

    // Selected ProductionBuilding
    public BuildingRequirement ChosenBuilding { get; set; }     

    // Infrastructures for ProductionBuildings
    // User selects an option from this list.
    public Dictionary<ProductionBuilding, int> AllSupportBuildings
    {
        get
        {
            Dictionary<ProductionBuilding, int> all = new Dictionary<ProductionBuilding, int>();

            // Add this level's support buildings
            foreach (var br in SupportBuildings)
            {
                if (all.ContainsKey(br.Building))
                    all[br.Building] += br.Count;
                else
                    all.Add(br.Building, br.Count);
            }

            // Recursively add from SubChains
            foreach (var sub in SubChains)
            {
                foreach (var kv in sub.AllSupportBuildings)
                {
                    if (all.ContainsKey(kv.Key))
                        all[kv.Key] += kv.Value;
                    else
                        all.Add(kv.Key, kv.Value);
                }
            }

            return all;
        }
    }

    // Selected SupportBuildings
    public List<BuildingRequirement> SupportBuildings { get; set; } = new List<BuildingRequirement>();     

    // Selected TransportationBuildings and their amount
    public List<TransportationInstance> TransportationBuildings = new List<TransportationInstance>();
    public int TotalBuildings { get; set; }

    // Helper methods
    public bool RemoveSupportBuilding(ProductionBuilding building)
    {
        // Remove ALL instances from this level
        int removedCount = SupportBuildings.RemoveAll(br => br.Building == building);

        // ALSO search all SubChains (don't stop early!)
        bool foundInSubChain = false;
        foreach (var subChain in SubChains)
        {
            if (subChain.RemoveSupportBuilding(building))
                foundInSubChain = true;
        }

        // Return true if we found it at ANY level
        return (removedCount > 0 || foundInSubChain);
    }
}