using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class IndustryPlan
{
    // Utilities - Requirement
    public double TotalPowerNeeded
    {
        get
        {
            double thisLevel = (ChosenBuilding.BuildingInstances.Count() > 0 ? ChosenBuilding.BuildingInstances.Sum(bi => bi.Building.PowerConsumption) : ChosenBuilding.TotalPowerNeeded);
            double subChainTotal = SubChains.Sum(sc => sc.TotalPowerNeeded);
            double supportTotal = AllSupportBuildings.Sum(kv => kv.Key.PowerConsumption * kv.Value);
            double residentialTotal = ResidentialBuildings.Sum(rb => rb.Building.PowerMW * rb.Count);
            double amenTotal = AmenityBuildings.Sum(a => a.Building.PowerConsumptionMWh * a.Count);
            double transTotal = TransportationBuildings.Sum(t => t.Building.PowerConsumptionMWh * t.Count);
            return thisLevel + subChainTotal + supportTotal + residentialTotal + amenTotal + transTotal;
        }
    }
    public double TotalWaterNeeded
    {
        get
        {
            double thisLevel = ChosenBuilding.TotalWaterNeeded;
            double subChainTotal = SubChains.Sum(sc => sc.TotalWaterNeeded);
            double supportTotal = AllSupportBuildings.Sum(kv => kv.Key.WaterConsumption * kv.Value);
            double residentialTotal = ResidentialBuildings.Sum(rb => rb.Building.WaterPerDay * rb.Count);
            double amenTotal = AmenityBuildings.Sum(a => a.Building.WaterConsumptionM3 * a.Count);
            return thisLevel + subChainTotal + supportTotal + residentialTotal + amenTotal;
        }
    }
    public double TotalSewageProduced
    {
        get
        {
            double thisLevel = ChosenBuilding.TotalSewageProduced;
            double subChainTotal = SubChains.Sum(sc => sc.TotalSewageProduced);
            double supportTotal = AllSupportBuildings.Sum(kv => kv.Key.SewageProduction * kv.Value);
            double residentialTotal = ResidentialBuildings.Sum(rb => rb.Building.WaterPerDay * rb.Count);
            double amenTotal = AmenityBuildings.Sum(a => a.Building.WaterConsumptionM3 * a.Count);
            return thisLevel + subChainTotal + supportTotal + residentialTotal;
        }
    }
    public double TotalHeatNeeded
    {
        get
        {
            double thisLevel = ChosenBuilding.TotalHeatNeeded;
            double subChainTotal = SubChains.Sum(sc => sc.TotalHeatNeeded);
            double supportTotal = AllSupportBuildings.Sum(kv => kv.Key.HeatConsumption * kv.Value);
            double residentialTotal = ResidentialBuildings.Sum(rb => rb.Building.HeatTankM3 * rb.Count);
            double amenTotal = AmenityBuildings.Sum(a => a.Building.HotWaterTankM3 * a.Count);
            return thisLevel + subChainTotal + supportTotal + residentialTotal + amenTotal;
        }
    }
    public double TotalGarbageProduced
    {
        get
        {
            double thisLevel = ChosenBuilding.TotalGarbageProduced;
            double subChainTotal = SubChains.Sum(sc => sc.TotalGarbageProduced);
            double residentialTotal = ResidentialBuildings.Sum(rb => rb.Building.GarbageProduction * rb.Count);
            double amenTotal = AmenityBuildings.Sum(a => a.Building.GarbageProduction * a.Count);
            return thisLevel + subChainTotal;
        }
    }
    public double TotalEnvironmentPollution
    {
        get
        {
            double thisLevel = ChosenBuilding.TotalEnvironmentPollution;
            double subChainTotal = SubChains.Sum(sc => sc.TotalEnvironmentPollution);
            return thisLevel + subChainTotal;
        }
    }
    public Dictionary<Resource, double> TotalUtilityNeeds    // Aggregate total utility needs (input + consumption for water)
    {
        get
        {
            Dictionary<Resource, double> result = new Dictionary<Resource, double>();
            result.Add(GameData.PowerResource, 0);
            result.Add(GameData.WaterResource, 0);
            result.Add(GameData.WasteWaterResource, 0);
            result.Add(GameData.HeatResource, 0);

            // Add utility consumption (power/water consumed to run buildings)
            if (TotalPowerNeeded > 0)
            {
                result[GameData.PowerResource] += TotalPowerNeeded;
            }
            if (TotalWaterNeeded > 0)
            {
                result[GameData.WaterResource] += TotalWaterNeeded;
            }
            // Add Sewage produced
            if (TotalSewageProduced > 0)
            {
                result[GameData.WasteWaterResource] += TotalSewageProduced;
            }
            if (TotalHeatProduced > 0)
            {
                result[GameData.HeatResource] += TotalHeatNeeded;
            }
            // Add utility as input resource (power/water as production inputs)
            foreach (var kv in ChosenBuilding.RequiredResources)
                if (kv.Key.IsUtility && kv.Key == GameData.WaterResource)   // Only Water as input resource
                {
                    if (result.ContainsKey(kv.Key))
                        result[kv.Key] += kv.Value;
                    else
                        result.Add(kv.Key, kv.Value);
                }
            // Recursively aggregate from SubChains
            foreach (var sc in SubChains)
            {
                CollectInputWater(result, sc);
            }
            return result;
        }
    }

    // Utilities - Residential
    public double TotalResidentialPower => ResidentialBuildings.Sum(rb => rb.Building.PowerMW * rb.Count);
    public double TotalResidentialWater => ResidentialBuildings.Sum(rb => rb.Building.WaterPerDay * rb.Count);
    public double TotalResidentialSewage => ResidentialBuildings.Sum(rb => rb.Building.WaterPerDay * rb.Count);
    public double TotalResidentialHeat => ResidentialBuildings.Sum(rb => rb.Building.HeatTankM3 * rb.Count);

    // Utilities - Produced
    public double TotalPowerProduced
    {
        get
        {
            double result = 0;
            return CalculateTotalPowerProduced(result, this);
        }
    }
    public double TotalWaterProduced
    {
        get
        {
            double result = 0;
            return CalculateTotalWaterProduced(result, this);
        }
    }
    public double TotalHeatProduced
    {
        get
        {
            double result = 0;
            return CalculateTotalHeatProduced(result, this);
        }
    }

    public Dictionary<Resource, BuildingRequirement> ExpandedUtilities
    {
        get
        {
            Dictionary<Resource, BuildingRequirement> result = new Dictionary<Resource, BuildingRequirement>();
            return CalculateExpandedUtility(result, this);
        }
    }

    public Dictionary<Resource, BuildingRequirement> CalculateExpandedUtility(Dictionary<Resource, BuildingRequirement> result, IndustryPlan cr)
    {
        if (cr.ChosenBuilding.Building.IsUtilityBuilding && cr.ChosenBuilding.ExpectedOutput.Any(eo => eo.Key.IsUtility))
        {
            foreach (var kv in cr.ChosenBuilding.ExpectedOutput)
                if (kv.Key == GameData.PowerResource)
                {
                    if (!result.ContainsKey(GameData.PowerResource))
                        result.Add(GameData.PowerResource, cr.ChosenBuilding);
                    else Console.WriteLine($"There's already existing utility building {cr.ChosenBuilding.Building.Name} for Power.");
                }
                else if (kv.Key == GameData.WaterResource)
                {
                    if (!result.ContainsKey(GameData.WaterResource))
                        result.Add(GameData.WaterResource, cr.ChosenBuilding);
                    else Console.WriteLine($"There's already existing utility building {cr.ChosenBuilding.Building.Name} for Water.");
                }
                else if (kv.Key == GameData.WasteWaterResource)
                {
                    if (!result.ContainsKey(GameData.WasteWaterResource))
                        result.Add(GameData.WasteWaterResource, cr.ChosenBuilding);
                    else Console.WriteLine($"There's already existing utility building {cr.ChosenBuilding.Building.Name} for Waste water.");
                }
                else if (kv.Key == GameData.HeatResource)
                {
                    if (!result.ContainsKey(GameData.HeatResource))
                        result.Add(GameData.HeatResource, cr.ChosenBuilding);
                    else Console.WriteLine($"There's already existing utility building {cr.ChosenBuilding.Building.Name} for Heat.");
                }
        }
        foreach (var sub in cr.SubChains)
            CalculateExpandedUtility(result, sub);
        return result;
    }
    public double CalculateTotalPowerProduced(double d, IndustryPlan cr)
    {
        if (cr.ChosenBuilding.ExpectedOutput.Any(eo => eo.Key == GameData.PowerResource))
            d += cr.ChosenBuilding.ExpectedOutput[GameData.PowerResource];
        foreach (var sub in cr.SubChains)
            d = CalculateTotalPowerProduced(d, sub);
        return d;
    }
    public double CalculateTotalWaterProduced(double d, IndustryPlan cr)
    {
        if (cr.ChosenBuilding.ExpectedOutput.Any(eo => eo.Key == GameData.WaterResource))
            d += cr.ChosenBuilding.ExpectedOutput[GameData.WaterResource];
        foreach (var sub in cr.SubChains)
            d = CalculateTotalWaterProduced(d, sub);
        return d;
    }
    public double CalculateTotalHeatProduced(double d, IndustryPlan cr)
    {
        if (cr.ChosenBuilding.ExpectedOutput.Any(eo => eo.Key == GameData.HeatResource))
            d += cr.ChosenBuilding.ExpectedOutput[GameData.HeatResource];
        foreach (var sub in cr.SubChains)
            d = CalculateTotalHeatProduced(d, sub);
        return d;
    }
    private void CollectInputWater(Dictionary<Resource, double> result, IndustryPlan cr)
    {
        // Add this level's input water
        foreach (var kv in cr.ChosenBuilding.RequiredResources)
        {
            if (kv.Key.IsUtility && kv.Key == GameData.WaterResource)
            {
                if (result.ContainsKey(kv.Key))
                    result[kv.Key] += kv.Value;
                else
                    result.Add(kv.Key, kv.Value);
            }
        }

        // Recurse to subchains
        foreach (var sc in cr.SubChains)
        {
            CollectInputWater(result, sc);
        }
    }
}
