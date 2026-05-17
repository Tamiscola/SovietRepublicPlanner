using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
public partial class IndustryPlan
{
    public string Name { get; set; }
    public City ParentCity { get; set; }

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

    // Selected SupportBuildings and their amount
    public List<SupportInstance> SupportBuildings { get; set; } = new List<SupportInstance>();     

    // Selected TransportationBuildings and their amount
    public List<TransportationInstance> TransportationBuildings = new List<TransportationInstance>();
    public int TotalBuildings { get; set; }
}