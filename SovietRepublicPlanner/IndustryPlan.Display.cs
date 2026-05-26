using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class IndustryPlan
{
    public void DisplayTotalReceipt()
    {
        if (ChosenBuilding == null) { Console.WriteLine("There's no saved plan!"); }
        else
        {
            Console.WriteLine("\n┌────────────────────────────────────────┐");
            Console.WriteLine("│         TOTAL RECEIPT                  │");
            Console.WriteLine("├────────────────────────────────────────┤");
            Console.WriteLine($"│ Target: {TargetResource.Name} {TargetAmount} t/day");
            Console.WriteLine("├────────────────────────────────────────┤");
            Console.WriteLine("│ Utilities Status:");
            Console.WriteLine("│                      Needed         Produced   Balance");
            Console.WriteLine($"│ Total Workers:      {TotalWorkers,8}");
            // Power
            string powerProduced = TotalPowerProduced > 0 ? $"{TotalPowerProduced,10:F2}" : "         —";
            double powerBalance = TotalPowerProduced > 0
                ? TotalPowerProduced - TotalPowerNeeded
                : -TotalPowerNeeded;
            string powerBalanceStr = TotalPowerProduced > 0
                ? $"{(powerBalance >= 0 ? "+" : "")}{powerBalance:F2}"
                : $"-{TotalPowerNeeded:F2}";
            Console.WriteLine($"│ Power (MW): {TotalPowerNeeded,16:F2}{powerProduced}  {powerBalanceStr,13}");
            // Water
            double waterInput = TotalUtilityNeeds.ContainsKey(GameData.WaterResource)
                ? TotalUtilityNeeds[GameData.WaterResource] - TotalWaterNeeded
                : 0;
            string waterLabel = waterInput > 0
                ? $"{TotalUtilityNeeds[GameData.WaterResource]:F2} ({TotalWaterNeeded:F2}+{waterInput:F2})"
                : $"{TotalWaterNeeded:F2}";
            string waterProduced = TotalWaterProduced > 0 ? $"{TotalWaterProduced,10:F2}" : "         —";
            double waterBalance = TotalWaterProduced > 0
                ? TotalWaterProduced - TotalUtilityNeeds[GameData.WaterResource]
                : -TotalWaterNeeded;
            string waterBalanceStr = TotalWaterProduced > 0
                ? $"{(waterBalance >= 0 ? "+" : "")}{waterBalance:F2}"
                : (TotalUtilityNeeds.ContainsKey(GameData.WaterResource)) ? $"-{TotalUtilityNeeds[GameData.WaterResource]:F2}" : "0.00";
            Console.WriteLine($"│ Water (t/day): {waterLabel,13}{waterProduced}  {waterBalanceStr,13}");
            // Heat
            string heatProduced = TotalHeatProduced > 0 ? $"{TotalHeatProduced,10:F2}" : "         —";
            double heatBalance = TotalHeatProduced > 0
                ? TotalHeatProduced - TotalHeatNeeded
                : -TotalHeatNeeded;
            string heatBalanceStr = TotalHeatProduced > 0
                ? $"{(heatBalance >= 0 ? "+" : "")}{heatBalance:F2}"
                : $"-{TotalHeatNeeded:F2}";
            Console.WriteLine($"│ Heat (MW):        {TotalHeatNeeded,10:F2}{heatProduced}  {heatBalanceStr,13}");
            // Sewage
            double sewageTreated = TotalUtilityNeeds.ContainsKey(GameData.WasteWaterResource)
                ? (ExpandedUtilities.Any(eu => eu.Key == GameData.WasteWaterResource) ? ExpandedUtilities[GameData.WasteWaterResource].TotalSewageDisposalCapacity : 0)
                : 0;
            if (SupportBuildings.Any(sb => sb.Building == GameData.SewageDischarge)) sewageTreated = TotalWaterProduced;
            string sewageProduced = TotalSewageProduced > 0 ? $"{TotalSewageProduced,10:F2}" : "         —";
            double sewageBalance = TotalSewageProduced > 0
                ? sewageTreated - TotalSewageProduced
                : -TotalSewageProduced;
            string sewageBalanceStr = TotalSewageProduced > 0
                ? $"{(sewageBalance >= 0 ? "+" : "")}{sewageBalance:F2}"
                : $"{TotalSewageProduced:F2}";
            Console.WriteLine($"│ Sewage (t/day):   {sewageProduced}{sewageTreated,10}  {sewageBalanceStr,13}");
            Console.WriteLine($"│ Garbage (t/day):  {TotalGarbageProduced,10:F2}        —  {TotalGarbageProduced,14:F2}");
            Console.WriteLine($"│ Pollution:        {TotalEnvironmentPollution,10:F6}        —  {TotalEnvironmentPollution,14:F6}");
            Console.WriteLine("├────────────────────────────────────────┤");
            Console.WriteLine("│ All Buildings in Chain:                │");
            Console.WriteLine("├────────────────────────────────────────┤");

            // Display all buildings recursively
            DisplayAllBuildings(this, 0);

            var totalSupBldgs = SupportBuildings;

            if (totalSupBldgs.Count > 0)
            {
                Console.WriteLine("├────────────────────────────────────────┤");
                Console.WriteLine("│ Support Infrastructures:               │");
                Console.WriteLine("├────────────────────────────────────────┤");
                foreach (var kv in totalSupBldgs)
                    Console.WriteLine($"│ · {kv.Count} × {kv.Building.Name}");
            }
            if (TransportationBuildings.Count() > 0)
            {
                Console.WriteLine("├────────────────────────────────────────┤");
                Console.WriteLine("│ Transportation Buildings:");
                Console.WriteLine("├────────────────────────────────────────┤");
                foreach (var ti in TransportationBuildings)
                    Console.WriteLine($"│ · {ti.Count} × {ti.Building.Name}");
            }
            Console.WriteLine("├────────────────────────────────────────┤");
            Console.WriteLine("│ Importing Resources:");
            foreach (var kv in TotalImports)
                Console.WriteLine($"│ ·{kv.Value,6:F2}t/day {kv.Key.Name,-20}");
            Console.WriteLine("├────────────────────────────────────────┤");
            Console.WriteLine("│ Resource Residues:");
            foreach (var kv in TotalResidues)
            {
                double displayResidue = kv.Value;

                // If this is the target resource and we have a target, show excess only
                if (kv.Key == this.TargetResource && this.TargetAmount > 0)
                    displayResidue -= this.TargetAmount;

                if (kv.Key != GameData.CropsResource)
                    Console.WriteLine($"│ ·{displayResidue,6:F2}t/day {kv.Key.Name,-20}");
                else
                    Console.WriteLine($"│ ·{displayResidue,6:F2}t/yr {kv.Key.Name,-20}");
            }
            if (TotalConstructionMaterials.Count() > 0)
            {
                Console.WriteLine("├────────────────────────────────────────┤");
                Console.WriteLine("│ Total Construction Materials:");
                Console.WriteLine("├────────────────────────────────────────┤");
                foreach (var kv in TotalConstructionMaterials)
                    Console.WriteLine($"│ · {kv.Value:F2} × {kv.Key.Name}");
                Console.WriteLine("└────────────────────────────────────────┘");
            }
        }
    }

    // Helper method to recursively list buildings
    public void DisplayAllBuildings(IndustryPlan result, int depth)
    {
        string indent = new string(' ', depth * 2);
        string supportIndent = indent + "  └";
        if (result.TargetResource.Name == "Crops")
        {
            Dictionary<ProductionBuilding, int> fieldsNeeded = new Dictionary<ProductionBuilding, int>();
            Dictionary<ProductionBuilding, int> farmsNeeded = new Dictionary<ProductionBuilding, int>();
            for (int i = 0; i < result.Buildings.Count(); i++)
            {
                foreach (var bi in result.Buildings[i].BuildingInstances)
                {
                    if (bi.Building.Name.Contains("Field"))
                        if (fieldsNeeded.ContainsKey(bi.Building))
                            fieldsNeeded[bi.Building]++;
                        else fieldsNeeded.Add(bi.Building, 1);
                    else if (bi.Building.Name.Contains("Farm"))
                        if (farmsNeeded.ContainsKey(bi.Building))
                            farmsNeeded[bi.Building]++;
                        else farmsNeeded.Add(bi.Building, 1);
                }
            }
            foreach (var k in fieldsNeeded)
                Console.WriteLine($"│ {indent}· {k.Value} × {k.Key.Name}");
            foreach (var k in farmsNeeded)
                Console.WriteLine($"│ {indent}· {k.Value} × {k.Key.Name}");
            foreach (var subChain in result.SubChains)
            {
                DisplayAllBuildings(subChain, depth + 1);
            }
        }
        else
        {
            // Display THIS level's building
            if (result.ChosenBuilding != null)
                Console.WriteLine($"│ {indent}· {result.ChosenBuilding.Count} × {result.ChosenBuilding.Building.Name}");

            // Aggregate SubChains at the NEXT level
            Dictionary<ProductionBuilding, int> subChainBuildings = new Dictionary<ProductionBuilding, int>();

            foreach (var subChain in result.SubChains)
            {
                if (subChain.ChosenBuilding != null)
                {
                    var building = subChain.ChosenBuilding.Building;
                    if (subChainBuildings.ContainsKey(building))
                        subChainBuildings[building] += subChain.ChosenBuilding.Count;
                    else
                        subChainBuildings.Add(building, subChain.ChosenBuilding.Count);
                }
            }

            // Display aggregated SubChain buildings
            string subIndent = new string(' ', (depth + 1) * 2);
            foreach (var kv in subChainBuildings)
                Console.WriteLine($"│ {subIndent}· {kv.Value} × {kv.Key.Name}");

            // Now recurse into SubChains' SubChains (depth + 2)
            foreach (var subChain in result.SubChains)
            {
                foreach (var subSubChain in subChain.SubChains)
                {
                    DisplayAllBuildings(subSubChain, depth + 2);
                }
            }
        }
    }
}
