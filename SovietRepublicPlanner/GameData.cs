using System;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

class GameData
{
    public enum FertilizerType
    {
        None,           // Max 100% fertility
        Solid,          // Max 150%
        Liquid,         // Max 150%
        Both            // Max 200%
    }
    const double smallFieldSize = 0.39;
    const double mediumFieldSize = 1.57;
    const double largeFieldSize = 4.81;

    // Resources instances
    // Regular resources
    public static Resource FoodResource = new Resource("Food", 2, true, false)
    {
        RequiresGeneralDistribution = true,
        IsConsumable = true,
        PerCapitalConsumption = 0.00034
    };
    public static Resource ClothesResource = new Resource("Clothes", 5, true, false) 
    { 
        RequiresGeneralDistribution = true,
        IsConsumable = true,
        PerCapitalConsumption = 0.000013
    };
    public static Resource MeatResource = new Resource("Meat", 1, true, false)
    {
        RequiresGeneralDistribution = true,
        IsConsumable = true,
        PerCapitalConsumption = 0.000076
    };
    public static Resource AlcoholResource = new Resource("Alcohol", 5, true, false)
    {
        RequiresGeneralDistribution = true,
        IsConsumable = true,
        PerCapitalConsumption = 0.000025
    };
    public static Resource ElectronicsResource = new Resource("Electronics", 5, true, false)
    {
        RequiresGeneralDistribution = true,
        IsConsumable = true,
        PerCapitalConsumption = 0.000023
    };
    public static Resource FabricResource = new Resource("Fabric", 4, true, false) { RequiresGeneralDistribution = true };
    public static Resource ChemicalsResource = new Resource("Chemicals", 3, true, false) { RequiresGeneralDistribution = true };
    public static Resource CropsResource = new Resource("Crops", 1, true, false) { RequiresGeneralDistribution = true };
    public static Resource WoodResource = new Resource("Wood", 1, true, false) { RequiresGeneralDistribution = true };
    public static Resource SteelResource = new Resource("Steel", 1, true, false) { RequiresGeneralDistribution = true };
    public static Resource SolidFertilizerResource = new Resource("Solid Fertilizer", 1, true, false) { RequiresSolidHandling = true };
    public static Resource NuclearFuelResource = new Resource("Nuclear Fuel", 5, true, false) { RequiresGeneralDistribution = true };
    public static Resource LivestockResource = new Resource("Livestock", 1, true, false) { };

    // Liquids
    public static Resource OilResource = new Resource("Oil", 1, true, false) { RequiresLiquidInfrastructure = true };
    public static Resource LiquidFertilizerResource = new Resource("Liquid Fertilizer", 1, true, false) { RequiresLiquidInfrastructure = true };
    public static Resource FuelResource = new Resource("Fuel", 1, true, false) { RequiresLiquidInfrastructure = true };
    public static Resource BitumenResource = new Resource("Bitumen", 0, false, false) { RequiresLiquidInfrastructure = true };

    // Bulk Raw Materials
    public static Resource CoalResource = new Resource("Coal", 1, true, false) { RequiresBulkHandling = true };
    public static Resource CoalOreResource = new Resource("Coal Ore", 1, true, false) { RequiresBulkHandling = true };
    public static Resource GravelResource = new Resource("Gravel", 1, true, false) { RequiresBulkHandling = true };
    public static Resource QuarriedStoneResource = new Resource("Quarried Stone", 1, true, false) { RequiresBulkHandling = true };
    public static Resource IronResource = new Resource("Iron", 1, true, false) { RequiresBulkHandling = true };
    public static Resource IronOreResource = new Resource("Iron Ore", 1, true, false) { RequiresBulkHandling = true };
    public static Resource RawBauxiteResource = new Resource("Raw Bauxite", 1, true, false) { RequiresBulkHandling = true };

    // Industrial Materials
    public static Resource CementResource = new Resource("Cement", 0, false, false) { RequiresDryBulkHandling = true };
    public static Resource ConcreteResource = new Resource("Concrete", 0, false, false) {};
    public static Resource BricksResource = new Resource("Bricks", 0, false, false) { RequiresSolidHandling = true };
    public static Resource AsphaltResource = new Resource("Asphalt", 0, false, false) {};
    public static Resource PrefabPanelsResource = new Resource("Prefab Panels", 0, false, false) { RequiresSolidHandling = true };
    public static Resource BoardsResource = new Resource("Boards", 1, false, false) { RequiresSolidHandling = true };
    public static Resource MechanicComponentsResource = new Resource("Mechanical Components", 3, false, false) { RequiresGeneralDistribution = true };
    public static Resource ElectroComponentsResource = new Resource("Electro Components", 4, false, false) { RequiresGeneralDistribution = true };
    public static Resource PlasticsResource = new Resource("Plastics", 4, false, false) { RequiresGeneralDistribution = true };

    // Utility resources (dual nature: input + service)
    public static Resource PowerResource = new Resource("Power", 1, true, true) { RequiresElectricalInfrastructure = true };
    public static Resource WaterResource = new Resource("Water", 1, true, true) { RequiresWaterInfrastructure = true };
    public static Resource IndustrialWaterResource = new Resource("Industrial Water", 1, true, true) { RequiresWaterInfrastructure = true };
    public static Resource WasteWaterResource = new Resource("Waste Water", 0, false, true) { RequiresSewageInfrastructure = true };
    public static Resource NuclearWasteResource = new Resource("Nuclear Waste", 0, false, true);
    public static Resource HeatResource = new Resource("Heat", 1, false, true) { RequiresHeatInfrastructure = true};

    // Wastes
    public static Resource BiologicalWasteResource = new Resource("Biological Waste", 0, false, false);

    // Production buildings instances
    public static ProductionBuilding FoodFactory { get; } = CreateFoodFactory();
    public static ProductionBuilding CreateFoodFactory()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Food Factory";
        result.Inputs = new List<ResourceAmount>()
        {
            new ResourceAmount(CropsResource, 42, TimePeriod.Day),
            new ResourceAmount(WaterResource, 8.5, TimePeriod.Day)
        };
        result.Outputs = new List<ResourceAmount>()
        {
            new ResourceAmount(FoodResource, 20.0, TimePeriod.Day),
        };
        result.WorkersPerShift = 170;
        result.PowerConsumption = 7.6;
        result.WaterConsumption = 3.4;
        result.HeatConsumption = 0;
        result.SewageProduction = 11.9;     // Resource + daily water consumption
        result.BaseGarbageProduction = 1.53;
        result.GarbagePerWorker = 0.0006;
        result.EnvironmentPollution = 7.8 / 365;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.GeneralDistribution;
        result.Workdays = 2426;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 46 },
            {GravelResource, 35 },
            {AsphaltResource, 28 },
            {SteelResource, 40 },
            {BricksResource, 139 },
            {BoardsResource, 46 }
        };
        return result;
    }

    public static ProductionBuilding Distillery { get; } = CreateDistillery();
    public static ProductionBuilding CreateDistillery()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Distillery";
        result.Inputs = new List<ResourceAmount>()
        {
            new ResourceAmount(CropsResource, 30, TimePeriod.Day),
            new ResourceAmount(WaterResource, 13.0, TimePeriod.Day),
            new ResourceAmount(PowerResource, 10.0, TimePeriod.Day)
        };
        result.Outputs = new List<ResourceAmount>()
        {
            new ResourceAmount(AlcoholResource, 6.0, TimePeriod.Day),
        };
        result.WorkersPerShift = 100;
        result.PowerConsumption = 13.0;
        result.WaterConsumption = 2.0;
        result.HeatConsumption = 0;
        result.SewageProduction = 13.0 + result.WaterConsumption;     // Resource + daily water consumption
        result.BaseGarbageProduction = 2.39;
        result.GarbagePerWorker = 0.0006;
        result.EnvironmentPollution = 9.6 / 365;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.GeneralDistribution;
        result.Workdays = 1542;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 25 },
            {GravelResource, 19 },
            {AsphaltResource, 15 },
            {SteelResource, 28 },
            {BricksResource, 91 },
            {BoardsResource, 30 }
        };
        return result;
    }

    public static ProductionBuilding ClothingFactory { get; } = CreateClothingFactory();
    public static ProductionBuilding CreateClothingFactory()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Clothing Factory";
        result.Inputs = new List<ResourceAmount>()
        {
            new ResourceAmount(FabricResource, 2.4, TimePeriod.Day)
        };
        result.Outputs = new List<ResourceAmount>()
        {
            new ResourceAmount(ClothesResource, 1.2, TimePeriod.Day)
        };
        result.WorkersPerShift = 80;
        result.PowerConsumption = 3.6;
        result.WaterConsumption = 1.60;
        result.HeatConsumption = 0;
        result.SewageProduction = 1.60;
        result.BaseGarbageProduction = 0.03;
        result.GarbagePerWorker = 0.0006;
        result.EnvironmentPollution = 0.012;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.GeneralDistribution;
        result.Workdays = 1728;
        result.ConstructionMaterials = new Dictionary<Resource, double>() 
        {
            {ConcreteResource, 224 },
            {GravelResource, 44 },
            {AsphaltResource, 35 },
            {SteelResource, 40 },
            {BricksResource, 25 },
            {BoardsResource, 8.6 }
        };
        return result;
    }

    public static ProductionBuilding FabricFactory { get; } = CreateFabricFactory();
    public static ProductionBuilding CreateFabricFactory()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Fabric Factory";
        result.Inputs = new List<ResourceAmount>()
        {
            new ResourceAmount(CropsResource, 20, TimePeriod.Day),
            new ResourceAmount(ChemicalsResource, 0.5, TimePeriod.Day),
            new ResourceAmount(WaterResource, 11, TimePeriod.Day),      // input resource
            new ResourceAmount(PowerResource, 10, TimePeriod.Day)
        };
        result.Outputs = new List<ResourceAmount>()
        {
            new ResourceAmount(FabricResource, 5.0, TimePeriod.Day),
        };
        result.WorkersPerShift = 100;
        result.PowerConsumption = 19;
        result.WaterConsumption = 2.0;      // building water consumption (drinkable)
        result.HeatConsumption = 0;
        result.SewageProduction = 13.0;     // Resource + daily water consumption
        result.BaseGarbageProduction = 1.87;
        result.GarbagePerWorker = 0.006;
        result.EnvironmentPollution = 8.2 / 365;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.GeneralDistribution;
        result.Workdays = 1715;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 62 },
            {GravelResource, 34 },
            {AsphaltResource, 27 },
            {SteelResource, 16 },
            {BricksResource, 94 },
            {BoardsResource, 31 },
            {MechanicComponentsResource, 0.062 }
        };
        return result;
    }

    public static ProductionBuilding SmallChemicalPlant { get; } = CreateSmallChemicalPlant();
    public static ProductionBuilding CreateSmallChemicalPlant()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Small Chemical Plant";
        result.Inputs = new List<ResourceAmount>()
        {
            new ResourceAmount(GravelResource, 0.72, TimePeriod.Day),
            new ResourceAmount(WoodResource, 0.84, TimePeriod.Day),
            new ResourceAmount(CropsResource, 0.78, TimePeriod.Day),
            new ResourceAmount(OilResource, 1.2, TimePeriod.Day),
            new ResourceAmount(WaterResource, 10, TimePeriod.Day),
            new ResourceAmount(PowerResource, 14, TimePeriod.Day)
        };
        result.Outputs = new List<ResourceAmount>()
        {
            new ResourceAmount(ChemicalsResource, 0.81, TimePeriod.Day)
        };
        result.WorkersPerShift = 60;
        result.PowerConsumption = 25;
        result.WaterConsumption = 1.2;
        result.HeatConsumption = 0;
        result.SewageProduction = 11.2;       // Resource + daily water consumption
        result.BaseGarbageProduction = 1.37;
        result.GarbagePerWorker = 0.006;
        result.EnvironmentPollution = 15.2 / 365;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.GeneralDistribution;
        result.Workdays = 2260;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 62 },
            {GravelResource, 48 },
            {AsphaltResource, 38 },
            {SteelResource, 45 },
            {BricksResource, 52 },
            {BoardsResource, 17 },
            {MechanicComponentsResource, 5.8 }
        };
        return result;
    }

    public static ProductionBuilding MediumChemicalPlant { get; } = CreateMediumChemicalPlant();
    public static ProductionBuilding CreateMediumChemicalPlant()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Medium Chemical Plant";
        result.Inputs = new List<ResourceAmount>()
        {
            new ResourceAmount(GravelResource, 2.4, TimePeriod.Day),
            new ResourceAmount(WoodResource, 2.8, TimePeriod.Day),
            new ResourceAmount(CropsResource, 2.6, TimePeriod.Day),
            new ResourceAmount(OilResource, 4.0, TimePeriod.Day),
            new ResourceAmount(WaterResource, 34, TimePeriod.Day),
            new ResourceAmount(PowerResource, 45, TimePeriod.Day)
        };
        result.Outputs = new List<ResourceAmount>()
        {
            new ResourceAmount(ChemicalsResource, 2.7, TimePeriod.Day)
        };
        result.WorkersPerShift = 200;
        result.PowerConsumption = 60;
        result.WaterConsumption = 4;
        result.HeatConsumption = 0;
        result.SewageProduction = result.WaterConsumption + 34;       // Resource + daily water consumption
        result.BaseGarbageProduction = 4.41;
        result.GarbagePerWorker = 0.006;
        result.EnvironmentPollution = 15 / 365;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.GeneralDistribution;
        result.Workdays = 6842;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 144 },
            {GravelResource, 111 },
            {SteelResource, 98 },
            {BricksResource, 294 },
            {BoardsResource, 98 },
            {MechanicComponentsResource, 12 }
        };
        return result;
    }

    public static ProductionBuilding BigChemicalPlant { get; } = CreateBigChemicalPlant();
    public static ProductionBuilding CreateBigChemicalPlant()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Big Chemical Plant";
        result.Inputs = new List<ResourceAmount>()
        {
            new ResourceAmount(GravelResource, 8.4, TimePeriod.Day),
            new ResourceAmount(WoodResource, 9.8, TimePeriod.Day),
            new ResourceAmount(CropsResource, 9.1, TimePeriod.Day),
            new ResourceAmount(OilResource, 14, TimePeriod.Day),
            new ResourceAmount(WaterResource, 119, TimePeriod.Day),
            new ResourceAmount(PowerResource, 168, TimePeriod.Day)
        };
        result.Outputs = new List<ResourceAmount>()
        {
            new ResourceAmount(ChemicalsResource, 9.4, TimePeriod.Day)
        };
        result.WorkersPerShift = 700;
        result.PowerConsumption = 197;
        result.WaterConsumption = 14;
        result.HeatConsumption = 0;
        result.SewageProduction = 133;       // Resource + daily water consumption
        result.BaseGarbageProduction = 15.94;
        result.GarbagePerWorker = 0.006;
        result.EnvironmentPollution = 65.2 / 365;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.GeneralDistribution;
        result.Workdays = 23681;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 2473 },
            {GravelResource, 445 },
            {AsphaltResource, 356 },
            {SteelResource, 607 },
            {BricksResource, 413 },
            {BoardsResource, 137 },
            {MechanicComponentsResource, 26 }
        };
        return result;
    }

    public static ProductionBuilding PlasticsFactory { get; } = CreatePlasticsFactory();
    public static ProductionBuilding CreatePlasticsFactory()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Plastics Factory";
        result.Inputs = new List<ResourceAmount>()
    {
        new ResourceAmount(ChemicalsResource, 3.0, TimePeriod.Day),
        new ResourceAmount(OilResource, 27, TimePeriod.Day),
        new ResourceAmount(PowerResource, 15, TimePeriod.Day)
    };
        result.Outputs = new List<ResourceAmount>()
    {
        new ResourceAmount(PlasticsResource, 6.6, TimePeriod.Day)
    };
        result.WorkersPerShift = 60;
        result.PowerConsumption = 20;
        result.WaterConsumption = 1.20;
        result.HeatConsumption = 0;
        result.SewageProduction = 1.20;
        result.BaseGarbageProduction = 1.95;
        result.GarbagePerWorker = 0.0006;
        result.EnvironmentPollution = 9.20 / 365;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.GeneralDistribution;
        result.Workdays = 1395;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
    {
        {ConcreteResource, 29 },
        {GravelResource, 22 },
        {AsphaltResource, 18 },
        {SteelResource, 26 },
        {BricksResource, 37 },
        {BoardsResource, 12 },
        {MechanicComponentsResource, 4.5 }
    };
        return result;
    }

    public static ProductionBuilding CompostingPlant { get; } = CreateCompostingPlant();
    public static ProductionBuilding CreateCompostingPlant()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Composting Plant";
        result.Inputs = new List<ResourceAmount>
    {
        new ResourceAmount(BiologicalWasteResource, 30, TimePeriod.Day),
        new ResourceAmount(ChemicalsResource, 0.075, TimePeriod.Day)
    };
        result.Outputs = new List<ResourceAmount>
    {
        new ResourceAmount(SolidFertilizerResource, 25, TimePeriod.Day)
    };
        result.WorkersPerShift = 25;
        result.PowerConsumption = 3.0; // 3.0 MWh/day
        result.WaterConsumption = 0.5;
        result.HeatConsumption = 0;
        result.SewageProduction = 0.5;
        result.GarbagePerWorker = 0.00060;
        result.EnvironmentPollution = 12.00 / 365.0; // 12.00 tons/year
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsQualityDependent = false;
        result.CanUseVehicles = false;
        result.SupportCategory = SupportCategory.None;
        result.Workdays = 532;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 15 },
            {GravelResource, 11 },
            {AsphaltResource, 9.5 },
            {SteelResource, 28 },
            {MechanicComponentsResource, 0.35 }
        };
        return result;
    }

    public static ProductionBuilding SyntheticFertilizerFactory { get; } = CreateSyntheticFertilizerFactory();
    public static ProductionBuilding CreateSyntheticFertilizerFactory()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Synthetic Fertilizer Factory";
        result.Inputs = new List<ResourceAmount>
    {
        new ResourceAmount(ChemicalsResource, 0.31, TimePeriod.Day)
    };
        result.Outputs = new List<ResourceAmount>
    {
        new ResourceAmount(LiquidFertilizerResource, 17, TimePeriod.Day)
    };
        result.WorkersPerShift = 25;
        result.PowerConsumption = 9.8; // 9.8 MWh/day
        result.WaterConsumption = 15.0; // 15m³/day
        result.HeatConsumption = 0;
        result.SewageProduction = 15.0; // 15m³/day
        result.GarbagePerWorker = 0.00060;
        result.EnvironmentPollution = 16.40 / 365.0; // 16.40 tons/year
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsQualityDependent = false;
        result.CanUseVehicles = false;
        result.SupportCategory = SupportCategory.LiquidHandling;
        result.Workdays = 947;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 16 },
            {GravelResource, 12 },
            {AsphaltResource, 10.0 },
            {SteelResource, 42 },
            {MechanicComponentsResource, 3.4 }
        };
        return result;
    }

    public static ProductionBuilding MechanicalComponentsFactory { get; } = CreateMechanicalComponentsFactory();
    public static ProductionBuilding CreateMechanicalComponentsFactory()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Mechanical Components Factory";
        result.Inputs = new List<ResourceAmount>
    {
        new ResourceAmount(SteelResource, 22, TimePeriod.Day)
    };
        result.Outputs = new List<ResourceAmount>
    {
        new ResourceAmount(MechanicComponentsResource, 15, TimePeriod.Day)
    };
        result.WorkersPerShift = 150;
        result.PowerConsumption = 8.1; // 8.1 MWh/day
        result.WaterConsumption = 3.0;
        result.HeatConsumption = 0;
        result.SewageProduction = 3.0;
        result.GarbagePerWorker = 0.00060;
        result.EnvironmentPollution = 7.80 / 365.0; // 7.80 tons/year
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsQualityDependent = false;
        result.CanUseVehicles = false;
        result.SupportCategory = SupportCategory.GeneralDistribution;
        result.Workdays = 5122;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 375 },
            {GravelResource, 88 },
            {AsphaltResource, 70 },
            {SteelResource, 133 },
            {BricksResource, 142 },
            {BoardsResource, 47 },
            {MechanicComponentsResource, 2.7 }
        };
        return result;
    }

    public static ProductionBuilding SteelMiil { get; } = CreateSteelMill();
    public static ProductionBuilding CreateSteelMill()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Steel Mill";
        result.Inputs = new List<ResourceAmount>()
        {
            new ResourceAmount(IronResource, 200, TimePeriod.Day),
            new ResourceAmount(CoalResource, 375, TimePeriod.Day),
            new ResourceAmount(PowerResource, 33, TimePeriod.Day)
        };
        result.Outputs = new List<ResourceAmount>()
        {
            new ResourceAmount(SteelResource, 9.4, TimePeriod.Day)
        };
        result.WorkersPerShift = 500;
        result.PowerConsumption = 48;
        result.WaterConsumption = 10;
        result.HeatConsumption = 0;
        result.SewageProduction = 10;       // Resource + daily water consumption
        result.BaseGarbageProduction = 11.67;
        result.GarbagePerWorker = 0.006;
        result.EnvironmentPollution = 39.9 / 365;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.SolidHandling;
        result.Workdays = 9553;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 484 },
            {GravelResource, 142 },
            {AsphaltResource, 113 },
            {SteelResource, 241 },
            {BricksResource, 141 },
            {BoardsResource, 47 },
            {MechanicComponentsResource, 32 }
        };
        return result;
    }

    public static ProductionBuilding IronProcessingPlant { get; } = CreateIronProcessingPlant();
    public static ProductionBuilding CreateIronProcessingPlant()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Iron Processing Plant";
        result.Inputs = new List<ResourceAmount>()
        {
            new ResourceAmount(IronOreResource, 225, TimePeriod.Day),
            new ResourceAmount(PowerResource, 16, TimePeriod.Day)
        };
        result.Outputs = new List<ResourceAmount>()
        {
            new ResourceAmount(IronResource, 105, TimePeriod.Day)
        };
        result.WorkersPerShift = 15;
        result.PowerConsumption = 17;
        result.WaterConsumption = 0.3;
        result.HeatConsumption = 0;
        result.SewageProduction = 0.3;       // Resource + daily water consumption
        result.BaseGarbageProduction = 6.84;
        result.GarbagePerWorker = 0.006;
        result.EnvironmentPollution = 12 / 365;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.BulkHandling;
        result.Workdays = 4122;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 360 },
            {GravelResource, 65 },
            {AsphaltResource, 52 },
            {SteelResource, 200 },
            {MechanicComponentsResource, 4.2 }
        };
        return result;
    }

    public static ProductionBuilding CoalProcessingPlant { get; } = CreateCoalProcessingPlant();
    public static ProductionBuilding CreateCoalProcessingPlant()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Coal Processing Plant";
        result.Inputs = new List<ResourceAmount>()
        {
            new ResourceAmount(CoalOreResource, 210, TimePeriod.Day),
            new ResourceAmount(PowerResource, 15, TimePeriod.Day)
        };
        result.Outputs = new List<ResourceAmount>()
        {
            new ResourceAmount(CoalResource, 120, TimePeriod.Day)
        };
        result.WorkersPerShift = 15;
        result.PowerConsumption = 15;
        result.WaterConsumption = 0.3;
        result.HeatConsumption = 0;
        result.SewageProduction = 0.3;       // Resource + daily water consumption
        result.BaseGarbageProduction = 5.25;
        result.GarbagePerWorker = 0.006;
        result.EnvironmentPollution = 12 / 365;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.BulkHandling;
        result.Workdays = 4122;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 360 },
            {GravelResource, 65 },
            {AsphaltResource, 52 },
            {SteelResource, 200 },
            {MechanicComponentsResource, 4.2 }
        };
        return result;
    }

    public static ProductionBuilding CoalMine { get; } = CreateCoalMine();
    public static ProductionBuilding CreateCoalMine()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Coal Mine";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>()
        {
            new ResourceAmount(CoalOreResource, 4.2 * 220, TimePeriod.Day)
        };
        result.WorkersPerShift = 220;
        result.PowerConsumption = 9.2;
        result.WaterConsumption = 4.4;
        result.HeatConsumption = 0;
        result.SewageProduction = 4.4;       // Resource + daily water consumption
        result.BaseGarbageProduction = 16.17;
        result.GarbagePerWorker = 0.0043;
        result.EnvironmentPollution = 12.2 / 365;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsQualityDependent = true;
        result.CanUseVehicles = true;
        result.SupportCategory = SupportCategory.BulkHandling;
        result.Workdays = 3878;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 249 },
            {GravelResource, 9.4 },
            {AsphaltResource, 7.6 },
            {SteelResource, 74 },
            {BoardsResource, 75 },
            {MechanicComponentsResource, 3.5 }
        };
        return result;
    }

    public static ProductionBuilding WoodcuttingPost { get; } = CreateWoodcuttingPost();
    public static ProductionBuilding CreateWoodcuttingPost()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Woodcutting Post";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>()
        {
            new ResourceAmount(WoodResource, 63, TimePeriod.Day),
        };
        result.WorkersPerShift = 10;
        result.PowerConsumption = 7.2;
        result.WaterConsumption = 0.2;
        result.HeatConsumption = 0;
        result.SewageProduction = 0.2;       // Resource + daily water consumption
        result.BaseGarbageProduction = 10;
        result.GarbagePerWorker = 0.0043;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.IsQualityDependent = true;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.SolidHandling;
        result.Workdays = 217;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 8.5 },
            {GravelResource, 6.5 },
            {AsphaltResource, 5.2 },
            {SteelResource, 1.5 },
            {BoardsResource, 10 }
        };
        return result;
    }

    public static ProductionBuilding Sawmill { get; } = CreateSawmill();
    public static ProductionBuilding CreateSawmill()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Sawmill";
        result.Inputs = new List<ResourceAmount>
    {
        new ResourceAmount(WoodResource, 180, TimePeriod.Day)
    };
        result.Outputs = new List<ResourceAmount>
    {
        new ResourceAmount(BoardsResource, 140, TimePeriod.Day)
    };
        result.WorkersPerShift = 20;
        result.PowerConsumption = 6.7; // 6.7 MWh/day
        result.WaterConsumption = 0.4;
        result.HeatConsumption = 0;
        result.SewageProduction = 0.4;
        result.GarbagePerWorker = 0.0006;
        result.EnvironmentPollution = 1.20 / 365.0; // 1.20 tons/year
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsQualityDependent = false;
        result.CanUseVehicles = false;
        result.SupportCategory = SupportCategory.SolidHandling;
        result.Workdays = 922;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 26 },
            {GravelResource, 20 },
            {AsphaltResource, 16 },
            {SteelResource, 6.6 },
            {BricksResource, 53 },
            {BoardsResource, 17 }
        };
        return result;
    }

    public static ProductionBuilding SmallGravelProcessingPlant { get; } = CreateSmallGravelProcessingPlant();
    public static ProductionBuilding CreateSmallGravelProcessingPlant()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Small Gravel Processing Plant";
        result.Inputs = new List<ResourceAmount>()
        {
            new ResourceAmount(QuarriedStoneResource, 40, TimePeriod.Day),
            new ResourceAmount(PowerResource, 9, TimePeriod.Day)
        };
        result.Outputs = new List<ResourceAmount>()
        {
            new ResourceAmount(GravelResource, 27, TimePeriod.Day)
        };
        result.WorkersPerShift = 5;
        result.PowerConsumption = 9.2;
        result.WaterConsumption = 0.1;
        result.HeatConsumption = 0;
        result.SewageProduction = 0.1;       // Resource + daily water consumption
        result.BaseGarbageProduction = 1.08;
        result.GarbagePerWorker = 0.006;
        result.EnvironmentPollution = 9.3 / 365;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.BulkHandling;
        result.Workdays = 373;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 2.8 },
            {GravelResource, 2.1 },
            {AsphaltResource, 1.7 },
            {SteelResource, 12 },
            {MechanicComponentsResource, 2.5 }
        };
        return result;
    }

    public static ProductionBuilding BigGravelProcessingPlant { get; } = CreateBigGravelProcessingPlant();
    public static ProductionBuilding CreateBigGravelProcessingPlant()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Big Gravel Processing Plant";
        result.Inputs = new List<ResourceAmount>()
        {
            new ResourceAmount(QuarriedStoneResource, 120, TimePeriod.Day),
            new ResourceAmount(PowerResource, 24, TimePeriod.Day)
        };
        result.Outputs = new List<ResourceAmount>()
        {
            new ResourceAmount(GravelResource, 82, TimePeriod.Day)
        };
        result.WorkersPerShift = 15;
        result.PowerConsumption = 24;
        result.WaterConsumption = 0.3;
        result.HeatConsumption = 0;
        result.SewageProduction = 0.1;       // Resource + daily water consumption
        result.BaseGarbageProduction = 3.08;
        result.GarbagePerWorker = 0.006;
        result.EnvironmentPollution = 12.6 / 365;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.BulkHandling;
        result.Workdays = 1193;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 109 },
            {GravelResource, 12 },
            {AsphaltResource, 10 },
            {SteelResource, 44 },
            {MechanicComponentsResource, 3.9 }
        };
        return result;
    }

    public static ProductionBuilding BigGravelQuarry { get; } = CreateBigGravelQuarry();
    public static ProductionBuilding CreateBigGravelQuarry()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Big Gravel Quarry";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>()
        {
            new ResourceAmount(QuarriedStoneResource, 3.5 * 100, TimePeriod.Day)
        };
        result.WorkersPerShift = 100;
        result.PowerConsumption = 23;
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;       // Resource + daily water consumption
        result.BaseGarbageProduction = 6.13;
        result.GarbagePerWorker = 0.0043;       // Include max workers
        result.EnvironmentPollution = 4.1 / 365;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsQualityDependent = true;
        result.CanUseVehicles = true;
        result.SupportCategory = SupportCategory.BulkHandling;
        result.Workdays = 397;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 9.9 },
            {GravelResource, 7.6 },
            {AsphaltResource, 6.1 },
            {SteelResource, 11 },
            {BricksResource, 14 },
            {BoardsResource, 4.9 }
        };
        return result;
    }

    public static ProductionBuilding SmallGravelQuarry { get; } = CreateSmallGravelQuarry();
    public static ProductionBuilding CreateSmallGravelQuarry()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Small Gravel Quarry";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>()
        {
            new ResourceAmount(QuarriedStoneResource, 3.5 * 40, TimePeriod.Day)
        };
        result.WorkersPerShift = 40;
        result.PowerConsumption = 9.8;
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;            // Resource + daily water consumption
        result.BaseGarbageProduction = 2.45;     // Include max workers
        result.GarbagePerWorker = 0.0043;
        result.EnvironmentPollution = 2 / 365;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsQualityDependent = true;
        result.CanUseVehicles = true;
        result.SupportCategory = SupportCategory.BulkHandling;
        result.Workdays = 79;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 1.3 },
            {GravelResource, 1.0 },
            {AsphaltResource, 0.82 },
            {SteelResource, 0.88 },
            {BricksResource, 4.8 },
            {BoardsResource, 2.1 }
        };
        return result;
    }

    public static ProductionBuilding Pumpjack { get; } = CreatePumpjack();
    public static ProductionBuilding CreatePumpjack()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Pumpjack";
        result.Inputs = new List<ResourceAmount>()
        {
            new ResourceAmount(PowerResource, 3.9, TimePeriod.Day)
        };
        result.Outputs = new List<ResourceAmount>()
        {
            new ResourceAmount(OilResource, 7, TimePeriod.Day)
        };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 6.9;
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;       // Resource + daily water consumption
        result.EnvironmentPollution = 4 / 365;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsQualityDependent = true;
        result.SupportCategory = SupportCategory.LiquidHandling;
        result.Workdays = 224;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 3.9 },
            {GravelResource, 3.0 },
            {AsphaltResource, 2.4 },
            {SteelResource, 6.3 },
            {MechanicComponentsResource, 1.3 }
        };
        return result;
    }

    public static ProductionBuilding BauxiteMine { get; } = CreateBauxiteMine();
    public static ProductionBuilding CreateBauxiteMine()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Bauxite Mine";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>()
        {
            new ResourceAmount(RawBauxiteResource, 0.5 * 45, TimePeriod.Day)
        };
        result.WorkersPerShift = 45;
        result.PowerConsumption = 4.3;
        result.WaterConsumption = 0.9;
        result.HeatConsumption = 0;
        result.SewageProduction = 0.9;       // Resource + daily water consumption
        result.BaseGarbageProduction = 0.39;       // Include max workers
        result.GarbagePerWorker = 0.0043;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsQualityDependent = true;
        result.CanUseVehicles = true;
        result.SupportCategory = SupportCategory.BulkHandling;
        result.Workdays = 335;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 6.9 },
            {GravelResource, 5.3 },
            {SteelResource, 21 },
            {MechanicComponentsResource, 0.21 }
        };
        return result;
    }

    public ProductionBuilding SmallField { get; } = CreateSmallField(FertilizerType.None);
    public static ProductionBuilding CreateSmallField(FertilizerType fertilizerType)
    {
        var result = new ProductionBuilding()
        {
            Name = "Small Field",
            IsSeasonDependent = true,
            Outputs = new List<ResourceAmount>
            {
                new ResourceAmount(CropsResource, smallFieldSize * 62, TimePeriod.Year) // 0.39 ha × 62 t/ha, 100% Fertility
            },
            Inputs = new List<ResourceAmount>(),
            BaseGarbageProduction = 0.015
        };
        switch (fertilizerType)
        {
            case FertilizerType.Solid:
                result.Inputs.Add(new ResourceAmount(SolidFertilizerResource, 0.83 * 0.39, TimePeriod.Year));
                break;
            case FertilizerType.Liquid:
                result.Inputs.Add(new ResourceAmount(LiquidFertilizerResource, 4.16 * 0.39, TimePeriod.Year));
                break;
            case FertilizerType.Both:
                result.Inputs.Add(new ResourceAmount(SolidFertilizerResource, 0.83 * 0.39, TimePeriod.Year));
                result.Inputs.Add(new ResourceAmount(LiquidFertilizerResource, 4.16 * 0.39, TimePeriod.Year));
                break;
        }
        return result;
    }

    public ProductionBuilding MediumField { get; } = CreateMediumField(FertilizerType.None);
    public static ProductionBuilding CreateMediumField(FertilizerType fertilizerType)
    {
        var result = new ProductionBuilding()
        {
            Name = "Medium Field",
            IsSeasonDependent = true,
            Outputs = new List<ResourceAmount>
            {
                new ResourceAmount(CropsResource, mediumFieldSize * 62, TimePeriod.Year) // 1.57 ha × 62 t/ha 100% Fertility
            },
            Inputs = new List<ResourceAmount>(),
            BaseGarbageProduction = 0.03
        };
        switch (fertilizerType)
        {
            case FertilizerType.Solid:
                result.Inputs.Add(new ResourceAmount(SolidFertilizerResource, 0.83 * 1.57, TimePeriod.Year));
                break;
            case FertilizerType.Liquid:
                result.Inputs.Add(new ResourceAmount(LiquidFertilizerResource, 4.16 * 1.57, TimePeriod.Year));
                break;
            case FertilizerType.Both:
                result.Inputs.Add(new ResourceAmount(SolidFertilizerResource, 0.83 * 1.57, TimePeriod.Year));
                result.Inputs.Add(new ResourceAmount(LiquidFertilizerResource, 4.16 * 1.57, TimePeriod.Year));
                break;
        }
        return result;
    }

    public ProductionBuilding LargeField { get; } = CreateLargeField(FertilizerType.None);
    public static ProductionBuilding CreateLargeField(FertilizerType fertilizerType)
    {
        var result = new ProductionBuilding()
        {
            Name = "Large Field",
            IsSeasonDependent = true,
            Outputs = new List<ResourceAmount>
            {
                new ResourceAmount(CropsResource, largeFieldSize * 62, TimePeriod.Year) // 4.81 ha × 62 t/ha 100% Fertility
            },
            Inputs = new List<ResourceAmount>(),
            BaseGarbageProduction = 0.05
        };
        switch (fertilizerType)
        {
            case FertilizerType.Solid:
                result.Inputs.Add(new ResourceAmount(SolidFertilizerResource, 0.83 * 4.81, TimePeriod.Year));
                break;
            case FertilizerType.Liquid:
                result.Inputs.Add(new ResourceAmount(LiquidFertilizerResource, 4.16 * 4.81, TimePeriod.Year));
                break;
            case FertilizerType.Both:
                result.Inputs.Add(new ResourceAmount(SolidFertilizerResource, 0.83 * 4.81, TimePeriod.Year));
                result.Inputs.Add(new ResourceAmount(LiquidFertilizerResource, 4.16 * 4.81, TimePeriod.Year));
                break;
        }
        return result;
    }

    public static ProductionBuilding SmallFarm { get; } = CreateSmallFarm();
    public static ProductionBuilding CreateSmallFarm()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Small Farm";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>() { };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 6.0;
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = true;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.GeneralDistribution;
        result.Workdays = 643;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 13 },
            {GravelResource, 10 },
            {AsphaltResource, 8.1 },
            {SteelResource, 28 },
            {BricksResource, 10 },
            {BoardsResource, 3.4 },
            {MechanicComponentsResource, 0.81 }
        };
        return result;
    }

    public static ProductionBuilding MediumFarm { get; } = CreateMediumFarm();
    public static ProductionBuilding CreateMediumFarm()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Medium Farm";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>() { };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 6.0;
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = true;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.GeneralDistribution;
        result.Workdays = 1140;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 37 },
            {GravelResource, 29 },
            {AsphaltResource, 23 },
            {SteelResource, 9 },
            {BricksResource, 13 },
            {BoardsResource, 4.5 },
            {MechanicComponentsResource, 1.5 }
        };
        return result;
    }

    public static ProductionBuilding LargeFarm { get; } = CreateLargeFarm();
    public static ProductionBuilding CreateLargeFarm()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Large Farm";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>() { };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 6.0;
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = true;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.GeneralDistribution;
        result.Workdays = 2575;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 117 },
            {GravelResource, 90 },
            {AsphaltResource, 72 },
            {SteelResource, 61 },
            {BricksResource, 32 },
            {BoardsResource, 10 },
            {MechanicComponentsResource, 2.4 }
        };
        return result;
    }

    public static ProductionBuilding LivestockFarm { get; } = CreateLivestockFarm();
    public static ProductionBuilding CreateLivestockFarm()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Livestock Farm";
        result.Inputs = new List<ResourceAmount>()
    {
        new ResourceAmount() { Resource = CropsResource, Amount = 20 },
        new ResourceAmount() { Resource = WaterResource, Amount = 12 }
    };
        result.Outputs = new List<ResourceAmount>()
    {
        new ResourceAmount() { Resource = LivestockResource, Amount = 10 }
    };
        result.WorkersPerShift = 50;
        result.PowerConsumption = 3.0;
        result.WaterConsumption = 2.30;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0.60;
        result.EnvironmentPollution = 3.60;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.GeneralDistribution;
        result.Workdays = 1993;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
    {
        {ConcreteResource, 39 },
        {GravelResource, 30 },
        {AsphaltResource, 24 },
        {SteelResource, 41 },
        {BricksResource, 101 },
        {BoardsResource, 33 }
    };
        return result;
    }

    public static ProductionBuilding Slaughterhouse { get; } = CreateSlaughterhouse();
    public static ProductionBuilding CreateSlaughterhouse()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Slaughterhouse";
        result.Inputs = new List<ResourceAmount>()
    {
        new ResourceAmount() { Resource = LivestockResource, Amount = 150 }
    };
        result.Outputs = new List<ResourceAmount>()
    {
        new ResourceAmount() { Resource = MeatResource, Amount = 60 }
    };
        result.WorkersPerShift = 50;
        result.PowerConsumption = 3.4;
        result.WaterConsumption = 1.56;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0.60;
        result.EnvironmentPollution = 6.90;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.GeneralDistribution;
        result.Workdays = 1446;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
    {
        {ConcreteResource, 34 },
        {GravelResource, 26 },
        {AsphaltResource, 21 },
        {SteelResource, 28 },
        {BricksResource, 28 },
        {BoardsResource, 9.5 },
        {PrefabPanelsResource, 71 }
    };
        return result;
    }

    // CONSTRUCTION BUILDINGS 
    // Cement Plants (3 variants)
    public static ProductionBuilding SmallPrefabPanelsFactory { get; } = CreateSmallPrefabPanelsFactory();
    public static ProductionBuilding CreateSmallPrefabPanelsFactory()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Small Prefab Panels Factory";
        result.Inputs = new List<ResourceAmount>()
    {
        new ResourceAmount() { Resource = CementResource, Amount = 9.8 },
        new ResourceAmount() { Resource = GravelResource, Amount = 65 }
    };
        result.Outputs = new List<ResourceAmount>()
    {
        new ResourceAmount() { Resource = PrefabPanelsResource, Amount = 71 }
    };
        result.WorkersPerShift = 65;
        result.PowerConsumption = 8.0;
        result.WaterConsumption = 1.30;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0.60;
        result.EnvironmentPollution = 6.20;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.SolidHandling;
        result.Workdays = 2917;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
    {
        {ConcreteResource, 207 },
        {GravelResource, 25 },
        {AsphaltResource, 20 },
        {SteelResource, 82 },
        {BricksResource, 63 },
        {BoardsResource, 21 },
        {MechanicComponentsResource, 7.3 }
    };
        return result;
    }

    public static ProductionBuilding LargePrefabPanelsFactory { get; } = CreateLargePrefabPanelsFactory();
    public static ProductionBuilding CreateLargePrefabPanelsFactory()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Large Prefab Panels Factory";
        result.Inputs = new List<ResourceAmount>()
    {
        new ResourceAmount() { Resource = CementResource, Amount = 7.5 },
        new ResourceAmount() { Resource = GravelResource, Amount = 50 }
    };
        result.Outputs = new List<ResourceAmount>()
    {
        new ResourceAmount() { Resource = PrefabPanelsResource, Amount = 55 }
    };
        result.WorkersPerShift = 50;
        result.PowerConsumption = 6.0;
        result.WaterConsumption = 1.00;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0.60;
        result.EnvironmentPollution = 6.60;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.SolidHandling;
        result.Workdays = 3129;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
    {
        {ConcreteResource, 118 },
        {GravelResource, 55 },
        {AsphaltResource, 44 },
        {SteelResource, 52 },
        {BricksResource, 112 },
        {BoardsResource, 37 },
        {MechanicComponentsResource, 5.7 }
    };
        return result;
    }

    public static ProductionBuilding LargeCementPlant { get; } = CreateLargeCementPlant();
    public static ProductionBuilding CreateLargeCementPlant()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Large Cement Plant";
        result.Inputs = new List<ResourceAmount>()
    {
        new ResourceAmount(CoalResource, 28, TimePeriod.Day),
        new ResourceAmount(GravelResource, 240, TimePeriod.Day),
        new ResourceAmount(PowerResource, 6.3, TimePeriod.Day)
    };
        result.Outputs = new List<ResourceAmount>()
    {
        new ResourceAmount(CementResource, 108, TimePeriod.Day),
    };
        result.WorkersPerShift = 40;
        result.PowerConsumption = 6.3;
        result.WaterConsumption = 0.80;
        result.HeatConsumption = 0;
        result.SewageProduction = 0.80;
        result.BaseGarbageProduction = 7.48;
        result.GarbagePerWorker = 0.0006;
        result.EnvironmentPollution = 17.20 / 365;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.SolidHandling;
        result.Workdays = 9051;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 1137 },
            {GravelResource, 262 },
            {AsphaltResource, 209 },
            {SteelResource, 233 },
            {MechanicComponentsResource, 11 }
        };
        return result;
    }

    public static ProductionBuilding MediumCementPlant { get; } = CreateMediumCementPlant();
    public static ProductionBuilding CreateMediumCementPlant()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Medium Cement Plant";
        result.Inputs = new List<ResourceAmount>()
    {
        new ResourceAmount(CoalResource, 22, TimePeriod.Day),
        new ResourceAmount(GravelResource, 210, TimePeriod.Day),
        new ResourceAmount(PowerResource, 6.0, TimePeriod.Day)
    };
        result.Outputs = new List<ResourceAmount>()
    {
        new ResourceAmount(CementResource, 81, TimePeriod.Day),
    };
        result.WorkersPerShift = 30;
        result.PowerConsumption = 6.0;
        result.WaterConsumption = 0.69;
        result.HeatConsumption = 0;
        result.SewageProduction = 0.69;
        result.BaseGarbageProduction = 7.19;
        result.GarbagePerWorker = 0.0006;
        result.EnvironmentPollution = 14.00 / 365;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.SolidHandling;
        result.Workdays = 5475;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 738 },
            {GravelResource, 69 },
            {AsphaltResource, 55 },
            {SteelResource, 248 },
            {MechanicComponentsResource, 4.2 }
        };
        return result;
    }

    // Concrete Plant
    public static ProductionBuilding ConcretePlant { get; } = CreateConcretePlant();
    public static ProductionBuilding CreateConcretePlant()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Concrete Plant";
        result.Inputs = new List<ResourceAmount>()
    {
        new ResourceAmount(GravelResource, 135, TimePeriod.Day),
        new ResourceAmount(CementResource, 30, TimePeriod.Day),
        new ResourceAmount(WaterResource, 85, TimePeriod.Day),      // input resource (55% quality required)
        new ResourceAmount(PowerResource, 15, TimePeriod.Day)
    };
        result.Outputs = new List<ResourceAmount>()
    {
        new ResourceAmount(ConcreteResource, 175, TimePeriod.Day),
    };
        result.WorkersPerShift = 5;
        result.PowerConsumption = 15;
        result.WaterConsumption = 0.10;      // building water consumption
        result.HeatConsumption = 0;
        result.SewageProduction = 85.10;     // Input water + building consumption
        result.BaseGarbageProduction = 4.35;
        result.GarbagePerWorker = 0.0006;
        result.EnvironmentPollution = 8.40 / 365;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.SolidHandling;
        result.Workdays = 1436;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 24 },
            {GravelResource, 18 },
            {AsphaltResource, 14 },
            {SteelResource, 29 },
            {BricksResource, 60 },
            {BoardsResource, 20 },
            {MechanicComponentsResource, 2.4 }
        };
        return result;
    }

    // Brick Factory
    public static ProductionBuilding BrickFactory { get; } = CreateBrickFactory();
    public static ProductionBuilding CreateBrickFactory()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Brick Factory";
        result.Inputs = new List<ResourceAmount>()
    {
        new ResourceAmount(CoalResource, 33, TimePeriod.Day),
        new ResourceAmount(PowerResource, 3.0, TimePeriod.Day)
    };
        result.Outputs = new List<ResourceAmount>()
    {
        new ResourceAmount(BricksResource, 51, TimePeriod.Day),
    };
        result.WorkersPerShift = 75;
        result.PowerConsumption = 3.0;
        result.WaterConsumption = 1.50;
        result.HeatConsumption = 0;
        result.SewageProduction = 1.50;
        result.BaseGarbageProduction = 0.004;
        result.GarbagePerWorker = 0.0006;
        result.EnvironmentPollution = 17.00 / 365;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.SolidHandling;
        result.Workdays = 2723;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 49 },
            {GravelResource, 37 },
            {AsphaltResource, 30 },
            {SteelResource, 30 },
            {BricksResource, 157 },
            {BoardsResource, 70 }
        };
        return result;
    }

    // Asphalt Plant
    public static ProductionBuilding AsphaltPlant { get; } = CreateAsphaltPlant();
    public static ProductionBuilding CreateAsphaltPlant()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Asphalt Plant";
        result.Inputs = new List<ResourceAmount>()
    {
        new ResourceAmount(GravelResource, 125, TimePeriod.Day),
        new ResourceAmount(BitumenResource, 20, TimePeriod.Day),
        new ResourceAmount(PowerResource, 18, TimePeriod.Day)
    };
        result.Outputs = new List<ResourceAmount>()
    {
        new ResourceAmount(AsphaltResource, 145, TimePeriod.Day),
    };
        result.WorkersPerShift = 5;
        result.PowerConsumption = 18;
        result.WaterConsumption = 0.10;
        result.HeatConsumption = 0;
        result.SewageProduction = 0.10;
        result.GarbagePerWorker = 0.0006;
        result.EnvironmentPollution = 7.00 / 365;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.SolidHandling;
        result.Workdays = 1439;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 25 },
            {GravelResource, 19 },
            {AsphaltResource, 15 },
            {SteelResource, 40 },
            {MechanicComponentsResource, 8.4 }
        };
        return result;
    }

    // Utility Buildings
    // Electricity
    public static ProductionBuilding SmallCoalPowerPlant { get; } = CreateSmallCoalPowerPlant();
    public static ProductionBuilding CreateSmallCoalPowerPlant()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Small Coal Power Plant";
        result.Inputs = new List<ResourceAmount>()
    {
        new ResourceAmount() { Resource = CoalResource, Amount = 12 }
    };
        result.Outputs = new List<ResourceAmount>()
    {
        new ResourceAmount() { Resource = PowerResource, Amount = 600 } // 600 MW * 24 hours = 14400 MWh/day
    };
        result.WorkersPerShift = 10;
        result.PowerConsumption = 0.30;
        result.WaterConsumption = 0.20;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0.33;
        result.EnvironmentPollution = 22.00;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.PowerHandling;
        result.Workdays = 4920;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
    {
        {ConcreteResource, 137 },
        {GravelResource, 105 },
        {AsphaltResource, 84 },
        {SteelResource, 61 },
        {BricksResource, 193 },
        {BoardsResource, 64 },
        {MechanicComponentsResource, 6.5 },
        {ElectroComponentsResource, 1.3 }
    };
        return result;
    }

    public static ProductionBuilding CoalPowerPlant { get; } = CreateCoalPowerPlant();
    public static ProductionBuilding CreateCoalPowerPlant()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Coal Power Plant";
        result.Inputs = new List<ResourceAmount>()
        {
            new ResourceAmount(CoalResource, 24, TimePeriod.Day)
        };
        result.Outputs = new List<ResourceAmount>()
        {
            new ResourceAmount(PowerResource, 1400, TimePeriod.Day),
        };
        result.WorkersPerShift = 20;
        result.PowerConsumption = 0.6;
        result.WaterConsumption = 0.4;
        result.HeatConsumption = 0;
        result.SewageProduction = 0.4;       // Resource + daily water consumption
        result.BaseGarbageProduction =0.03;
        result.GarbagePerWorker = 0.00033;
        result.EnvironmentPollution = 39.5 / 365;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsUtilityBuilding = true;
        result.Workdays = 7631;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 871 },
            {GravelResource, 142 },
            {AsphaltResource, 113 },
            {SteelResource, 287 },
            {BoardsResource, 73 },
            {MechanicComponentsResource, 7.3 },
            {ElectroComponentsResource, 3.7 }
        };
        return result;
    }

    public static ProductionBuilding GasPowerPlant { get; } = CreateGasPowerPlant();
    public static ProductionBuilding CreateGasPowerPlant()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Gas Power Plant";
        result.Inputs = new List<ResourceAmount>()
        {
            new ResourceAmount(OilResource, 8.8, TimePeriod.Day)
        };
        result.Outputs = new List<ResourceAmount>()
        {
            new ResourceAmount(PowerResource, 1050, TimePeriod.Day),
        };
        result.WorkersPerShift = 15;
        result.PowerConsumption = 3.4;
        result.WaterConsumption = 0.3;
        result.HeatConsumption = 0;
        result.SewageProduction = 0.3;       // Resource + daily water consumption
        result.BaseGarbageProduction = 0.11;
        result.GarbagePerWorker = 0.00033;
        result.EnvironmentPollution = 35.8 / 365;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsUtilityBuilding = true;
        result.Workdays = 6328;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 865 },
            {GravelResource, 125 },
            {SteelResource, 203 },
            {MechanicComponentsResource, 6.3 },
            {ElectroComponentsResource, 2.9 }
        };
        return result;
    }

    public static ProductionBuilding SingleReactorNuclearPowerPlant { get; } = CreateSingleReactorNuclearPowerPlant();
    public static ProductionBuilding CreateSingleReactorNuclearPowerPlant()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Single Reactor Nuclear Power Plant";
        result.Inputs = new List<ResourceAmount>()
        {
            new ResourceAmount(NuclearFuelResource, 0.04, TimePeriod.Day),
            new ResourceAmount(WaterResource, 6.8, TimePeriod.Day)
        };
        result.Outputs = new List<ResourceAmount>()
        {
            new ResourceAmount(PowerResource, 4680, TimePeriod.Day),
            new ResourceAmount(NuclearWasteResource, 0.020, TimePeriod.Day)
        };
        result.WorkersPerShift = 60;
        result.PowerConsumption = 7.8;
        result.WaterConsumption = 1.2;
        result.HeatConsumption = 0;
        result.SewageProduction = 1.2;       // Resource + daily water consumption
        result.GarbagePerWorker = 0.00033;
        result.EnvironmentPollution = 9 / 365;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsUtilityBuilding = true;
        result.Workdays = 12561;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 235 },
            {GravelResource, 181 },
            {AsphaltResource, 145 },
            {SteelResource, 439 },
            {BricksResource, 129 },
            {BoardsResource, 43 },
            {MechanicComponentsResource, 25 },
            {ElectroComponentsResource, 18 }
        };
        return result;
    }

    public static ProductionBuilding AltSingleReactorNuclearPowerPlant { get; } = CreateAltSingleReactorNuclearPowerPlant();
    public static ProductionBuilding CreateAltSingleReactorNuclearPowerPlant()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Alt Single-Reactor Nuclear Power Plant";
        result.Inputs = new List<ResourceAmount>()
    {
        new ResourceAmount() { Resource = NuclearFuelResource, Amount = 0.020 },
        new ResourceAmount() { Resource = WaterResource, Amount = 3.4 }
    };
        result.Outputs = new List<ResourceAmount>()
    {
        new ResourceAmount() { Resource = PowerResource, Amount = 2320 },
        new ResourceAmount() { Resource = NuclearWasteResource, Amount = 0.010 }
    };
        result.WorkersPerShift = 40;
        result.PowerConsumption = 7.8;
        result.WaterConsumption = 1.20;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0.33;
        result.EnvironmentPollution = 3.00;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.PowerHandling;
        result.Workdays = 10548;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
    {
        {ConcreteResource, 459 },
        {GravelResource, 199 },
        {AsphaltResource, 159 },
        {SteelResource, 170 },
        {BricksResource, 381 },
        {BoardsResource, 127 },
        {MechanicComponentsResource, 8.1 },
        {ElectroComponentsResource, 8.1 }
    };
        return result;
    }

    public static ProductionBuilding TwinReactorNuclearPowerPlant { get; } = CreateTwinReactorNuclearPowerPlant();
    public static ProductionBuilding CreateTwinReactorNuclearPowerPlant()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Twin Reactor Nuclear Power Plant";
        result.Inputs = new List<ResourceAmount>()
        {
            new ResourceAmount(NuclearFuelResource, 0.08, TimePeriod.Day),
            new ResourceAmount(WaterResource, 13, TimePeriod.Day)
        };
        result.Outputs = new List<ResourceAmount>()
        {
            new ResourceAmount(PowerResource, 9360, TimePeriod.Day),
            new ResourceAmount(NuclearWasteResource, 0.040, TimePeriod.Day)
        };
        result.WorkersPerShift = 120;
        result.PowerConsumption = 9.6;
        result.WaterConsumption = 2.4;
        result.HeatConsumption = 0;
        result.SewageProduction = 2.4;       // Resource + daily water consumption
        result.GarbagePerWorker = 0.00033;
        result.EnvironmentPollution = 14.2 / 365;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsUtilityBuilding = true;
        result.Workdays = 20418;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 367 },
            {GravelResource, 282 },
            {AsphaltResource, 226 },
            {SteelResource, 769 },
            {BricksResource, 162 },
            {BoardsResource, 54 },
            {MechanicComponentsResource, 47 },
            {ElectroComponentsResource, 23 }
        };
        return result;
    }

    public static ProductionBuilding ZaporozieReactor { get; } = CreateZaporozieReactor();
    public static ProductionBuilding CreateZaporozieReactor()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Zaporozie Reactor";
        result.Inputs = new List<ResourceAmount>()
    {
        new ResourceAmount() { Resource = NuclearFuelResource, Amount = 0.045 },
        new ResourceAmount() { Resource = WaterResource, Amount = 7.7 }
    };
        result.Outputs = new List<ResourceAmount>()
    {
        new ResourceAmount() { Resource = PowerResource, Amount = 5850 }, // 5850 MW * 24 hours = 140400 MWh/day
        new ResourceAmount() { Resource = NuclearWasteResource, Amount = 0.023 }
    };
        result.WorkersPerShift = 45;
        result.PowerConsumption = 8.0;
        result.WaterConsumption = 1.36;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0.33;
        result.EnvironmentPollution = 4.20;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.PowerHandling;
        result.Workdays = 13499;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
    {
        {ConcreteResource, 318 },
        {GravelResource, 245 },
        {AsphaltResource, 196 },
        {SteelResource, 280 },
        {BricksResource, 212 },
        {BoardsResource, 70 },
        {PrefabPanelsResource, 768 },
        {MechanicComponentsResource, 4.3 }
    };
        return result;
    }

    public static ProductionBuilding SmallWindPowerPlant { get; } = CreateSmallWindPowerPlant();
    public static ProductionBuilding CreateSmallWindPowerPlant()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Small Wind Power Plant";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>()
    {
        new ResourceAmount() { Resource = PowerResource, Amount = 15} // 15 MW * 24 hours = 360 MWh/day
    };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 0;
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.PowerHandling;
        result.Workdays = 239;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
    {
        {ConcreteResource, 1.3 },
        {GravelResource, 0.99 },
        {AsphaltResource, 0.80 },
        {SteelResource, 5.9 },
        {MechanicComponentsResource, 1.0 },
        {ElectroComponentsResource, 0.65 }
    };
        return result;
    }

    public static ProductionBuilding BigWindPowerPlant { get; } = CreateBigWindPowerPlant();
    public static ProductionBuilding CreateBigWindPowerPlant()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Big Wind Power Plant";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>()
    {
        new ResourceAmount() { Resource = PowerResource, Amount = 35} // 35 MW * 24 hours = 840 MWh/day
    };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 0;
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.PowerHandling;
        result.Workdays = 689;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
    {
        {ConcreteResource, 3.2 },
        {GravelResource, 2.5 },
        {AsphaltResource, 2.0 },
        {SteelResource, 17 },
        {MechanicComponentsResource, 2.9 },
        {ElectroComponentsResource, 1.9 }
    };
        return result;
    }

    public static ProductionBuilding SolarPowerPlant { get; } = CreateSolarPowerPlant();
    public static ProductionBuilding CreateSolarPowerPlant()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Solar Power Plant";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>()
    {
        new ResourceAmount() { Resource = PowerResource, Amount = 560 } 
    };
        result.WorkersPerShift = 8;
        result.PowerConsumption = 0.24;
        result.WaterConsumption = 0.16;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0.33;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.PowerHandling;
        result.Workdays = 8413;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
    {
        {ConcreteResource, 153 },
        {GravelResource, 34 },
        {SteelResource, 285 },
        {MechanicComponentsResource, 9.8 },
        {ElectroComponentsResource, 44 }
    };
        return result;
    }

    public static ProductionBuilding SmallWaterWell { get; } = CreateSmallWaterWell();
    public static ProductionBuilding CreateSmallWaterWell()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Small Water Well";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>()
    {
        new ResourceAmount() { Resource = WaterResource, Amount = 70 }
    };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 2.1;
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.WaterHandling;
        result.Workdays = 393;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
    {
        {ConcreteResource, 3.6 },
        {GravelResource, 2.7 },
        {AsphaltResource, 2.2 },
        {SteelResource, 8.3 },
        {BricksResource, 14 },
        {BoardsResource, 4.8 },
        {MechanicComponentsResource, 1.3 }
    };
        return result;
    }

    public static ProductionBuilding BigWaterWell { get; } = CreateBigWaterWell();
    public static ProductionBuilding CreateBigWaterWell()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Big Water Well";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>()
    {
        new ResourceAmount() { Resource = WaterResource, Amount = 215 }
    };
        result.WorkersPerShift = 7;
        result.PowerConsumption = 5.7;
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0.20;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.WaterHandling;
        result.Workdays = 744;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
    {
        {ConcreteResource, 7.5 },
        {GravelResource, 5.8 },
        {AsphaltResource, 4.6 },
        {SteelResource, 16 },
        {BricksResource, 25 },
        {BoardsResource, 8.4 },
        {MechanicComponentsResource, 2.7 }
    };
        return result;
    }

    public static ProductionBuilding SurfaceWaterIntake { get; } = CreateSurfaceWaterIntake();
    public static ProductionBuilding CreateSurfaceWaterIntake()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Surface Water Intake";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>()
    {
        new ResourceAmount() { Resource = WaterResource, Amount = 150 }
    };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 0;
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.WaterHandling;
        result.Workdays = 22;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
    {
        {ConcreteResource, 4.1 },
        {GravelResource, 0.39 },
        {AsphaltResource, 0.31 },
        {SteelResource, 0.81 }
    };
        return result;
    }

    public static ProductionBuilding SmallWaterTreatment { get; } = CreateSmallWaterTreatment();
    public static ProductionBuilding CreateSmallWaterTreatment()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Small Water Treatment";
        result.Inputs = new List<ResourceAmount>()
        {
            new ResourceAmount(PowerResource, 3.9, TimePeriod.Day),
            new ResourceAmount(ChemicalsResource, 0.25, TimePeriod.Day)
        };
        result.Outputs = new List<ResourceAmount>()
        {
            new ResourceAmount(WaterResource, 120, TimePeriod.Day),
        };
        result.WorkersPerShift = 5;
        result.PowerConsumption = 10;
        result.WaterConsumption = 0.1;
        result.HeatConsumption = 0;
        result.SewageProduction = 0.1 + 13;       // Resource + daily water consumption
        result.GarbagePerWorker = 0.0006;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsUtilityBuilding = true;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 10 },
            {GravelResource, 8.3 },
            {AsphaltResource, 6.7 },
            {SteelResource, 10 },
            {BricksResource, 23 },
            {BoardsResource, 7.8 },
            {MechanicComponentsResource, 1.6 }
        };
        return result;
    }

    public static ProductionBuilding BigWaterTreatment { get; } = CreateBigWaterTreatment();
    public static ProductionBuilding CreateBigWaterTreatment()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Big Water Treatment";
        result.Inputs = new List<ResourceAmount>()
        {
            new ResourceAmount(PowerResource, 7.6, TimePeriod.Day),
            new ResourceAmount(ChemicalsResource, 0.47, TimePeriod.Day)
        };
        result.Outputs = new List<ResourceAmount>()
        {
            new ResourceAmount(WaterResource, 300, TimePeriod.Day),
        };
        result.WorkersPerShift = 10;
        result.PowerConsumption = 13;
        result.WaterConsumption = 0.2;
        result.HeatConsumption = 0;
        result.SewageProduction = 0.2 + 30;       // Resource + daily water consumption
        result.GarbagePerWorker = 0.0006;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsUtilityBuilding = true;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 26 },
            {GravelResource, 20 },
            {AsphaltResource, 16 },
            {SteelResource, 41 },
            {BricksResource, 45 },
            {BoardsResource, 15 },
            {MechanicComponentsResource, 7.4 }
        };
        return result;
    }

    public static ProductionBuilding SmallSewageTreatment { get; } = CreateSmallSewageTreatment();
    public static ProductionBuilding CreateSmallSewageTreatment()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Small Sewage Treatment";
        result.Inputs = new List<ResourceAmount>()
        {
            new ResourceAmount(PowerResource, 5.1, TimePeriod.Day),
            new ResourceAmount(ChemicalsResource, 0.32, TimePeriod.Day)
        };
        result.Outputs = new List<ResourceAmount>()
        {
            new ResourceAmount(WasteWaterResource, 80, TimePeriod.Day)
        };
        result.WorkersPerShift = 10;
        result.PowerConsumption = 11;
        result.WaterConsumption = 0.2;
        result.HeatConsumption = 0;
        result.SewageProduction = 0.2;       // daily water consumption
        result.SewageDisposalCapacity = 80;
        result.GarbagePerWorker = 0.0006;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsUtilityBuilding = true;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 80 },
            {GravelResource, 32 },
            {AsphaltResource, 26 },
            {SteelResource, 32 },
            {BricksResource, 12 },
            {BoardsResource, 4.2 },
            {MechanicComponentsResource, 4.5 }
        };
        return result;
    }

    public static ProductionBuilding BigSewageTreatment { get; } = CreateBigSewageTreatment();
    public static ProductionBuilding CreateBigSewageTreatment()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Big Sewage Treatment";
        result.Inputs = new List<ResourceAmount>()
        {
            new ResourceAmount(PowerResource, 9.2, TimePeriod.Day),
            new ResourceAmount(ChemicalsResource, 0.6, TimePeriod.Day)
        };
        result.Outputs = new List<ResourceAmount>()
        {
            new ResourceAmount(WasteWaterResource, 220, TimePeriod.Day)
        };
        result.WorkersPerShift = 20;
        result.PowerConsumption = 15;
        result.WaterConsumption = 0.4;
        result.HeatConsumption = 0;
        result.SewageProduction = 0.4;       // daily water consumption
        result.SewageDisposalCapacity = 220;
        result.GarbagePerWorker = 0.0006;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsUtilityBuilding = true;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 202 },
            {GravelResource, 63 },
            {AsphaltResource, 51 },
            {SteelResource, 75 },
            {BricksResource, 33 },
            {BoardsResource, 11 },
            {MechanicComponentsResource, 9.2 }
        };
        return result;
    }

    public static ProductionBuilding SmallHeatingPlant { get; } = CreateSmallHeatingPlant();
    public static ProductionBuilding CreateSmallHeatingPlant()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Small Heating Plant";
        result.Inputs = new List<ResourceAmount>()
    {
        new ResourceAmount(CoalResource, 2.1, TimePeriod.Day)
    };
        result.Outputs = new List<ResourceAmount>()
    {
        new ResourceAmount(HeatResource, 42, TimePeriod.Day),
    };
        result.WorkersPerShift = 7;
        result.PowerConsumption = 27;
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0.00033;
        result.EnvironmentPollution = 7.0 / 365;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsUtilityBuilding = true;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
    {
        {ConcreteResource, 18 },
        {GravelResource, 14 },
        {AsphaltResource, 11 },
        {SteelResource, 20 },
        {BricksResource, 32 },
        {BoardsResource, 10 },
        {MechanicComponentsResource, 1.6 }
    };
        return result;
    }

    public static ProductionBuilding HeatingPlant { get; } = CreateHeatingPlant();
    public static ProductionBuilding CreateHeatingPlant()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Heating Plant";
        result.Inputs = new List<ResourceAmount>()
    {
        new ResourceAmount(CoalResource, 8.41, TimePeriod.Day)
    };
        result.Outputs = new List<ResourceAmount>()
    {
        new ResourceAmount(HeatResource, 210, TimePeriod.Day),
    };
        result.WorkersPerShift = 30;
        result.PowerConsumption = 63;
        result.WaterConsumption = 0.60;
        result.HeatConsumption = 0;
        result.SewageProduction = 0.60;
        result.BaseGarbageProduction = 0.01;
        result.GarbagePerWorker = 0.00033;
        result.EnvironmentPollution = 26.50 / 365;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsUtilityBuilding = true;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
    {
        {ConcreteResource, 58 },
        {GravelResource, 45 },
        {AsphaltResource, 36 },
        {SteelResource, 73 },
        {BricksResource, 25 },
        {BoardsResource, 8.5 },
        {MechanicComponentsResource, 2.2 }
    };
        return result;
    }

    // Support Buildings
    public static ProductionBuilding LiquidPumpingStation { get; } = CreateLiquidPumpingStation();
    public static ProductionBuilding CreateLiquidPumpingStation()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Liquid Pumping Station";
        result.Inputs = new List<ResourceAmount>() {};
        result.Outputs = new List<ResourceAmount>() {};
        result.WorkersPerShift = 0;
        result.PowerConsumption = 9.0;  // MWh/day
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsUtilityBuilding = false;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.LiquidHandling;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 7.7 },
            {GravelResource, 5.9 },
            {AsphaltResource, 4.7 },
            {SteelResource, 10 },
            {MechanicComponentsResource, 2.1 }
        };
        return result;
    }
    public static ProductionBuilding OilLoadingUnloading { get; } = CreateOilLoadingUnloading();
    public static ProductionBuilding CreateOilLoadingUnloading()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Oil Loading Unloading Station";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>() { };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 14.0;  // MWh/day
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsUtilityBuilding = false;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.LiquidHandling;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 9.4 },
            {GravelResource, 7.2 },
            {AsphaltResource, 5.8 },
            {SteelResource, 8.7 },
            {MechanicComponentsResource, 1.8 }
        };
        return result;
    }
    public static ProductionBuilding BigOilStorage { get; } = CreateBigOilStorage();
    public static ProductionBuilding CreateBigOilStorage()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Big Oil Storage";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>() { };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 3.0;  // MWh/day
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsUtilityBuilding = false;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.LiquidHandling;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 31 },
            {GravelResource, 23 },
            {AsphaltResource, 19 },
            {SteelResource, 11 },
            {MechanicComponentsResource, 2.4 }
        };
        return result;
    }
    public static ProductionBuilding MediumOilStorage { get; } = CreateMediumOilStorage();
    public static ProductionBuilding CreateMediumOilStorage()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Medium Oil Storage";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>() { };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 3.0;  // MWh/day
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsUtilityBuilding = false;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.LiquidHandling;
        result.Workdays = 184;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 6.6 },
            {GravelResource, 5.1 },
            {AsphaltResource, 4.1 },
            {SteelResource, 3.8 },
            {MechanicComponentsResource, 0.79 }
        };
        return result;
    }
    public static ProductionBuilding SmallOilStorage { get; } = CreateSmallOilStorage();
    public static ProductionBuilding CreateSmallOilStorage()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Small Oil Storage";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>() { };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 3.0;  // MWh/day
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsUtilityBuilding = false;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.LiquidHandling;
        result.Workdays = 51;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 1.5 },
            {GravelResource, 1.2 },
            {AsphaltResource, 0.95 },
            {SteelResource, 1.2 },
            {MechanicComponentsResource, 0.25 }
        };
        return result;
    }
    public static ProductionBuilding UndergroundPumpingStation { get; } = CreateUndergroundPumpingStation();
    public static ProductionBuilding CreateUndergroundPumpingStation()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Underground Pumping Station";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>() { };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 9.0;  // MWh/day
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsUtilityBuilding = false;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.LiquidHandling;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 17 },
            {GravelResource, 13 },
            {AsphaltResource, 10 },
            {SteelResource, 12 },
            {MechanicComponentsResource, 2.6 }
        };
        return result;
    }
    public static ProductionBuilding ConveyorEngineTransfer { get; } = CreateConveyorEngineTransfer();
    public static ProductionBuilding CreateConveyorEngineTransfer()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Conveyor Engine Transfer";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>() { };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 6.0;  // MWh/day
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsUtilityBuilding = false;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.BulkHandling;
        result.Workdays = 194;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 2.7 },
            {GravelResource, 2.0 },
            {AsphaltResource, 1.6 },
            {SteelResource, 5.8 },
            {MechanicComponentsResource, 1.2 }
        };
        return result;
    }
    public static ProductionBuilding ConveyorOverpass { get; } = CreateConveyorOverpass();
    public static ProductionBuilding CreateConveyorOverpass()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Conveyor Overpass";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>() { };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 6.0;  // MWh/day
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsUtilityBuilding = false;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.BulkHandling;
        result.Workdays = 429;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 8.6 },
            {GravelResource, 6.6 },
            {AsphaltResource, 5.3 },
            {SteelResource, 11 },
            {MechanicComponentsResource, 2.4 }
        };
        return result;
    }
    public static ProductionBuilding LivestockHall { get; } = CreateLivestockHall();
    public static ProductionBuilding CreateLivestockHall()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Livestock Hall";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>() { };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 3.0;
        result.WaterConsumption = 11.10;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.GeneralDistribution;
        result.Workdays = 487;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
    {
        {ConcreteResource, 11 },
        {GravelResource, 8.7 },
        {AsphaltResource, 7.0 },
        {SteelResource, 13 },
        {BricksResource, 19 },
        {BoardsResource, 6.3 }
    };
        return result;
    }
    public static ProductionBuilding HighVoltageSwitch { get; } = CreateHighVoltageSwitch();
    public static ProductionBuilding CreateHighVoltageSwitch()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "High-Voltage Switch";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>() { };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 0;
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.PowerHandling;
        result.Workdays = 100;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
    {
        {ConcreteResource, 4.3 },
        {GravelResource, 3.3 },
        {AsphaltResource, 2.6 },
        {SteelResource, 1.8 },
        {ElectroComponentsResource, 0.38 }
    };
        return result;
    }
    public static ProductionBuilding HighVoltageSwitch6x { get; } = CreateHighVoltageSwitch6x();
    public static ProductionBuilding CreateHighVoltageSwitch6x()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "High-Voltage Switch 6x";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>() { };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 0;
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.PowerHandling;
        result.Workdays = 144;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
    {
        {ConcreteResource, 6.3 },
        {GravelResource, 4.9 },
        {AsphaltResource, 3.9 },
        {SteelResource, 2.5 },
        {ElectroComponentsResource, 0.53 }
    };
        return result;
    }
    public static ProductionBuilding HighVoltageSwitchPriority { get; } = CreateHighVoltageSwitchPriority();
    public static ProductionBuilding CreateHighVoltageSwitchPriority()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "High-Voltage Switch Priority";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>() { };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 0;
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.PowerHandling;
        result.Workdays = 198;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
    {
        {ConcreteResource, 6.3 },
        {GravelResource, 4.9 },
        {AsphaltResource, 3.9 },
        {SteelResource, 4.4 },
        {ElectroComponentsResource, 0.92 }
    };
        return result;
    }
    public static ProductionBuilding ZaporozieVeza { get; } = CreateZaporozieVeza();
    public static ProductionBuilding CreateZaporozieVeza()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Zaporozie Veza";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>() { };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 3.0;
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 12.00;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.PowerHandling;
        result.Workdays = 4564;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
    {
        {ConcreteResource, 436 },
        {GravelResource, 217 },
        {AsphaltResource, 173 },
        {SteelResource, 56 },
        {MechanicComponentsResource, 4.4 }
    };
        return result;
    }
    public static ProductionBuilding CoolingTower { get; } = CreateCoolingTower();
    public static ProductionBuilding CreateCoolingTower()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Cooling Tower";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>() { };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 3.0;
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 12.00;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.PowerHandling;
        result.Workdays = 5320;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
    {
        {ConcreteResource, 506 },
        {GravelResource, 253 },
        {AsphaltResource, 203 },
        {SteelResource, 65 },
        {MechanicComponentsResource, 5.2 }
    };
        return result;
    }

    // Support Buildings - Aggregate Storage Buildings
    public static ProductionBuilding AggregateStorage870 { get; } = CreateAggregateStorage870();
    public static ProductionBuilding CreateAggregateStorage870()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Aggregate Storage (870t)";
        result.Inputs = new List<ResourceAmount>();
        result.Outputs = new List<ResourceAmount>();
        result.WorkersPerShift = 0;
        result.PowerConsumption = 3.0; // 3.0 MWh/day from screenshot
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsQualityDependent = false;
        result.CanUseVehicles = false;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.BulkHandling;
        result.Workdays = 344;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 8.8 },
            {GravelResource, 6.8 },
            {AsphaltResource, 5.4 },
            {SteelResource, 19 },
            {MechanicComponentsResource, 0.34 }
        };
        return result;
    }
    public static ProductionBuilding AggregateStorage1000 { get; } = CreateAggregateStorage1000();
    public static ProductionBuilding CreateAggregateStorage1000()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Aggregate Storage (1000t)";
        result.Inputs = new List<ResourceAmount>();
        result.Outputs = new List<ResourceAmount>();
        result.WorkersPerShift = 0;
        result.PowerConsumption = 3.0;
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsQualityDependent = false;
        result.CanUseVehicles = false;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.BulkHandling;
        result.Workdays = 440;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 61 },
            {GravelResource, 6.9 },
            {AsphaltResource, 5.5 },
            {SteelResource, 15 },
            {MechanicComponentsResource, 0.73 }
        };
        return result;
    }
    public static ProductionBuilding AggregateStorage1950 { get; } = CreateAggregateStorage1950();
    public static ProductionBuilding CreateAggregateStorage1950()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Aggregate Storage (1950t)";
        result.Inputs = new List<ResourceAmount>();
        result.Outputs = new List<ResourceAmount>();
        result.WorkersPerShift = 0;
        result.PowerConsumption = 3.0;
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsQualityDependent = false;
        result.CanUseVehicles = false;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.BulkHandling;
        result.Workdays = 672;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 16 },
            {GravelResource, 13 },
            {AsphaltResource, 10 },
            {SteelResource, 34 },
            {MechanicComponentsResource, 1.0 }
        };
        return result;
    }
    public static ProductionBuilding AggregateStorage2000 { get; } = CreateAggregateStorage2000();
    public static ProductionBuilding CreateAggregateStorage2000()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Aggregate Storage (2000t)";
        result.Inputs = new List<ResourceAmount>();
        result.Outputs = new List<ResourceAmount>();
        result.WorkersPerShift = 0;
        result.PowerConsumption = 3.0;
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsQualityDependent = false;
        result.CanUseVehicles = false;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.BulkHandling;
        result.Workdays = 784;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 104 },
            {GravelResource, 12 },
            {AsphaltResource, 9.9 },
            {SteelResource, 27 },
            {MechanicComponentsResource, 1.5 }
        };
        return result;
    }
    public static ProductionBuilding AggregateStorage2500 { get; } = CreateAggregateStorage2500();
    public static ProductionBuilding CreateAggregateStorage2500()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Aggregate Storage (2500t)";
        result.Inputs = new List<ResourceAmount>();
        result.Outputs = new List<ResourceAmount>();
        result.WorkersPerShift = 0;
        result.PowerConsumption = 3.0;
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsQualityDependent = false;
        result.CanUseVehicles = false;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.BulkHandling;
        result.Workdays = 1111;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 133 },
            {GravelResource, 29 },
            {AsphaltResource, 23 },
            {SteelResource, 29 },
            {MechanicComponentsResource, 1.7 }
        };
        return result;
    }
    public static ProductionBuilding AggregateStorage5000 { get; } = CreateAggregateStorage5000();
    public static ProductionBuilding CreateAggregateStorage5000()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Aggregate Storage (5000t)";
        result.Inputs = new List<ResourceAmount>();
        result.Outputs = new List<ResourceAmount>();
        result.WorkersPerShift = 0;
        result.PowerConsumption = 3.0;
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsQualityDependent = false;
        result.CanUseVehicles = false;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.BulkHandling;
        result.Workdays = 1994;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 239 },
            {GravelResource, 47 },
            {AsphaltResource, 38 },
            {SteelResource, 57 },
            {MechanicComponentsResource, 3.5 }
        };
        return result;
    }
    public static ProductionBuilding TrainAggregateLoading123m { get; } = CreateTrainAggregateLoading123m();
    public static ProductionBuilding CreateTrainAggregateLoading123m()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Train Aggregate Loading (123m)";
        result.Inputs = new List<ResourceAmount>();  // No inputs - it's a loading station
        result.Outputs = new List<ResourceAmount>();  // No outputs - it's infrastructure
        result.WorkersPerShift = 0;  // Automated
        result.PowerConsumption = 211.0; // 211 MWh/day converted to MW
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsQualityDependent = false;
        result.CanUseVehicles = false;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.BulkHandling;
        result.Workdays = 2822;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 310 },
            {GravelResource, 36 },
            {AsphaltResource, 29 },
            {SteelResource, 97 },
            {MechanicComponentsResource, 7.9 }
        };
        return result;
    }
    public static ProductionBuilding TrainAggregateLoading123mLarge { get; } = CreateTrainAggregateLoading123mLarge();
    public static ProductionBuilding CreateTrainAggregateLoading123mLarge()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Train Aggregate Loading (123m - Large) ";
        result.Inputs = new List<ResourceAmount>();  // No inputs - it's a loading station
        result.Outputs = new List<ResourceAmount>();  // No outputs - it's infrastructure
        result.WorkersPerShift = 0;  // Automated
        result.PowerConsumption = 211.0; // 211 MWh/day converted to MW
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsQualityDependent = false;
        result.CanUseVehicles = false;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.BulkHandling;
        result.Workdays = 3295;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 351 },
            {GravelResource, 44 },
            {AsphaltResource, 35 },
            {SteelResource, 112 },
            {MechanicComponentsResource, 9.5 }
        };
        return result;
    }
    public static ProductionBuilding TrainAggregateLoading100m { get; } = CreateTrainAggregateLoading100m();
    public static ProductionBuilding CreateTrainAggregateLoading100m()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Train Aggregate Loading (100m)";
        result.Inputs = new List<ResourceAmount>();  // No inputs - it's a loading station
        result.Outputs = new List<ResourceAmount>();  // No outputs - it's infrastructure
        result.WorkersPerShift = 0;  // Automated
        result.PowerConsumption = 74.0; // 211 MWh/day converted to MW
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsQualityDependent = false;
        result.CanUseVehicles = false;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.BulkHandling;
        result.Workdays = 2340;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 258 },
            {GravelResource, 39 },
            {AsphaltResource, 31 },
            {SteelResource, 75 },
            {MechanicComponentsResource, 5.9 }
        };
        return result;
    }
    public static ProductionBuilding TrainAggregateLoading32m { get; } = CreateTrainAggregateLoading32m();
    public static ProductionBuilding CreateTrainAggregateLoading32m()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Train Aggregate Loading (32m)";
        result.Inputs = new List<ResourceAmount>();  // No inputs - it's a loading station
        result.Outputs = new List<ResourceAmount>();  // No outputs - it's infrastructure
        result.WorkersPerShift = 0;  // Automated
        result.PowerConsumption = 38; // 211 MWh/day converted to MW
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsQualityDependent = false;
        result.CanUseVehicles = false;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.BulkHandling;
        result.Workdays = 1141;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 12 },
            {GravelResource, 13 },
            {AsphaltResource, 10 },
            {SteelResource, 40 },
            {MechanicComponentsResource, 3.3 }
        };
        return result;
    }
    public static ProductionBuilding TrainAggregateLoading23m { get; } = CreateTrainAggregateLoading23m();
    public static ProductionBuilding CreateTrainAggregateLoading23m()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Train Aggregate Loading (23m)";
        result.Inputs = new List<ResourceAmount>();  // No inputs - it's a loading station
        result.Outputs = new List<ResourceAmount>();  // No outputs - it's infrastructure
        result.WorkersPerShift = 0;  // Automated
        result.PowerConsumption = 5.7; // 211 MWh/day converted to MW
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsQualityDependent = false;
        result.CanUseVehicles = false;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.BulkHandling;
        result.Workdays = 608;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 9.5 },
            {GravelResource, 7.3 },
            {AsphaltResource, 5.8 },
            {BricksResource, 28 },
            {BoardsResource, 9.7 },
            {SteelResource, 9.3 },
            {MechanicComponentsResource, 1.2 }
        };
        return result;
    }
    public static ProductionBuilding TrainAggregateLoading98m { get; } = CreateTrainAggregateLoading98m();
    public static ProductionBuilding CreateTrainAggregateLoading98m()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Train Aggregate Loading (98m)";
        result.Inputs = new List<ResourceAmount>();  // No inputs - it's a loading station
        result.Outputs = new List<ResourceAmount>();  // No outputs - it's infrastructure
        result.WorkersPerShift = 0;  // Automated
        result.PowerConsumption = 74; 
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsQualityDependent = false;
        result.CanUseVehicles = false;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.BulkHandling;
        result.Workdays = 1120;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 16 },
            {GravelResource, 12 },
            {AsphaltResource, 9.9 },
            {BricksResource, 58 },
            {BoardsResource, 19 },
            {SteelResource, 16 },
            {MechanicComponentsResource, 1.8 }
        };
        return result;
    }
    public static ProductionBuilding TruckAggregateLoadingSmall { get; } = CreateTruckAggregateLoadingSmall();
    public static ProductionBuilding CreateTruckAggregateLoadingSmall()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Truck Aggregate Loading (Small)";
        result.Inputs = new List<ResourceAmount>();
        result.Outputs = new List<ResourceAmount>();
        result.WorkersPerShift = 0;  // Automated
        result.PowerConsumption = 23.0; // 23 MWh/day → MW
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsQualityDependent = false;
        result.CanUseVehicles = false;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.BulkHandling;
        result.Workdays = 281;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 3.3 },
            {GravelResource, 2.5 },
            {AsphaltResource, 2.0 },
            {BricksResource, 19 },
            {BoardsResource, 6.4 },
            {SteelResource, 3.0 },
            {MechanicComponentsResource, 0.13 }
        };
        return result;
    }
    public static ProductionBuilding TruckAggregateLoadingBig { get; } = CreateTruckAggregateLoadingBig();
    public static ProductionBuilding CreateTruckAggregateLoadingBig()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Truck Aggregate Loading (Big)";
        result.Inputs = new List<ResourceAmount>();
        result.Outputs = new List<ResourceAmount>();
        result.WorkersPerShift = 0;  // Automated
        result.PowerConsumption = 23.0; // 23 MWh/day → MW
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsQualityDependent = false;
        result.CanUseVehicles = false;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.BulkHandling;
        result.Workdays = 92;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 1.5 },
            {GravelResource, 1.1 },
            {AsphaltResource, 0.92 },
            {SteelResource, 6.2 },
            {MechanicComponentsResource, 0.074 }
        };
        return result;
    }

    // Support Buildings - Open Storage Buildings
    public static ProductionBuilding OpenStorageSmall250 { get; } = CreateOpenStorageSmall250();
    public static ProductionBuilding CreateOpenStorageSmall250()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Open Storage (Small 250t)";
        result.Inputs = new List<ResourceAmount>();
        result.Outputs = new List<ResourceAmount>();
        result.WorkersPerShift = 0;
        result.PowerConsumption = 0; // No power listed in screenshot
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsQualityDependent = false;
        result.CanUseVehicles = false;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.SolidHandling;
        result.Workdays = 166;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 14 },
            {GravelResource, 11 },
            {AsphaltResource, 8.9 }
        };
        return result;
    }
    public static ProductionBuilding OpenStorageSmall330 { get; } = CreateOpenStorageSmall330();
    public static ProductionBuilding CreateOpenStorageSmall330()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Open Storage (Small 330t)";
        result.Inputs = new List<ResourceAmount>();
        result.Outputs = new List<ResourceAmount>();
        result.WorkersPerShift = 0;
        result.PowerConsumption = 20.0; // 20 MWh/day from screenshot
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsQualityDependent = false;
        result.CanUseVehicles = false;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.SolidHandling;
        result.Workdays = 460;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 14 },
            {GravelResource, 10 },
            {AsphaltResource, 8.7 },
            {SteelResource, 10 },
            {MechanicComponentsResource, 2.2 }
        };
        return result;
    }
    public static ProductionBuilding OpenStorageMedium { get; } = CreateOpenStorageMedium();
    public static ProductionBuilding CreateOpenStorageMedium()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Open Storage (Medium 1170t)";
        result.Inputs = new List<ResourceAmount>();
        result.Outputs = new List<ResourceAmount>();
        result.WorkersPerShift = 0;
        result.PowerConsumption = 26.0; // 26 MWh/day from screenshot
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsQualityDependent = false;
        result.CanUseVehicles = false;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.SolidHandling;
        result.Workdays = 563;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 14 },
            {GravelResource, 10 },
            {AsphaltResource, 8.7 },
            {SteelResource, 14 },
            {MechanicComponentsResource, 2.9 }
        };
        return result;
    }

    // Support Buildings - Utility
    public static ProductionBuilding WaterLoadingUnloadingStation { get; } = CreateWaterLoadingUnloadingStation();
    public static ProductionBuilding CreateWaterLoadingUnloadingStation()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Water LoadingUnloading Station";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>() { };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 7.5;  // MWh/day
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsUtilityBuilding = false;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.WaterHandling;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 8.2 },
            {GravelResource, 6.3 },
            {BricksResource, 8.8 },
            {BoardsResource, 2.9 },
            {SteelResource, 4.7 },
            {MechanicComponentsResource, 0.76 }
        };
        return result;
    }
    public static ProductionBuilding BigWaterPumpingStation { get; } = CreateBigWaterPumpingStation();
    public static ProductionBuilding CreateBigWaterPumpingStation()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Big Water Pumping Station";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>() { };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 74;  // MWh/day
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsUtilityBuilding = false;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.WaterHandling;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 1.7 },
            {GravelResource, 1.3 },
            {AsphaltResource, 1.1 },
            {SteelResource, 1.1 },
            {BricksResource, 2.5 },
            {BoardsResource, 0.83 },
            {MechanicComponentsResource, 0.17 }
        };
        return result;
    }
    public static ProductionBuilding SmallWaterPumpingStation { get; } = CreateSmallWaterPumpingStation();
    public static ProductionBuilding CreateSmallWaterPumpingStation()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Small Water Pumping Station";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>() { };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 39;  // MWh/day
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsUtilityBuilding = false;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.WaterHandling;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 0.66 },
            {GravelResource, 0.50 },
            {AsphaltResource, 0.40 },
            {SteelResource, 0.65 },
            {BricksResource, 1.8 },
            {BoardsResource, 0.59 },
            {MechanicComponentsResource, 0.090 }
        };
        return result;
    }
    public static ProductionBuilding SewageLoadingUnloadingStation { get; } = CreateSewageLoadingUnloadingStation();
    public static ProductionBuilding CreateSewageLoadingUnloadingStation()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Sewage Loading/Unloading Station";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>() { };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 7.5;
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.SewageHandling;
        result.Workdays = 373;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
    {
        {ConcreteResource, 8.6 },
        {GravelResource, 6.6 },
        {BricksResource, 10 },
        {BoardsResource, 3.4 },
        {SteelResource, 6.7 },
        {MechanicComponentsResource, 1.1 }
    };
        return result;
    }
    public static ProductionBuilding SewagePump5m { get; } = CreateSewagePump5m();
    public static ProductionBuilding CreateSewagePump5m()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Sewage Pump (5m)";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>() { };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 6.0;
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.SewageHandling;
        result.Workdays = 23;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
    {
        {ConcreteResource, 0.16 },
        {GravelResource, 0.12 },
        {PrefabPanelsResource, 1.9 },
        {SteelResource, 0.52 },
        {MechanicComponentsResource, 0.068 }
    };
        return result;
    }
    public static ProductionBuilding SewagePump10m { get; } = CreateSewagePump10m();
    public static ProductionBuilding CreateSewagePump10m()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Sewage Pump (10m)";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>() { };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 9.0;
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.SewageHandling;
        result.Workdays = 35;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
    {
        {ConcreteResource, 0.16 },
        {GravelResource, 0.12 },
        {PrefabPanelsResource, 2.9 },
        {SteelResource, 0.79 },
        {MechanicComponentsResource, 0.10 }
    };
        return result;
    }
    public static ProductionBuilding SewagePump15m { get; } = CreateSewagePump15m();
    public static ProductionBuilding CreateSewagePump15m()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Sewage Pump (15m)";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>() { };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 11;
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.SewageHandling;
        result.Workdays = 52;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
    {
        {ConcreteResource, 0.17 },
        {GravelResource, 0.13 },
        {PrefabPanelsResource, 4.5 },
        {SteelResource, 1.2 },
        {MechanicComponentsResource, 0.16 }
    };
        return result;
    }
    public static ProductionBuilding SewageTank { get; } = CreateSewageTank();
    public static ProductionBuilding CreateSewageTank()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Sewage Tank";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>() { };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 0;
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.SewageHandling;
        result.Workdays = 24;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
    {
        {ConcreteResource, 1.1 },
        {GravelResource, 0.82 },
        {AsphaltResource, 0.66 },
        {SteelResource, 0.30 },
        {BricksResource, 0.48 },
        {BoardsResource, 0.16 },
        {MechanicComponentsResource, 0.050 }
    };
        return result;
    }
    public static ProductionBuilding SewageDischarge { get; } = CreateSewageDischarge();
    public static ProductionBuilding CreateSewageDischarge()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Sewage Discharge";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>() { };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 0;
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.SupportCategory = SupportCategory.SewageHandling;
        result.Workdays = 284;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
    {
        {ConcreteResource, 4.8 },
        {GravelResource, 3.7 },
        {AsphaltResource, 3.0 },
        {SteelResource, 4.2 },
        {BricksResource, 13 },
        {BoardsResource, 4.5 },
        {MechanicComponentsResource, 0.54 }
    };
        return result;
    }
    public static ProductionBuilding HeatExchanger { get; } = CreateHeatExchanger();
    public static ProductionBuilding CreateHeatExchanger()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Heat Exchanger";
        result.Inputs = new List<ResourceAmount>();  // No fuel input
        result.Outputs = new List<ResourceAmount>()
        {
        };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 18;
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsUtilityBuilding = true;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.HeatHandling;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 10 },
            {GravelResource, 8.2 },
            {AsphaltResource, 6.6 },
            {SteelResource, 12 },
            {BricksResource, 34 },
            {BoardsResource, 11 },
            {MechanicComponentsResource, 1.7 }
        };
        return result;
    }
    public static ProductionBuilding SmallHeatExchanger { get; } = CreateSmallHeatExchanger();
    public static ProductionBuilding CreateSmallHeatExchanger()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Small Heat Exchanger";
        result.Inputs = new List<ResourceAmount>();
        result.Outputs = new List<ResourceAmount>()
        {
        };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 6.0;
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsUtilityBuilding = true;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.HeatHandling;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 6.9 },
            {GravelResource, 5.3 },
            {AsphaltResource, 4.2 },
            {SteelResource, 4.7 },
            {BricksResource, 15 },
            {BoardsResource, 5.1 },
            {MechanicComponentsResource, 0.59 }
        };
        return result;
    }
    public static ProductionBuilding SmallHeatPumpingStation { get; } = CreateSmallHeatPumpingStation();
    public static ProductionBuilding CreateSmallHeatPumpingStation()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Small Heat-Pumping Station";
        result.Inputs = new List<ResourceAmount>();
        result.Outputs = new List<ResourceAmount>()
        {
        };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 11;
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsUtilityBuilding = true;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.HeatHandling;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 1.7 },
            {GravelResource, 1.3 },
            {AsphaltResource, 1.0 },
            {SteelResource, 1.5 },
            {BricksResource, 3.4 },
            {BoardsResource, 1.1 },
            {MechanicComponentsResource, 0.22 }
        };
        return result;
    }
    public static ProductionBuilding HeatPumpingStation { get; } = CreateHeatPumpingStation();
    public static ProductionBuilding CreateHeatPumpingStation()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Heat-Pumping Station";
        result.Inputs = new List<ResourceAmount>();
        result.Outputs = new List<ResourceAmount>()
        {
        };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 24;
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsUtilityBuilding = true;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.HeatHandling;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 3.0 },
            {GravelResource, 2.3 },
            {AsphaltResource, 1.8 },
            {SteelResource, 2.2 },
            {BricksResource, 4.6 },
            {BoardsResource, 1.5 },
            {MechanicComponentsResource, 0.33 }
        };
        return result;
    }

    // Distribution Office (consumes power + fuel)
    public static ProductionBuilding SmallDistributionOffice { get; } = CreateSmallDistributionOffice();
    public static ProductionBuilding CreateSmallDistributionOffice()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Small Distribution Office";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>() { };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 3.0;  // MWh/day
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsUtilityBuilding = false;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.GeneralDistribution;
        result.Workdays = 721;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
    {
        {ConcreteResource, 18 },
        {GravelResource, 14 },
        {AsphaltResource, 11 },
        {BricksResource, 32 },
        {BoardsResource, 10 },
        {SteelResource, 13 }
    };
        return result;
    }

    // Warehouse
    public static ProductionBuilding Warehouse { get; } = CreateWarehouse();
    public static ProductionBuilding CreateWarehouse()
    {
        ProductionBuilding result = new ProductionBuilding();
        result.Name = "Warehouse";
        result.Inputs = new List<ResourceAmount>() { };
        result.Outputs = new List<ResourceAmount>() { };
        result.WorkersPerShift = 0;
        result.PowerConsumption = 3.0;  // MWh/day
        result.WaterConsumption = 0;
        result.HeatConsumption = 0;
        result.SewageProduction = 0;
        result.GarbagePerWorker = 0;
        result.EnvironmentPollution = 0;
        result.IsSeasonDependent = false;
        result.SeasonalMultiplier = 0;
        result.IsUtilityBuilding = false;
        result.IsSupportBuildings = true;
        result.SupportCategory = SupportCategory.GeneralDistribution;
        result.Workdays = 755;
        result.ConstructionMaterials = new Dictionary<Resource, double>()
        {
            {ConcreteResource, 23 },
            {GravelResource, 3.8 },
            {AsphaltResource, 3.0 },
            {SteelResource, 9.4 },
            {BricksResource, 44 },
            {BoardsResource, 28 }
        };
        return result;
    }

    // Small Residential Buildings
    public static List<ResidentialBuilding> SmallResidentialBuildings = new List<ResidentialBuilding>
    {
        new ResidentialBuilding {
            Name = "Flats - brick (48w, 2.16m³/d, 80%)",
            WorkerCapacity = 48,
            PowerMW = 3.0,
            WaterPerDay = 2.16,
            HeatTankM3 = 0,
            Quality = 80,
            Workdays = 500,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 9.8},
                {GravelResource, 7.6},
                {BricksResource, 33},
                {BoardsResource, 11},
                {SteelResource, 4.1},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (36w, 1.62m³/d, 85%)",
            WorkerCapacity = 36,
            PowerMW = 3.0,
            WaterPerDay = 1.62,
            HeatTankM3 = 0,
            Quality = 85,
            Workdays = 535,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 11},
                {GravelResource, 8.7},
                {BricksResource, 34},
                {BoardsResource, 11},
                {SteelResource, 4.3},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (10w, 0.45m³/d, 85%)",
            WorkerCapacity = 10,
            PowerMW = 3.0,
            WaterPerDay = 0.45,
            HeatTankM3 = 0,
            Quality = 85,
            Workdays = 158,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 3.2},
                {GravelResource, 2.5},
                {BricksResource, 10},
                {BoardsResource, 3.5},
                {SteelResource, 1.3},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (18w, 0.81m³/d, 80%)",
            WorkerCapacity = 18,
            PowerMW = 3.0,
            WaterPerDay = 0.81,
            HeatTankM3 = 0,
            Quality = 80,
            Workdays = 235,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 5.0},
                {GravelResource, 3.9},
                {BricksResource, 15},
                {BoardsResource, 5.1},
                {SteelResource, 1.9},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (20w, 0.90m³/d, 85%)",
            WorkerCapacity = 20,
            PowerMW = 3.0,
            WaterPerDay = 0.90,
            HeatTankM3 = 0,
            Quality = 85,
            Workdays = 305,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 7.1},
                {GravelResource, 5.5},
                {BricksResource, 19},
                {BoardsResource, 6.4},
                {SteelResource, 2.4},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (48w, 2.16m³/d, 80%)",
            WorkerCapacity = 48,
            PowerMW = 3.0,
            WaterPerDay = 2.16,
            HeatTankM3 = 0,
            Quality = 80,
            Workdays = 478,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 7.9},
                {GravelResource, 6.1},
                {BricksResource, 33},
                {BoardsResource, 11},
                {SteelResource, 4.1},
            }
        },
        new ResidentialBuilding {
            Name = "Low-quality rural flats (20w, 0.90m³/d, 60%)",
            WorkerCapacity = 20,
            PowerMW = 3.0,
            WaterPerDay = 0.90,
            HeatTankM3 = 0,
            Quality = 60,
            Workdays = 106,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 2.6},
                {GravelResource, 2.0},
                {BricksResource, 6.6},
                {BoardsResource, 2.2},
                {SteelResource, 0.82},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (32w, 1.44m³/d, 80%)",
            WorkerCapacity = 32,
            PowerMW = 3.0,
            WaterPerDay = 1.44,
            HeatTankM3 = 0,
            Quality = 80,
            Workdays = 324,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 6.9},
                {GravelResource, 5.3},
                {BricksResource, 21},
                {BoardsResource, 7.0},
                {SteelResource, 2.6},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (40w, 1.80m³/d, 85%)",
            WorkerCapacity = 40,
            PowerMW = 3.0,
            WaterPerDay = 1.80,
            HeatTankM3 = 0,
            Quality = 85,
            Workdays = 341,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 11},
                {GravelResource, 9.0},
                {AsphaltResource, 7.2},
                {PrefabPanelsResource, 31},
                {SteelResource, 3.2},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (1Lg-600A) (29w, 1.31m³/d, 80%)",
            WorkerCapacity = 29,
            PowerMW = 3.0,
            WaterPerDay = 1.31,
            HeatTankM3 = 0,
            Quality = 80,
            Workdays = 246,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 42},
                {GravelResource, 4.0},
                {BricksResource, 0.040},
                {BoardsResource, 5.2},
                {PrefabPanelsResource, 12},
                {SteelResource, 0.40},
            }
        },
        new ResidentialBuilding {
            Name = "Low-quality rural flats (30w, 1.35m³/d, 63%)",
            WorkerCapacity = 30,
            PowerMW = 3.0,
            WaterPerDay = 1.35,
            HeatTankM3 = 0,
            Quality = 63,
            Workdays = 227,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 7.2},
                {GravelResource, 5.6},
                {AsphaltResource, 4.5},
                {BricksResource, 11},
                {BoardsResource, 4.6},
                {SteelResource, 1.9},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (42w, 1.89m³/d, 83%)",
            WorkerCapacity = 42,
            PowerMW = 3.0,
            WaterPerDay = 1.89,
            HeatTankM3 = 0,
            Quality = 83,
            Workdays = 364,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 11},
                {GravelResource, 8.8},
                {AsphaltResource, 7.0},
                {PrefabPanelsResource, 29},
                {SteelResource, 6.5},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (48w, 2.16m³/d, 82%)",
            WorkerCapacity = 48,
            PowerMW = 3.0,
            WaterPerDay = 2.16,
            HeatTankM3 = 0,
            Quality = 82,
            Workdays = 320,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 9.2},
                {GravelResource, 7.1},
                {AsphaltResource, 5.7},
                {PrefabPanelsResource, 25},
                {SteelResource, 7.0},
            }
        },
        new ResidentialBuilding {
            Name = "Low-quality rural flats (20w, 0.90m³/d, 55%)",
            WorkerCapacity = 20,
            PowerMW = 3.0,
            WaterPerDay = 0.90,
            HeatTankM3 = 0,
            Quality = 55,
            Workdays = 129,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 5.4},
                {GravelResource, 4.1},
                {AsphaltResource, 3.3},
                {BricksResource, 5.8},
                {BoardsResource, 1.9},
                {SteelResource, 0.72},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (1Lg-600A) (43w, 1.94m³/d, 80%)",
            WorkerCapacity = 43,
            PowerMW = 3.0,
            WaterPerDay = 1.94,
            HeatTankM3 = 0,
            Quality = 80,
            Workdays = 424,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 54},
                {GravelResource, 5.0},
                {BricksResource, 0.10},
                {BoardsResource, 11},
                {PrefabPanelsResource, 25},
                {SteelResource, 1.0},
            }
        },
        new ResidentialBuilding {
            Name = "Low-quality rural flats (20w, 0.90m³/d, 55%)",
            WorkerCapacity = 20,
            PowerMW = 3.0,
            WaterPerDay = 0.90,
            HeatTankM3 = 0,
            Quality = 55,
            Workdays = 137,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 5.5},
                {GravelResource, 4.2},
                {AsphaltResource, 3.4},
                {BricksResource, 6.3},
                {BoardsResource, 2.1},
                {SteelResource, 0.79},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (type 75) (29w, 1.31m³/d, 84%)",
            WorkerCapacity = 29,
            PowerMW = 3.0,
            WaterPerDay = 1.31,
            HeatTankM3 = 0,
            Quality = 84,
            Workdays = 262,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 2.0},
                {GravelResource, 1.5},
                {AsphaltResource, 1.2},
                {BricksResource, 0.23},
                {BoardsResource, 11},
                {PrefabPanelsResource, 20},
                {SteelResource, 2.1},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (type 75) (48w, 2.16m³/d, 84%)",
            WorkerCapacity = 48,
            PowerMW = 3.0,
            WaterPerDay = 2.16,
            HeatTankM3 = 0,
            Quality = 84,
            Workdays = 376,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 2.7},
                {GravelResource, 2.1},
                {AsphaltResource, 1.7},
                {BricksResource, 0.31},
                {BoardsResource, 18},
                {PrefabPanelsResource, 27},
                {SteelResource, 2.8},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (43w, 1.94m³/d, 83%)",
            WorkerCapacity = 43,
            PowerMW = 3.0,
            WaterPerDay = 1.94,
            HeatTankM3 = 0,
            Quality = 83,
            Workdays = 374,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 10},
                {GravelResource, 7.8},
                {AsphaltResource, 6.2},
                {PrefabPanelsResource, 32},
                {SteelResource, 7.2},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (42w, 1.89m³/d, 83%)",
            WorkerCapacity = 42,
            PowerMW = 3.0,
            WaterPerDay = 1.89,
            HeatTankM3 = 0,
            Quality = 83,
            Workdays = 404,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 13},
                {GravelResource, 10},
                {AsphaltResource, 8.4},
                {PrefabPanelsResource, 31},
                {SteelResource, 6.9},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (type 75) (40w, 1.80m³/d, 84%)",
            WorkerCapacity = 40,
            PowerMW = 3.0,
            WaterPerDay = 1.80,
            HeatTankM3 = 0,
            Quality = 84,
            Workdays = 336,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 2.7},
                {GravelResource, 2.1},
                {AsphaltResource, 1.6},
                {BricksResource, 0.31},
                {BoardsResource, 16},
                {PrefabPanelsResource, 24},
                {SteelResource, 2.5},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (type 75) (30w, 1.35m³/d, 84%)",
            WorkerCapacity = 30,
            PowerMW = 3.0,
            WaterPerDay = 1.35,
            HeatTankM3 = 0,
            Quality = 84,
            Workdays = 266,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 2.0},
                {GravelResource, 1.5},
                {AsphaltResource, 1.2},
                {BricksResource, 0.23},
                {BoardsResource, 12},
                {PrefabPanelsResource, 20},
                {SteelResource, 2.1},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (20w, 0.90m³/d, 85%)",
            WorkerCapacity = 20,
            PowerMW = 3.0,
            WaterPerDay = 0.90,
            HeatTankM3 = 0,
            Quality = 85,
            Workdays = 322,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 8.2},
                {GravelResource, 6.3},
                {AsphaltResource, 5.0},
                {BricksResource, 15},
                {BoardsResource, 8.1},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (type 75) (40w, 1.80m³/d, 84%)",
            WorkerCapacity = 40,
            PowerMW = 3.0,
            WaterPerDay = 1.80,
            HeatTankM3 = 0,
            Quality = 84,
            Workdays = 413,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 4.3},
                {GravelResource, 3.3},
                {AsphaltResource, 2.6},
                {BricksResource, 0.42},
                {BoardsResource, 18},
                {PrefabPanelsResource, 30},
                {SteelResource, 3.0},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (10w, 0.45m³/d, 89%)",
            WorkerCapacity = 10,
            PowerMW = 3.0,
            WaterPerDay = 0.45,
            HeatTankM3 = 0,
            Quality = 89,
            Workdays = 191,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 3.6},
                {GravelResource, 2.8},
                {AsphaltResource, 2.2},
                {BricksResource, 10},
                {BoardsResource, 4.9},
                {SteelResource, 2.1},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (27w, 1.22m³/d, 91%)",
            WorkerCapacity = 27,
            PowerMW = 3.0,
            WaterPerDay = 1.22,
            HeatTankM3 = 0,
            Quality = 91,
            Workdays = 271,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 9.4},
                {GravelResource, 7.2},
                {AsphaltResource, 5.8},
                {PrefabPanelsResource, 25},
                {SteelResource, 2.5},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (30w, 1.35m³/d, 77%)",
            WorkerCapacity = 30,
            PowerMW = 3.0,
            WaterPerDay = 1.35,
            HeatTankM3 = 0,
            Quality = 77,
            Workdays = 302,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 7.7},
                {GravelResource, 5.9},
                {AsphaltResource, 4.7},
                {BricksResource, 14},
                {BoardsResource, 7.6},
                {SteelResource, 3.5},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (30w, 1.35m³/d, 80%)",
            WorkerCapacity = 30,
            PowerMW = 3.0,
            WaterPerDay = 1.35,
            HeatTankM3 = 0,
            Quality = 80,
            Workdays = 326,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 5.8},
                {GravelResource, 4.5},
                {AsphaltResource, 3.6},
                {BricksResource, 19},
                {BoardsResource, 8.5},
                {SteelResource, 3.7},
            }
        },
    };

    // Medium Residential Buildings
    public static List<ResidentialBuilding> MediumResidentialBuildings = new List<ResidentialBuilding>
    {
        new ResidentialBuilding {
            Name = "Flats - brick (90w, 4.05m³/d, 80%)",
            WorkerCapacity = 90,
            PowerMW = 5.4,
            WaterPerDay = 4.05,
            HeatTankM3 = 0,
            Quality = 80,
            Workdays = 845,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 16},
                {GravelResource, 12},
                {BricksResource, 56},
                {BoardsResource, 18},
                {SteelResource, 7.1},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (60w, 2.70m³/d, 80%)",
            WorkerCapacity = 60,
            PowerMW = 3.6,
            WaterPerDay = 2.70,
            HeatTankM3 = 0,
            Quality = 80,
            Workdays = 613,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 11},
                {GravelResource, 8.5},
                {BricksResource, 41},
                {BoardsResource, 13},
                {SteelResource, 5.2},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (115w, 5.18m³/d, 80%) [large-dup]",
            WorkerCapacity = 115,
            PowerMW = 6.9,
            WaterPerDay = 5.18,
            HeatTankM3 = 0,
            Quality = 80,
            Workdays = 1069,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 22},
                {GravelResource, 17},
                {BricksResource, 68},
                {BoardsResource, 22},
                {SteelResource, 8.8},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (56w, 2.52m³/d, 85%)",
            WorkerCapacity = 56,
            PowerMW = 3.4,
            WaterPerDay = 2.52,
            HeatTankM3 = 0,
            Quality = 85,
            Workdays = 769,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 14},
                {GravelResource, 11},
                {BricksResource, 51},
                {BoardsResource, 17},
                {SteelResource, 6.4},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (65w, 2.93m³/d, 80%)",
            WorkerCapacity = 65,
            PowerMW = 3.9,
            WaterPerDay = 2.93,
            HeatTankM3 = 0,
            Quality = 80,
            Workdays = 714,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 16},
                {GravelResource, 12},
                {BricksResource, 45},
                {BoardsResource, 15},
                {SteelResource, 5.7},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (72w, 3.24m³/d, 80%)",
            WorkerCapacity = 72,
            PowerMW = 4.3,
            WaterPerDay = 3.24,
            HeatTankM3 = 0,
            Quality = 80,
            Workdays = 695,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 14},
                {GravelResource, 11},
                {BricksResource, 45},
                {BoardsResource, 15},
                {SteelResource, 5.6},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (50w, 2.25m³/d, 85%)",
            WorkerCapacity = 50,
            PowerMW = 3.0,
            WaterPerDay = 2.25,
            HeatTankM3 = 0,
            Quality = 85,
            Workdays = 673,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 13},
                {GravelResource, 10},
                {AsphaltResource, 8.1},
                {BricksResource, 44},
                {BoardsResource, 14},
                {SteelResource, 5.6},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (50w, 2.25m³/d, 85%) [dup]",
            WorkerCapacity = 50,
            PowerMW = 3.0,
            WaterPerDay = 2.25,
            HeatTankM3 = 0,
            Quality = 85,
            Workdays = 713,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 17},
                {GravelResource, 13},
                {BricksResource, 44},
                {BoardsResource, 14},
                {SteelResource, 5.5},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (65w, 2.93m³/d, 80%) [dup]",
            WorkerCapacity = 65,
            PowerMW = 3.9,
            WaterPerDay = 2.93,
            HeatTankM3 = 0,
            Quality = 80,
            Workdays = 728,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 17},
                {GravelResource, 13},
                {BricksResource, 45},
                {BoardsResource, 15},
                {SteelResource, 5.7},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (60w, 2.70m³/d, 80%) [dup]",
            WorkerCapacity = 60,
            PowerMW = 3.6,
            WaterPerDay = 2.70,
            HeatTankM3 = 0,
            Quality = 80,
            Workdays = 691,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 15},
                {GravelResource, 11},
                {BricksResource, 44},
                {BoardsResource, 14},
                {SteelResource, 5.5},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (118w, 5.31m³/d, 70%)",
            WorkerCapacity = 118,
            PowerMW = 7.1,
            WaterPerDay = 5.31,
            HeatTankM3 = 0,
            Quality = 70,
            Workdays = 635,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 13},
                {GravelResource, 10},
                {AsphaltResource, 8.4},
                {BricksResource, 28},
                {BoardsResource, 14},
                {PrefabPanelsResource, 75},
                {SteelResource, 7.5},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (55w, 2.48m³/d, 80%) [dup]",
            WorkerCapacity = 55,
            PowerMW = 3.3,
            WaterPerDay = 2.48,
            HeatTankM3 = 0,
            Quality = 80,
            Workdays = 569,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 16},
                {GravelResource, 12},
                {BricksResource, 32},
                {BoardsResource, 10},
                {SteelResource, 4.1},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (90w, 4.05m³/d, 80%) [dup]",
            WorkerCapacity = 90,
            PowerMW = 5.4,
            WaterPerDay = 4.05,
            HeatTankM3 = 0,
            Quality = 80,
            Workdays = 942,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 22},
                {GravelResource, 17},
                {BricksResource, 58},
                {BoardsResource, 19},
                {SteelResource, 7.4},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (80w, 3.60m³/d, 85%)",
            WorkerCapacity = 80,
            PowerMW = 4.8,
            WaterPerDay = 3.60,
            HeatTankM3 = 0,
            Quality = 85,
            Workdays = 862,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 16},
                {GravelResource, 12},
                {BricksResource, 57},
                {BoardsResource, 19},
                {SteelResource, 7.2},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (75w, 3.38m³/d, 80%)",
            WorkerCapacity = 75,
            PowerMW = 4.5,
            WaterPerDay = 3.38,
            HeatTankM3 = 0,
            Quality = 80,
            Workdays = 862,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 18},
                {GravelResource, 14},
                {BricksResource, 55},
                {BoardsResource, 18},
                {SteelResource, 7.0},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (62w, 2.79m³/d, 86%)",
            WorkerCapacity = 62,
            PowerMW = 3.7,
            WaterPerDay = 2.79,
            HeatTankM3 = 0,
            Quality = 86,
            Workdays = 469,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 14},
                {GravelResource, 10},
                {AsphaltResource, 8.8},
                {PrefabPanelsResource, 39},
                {SteelResource, 8.0},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (type 75) (63w, 2.84m³/d, 86%)",
            WorkerCapacity = 63,
            PowerMW = 3.8,
            WaterPerDay = 2.84,
            HeatTankM3 = 0,
            Quality = 86,
            Workdays = 535,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 3.3},
                {GravelResource, 2.5},
                {AsphaltResource, 2.0},
                {BricksResource, 0.37},
                {BoardsResource, 24},
                {PrefabPanelsResource, 39},
                {SteelResource, 4.6},
                {MechanicComponentsResource, 0.14},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (68w, 3.06m³/d, 85%)",
            WorkerCapacity = 68,
            PowerMW = 4.1,
            WaterPerDay = 3.06,
            HeatTankM3 = 0,
            Quality = 85,
            Workdays = 569,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 16},
                {GravelResource, 12},
                {AsphaltResource, 9.9},
                {PrefabPanelsResource, 58},
                {SteelResource, 5.9},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (72w, 3.24m³/d, 75%)",
            WorkerCapacity = 72,
            PowerMW = 4.3,
            WaterPerDay = 3.24,
            HeatTankM3 = 0,
            Quality = 75,
            Workdays = 515,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 14},
                {GravelResource, 11},
                {AsphaltResource, 9.2},
                {PrefabPanelsResource, 52},
                {SteelResource, 5.3},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (82w, 3.69m³/d, 87%)",
            WorkerCapacity = 82,
            PowerMW = 4.9,
            WaterPerDay = 3.69,
            HeatTankM3 = 0,
            Quality = 87,
            Workdays = 642,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 21},
                {GravelResource, 16},
                {AsphaltResource, 13},
                {SteelResource, 12},
                {PrefabPanelsResource, 47},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (80w, 3.60m³/d, 87%)",
            WorkerCapacity = 80,
            PowerMW = 4.8,
            WaterPerDay = 3.60,
            HeatTankM3 = 0,
            Quality = 87,
            Workdays = 617,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 21},
                {GravelResource, 16},
                {AsphaltResource, 13},
                {SteelResource, 10},
                {PrefabPanelsResource, 47},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (1Lg-600A) (85w, 3.83m³/d, 80%)",
            WorkerCapacity = 85,
            PowerMW = 5.1,
            WaterPerDay = 3.83,
            HeatTankM3 = 0,
            Quality = 80,
            Workdays = 603,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 77},
                {GravelResource, 7.1},
                {BricksResource, 0.10},
                {BoardsResource, 15},
                {PrefabPanelsResource, 35},
                {SteelResource, 1.4},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (65w, 2.93m³/d, 87%)",
            WorkerCapacity = 65,
            PowerMW = 3.9,
            WaterPerDay = 2.93,
            HeatTankM3 = 0,
            Quality = 87,
            Workdays = 529,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 17},
                {GravelResource, 13},
                {AsphaltResource, 10},
                {SteelResource, 10},
                {PrefabPanelsResource, 39},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (1Lg-600A) (58w, 2.61m³/d, 80%)",
            WorkerCapacity = 58,
            PowerMW = 3.5,
            WaterPerDay = 2.61,
            HeatTankM3 = 0,
            Quality = 80,
            Workdays = 361,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 62},
                {GravelResource, 5.8},
                {BricksResource, 0.068},
                {BoardsResource, 7.8},
                {PrefabPanelsResource, 17},
                {SteelResource, 0.58},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (type 75) (108w, 4.86m³/d, 84%)",
            WorkerCapacity = 108,
            PowerMW = 6.5,
            WaterPerDay = 4.86,
            HeatTankM3 = 0,
            Quality = 84,
            Workdays = 909,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 6.8},
                {GravelResource, 5.2},
                {AsphaltResource, 4.2},
                {BricksResource, 0.77},
                {BoardsResource, 43},
                {PrefabPanelsResource, 61},
                {SteelResource, 7.6},
                {MechanicComponentsResource, 0.29},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (type 75) (81w, 3.65m³/d, 84%)",
            WorkerCapacity = 81,
            PowerMW = 4.9,
            WaterPerDay = 3.65,
            HeatTankM3 = 0,
            Quality = 84,
            Workdays = 676,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 6.9},
                {GravelResource, 5.3},
                {AsphaltResource, 4.2},
                {BricksResource, 0.80},
                {BoardsResource, 31},
                {PrefabPanelsResource, 48},
                {SteelResource, 4.8},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (80w, 3.60m³/d, 70%)",
            WorkerCapacity = 80,
            PowerMW = 4.8,
            WaterPerDay = 3.60,
            HeatTankM3 = 0,
            Quality = 70,
            Workdays = 464,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 9.7},
                {GravelResource, 7.5},
                {AsphaltResource, 6.0},
                {BricksResource, 20},
                {BoardsResource, 10},
                {PrefabPanelsResource, 74},
                {SteelResource, 5.5},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (55w, 2.48m³/d, 70%)",
            WorkerCapacity = 55,
            PowerMW = 3.3,
            WaterPerDay = 2.48,
            HeatTankM3 = 0,
            Quality = 70,
            Workdays = 329,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 6.5},
                {GravelResource, 5.0},
                {AsphaltResource, 4.0},
                {BricksResource, 15},
                {BoardsResource, 7.5},
                {PrefabPanelsResource, 54},
                {SteelResource, 3.9},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (1Lg-600A) (74w, 3.33m³/d, 80%)",
            WorkerCapacity = 74,
            PowerMW = 4.4,
            WaterPerDay = 3.33,
            HeatTankM3 = 0,
            Quality = 80,
            Workdays = 483,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 42},
                {GravelResource, 3.8},
                {BricksResource, 0.13},
                {BoardsResource, 14},
                {PrefabPanelsResource, 33},
                {SteelResource, 1.4},
                {ElectroComponentsResource, 0.038},
                {MechanicComponentsResource, 0.063},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (type 75) (67w, 3.02m³/d, 84%)",
            WorkerCapacity = 67,
            PowerMW = 4.0,
            WaterPerDay = 3.02,
            HeatTankM3 = 0,
            Quality = 84,
            Workdays = 557,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 3.5},
                {GravelResource, 2.7},
                {AsphaltResource, 2.1},
                {BricksResource, 0.63},
                {BoardsResource, 26},
                {PrefabPanelsResource, 39},
                {SteelResource, 4.7},
                {MechanicComponentsResource, 0.15},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (1Lg-600A) (85w, 3.83m³/d, 80%) [dup]",
            WorkerCapacity = 85,
            PowerMW = 5.1,
            WaterPerDay = 3.83,
            HeatTankM3 = 0,
            Quality = 80,
            Workdays = 556,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 49},
                {GravelResource, 4.2},
                {BricksResource, 0.13},
                {BoardsResource, 16},
                {PrefabPanelsResource, 37},
                {SteelResource, 1.6},
                {ElectroComponentsResource, 0.042},
                {MechanicComponentsResource, 0.063},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (type 75) (74w, 3.33m³/d, 84%)",
            WorkerCapacity = 74,
            PowerMW = 4.4,
            WaterPerDay = 3.33,
            HeatTankM3 = 0,
            Quality = 84,
            Workdays = 589,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 5.8},
                {GravelResource, 4.4},
                {AsphaltResource, 3.6},
                {BricksResource, 0.67},
                {BoardsResource, 21},
                {PrefabPanelsResource, 42},
                {SteelResource, 4.2},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (50w, 2.25m³/d, 70%)",
            WorkerCapacity = 50,
            PowerMW = 3.0,
            WaterPerDay = 2.25,
            HeatTankM3 = 0,
            Quality = 70,
            Workdays = 432,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 8.0},
                {GravelResource, 6.2},
                {AsphaltResource, 4.9},
                {BricksResource, 25},
                {BoardsResource, 11},
                {SteelResource, 4.8},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (type 464) (115w, 5.18m³/d, 60%)",
            WorkerCapacity = 115,
            PowerMW = 6.9,
            WaterPerDay = 5.18,
            HeatTankM3 = 0,
            Quality = 60,
            Workdays = 510,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 19},
                {GravelResource, 15},
                {AsphaltResource, 12},
                {PrefabPanelsResource, 43},
                {SteelResource, 4.4},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (type 75) (52w, 2.34m³/d, 84%)",
            WorkerCapacity = 52,
            PowerMW = 3.1,
            WaterPerDay = 2.34,
            HeatTankM3 = 0,
            Quality = 84,
            Workdays = 430,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 3.9},
                {GravelResource, 3.0},
                {AsphaltResource, 2.4},
                {BricksResource, 0.45},
                {BoardsResource, 20},
                {PrefabPanelsResource, 31},
                {SteelResource, 3.1},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (110w, 4.95m³/d, 87%)",
            WorkerCapacity = 110,
            PowerMW = 6.6,
            WaterPerDay = 4.95,
            HeatTankM3 = 0,
            Quality = 87,
            Workdays = 1200,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 28},
                {GravelResource, 22},
                {AsphaltResource, 17},
                {BricksResource, 60},
                {BoardsResource, 29},
                {SteelResource, 13},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (type 75) (96w, 4.32m³/d, 84%)",
            WorkerCapacity = 96,
            PowerMW = 5.8,
            WaterPerDay = 4.32,
            HeatTankM3 = 0,
            Quality = 84,
            Workdays = 769,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 3.6},
                {GravelResource, 2.8},
                {AsphaltResource, 2.2},
                {BricksResource, 0.50},
                {BoardsResource, 35},
                {PrefabPanelsResource, 59},
                {SteelResource, 6.8},
                {MechanicComponentsResource, 0.20},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (type 75) (63w, 2.84m³/d, 84%) [dup]",
            WorkerCapacity = 63,
            PowerMW = 3.8,
            WaterPerDay = 2.84,
            HeatTankM3 = 0,
            Quality = 84,
            Workdays = 532,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 3.3},
                {GravelResource, 2.5},
                {AsphaltResource, 2.0},
                {BricksResource, 0.37},
                {BoardsResource, 24},
                {PrefabPanelsResource, 39},
                {SteelResource, 4.6},
                {MechanicComponentsResource, 0.14},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (87w, 3.92m³/d, 94%)",
            WorkerCapacity = 87,
            PowerMW = 5.2,
            WaterPerDay = 3.92,
            HeatTankM3 = 0,
            Quality = 94,
            Workdays = 658,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 13},
                {GravelResource, 10},
                {AsphaltResource, 8.5},
                {PrefabPanelsResource, 76},
                {SteelResource, 7.7},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (105w, 4.73m³/d, 91%)",
            WorkerCapacity = 105,
            PowerMW = 6.3,
            WaterPerDay = 4.73,
            HeatTankM3 = 0,
            Quality = 91,
            Workdays = 799,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 21},
                {GravelResource, 16},
                {AsphaltResource, 13},
                {PrefabPanelsResource, 84},
                {SteelResource, 8.4},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (type 75) (64w, 2.88m³/d, 84%)",
            WorkerCapacity = 64,
            PowerMW = 3.8,
            WaterPerDay = 2.88,
            HeatTankM3 = 0,
            Quality = 84,
            Workdays = 504,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 2.9},
                {GravelResource, 2.2},
                {AsphaltResource, 1.8},
                {BricksResource, 0.32},
                {BoardsResource, 24},
                {PrefabPanelsResource, 36},
                {SteelResource, 4.3},
                {MechanicComponentsResource, 0.14},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (type 75) (55w, 2.48m³/d, 84%)",
            WorkerCapacity = 55,
            PowerMW = 3.3,
            WaterPerDay = 2.48,
            HeatTankM3 = 0,
            Quality = 84,
            Workdays = 470,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 3.9},
                {GravelResource, 3.0},
                {AsphaltResource, 2.4},
                {BricksResource, 0.45},
                {BoardsResource, 22},
                {PrefabPanelsResource, 34},
                {SteelResource, 3.5},
            }
        },
    };

    // Large Residential Buildings
    public static List<ResidentialBuilding> LargeResidentialBuildings = new List<ResidentialBuilding>
    {
        new ResidentialBuilding {
            Name = "Flats - brick (200w, 9.00m³/d, 75%)",
            WorkerCapacity = 200,
            PowerMW = 12,
            WaterPerDay = 9.00,
            HeatTankM3 = 0, // Not shown in screenshot
            Quality = 75,
            Workdays = 1908,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 34},
                {GravelResource, 26},
                {BricksResource, 128},
                {BoardsResource, 42},
                {SteelResource, 16},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (150w, 6.75m³/d, 75%)",
            WorkerCapacity = 150,
            PowerMW = 9,
            WaterPerDay = 6.75,
            HeatTankM3 = 0,
            Quality = 75,
            Workdays = 1481,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 26},
                {GravelResource, 20},
                {BricksResource, 100},
                {BoardsResource, 33},
                {SteelResource, 12},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (157w, 7.07m³/d, 68%)",
            WorkerCapacity = 157,
            PowerMW = 9.4,
            WaterPerDay = 7.07,
            HeatTankM3 = 10.00,
            Quality = 68,
            Workdays = 1382,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 27},
                {GravelResource, 20},
                {AsphaltResource, 16},
                {BricksResource, 80},
                {BoardsResource, 34},
                {SteelResource, 14},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (145w, 6.53m³/d, 80%)",
            WorkerCapacity = 145,
            PowerMW = 8.7,
            WaterPerDay = 6.53,
            HeatTankM3 = 0,
            Quality = 80,
            Workdays = 1600,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 39},
                {GravelResource, 30},
                {BricksResource, 98},
                {BoardsResource, 32},
                {SteelResource, 12},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (125w, 5.63m³/d, 80%)",
            WorkerCapacity = 125,
            PowerMW = 7.5,
            WaterPerDay = 5.63,
            HeatTankM3 = 0,
            Quality = 80,
            Workdays = 1257,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 26},
                {GravelResource, 20},
                {BricksResource, 81},
                {BoardsResource, 27},
                {SteelResource, 10},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (120w, 5.40m³/d, 80%)",
            WorkerCapacity = 120,
            PowerMW = 7.2,
            WaterPerDay = 5.40,
            HeatTankM3 = 0,
            Quality = 80,
            Workdays = 1077,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 21},
                {GravelResource, 16},
                {BricksResource, 71},
                {BoardsResource, 23},
                {SteelResource, 8.9},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (125w, 5.63m³/d, 80%) [v2]", // Added v2 to distinguish from previous 125w
            WorkerCapacity = 125,
            PowerMW = 7.5,
            WaterPerDay = 5.63,
            HeatTankM3 = 0,
            Quality = 80,
            Workdays = 1233,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 19},
                {GravelResource, 14},
                {BricksResource, 86},
                {BoardsResource, 28},
                {SteelResource, 10},
            }
        },
        new ResidentialBuilding {
            Name = "Dnipro Flats (593w, 26.69m³/d, 90%)",
            WorkerCapacity = 593,
            PowerMW = 35,
            WaterPerDay = 26.69,
            HeatTankM3 = 0,
            Quality = 90,
            Workdays = 4323,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 490},
                {GravelResource, 23},
                {AsphaltResource, 18},
                {PrefabPanelsResource, 179}, // Note: This uses Prefab panels!
                {SteelResource, 185},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (145w, 6.53m³/d, 72%)",
            WorkerCapacity = 145,
            PowerMW = 8.7,
            WaterPerDay = 6.53,
            HeatTankM3 = 0,
            Quality = 72,
            Workdays = 723,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 10},
                {GravelResource, 8.4},
                {AsphaltResource, 6.7},
                {PrefabPanelsResource, 74},
                {SteelResource, 17},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (150w, 6.75m³/d, 80%)",
            WorkerCapacity = 150,
            PowerMW = 9,
            WaterPerDay = 6.75,
            HeatTankM3 = 0,
            Quality = 80,
            Workdays = 1307,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 23},
                {GravelResource, 17},
                {AsphaltResource, 14},
                {BricksResource, 89},
                {BoardsResource, 29},
                {SteelResource, 11},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - brick (210w, 9.45m³/d, 80%)",
            WorkerCapacity = 210,
            PowerMW = 12,
            WaterPerDay = 9.45,
            HeatTankM3 = 0,
            Quality = 80,
            Workdays = 2005,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 39},
                {GravelResource, 30},
                {BricksResource, 133},
                {BoardsResource, 44},
                {SteelResource, 16},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (160w, 7.20m³/d, 76%)",
            WorkerCapacity = 160,
            PowerMW = 9.6,
            WaterPerDay = 7.20,
            HeatTankM3 = 0,
            Quality = 76,
            Workdays = 877,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 17},
                {GravelResource, 13},
                {AsphaltResource, 10},
                {PrefabPanelsResource, 88},
                {SteelResource, 17},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (120w, 5.40m³/d, 86%)",
            WorkerCapacity = 120,
            PowerMW = 7.2,
            WaterPerDay = 5.40,
            HeatTankM3 = 0,
            Quality = 86,
            Workdays = 901,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 30},
                {GravelResource, 23},
                {AsphaltResource, 18},
                {PrefabPanelsResource, 72},
                {SteelResource, 14},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (165w, 7.43m³/d, 78%)",
            WorkerCapacity = 165,
            PowerMW = 9.9,
            WaterPerDay = 7.43,
            HeatTankM3 = 0,
            Quality = 78,
            Workdays = 895,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 15},
                {GravelResource, 12},
                {AsphaltResource, 9.7},
                {PrefabPanelsResource, 89},
                {SteelResource, 20},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (210w, 9.45m³/d, 70%)",
            WorkerCapacity = 210,
            PowerMW = 12,
            WaterPerDay = 9.45,
            HeatTankM3 = 0,
            Quality = 70,
            Workdays = 1110,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 17},
                {GravelResource, 13},
                {BricksResource, 56},
                {BoardsResource, 25},
                {PrefabPanelsResource, 20},
                {SteelResource, 12},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (180w, 8.10m³/d, 75%)",
            WorkerCapacity = 180,
            PowerMW = 10,
            WaterPerDay = 8.10,
            HeatTankM3 = 0,
            Quality = 75,
            Workdays = 962,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 21},
                {GravelResource, 16},
                {AsphaltResource, 13},
                {PrefabPanelsResource, 85},
                {SteelResource, 22},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (160w, 7.20m³/d, 70%)",
            WorkerCapacity = 160,
            PowerMW = 9.6,
            WaterPerDay = 7.20,
            HeatTankM3 = 0,
            Quality = 70,
            Workdays = 921,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 20},
                {GravelResource, 15},
                {AsphaltResource, 12},
                {BricksResource, 39},
                {BoardsResource, 20},
                {PrefabPanelsResource, 14},
                {SteelResource, 10},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (1Lg-600A) (287w, 12.92m³/d, 80%)",
            WorkerCapacity = 287,
            PowerMW = 17,
            WaterPerDay = 12.92,
            HeatTankM3 = 0,
            Quality = 80,
            Workdays = 2029,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 178},
                {GravelResource, 15},
                {BricksResource, 0.47},
                {BoardsResource, 61},
                {PrefabPanelsResource, 138},
                {ElectroComponentsResource, 0.23}, // New resource type!
                {MechanicComponentsResource, 0.35}, // New resource type!
                {SteelResource, 5.4},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (140w, 6.30m³/d, 70%)",
            WorkerCapacity = 140,
            PowerMW = 8.4,
            WaterPerDay = 6.30,
            HeatTankM3 = 0,
            Quality = 70,
            Workdays = 719,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 10},
                {GravelResource, 8.3},
                {AsphaltResource, 6.7},
                {BricksResource, 37},
                {BoardsResource, 16},
                {PrefabPanelsResource, 13},
                {SteelResource, 8.4},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (1Lg-600A) (147w, 6.62m³/d, 80%)",
            WorkerCapacity = 147,
            PowerMW = 8.8,
            WaterPerDay = 6.62,
            HeatTankM3 = 0,
            Quality = 80,
            Workdays = 1059,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 66},
                {GravelResource, 5.4},
                {BricksResource, 0.30},
                {BoardsResource, 34},
                {PrefabPanelsResource, 78},
                {ElectroComponentsResource, 0.10},
                {MechanicComponentsResource, 0.15},
                {SteelResource, 3.4},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (1Lg-600A) (245w, 11.03m³/d, 80%)",
            WorkerCapacity = 245,
            PowerMW = 14,
            WaterPerDay = 11.03,
            HeatTankM3 = 0,
            Quality = 80,
            Workdays = 1650,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 145},
                {GravelResource, 12},
                {BricksResource, 0.35},
                {BoardsResource, 50},
                {PrefabPanelsResource, 112},
                {ElectroComponentsResource, 0.22},
                {MechanicComponentsResource, 0.33},
                {SteelResource, 4.6},
            }
        },
        new ResidentialBuilding {
            Name = "Flats - prefab (167w, 7.52m³/d, 70%)",
            WorkerCapacity = 167,
            PowerMW = 10,
            WaterPerDay = 7.52,
            HeatTankM3 = 0,
            Quality = 70,
            Workdays = 914,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {ConcreteResource, 14},
                {GravelResource, 11},
                {AsphaltResource, 8.8},
                {BricksResource, 47},
                {BoardsResource, 20},
                {PrefabPanelsResource, 16},
                {SteelResource, 10},
            }
        },
    };

    // Amenity Buildings
    public static List<AmenityBuilding> GroceryBuildings = new List<AmenityBuilding>()
    {
        new AmenityBuilding
        {
            Name = "Shopping center",
            Type = AmenityType.Shopping,
            WorkersPerShift = 45,
            PowerConsumptionMWh = 5.6,
            WattageKW = 93,
            WaterConsumptionM3 = 0.90,
            HotWaterTankM3 = 22,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.31,
            GarbagePerCustomer = 0.20,
            MaxVisitors = 405,
            AttractionScore = 1.5,
            Workdays = 3117,
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                // Leave empty for now
            }
        },
        new AmenityBuilding
        {
            Name = "Small shopping center (180 visitors)",
            Type = AmenityType.Shopping,
            WorkersPerShift = 30,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.60,
            HotWaterTankM3 = 10,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.31,
            GarbagePerCustomer = 0.20,
            MaxVisitors = 180,
            AttractionScore = null,
            Workdays = 929,
            ConstructionMaterials = new Dictionary<Resource, double>()
        },
        new AmenityBuilding
        {
            Name = "Small shopping center (150 visitors, 829 workdays)",
            Type = AmenityType.Shopping,
            WorkersPerShift = 25,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.50,
            HotWaterTankM3 = 9,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.31,
            GarbagePerCustomer = 0.20,
            MaxVisitors = 150,
            AttractionScore = null,
            Workdays = 829,
            ConstructionMaterials = new Dictionary<Resource, double>()
        },
        new AmenityBuilding
        {
            Name = "Small shopping center (150 visitors, 579 workdays)",
            Type = AmenityType.Shopping,
            WorkersPerShift = 25,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.50,
            HotWaterTankM3 = 9,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.31,
            GarbagePerCustomer = 0.20,
            MaxVisitors = 150,
            AttractionScore = null,
            Workdays = 579,
            ConstructionMaterials = new Dictionary<Resource, double>()
        },
        new AmenityBuilding
        {
            Name = "Grocery store",
            Type = AmenityType.Shopping,
            WorkersPerShift = 15,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.30,
            HotWaterTankM3 = 6,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.31,
            GarbagePerCustomer = 0.20,
            MaxVisitors = 105,
            AttractionScore = null,
            Workdays = 189,  
            ConstructionMaterials = new Dictionary<Resource, double>()
            {
                {GravelResource, 6.6 },
                {BricksResource, 6.6 },
                {SteelResource, 1.5 },
                {ConcreteResource, 11 },
                {AsphaltResource, 5.2 },
                {BoardsResource, 2.2 },
            }
        },
        new AmenityBuilding
        {
            Name = "Small store",
            Type = AmenityType.Shopping,
            WorkersPerShift = 15,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.30,
            HotWaterTankM3 = 6,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.31,
            GarbagePerCustomer = 0.20,
            MaxVisitors = 105,
            AttractionScore = null,
            Workdays = 320,
            ConstructionMaterials = new Dictionary<Resource, double>()
        },
        new AmenityBuilding
        {
            Name = "Grocery kiosk",
            Type = AmenityType.Shopping,
            WorkersPerShift = 1,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.02,
            HotWaterTankM3 = 0.56,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.31,
            GarbagePerCustomer = 0.20,
            MaxVisitors = 10,
            AttractionScore = null,
            Workdays = 0,  // Buy with rubles only
            ConstructionMaterials = new Dictionary<Resource, double>()
        },
        new AmenityBuilding
        {
            Name = "\"Panorama\" (restaurant)",
            Type = AmenityType.Shopping,
            WorkersPerShift = 25,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.50,
            HotWaterTankM3 = 4,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.31,
            GarbagePerCustomer = 0.20,
            MaxVisitors = 50,
            AttractionScore = 4.8,
            Workdays = 2249,
            ConstructionMaterials = new Dictionary<Resource, double>()
        }
    };
    public static List<AmenityBuilding> CityServiceBuildings = new List<AmenityBuilding>()
    {
        new AmenityBuilding
        {
            Name = "Technical services (2123 workdays, 15 parking)",
            Type = AmenityType.CityService,  // Or create new CityService type if you prefer
            WorkersPerShift = 0,  // Not shown on card
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,  // Not shown
            HotWaterTankM3 = 0,  // Not shown
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0,  // Not shown
            GarbagePerCustomer = 0,  // N/A for city services
            MaxVisitors = 0,  // N/A for city services
            AttractionScore = null,
            Workdays = 2123,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 96t Concrete, 59t Asphalt, 67t Bricks, 25t Steel, 74t Gravel, 22t Boards
            // Parking: 15 spots, Oil tank: 30t Fuel, Stations: 2
        },
        new AmenityBuilding
        {
            Name = "Technical services (1382 workdays, 8 parking)",
            Type = AmenityType.CityService,
            WorkersPerShift = 0,  // Not shown on card
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,  // Not shown
            HotWaterTankM3 = 0,  // Not shown
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0,  // Not shown
            GarbagePerCustomer = 0,
            MaxVisitors = 0,
            AttractionScore = null,
            Workdays = 1382,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 32t Concrete, 19t Asphalt, 67t Bricks, 25t Steel, 24t Gravel, 22t Boards
            // Parking: 8 spots, Oil tank: 30t Fuel, Stations: 2
        },
        new AmenityBuilding
        {
            Name = "City accounting office",
            Type = AmenityType.CityService,
            WorkersPerShift = 25,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.50,
            HotWaterTankM3 = 1.75,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.45,
            GarbagePerCustomer = 0,
            MaxVisitors = 0,
            AttractionScore = null,
            Workdays = 1120,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 136t Concrete, 18t Asphalt, 24t Bricks, 27t Steel, 23t Gravel, 8.3t Boards
            // Stations: 2
        },
        new AmenityBuilding
        {
            Name = "Technical services (785 workdays, 5 parking)",
            Type = AmenityType.CityService,
            WorkersPerShift = 0,  // Not shown on card
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,  // Not shown
            HotWaterTankM3 = 0,  // Not shown
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0,  // Not shown
            GarbagePerCustomer = 0,
            MaxVisitors = 0,
            AttractionScore = null,
            Workdays = 785,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 23t Concrete, 14t Asphalt, 32t Bricks, 13t Steel, 18t Gravel, 10t Boards
            // Parking: 5 spots, Oil tank: 30t Fuel, Stations: 2
        }
    };
    public static List<AmenityBuilding> EmergencyBuildings = new List<AmenityBuilding>()
    {
        new AmenityBuilding
        {
            Name = "Hospital (90 patients)",
            Type = AmenityType.Healthcare,
            WorkersPerShift = 60,  // 30 workers + 30 nurses
            PowerConsumptionMWh = 5.9,
            WattageKW = 98,
            WaterConsumptionM3 = 1.20,
            HotWaterTankM3 = 8,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.62,
            GarbagePerCustomer = 1.10,  // Per patient
            MaxVisitors = 90,  // Max patients
            AttractionScore = null,
            Workdays = 1002,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 54t Concrete, 14t Asphalt, 17t Bricks, 43t Steel, 17t Gravel, 5.8t Boards
            // Parking: 8 spots, Oil tank: 40t Fuel, Stations: 1
        },
        new AmenityBuilding
        {
            Name = "Hospital (450 patients)",
            Type = AmenityType.Healthcare,
            WorkersPerShift = 180,  // 90 workers + 90 nurses
            PowerConsumptionMWh = 13,
            WattageKW = 230,
            WaterConsumptionM3 = 3.60,
            HotWaterTankM3 = 34,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.62,
            GarbagePerCustomer = 1.10,  // Per patient
            MaxVisitors = 450,  // Max patients
            AttractionScore = null,
            Workdays = 8828,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 787t Concrete, 127t Asphalt, 469t Steel, 159t Gravel
            // Parking: 24 spots, Oil tank: 25t Fuel, Stations: 2, Helicopter stations: 2
        },
        new AmenityBuilding
        {
            Name = "Small clinic",
            Type = AmenityType.Healthcare,
            WorkersPerShift = 10,  // 5 workers + 5 nurses
            PowerConsumptionMWh = 3.5,
            WattageKW = 57,
            WaterConsumptionM3 = 0.20,
            HotWaterTankM3 = 1.44,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.62,
            GarbagePerCustomer = 1.10,  // Per patient
            MaxVisitors = 15,  // Max patients
            AttractionScore = null,
            Workdays = 365,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 10t Concrete, 6.3t Asphalt, 15t Bricks, 4.3t Steel, 7.9t Gravel, 9.0t Boards
            // Parking: 1 spot, Oil tank: 15t Fuel, Stations: 1
        },
        new AmenityBuilding
        {
            Name = "Fire station (40 workers, 4 parking)",
            Type = AmenityType.Fireservice,  // Or add new Emergency type
            WorkersPerShift = 40,
            PowerConsumptionMWh = 4.2,
            WattageKW = 70,
            WaterConsumptionM3 = 0.80,
            HotWaterTankM3 = 2.80,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.20,
            GarbagePerCustomer = 0,  // N/A for fire stations
            MaxVisitors = 0,  // N/A
            AttractionScore = null,
            Workdays = 965,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 22t Concrete, 13t Asphalt, 52t Bricks, 15t Steel, 17t Gravel, 17t Boards
            // Parking: 4 spots, Oil tank: 40t Fuel, Stations: 1
        },
        new AmenityBuilding
        {
            Name = "Fire station (50 workers, 8 parking)",
            Type = AmenityType.Fireservice,
            WorkersPerShift = 50,
            PowerConsumptionMWh = 4.5,
            WattageKW = 74,
            WaterConsumptionM3 = 1.00,
            HotWaterTankM3 = 3,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.20,
            GarbagePerCustomer = 0,
            MaxVisitors = 0,
            AttractionScore = null,
            Workdays = 2370,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 67t Concrete, 41t Asphalt, 99t Bricks, 27t Steel, 51t Gravel, 58t Boards
            // Parking: 8 spots, Oil tank: 60t Fuel, Stations: 1
        },
        new AmenityBuilding
        {
            Name = "Fire station (30 workers, 2 parking)",
            Type = AmenityType.Fireservice,
            WorkersPerShift = 30,
            PowerConsumptionMWh = 3.9,
            WattageKW = 65,
            WaterConsumptionM3 = 0.60,
            HotWaterTankM3 = 0,  // Not shown on card
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.20,
            GarbagePerCustomer = 0,
            MaxVisitors = 0,
            AttractionScore = null,
            Workdays = 469,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 8.4t Concrete, 5.2t Asphalt, 24t Bricks, 6.0t Steel, 6.5t Gravel, 13t Boards
            // Parking: 2 spots, Oil tank: 30t Fuel, Stations: 1
        }
    };
    public static List<AmenityBuilding> CultureBuildings = new List<AmenityBuilding>()
    {
        new AmenityBuilding
        {
            Name = "Gallery of art",
            Type = AmenityType.Culture,
            WorkersPerShift = 15,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.30,
            HotWaterTankM3 = 5,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.32,
            GarbagePerCustomer = 0.30,
            MaxVisitors = 90,
            AttractionScore = 0.8,
            Workdays = 728,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 145t Concrete, 3.4t Asphalt, 33t Steel, 4.3t Gravel
        },
        new AmenityBuilding
        {
            Name = "\"The Palace of Communism\" (museum)",
            Type = AmenityType.Culture,
            WorkersPerShift = 20,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.40,
            HotWaterTankM3 = 5,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.32,
            GarbagePerCustomer = 0.30,
            MaxVisitors = 80,
            AttractionScore = 3.8,
            Workdays = 2085,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 266t Concrete, 8.4t Asphalt, 126t Steel, 10t Gravel
        },
        new AmenityBuilding
        {
            Name = "Cinema",
            Type = AmenityType.Culture,
            WorkersPerShift = 6,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.12,
            HotWaterTankM3 = 7,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.50,
            GarbagePerCustomer = 0.67,
            MaxVisitors = 150,
            AttractionScore = null,  // Not shown on card
            Workdays = 1152,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 184t Concrete, 27t Asphalt, 31t Steel, 34t Gravel
        },
        new AmenityBuilding
        {
            Name = "Pyramid museum",
            Type = AmenityType.Culture,
            WorkersPerShift = 20,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.40,
            HotWaterTankM3 = 6,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.32,
            GarbagePerCustomer = 0.30,
            MaxVisitors = 100,
            AttractionScore = 2.8,
            Workdays = 4944,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 433t Concrete, 133t Steel, 119t Gravel, 202t Prefab panels
        },
        new AmenityBuilding
        {
            Name = "National uprising museum",
            Type = AmenityType.Culture,
            WorkersPerShift = 8,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.16,
            HotWaterTankM3 = 2.52,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.32,
            GarbagePerCustomer = 0.30,
            MaxVisitors = 40,
            AttractionScore = 2.4,
            Workdays = 1598,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 239t Concrete, 24t Asphalt, 66t Steel, 30t Gravel
        },
        new AmenityBuilding
        {
            Name = "Republic theater",
            Type = AmenityType.Culture,
            WorkersPerShift = 20,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.40,
            HotWaterTankM3 = 6,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.50,
            GarbagePerCustomer = 0.67,
            MaxVisitors = 100,
            AttractionScore = 2.4,
            Workdays = 1848,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 282t Concrete, 49t Asphalt, 45t Steel, 61t Gravel
        },
        new AmenityBuilding
        {
            Name = "Museum of the Republic",
            Type = AmenityType.Culture,
            WorkersPerShift = 15,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.30,
            HotWaterTankM3 = 3,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.32,
            GarbagePerCustomer = 0.30,
            MaxVisitors = 60,
            AttractionScore = 3.2,
            Workdays = 836,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 23t Concrete, 9.3t Asphalt, 10t Steel, 11t Gravel, 82t Prefab panels, 10t Boards
        },
        new AmenityBuilding
        {
            Name = "House of culture",
            Type = AmenityType.Culture,
            WorkersPerShift = 5,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.10,
            HotWaterTankM3 = 1.82,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.50,
            GarbagePerCustomer = 0.67,
            MaxVisitors = 30,
            AttractionScore = null,  // Not shown on card
            Workdays = 381,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 12t Concrete, 7.9t Asphalt, 20t Bricks, 2.5t Steel, 9.8t Gravel, 6.7t Boards
        },
        new AmenityBuilding
        {
            Name = "Amphitheatre",
            Type = AmenityType.Culture,
            WorkersPerShift = 5,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,  // Not shown on card
            HotWaterTankM3 = 0,  // Not shown on card
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0,  // Not shown on card
            GarbagePerCustomer = 0,  // Not shown on card
            MaxVisitors = 150,
            AttractionScore = null,  // Not shown on card
            Workdays = 437,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 46t Concrete, 3.1t Steel, 24t Gravel
            // Note: Requires weather temperature above 10°C
        }
    };
    public static List<AmenityBuilding> EducationBuildings = new List<AmenityBuilding>()
    {
        // Schools
        new AmenityBuilding
        {
            Name = "School (156 students)",
            Type = AmenityType.Education,
            WorkersPerShift = 26,  // 13 + 13
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.52,
            HotWaterTankM3 = 9,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.35,
            GarbagePerCustomer = 0.60,  // Per student
            MaxVisitors = 156,  // Max students
            AttractionScore = null,
            Workdays = 488,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 17t Concrete, 11t Asphalt, 8.5t Steel, 13t Gravel, 34t Prefab panels
        },
        new AmenityBuilding
        {
            Name = "School (84 students)",
            Type = AmenityType.Education,
            WorkersPerShift = 14,  // 7 + 7
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.28,
            HotWaterTankM3 = 5,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.35,
            GarbagePerCustomer = 0.60,
            MaxVisitors = 84,
            AttractionScore = null,
            Workdays = 319,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 13t Concrete, 8.6t Asphalt, 13t Bricks, 1.7t Steel, 10t Gravel, 4.5t Boards
        },
        new AmenityBuilding
        {
            Name = "School (540 students)",
            Type = AmenityType.Education,
            WorkersPerShift = 90,  // 45 + 45
            PowerConsumptionMWh = 9.2,
            WattageKW = 153,
            WaterConsumptionM3 = 1.80,
            HotWaterTankM3 = 32,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.35,
            GarbagePerCustomer = 0.60,
            MaxVisitors = 540,
            AttractionScore = null,
            Workdays = 1980,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 89t Concrete, 54t Asphalt, 26t Steel, 68t Gravel, 121t Prefab panels
        },
        new AmenityBuilding
        {
            Name = "School (360 students)",
            Type = AmenityType.Education,
            WorkersPerShift = 60,  // 30 + 30
            PowerConsumptionMWh = 6.1,
            WattageKW = 102,
            WaterConsumptionM3 = 1.20,
            HotWaterTankM3 = 21,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.35,
            GarbagePerCustomer = 0.60,
            MaxVisitors = 360,
            AttractionScore = null,
            Workdays = 973,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 34t Concrete, 21t Asphalt, 19t Steel, 26t Gravel, 65t Prefab panels
        },

        // Kindergartens
        new AmenityBuilding
        {
            Name = "Kindergarten (270 children)",
            Type = AmenityType.Education,
            WorkersPerShift = 27,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.54,
            HotWaterTankM3 = 15,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.20,
            GarbagePerCustomer = 0.62,  // Per child
            MaxVisitors = 270,  // Max children
            AttractionScore = null,
            Workdays = 915,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 49t Concrete, 30t Asphalt, 10t Steel, 37t Gravel, 42t Prefab panels
        },
        new AmenityBuilding
        {
            Name = "Kindergarten (180 children)",
            Type = AmenityType.Education,
            WorkersPerShift = 18,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.36,
            HotWaterTankM3 = 10,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.20,
            GarbagePerCustomer = 0.62,
            MaxVisitors = 180,
            AttractionScore = null,
            Workdays = 509,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 25t Concrete, 15t Asphalt, 6.6t Steel, 19t Gravel, 26t Prefab panels
        },
        new AmenityBuilding
        {
            Name = "Kindergarten (120 children)",
            Type = AmenityType.Education,
            WorkersPerShift = 12,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.24,
            HotWaterTankM3 = 6,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.20,
            GarbagePerCustomer = 0.62,
            MaxVisitors = 120,
            AttractionScore = null,
            Workdays = 392,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 12t Concrete, 8.0t Asphalt, 20t Bricks, 2.8t Steel, 9.9t Gravel, 7.2t Boards
        },

        // Universities & Party Schools
        new AmenityBuilding
        {
            Name = "Small headquarters of the Party",
            Type = AmenityType.Education,
            WorkersPerShift = 80,  // 40 + 40
            PowerConsumptionMWh = 3.4,
            WattageKW = 56,
            WaterConsumptionM3 = 1.60,
            HotWaterTankM3 = 9,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.40,
            GarbagePerCustomer = 0.48,  // Per student
            MaxVisitors = 80,  // Max students
            AttractionScore = null,
            Workdays = 1545,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 23t Concrete, 14t Asphalt, 78t Bricks, 9.1t Steel, 18t Gravel, 33t Boards, 34t Prefab panels
        },
        new AmenityBuilding
        {
            Name = "Headquarters of the Communist Party",
            Type = AmenityType.Education,
            WorkersPerShift = 140,  // 70 + 70
            PowerConsumptionMWh = 9.2,
            WattageKW = 154,
            WaterConsumptionM3 = 2.80,
            HotWaterTankM3 = 30,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.40,
            GarbagePerCustomer = 0.48,
            MaxVisitors = 420,  // Max students
            AttractionScore = null,
            Workdays = 7065,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 520t Concrete, 132t Asphalt, 201t Steel, 165t Gravel, 166t Bricks, 55t Boards
            // Stations: 3
        },
        new AmenityBuilding
        {
            Name = "Medical university",
            Type = AmenityType.Education,
            WorkersPerShift = 100,  // 50 + 50
            PowerConsumptionMWh = 6.6,
            WattageKW = 110,
            WaterConsumptionM3 = 2.00,
            HotWaterTankM3 = 21,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.40,
            GarbagePerCustomer = 0.48,
            MaxVisitors = 300,  // Max students
            AttractionScore = null,
            Workdays = 7388,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 541t Concrete, 130t Asphalt, 217t Steel, 163t Gravel, 179t Bricks, 59t Boards
            // Stations: 3
        },
        new AmenityBuilding
        {
            Name = "Technical university (60 students)",
            Type = AmenityType.Education,
            WorkersPerShift = 60,  // 30 + 30
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 1.20,
            HotWaterTankM3 = 7,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.40,
            GarbagePerCustomer = 0.48,
            MaxVisitors = 60,  // Max students
            AttractionScore = null,
            Workdays = 1971,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 44t Concrete, 27t Asphalt, 124t Bricks, 15t Steel, 34t Gravel, 41t Boards
        },
        new AmenityBuilding
        {
            Name = "Technical university (225 students)",
            Type = AmenityType.Education,
            WorkersPerShift = 150,  // 75 + 75
            PowerConsumptionMWh = 7.2,
            WattageKW = 120,
            WaterConsumptionM3 = 3.00,
            HotWaterTankM3 = 21,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.40,
            GarbagePerCustomer = 0.48,
            MaxVisitors = 225,  // Max students
            AttractionScore = null,
            Workdays = 5186,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 469t Concrete, 87t Asphalt, 159t Steel, 109t Gravel, 107t Bricks, 35t Boards
            // Stations: 2
        },

        // University Dorms (Residential buildings for students)
        new AmenityBuilding
        {
            Name = "University halls of residence (82 bricks)",
            Type = AmenityType.Education,
            WorkersPerShift = 0,  // No workers, it's a dorm
            PowerConsumptionMWh = 10,
            WattageKW = 170,
            WaterConsumptionM3 = 3.83,
            HotWaterTankM3 = 5,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0,  // N/A
            GarbagePerCustomer = 0,  // N/A
            MaxVisitors = 85,  // Passenger capacity (students living here)
            AttractionScore = null,
            Workdays = 1240,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 24t Concrete, 14t Asphalt, 82t Bricks, 10t Steel, 18t Gravel, 27t Boards
        },
        new AmenityBuilding
        {
            Name = "University halls of residence (72 bricks)",
            Type = AmenityType.Education,
            WorkersPerShift = 0,  // No workers
            PowerConsumptionMWh = 10,
            WattageKW = 170,
            WaterConsumptionM3 = 3.83,
            HotWaterTankM3 = 5,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0,
            GarbagePerCustomer = 0,
            MaxVisitors = 85,  // Passenger capacity
            AttractionScore = null,
            Workdays = 1127,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 24t Concrete, 14t Asphalt, 72t Bricks, 9.1t Steel, 18t Gravel, 24t Boards
        }
    };
    public static List<AmenityBuilding> SportsBuildings = new List<AmenityBuilding>()
    {
        // Sport Halls
        new AmenityBuilding
        {
            Name = "Sport hall (105 visitors)",
            Type = AmenityType.Sports,
            WorkersPerShift = 15,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.30,
            HotWaterTankM3 = 6,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.20,
            GarbagePerCustomer = 0.20,
            MaxVisitors = 105,
            AttractionScore = 2.5,
            Workdays = 1919,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 130t Concrete, 36t Asphalt, 77t Bricks, 25t Steel, 45t Gravel, 25t Boards
        },
        new AmenityBuilding
        {
            Name = "Sport hall (175 visitors)",
            Type = AmenityType.Sports,
            WorkersPerShift = 25,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.50,
            HotWaterTankM3 = 10,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.20,
            GarbagePerCustomer = 0.20,
            MaxVisitors = 175,
            AttractionScore = 2.0,
            Workdays = 3587,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 119t Concrete, 73t Asphalt, 120t Bricks, 43t Steel, 91t Gravel, 87t Boards
        },

        // Pools
        new AmenityBuilding
        {
            Name = "Indoor pool",
            Type = AmenityType.Sports,
            WorkersPerShift = 18,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.36,
            HotWaterTankM3 = 5,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.32,
            GarbagePerCustomer = 0.30,
            MaxVisitors = 90,
            AttractionScore = 1.8,
            Workdays = 716,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 31t Concrete, 11t Asphalt, 37t Bricks, 8.5t Steel, 14t Gravel, 12t Boards
        },
        new AmenityBuilding
        {
            Name = "Outdoor swimming pool",
            Type = AmenityType.Sports,
            WorkersPerShift = 15,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.30,
            HotWaterTankM3 = 0,  // No hot water tank listed
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.32,
            GarbagePerCustomer = 0.30,
            MaxVisitors = 105,
            AttractionScore = 2.0,
            Workdays = 518,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 53t Concrete, 20t Asphalt, 3.6t Bricks, 5.0t Steel, 25t Gravel, 1.2t Boards
            // Requires weather temperature above 15 °C
        },
        new AmenityBuilding
        {
            Name = "Beach",
            Type = AmenityType.Sports,
            WorkersPerShift = 5,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.10,
            HotWaterTankM3 = 0,  // No hot water tank listed
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.32,
            GarbagePerCustomer = 0.30,
            MaxVisitors = 90,
            AttractionScore = 4.0,
            Workdays = 282,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 39t Concrete, 4.6t Asphalt, 7.3t Steel, 5.7t Gravel, 5.8t Boards
            // Requires weather temperature above 20 °C
        },

        // Stadiums
        new AmenityBuilding
        {
            Name = "City stadium",
            Type = AmenityType.Sports,
            WorkersPerShift = 15,
            PowerConsumptionMWh = 3.2,
            WattageKW = 0,  // Not listed
            WaterConsumptionM3 = 0,  // Not listed
            HotWaterTankM3 = 0,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0,  // Not listed
            GarbagePerCustomer = 0,  // Not listed
            MaxVisitors = 450,
            AttractionScore = null,
            Workdays = 2828,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 323t Concrete, 29t Steel, 149t Gravel
            // Requires weather temperature above 5 °C
        },

        // Sports Courts
        new AmenityBuilding
        {
            Name = "Volleyball court",
            Type = AmenityType.Sports,
            WorkersPerShift = 3,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,  // Not listed
            HotWaterTankM3 = 0,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.20,
            GarbagePerCustomer = 0.20,
            MaxVisitors = 15,
            AttractionScore = null,
            Workdays = 35,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 3.1t Concrete, 1.9t Asphalt, 2.4t Gravel
            // Requires weather temperature above 5 °C
        },
        new AmenityBuilding
        {
            Name = "Tennis court",
            Type = AmenityType.Sports,
            WorkersPerShift = 1,
            PowerConsumptionMWh = 3.0,
            WattageKW = 0,  // Not listed
            WaterConsumptionM3 = 0,  // Not listed
            HotWaterTankM3 = 0,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0,  // Not listed
            GarbagePerCustomer = 0,  // Not listed
            MaxVisitors = 5,
            AttractionScore = null,
            Workdays = 55,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 4.8t Concrete, 3.7t Gravel
            // Requires weather temperature above 5 °C
        },
        new AmenityBuilding
        {
            Name = "Basketball and volleyball court (48 visitors)",
            Type = AmenityType.Sports,
            WorkersPerShift = 6,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,  // Not listed
            HotWaterTankM3 = 0,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.20,
            GarbagePerCustomer = 0.20,
            MaxVisitors = 48,
            AttractionScore = null,
            Workdays = 129,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 11t Concrete, 6.9t Asphalt, 8.7t Gravel
            // Requires weather temperature above 5 °C
        },
        new AmenityBuilding
        {
            Name = "Basketball and volleyball court (21 visitors)",
            Type = AmenityType.Sports,
            WorkersPerShift = 3,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,  // Not listed
            HotWaterTankM3 = 0,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.20,
            GarbagePerCustomer = 0.20,
            MaxVisitors = 21,
            AttractionScore = null,
            Workdays = 52,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 4.6t Concrete, 2.8t Asphalt, 3.5t Gravel
            // Requires weather temperature above 5 °C
        },

        // Football Fields
        new AmenityBuilding
        {
            Name = "Football field (32 visitors)",
            Type = AmenityType.Sports,
            WorkersPerShift = 4,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,  // Not listed
            HotWaterTankM3 = 0,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.20,
            GarbagePerCustomer = 0.20,
            MaxVisitors = 32,
            AttractionScore = null,
            Workdays = 132,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 11t Concrete, 8.8t Gravel
            // Requires weather temperature above 5 °C
        },
        new AmenityBuilding
        {
            Name = "Football field (22 visitors)",
            Type = AmenityType.Sports,
            WorkersPerShift = 1,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,  // Not listed
            HotWaterTankM3 = 0,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0,  // Not listed
            GarbagePerCustomer = 0,  // Not listed
            MaxVisitors = 22,
            AttractionScore = null,
            Workdays = 70,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 6.1t Concrete, 4.7t Gravel
            // Requires weather temperature above 5 °C
        }
    };
    public static List<AmenityBuilding> PubsAndBars = new List<AmenityBuilding>()
    {
        // Alcohol Kiosk
        new AmenityBuilding
        {
            Name = "Alcohol kiosk",
            Type = AmenityType.Pub,
            WorkersPerShift = 1,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.02,
            HotWaterTankM3 = 0.66,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.53,
            GarbagePerCustomer = 0.60,
            MaxVisitors = 12,
            AttractionScore = null,
            Workdays = 26,
            WarehouseCapacity = new Dictionary<Resource, double>()
            {
                { AlcoholResource, 2.0 }
            },
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 0.89t Concrete, 0.55t Asphalt, 1.5t Steel, 0.68t Gravel
        },

        // Pubs
        new AmenityBuilding
        {
            Name = "Pub (60 visitors)",
            Type = AmenityType.Pub,
            WorkersPerShift = 5,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.10,
            HotWaterTankM3 = 3,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.53,
            GarbagePerCustomer = 0.60,
            MaxVisitors = 60,
            AttractionScore = null,
            Workdays = 149,
            WarehouseCapacity = new Dictionary<Resource, double>()
            {
                { AlcoholResource, 7.5 }
            },
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 6.6t Concrete, 4.1t Asphalt, 2.4t Bricks, 4.3t Steel, 5.1t Gravel, 0.80t Boards
            // Stations: 1
        },
        new AmenityBuilding
        {
            Name = "Pub (120 visitors)",
            Type = AmenityType.Pub,
            WorkersPerShift = 10,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.20,
            HotWaterTankM3 = 6,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.53,
            GarbagePerCustomer = 0.60,
            MaxVisitors = 120,
            AttractionScore = null,
            Workdays = 993,
            WarehouseCapacity = new Dictionary<Resource, double>()
            {
                { AlcoholResource, 15.0 }
            },
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 47t Concrete, 29t Asphalt, 38t Bricks, 4.8t Steel, 36t Gravel, 12t Boards
            // Stations: 2
        },

        // Cafes/Bars
        new AmenityBuilding
        {
            Name = "\"The View\" (cafe/bar)",
            Type = AmenityType.Pub,
            WorkersPerShift = 8,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.16,
            HotWaterTankM3 = 3,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.53,
            GarbagePerCustomer = 0.60,
            MaxVisitors = 56,
            AttractionScore = 3.6,
            Workdays = 1294,
            WarehouseCapacity = new Dictionary<Resource, double>()
            {
                { AlcoholResource, 7.5 }
            },
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 124t Concrete, 9.6t Asphalt, 24t Bricks, 42t Steel, 25t Gravel, 8.1t Boards
            // Stations: 1
        },
        new AmenityBuilding
        {
            Name = "Beach cafe/bar",
            Type = AmenityType.Pub,
            WorkersPerShift = 7,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.14,
            HotWaterTankM3 = 3,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.53,
            GarbagePerCustomer = 0.60,
            MaxVisitors = 70,
            AttractionScore = 2.4,
            Workdays = 170,
            WarehouseCapacity = new Dictionary<Resource, double>()
            {
                { AlcoholResource, 2.0 }
            },
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 8.7t Concrete, 5.3t Asphalt, 3.5t Prefab panels, 3.2t Steel, 6.7t Gravel, 1.8t Boards
            // Requires weather temperature above 10 °C
            // Stations: 1
        },
        new AmenityBuilding
        {
            Name = "\"Delicje\" (café/bar)",
            Type = AmenityType.Pub,
            WorkersPerShift = 10,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.20,
            HotWaterTankM3 = 4,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.53,
            GarbagePerCustomer = 0.60,
            MaxVisitors = 70,
            AttractionScore = 2.5,
            Workdays = 712,
            WarehouseCapacity = new Dictionary<Resource, double>()
            {
                { AlcoholResource, 7.5 }
            },
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 20t Concrete, 12t Asphalt, 29t Bricks, 8.3t Steel, 15t Gravel, 17t Boards
            // Stations: 2
        }
    };
    public static List<AmenityBuilding> Fountains = new List<AmenityBuilding>()
    {
        // Big Fountain
        new AmenityBuilding
        {
            Name = "Big fountain",
            Type = AmenityType.Fountain,
            WorkersPerShift = 0,  // Cosmetic, no workers
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.06,
            HotWaterTankM3 = 0,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0,
            GarbagePerCustomer = 0,
            MaxVisitors = 0,
            AttractionScore = null,
            Workdays = 0,  // Not listed - requires Grand Monuments research
            ConstructionMaterials = new Dictionary<Resource, double>()
            // No construction materials listed
        },

        // Medium Fountain
        new AmenityBuilding
        {
            Name = "Medium fountain",
            Type = AmenityType.Fountain,
            WorkersPerShift = 0,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.03,
            HotWaterTankM3 = 0,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0,
            GarbagePerCustomer = 0,
            MaxVisitors = 0,
            AttractionScore = null,
            Workdays = 231,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 25t Concrete, 10t Asphalt, 2.1t Steel, 12t Gravel
        },

        // Small Fountain
        new AmenityBuilding
        {
            Name = "Small fountain",
            Type = AmenityType.Fountain,
            WorkersPerShift = 0,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.02,
            HotWaterTankM3 = 0,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0,
            GarbagePerCustomer = 0,
            MaxVisitors = 0,
            AttractionScore = null,
            Workdays = 100,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 13t Concrete, 1.7t Steel, 4.4t Gravel
        },

        // Rectangular Fountain
        new AmenityBuilding
        {
            Name = "Rectangular fountain",
            Type = AmenityType.Fountain,
            WorkersPerShift = 0,
            PowerConsumptionMWh = 3.6,
            WattageKW = 60,
            WaterConsumptionM3 = 0.09,
            HotWaterTankM3 = 0,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0,
            GarbagePerCustomer = 0,
            MaxVisitors = 0,
            AttractionScore = null,
            Workdays = 89,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 5.5t Concrete, 3.4t Asphalt, 2.3t Steel, 4.2t Gravel
        },

        // Small Round Fountain (Variant 1)
        new AmenityBuilding
        {
            Name = "Small round fountain (v1)",
            Type = AmenityType.Fountain,
            WorkersPerShift = 0,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.04,
            HotWaterTankM3 = 0,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0,
            GarbagePerCustomer = 0,
            MaxVisitors = 0,
            AttractionScore = null,
            Workdays = 25,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 1.1t Concrete, 0.66t Asphalt, 1.2t Steel, 0.83t Gravel
        },

        // Small Round Fountain (Variant 2)
        new AmenityBuilding
        {
            Name = "Small round fountain (v2)",
            Type = AmenityType.Fountain,
            WorkersPerShift = 0,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.01,
            HotWaterTankM3 = 0,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0,
            GarbagePerCustomer = 0,
            MaxVisitors = 0,
            AttractionScore = null,
            Workdays = 27,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 1.1t Concrete, 0.67t Asphalt, 1.4t Steel, 0.83t Gravel
        },

        // Medium Round Fountain (Variant 1)
        new AmenityBuilding
        {
            Name = "Medium round fountain (v1)",
            Type = AmenityType.Fountain,
            WorkersPerShift = 0,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.04,
            HotWaterTankM3 = 0,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0,
            GarbagePerCustomer = 0,
            MaxVisitors = 0,
            AttractionScore = null,
            Workdays = 110,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 4.2t Concrete, 2.6t Asphalt, 5.6t Steel, 3.2t Gravel
        },

        // Medium Round Fountain (Variant 2 - highest water consumption!)
        new AmenityBuilding
        {
            Name = "Medium round fountain (v2)",
            Type = AmenityType.Fountain,
            WorkersPerShift = 0,
            PowerConsumptionMWh = 4.6,
            WattageKW = 76,
            WaterConsumptionM3 = 0.13,
            HotWaterTankM3 = 0,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0,
            GarbagePerCustomer = 0,
            MaxVisitors = 0,
            AttractionScore = null,
            Workdays = 86,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 4.1t Concrete, 2.5t Asphalt, 3.5t Steel, 3.2t Gravel
        },

        // Medium Round Fountain (Variant 3)
        new AmenityBuilding
        {
            Name = "Medium round fountain (v3)",
            Type = AmenityType.Fountain,
            WorkersPerShift = 0,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.03,
            HotWaterTankM3 = 0,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0,
            GarbagePerCustomer = 0,
            MaxVisitors = 0,
            AttractionScore = null,
            Workdays = 99,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 4.1t Concrete, 2.6t Asphalt, 4.6t Steel, 3.2t Gravel
        }
    };
    public static List<AmenityBuilding> CrimeJusticeBuildings = new List<AmenityBuilding>()
    {
        // Prisons (Large)
        new AmenityBuilding
        {
            Name = "Prison (210 visitors)",
            Type = AmenityType.CrimeJustice,
            WorkersPerShift = 70,
            PowerConsumptionMWh = 4.0,
            WattageKW = 66,
            WaterConsumptionM3 = 10.85,
            HotWaterTankM3 = 15,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.45,
            GarbagePerCustomer = 0,  // Visitors don't produce garbage in this context
            MaxVisitors = 210,
            AttractionScore = null,
            Workdays = 6338,
            WarehouseCapacity = new Dictionary<Resource, double>()
            {
                { FoodResource, 15.0 },
                { ClothesResource, 15.0 }
            },
            ColdStorageCapacity = new Dictionary<Resource, double>()
            {
                { MeatResource, 2.0 }
            },
            VehicleStations = 2,
            QualityOfFlats = 0.50,  // 50%
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 125t Concrete, 77t Asphalt, 419t Bricks, 52t Steel, 96t Gravel, 139t Boards
        },
        new AmenityBuilding
        {
            Name = "Prison (60 visitors)",
            Type = AmenityType.CrimeJustice,
            WorkersPerShift = 20,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 3.10,
            HotWaterTankM3 = 4,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.45,
            GarbagePerCustomer = 0,
            MaxVisitors = 60,
            AttractionScore = null,
            Workdays = 1060,
            WarehouseCapacity = new Dictionary<Resource, double>()
            {
                { FoodResource, 12.0 },
                { ClothesResource, 12.0 }
            },
            ColdStorageCapacity = new Dictionary<Resource, double>()
            {
                { MeatResource, 4.0 }
            },
            VehicleStations = 2,
            QualityOfFlats = 0.48,  // 48%
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 37t Concrete, 23t Asphalt, 33t Bricks, 12t Steel, 28t Gravel, 25t Boards
        },

        // Court Houses
        new AmenityBuilding
        {
            Name = "Court house",
            Type = AmenityType.CrimeJustice,
            WorkersPerShift = 50,  // Max workers
            PowerConsumptionMWh = 4.0,
            WattageKW = 50,
            WaterConsumptionM3 = 1.60,
            HotWaterTankM3 = 5,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.45,
            GarbagePerCustomer = 0,
            MaxVisitors = 30,  // Listed as separate icon
            AttractionScore = null,
            Workdays = 3071,
            VehicleStations = 2,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 102t Concrete, 63t Asphalt, 290t Prefab panels, 29t Steel, 79t Gravel
        },
        new AmenityBuilding
        {
            Name = "Court house (small)",
            Type = AmenityType.CrimeJustice,
            WorkersPerShift = 15,  // Max workers
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0.50,
            HotWaterTankM3 = 1.75,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.45,
            GarbagePerCustomer = 0,
            MaxVisitors = 10,  // Listed as separate icon
            AttractionScore = null,
            Workdays = 702,
            VehicleStations = 1,
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 111t Concrete, 17t Asphalt, 19t Steel, 21t Gravel
        },

        // Police Stations
        new AmenityBuilding
        {
            Name = "Police station",
            Type = AmenityType.CrimeJustice,
            WorkersPerShift = 20,  // Max workers
            PowerConsumptionMWh = 4.8,
            WattageKW = 80,
            WaterConsumptionM3 = 1.20,
            HotWaterTankM3 = 4,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.45,
            GarbagePerCustomer = 0,
            MaxVisitors = 40,  // Listed as separate icon
            AttractionScore = null,
            Workdays = 1048,
            VehicleStations = 1,
            ParkingSpots = 8,
            FuelImport = 40.0,  // Oil tank import
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 192t Concrete, 15t Asphalt, 38t Steel, 18t Gravel
        },
        new AmenityBuilding
        {
            Name = "Police station (small)",
            Type = AmenityType.CrimeJustice,
            WorkersPerShift = 8,  // Max workers
            PowerConsumptionMWh = 3.5,
            WattageKW = 58,
            WaterConsumptionM3 = 0.36,
            HotWaterTankM3 = 1.26,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.45,
            GarbagePerCustomer = 0,
            MaxVisitors = 10,  // Listed as separate icon
            AttractionScore = null,
            Workdays = 485,
            VehicleStations = 1,
            ParkingSpots = 4,
            FuelImport = 15.0,  // Oil tank import
            ConstructionMaterials = new Dictionary<Resource, double>()
            // 14t Concrete, 9.2t Asphalt, 26t Bricks, 3.4t Steel, 11t Gravel, 9.0t Boards
        },

        // Secret Police
        new AmenityBuilding
        {
            Name = "Secret police",
            Type = AmenityType.CrimeJustice,
            WorkersPerShift = 30,
            PowerConsumptionMWh = 4.8,
            WattageKW = 65,
            WaterConsumptionM3 = 0.60,
            HotWaterTankM3 = 2.10,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.45,
            GarbagePerCustomer = 0,
            MaxVisitors = 0,  // No visitors listed
            AttractionScore = null,
            Workdays = 0,  // No construction workdays listed - requires research
            VehicleStations = 1,
            ParkingSpots = 8,
            FuelImport = 40.0,  // Oil tank import
            RequiresResearch = "Secret Police",
            ConstructionMaterials = new Dictionary<Resource, double>()
            // No construction materials listed
        },
        new AmenityBuilding
        {
            Name = "Secret police (small)",
            Type = AmenityType.CrimeJustice,
            WorkersPerShift = 18,
            PowerConsumptionMWh = 3.5,
            WattageKW = 58,
            WaterConsumptionM3 = 0.36,
            HotWaterTankM3 = 1.26,
            HeatConsumptionMW = 0,
            GarbagePerWorker = 0.45,
            GarbagePerCustomer = 0,
            MaxVisitors = 0,  // No visitors listed
            AttractionScore = null,
            Workdays = 0,  // No construction workdays listed - requires research
            VehicleStations = 1,
            ParkingSpots = 4,
            FuelImport = 25.0,  // Oil tank import
            RequiresResearch = "Secret Police",
            ConstructionMaterials = new Dictionary<Resource, double>()
            // No construction materials listed
        }
    };

    // Transportation Buildings
    public static List<TransportationBuilding> TransportationBuildings = new List<TransportationBuilding>()
    {
        // Bus Stops
        new TransportationBuilding
        {
            Name = "Bus stop (simple)",
            Type = TransportationType.Bus,
            Workdays = 24,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,
            HeatConsumptionMW = 0,
            ParkingSpots = null,
            FuelStorageCapacity = null,
            PassengerCapacity = 20,
            ConstructionMaterials = new Dictionary<Resource, double>
            {
                { GameData.ConcreteResource, 3.7 },
                { GameData.AsphaltResource, 1.1 },
                { GameData.BricksResource, 0.8 },
                { GameData.SteelResource, 0.6 },
                { GameData.GravelResource, 0.8 },
                { GameData.BoardsResource, 0.4 }
            }
        },
        new TransportationBuilding
        {
            Name = "Bus stop (medium)",
            Type = TransportationType.Bus,
            Workdays = 72,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,
            HeatConsumptionMW = 0,
            ParkingSpots = null,
            FuelStorageCapacity = null,
            PassengerCapacity = 40,
            ConstructionMaterials = new Dictionary<Resource, double>
            {
                { GameData.ConcreteResource, 7.4 },
                { GameData.AsphaltResource, 2.2 },
                { GameData.BricksResource, 1.6 },
                { GameData.SteelResource, 1.3 },
                { GameData.GravelResource, 1.5 },
                { GameData.BoardsResource, 0.7 }
            }
        },
        new TransportationBuilding
        {
            Name = "Bus stop (advanced)",
            Type = TransportationType.Bus,
            Workdays = 147,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,
            HeatConsumptionMW = 0,
            ParkingSpots = null,
            FuelStorageCapacity = null,
            PassengerCapacity = 100,
            ConstructionMaterials = new Dictionary<Resource, double>
            {
                { GameData.ConcreteResource, 14.8 },
                { GameData.AsphaltResource, 4.4 },
                { GameData.BricksResource, 3.2 },
                { GameData.SteelResource, 2.5 },
                { GameData.GravelResource, 3.1 },
                { GameData.BoardsResource, 1.5 }
            }
        },

        // Bus End Stations
        new TransportationBuilding
        {
            Name = "Bus end station (2 parking)",
            Type = TransportationType.Station,
            Workdays = 229,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,
            HeatConsumptionMW = 0,
            ParkingSpots = 2,
            FuelStorageCapacity = 30, // tons
            PassengerCapacity = 100,
            ConstructionMaterials = new Dictionary<Resource, double>
            {
                { GameData.ConcreteResource, 18.5 },
                { GameData.AsphaltResource, 9.9 },
                { GameData.BricksResource, 3.2 },
                { GameData.SteelResource, 2.5 },
                { GameData.GravelResource, 6.9 },
                { GameData.BoardsResource, 1.5 }
            }
        },
        new TransportationBuilding
        {
            Name = "Bus end station (5 parking)",
            Type = TransportationType.Station,
            Workdays = 396,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,
            HeatConsumptionMW = 0,
            ParkingSpots = 5,
            FuelStorageCapacity = 30,
            PassengerCapacity = 100,
            ConstructionMaterials = new Dictionary<Resource, double>
            {
                { GameData.ConcreteResource, 31.5 },
                { GameData.AsphaltResource, 19.8 },
                { GameData.BricksResource, 3.2 },
                { GameData.SteelResource, 2.5 },
                { GameData.GravelResource, 13.8 },
                { GameData.BoardsResource, 1.5 }
            }
        },
        new TransportationBuilding
        {
            Name = "Bus end station (8 parking)",
            Type = TransportationType.Station,
            Workdays = 562,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,
            HeatConsumptionMW = 0,
            ParkingSpots = 8,
            FuelStorageCapacity = 30,
            PassengerCapacity = 100,
            ConstructionMaterials = new Dictionary<Resource, double>
            {
                { GameData.ConcreteResource, 44.4 },
                { GameData.AsphaltResource, 29.7 },
                { GameData.BricksResource, 3.2 },
                { GameData.SteelResource, 2.5 },
                { GameData.GravelResource, 20.6 },
                { GameData.BoardsResource, 1.5 }
            }
        },
        new TransportationBuilding
        {
            Name = "Bus end station (15 parking)",
            Type = TransportationType.Station,
            Workdays = 977,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,
            HeatConsumptionMW = 0,
            ParkingSpots = 15,
            FuelStorageCapacity = 30,
            PassengerCapacity = 100,
            ConstructionMaterials = new Dictionary<Resource, double>
            {
                { GameData.ConcreteResource, 77.0 },
                { GameData.AsphaltResource, 49.6 },
                { GameData.BricksResource, 3.2 },
                { GameData.SteelResource, 2.5 },
                { GameData.GravelResource, 34.4 },
                { GameData.BoardsResource, 1.5 }
            }
        },

        // Trolleybus Stops
        new TransportationBuilding
        {
            Name = "Trolleybus stop (simple)",
            Type = TransportationType.Trolley,
            Workdays = 30,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,
            HeatConsumptionMW = 0,
            ParkingSpots = null,
            FuelStorageCapacity = null,
            PassengerCapacity = 20,
            ConstructionMaterials = new Dictionary<Resource, double>
            {
                { GameData.ConcreteResource, 3.7 },
                { GameData.AsphaltResource, 1.1 },
                { GameData.BricksResource, 0.8 },
                { GameData.SteelResource, 1.2 },
                { GameData.GravelResource, 0.8 },
                { GameData.BoardsResource, 0.4 }
            }
        },
        new TransportationBuilding
        {
            Name = "Trolleybus stop (medium)",
            Type = TransportationType.Trolley,
            Workdays = 90,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,
            HeatConsumptionMW = 0,
            ParkingSpots = null,
            FuelStorageCapacity = null,
            PassengerCapacity = 40,
            ConstructionMaterials = new Dictionary<Resource, double>
            {
                { GameData.ConcreteResource, 7.4 },
                { GameData.AsphaltResource, 2.2 },
                { GameData.BricksResource, 1.6 },
                { GameData.SteelResource, 2.5 },
                { GameData.GravelResource, 1.5 },
                { GameData.BoardsResource, 0.7 }
            }
        },
        new TransportationBuilding
        {
            Name = "Trolleybus stop (advanced)",
            Type = TransportationType.Trolley,
            Workdays = 184,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,
            HeatConsumptionMW = 0,
            ParkingSpots = null,
            FuelStorageCapacity = null,
            PassengerCapacity = 100,
            ConstructionMaterials = new Dictionary<Resource, double>
            {
                { GameData.ConcreteResource, 14.8 },
                { GameData.AsphaltResource, 4.4 },
                { GameData.BricksResource, 3.2 },
                { GameData.SteelResource, 5.0 },
                { GameData.GravelResource, 3.1 },
                { GameData.BoardsResource, 1.5 }
            }
        },

        // Trolleybus End Stations
        new TransportationBuilding
        {
            Name = "Trolleybus end station (2 parking)",
            Type = TransportationType.Station,
            Workdays = 286,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,
            HeatConsumptionMW = 0,
            ParkingSpots = 2,
            FuelStorageCapacity = null,
            PassengerCapacity = 100,
            ConstructionMaterials = new Dictionary<Resource, double>
            {
                { GameData.ConcreteResource, 18.5 },
                { GameData.AsphaltResource, 9.9 },
                { GameData.BricksResource, 3.2 },
                { GameData.SteelResource, 5.0 },
                { GameData.GravelResource, 6.9 },
                { GameData.BoardsResource, 1.5 }
            }
        },
        new TransportationBuilding
        {
            Name = "Trolleybus end station (5 parking)",
            Type = TransportationType.Station,
            Workdays = 495,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,
            HeatConsumptionMW = 0,
            ParkingSpots = 5,
            FuelStorageCapacity = null,
            PassengerCapacity = 100,
            ConstructionMaterials = new Dictionary<Resource, double>
            {
                { GameData.ConcreteResource, 31.5 },
                { GameData.AsphaltResource, 19.8 },
                { GameData.BricksResource, 3.2 },
                { GameData.SteelResource, 5.0 },
                { GameData.GravelResource, 13.8 },
                { GameData.BoardsResource, 1.5 }
            }
        },
        new TransportationBuilding
        {
            Name = "Trolleybus end station (8 parking)",
            Type = TransportationType.Station,
            Workdays = 703,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,
            HeatConsumptionMW = 0,
            ParkingSpots = 8,
            FuelStorageCapacity = null,
            PassengerCapacity = 100,
            ConstructionMaterials = new Dictionary<Resource, double>
            {
                { GameData.ConcreteResource, 44.4 },
                { GameData.AsphaltResource, 29.7 },
                { GameData.BricksResource, 3.2 },
                { GameData.SteelResource, 5.0 },
                { GameData.GravelResource, 20.6 },
                { GameData.BoardsResource, 1.5 }
            }
        },
        new TransportationBuilding
        {
            Name = "Trolleybus end station (15 parking)",
            Type = TransportationType.Station,
            Workdays = 1221,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,
            HeatConsumptionMW = 0,
            ParkingSpots = 15,
            FuelStorageCapacity = null,
            PassengerCapacity = 100,
            ConstructionMaterials = new Dictionary<Resource, double>
            {
                { GameData.ConcreteResource, 77.0 },
                { GameData.AsphaltResource, 49.6 },
                { GameData.BricksResource, 3.2 },
                { GameData.SteelResource, 5.0 },
                { GameData.GravelResource, 34.4 },
                { GameData.BoardsResource, 1.5 }
            }
        },

        // Tram Platforms
        new TransportationBuilding
        {
            Name = "Tram platform (simple)",
            Type = TransportationType.Tram,
            Workdays = 121,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,
            HeatConsumptionMW = 0,
            ParkingSpots = null,
            FuelStorageCapacity = null,
            PassengerCapacity = 60,
            ConstructionMaterials = new Dictionary<Resource, double>
            {
                { GameData.ConcreteResource, 7.4 },
                { GameData.AsphaltResource, 2.2 },
                { GameData.BricksResource, 1.6 },
                { GameData.SteelResource, 3.7 },
                { GameData.GravelResource, 1.5 },
                { GameData.BoardsResource, 0.7 }
            }
        },
        new TransportationBuilding
        {
            Name = "Tram platform (advanced)",
            Type = TransportationType.Tram,
            Workdays = 248,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,
            HeatConsumptionMW = 0,
            ParkingSpots = null,
            FuelStorageCapacity = null,
            PassengerCapacity = 150,
            ConstructionMaterials = new Dictionary<Resource, double>
            {
                { GameData.ConcreteResource, 14.8 },
                { GameData.AsphaltResource, 4.4 },
                { GameData.BricksResource, 3.2 },
                { GameData.SteelResource, 7.5 },
                { GameData.GravelResource, 3.1 },
                { GameData.BoardsResource, 1.5 }
            }
        },

        // Tram End Stations
        new TransportationBuilding
        {
            Name = "Tram end station (2 parking)",
            Type = TransportationType.Station,
            Workdays = 385,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,
            HeatConsumptionMW = 0,
            ParkingSpots = 2,
            FuelStorageCapacity = null,
            PassengerCapacity = 150,
            ConstructionMaterials = new Dictionary<Resource, double>
            {
                { GameData.ConcreteResource, 18.5 },
                { GameData.AsphaltResource, 9.9 },
                { GameData.BricksResource, 3.2 },
                { GameData.SteelResource, 7.5 },
                { GameData.GravelResource, 6.9 },
                { GameData.BoardsResource, 1.5 }
            }
        },
        new TransportationBuilding
        {
            Name = "Tram end station (5 parking)",
            Type = TransportationType.Station,
            Workdays = 666,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,
            HeatConsumptionMW = 0,
            ParkingSpots = 5,
            FuelStorageCapacity = null,
            PassengerCapacity = 150,
            ConstructionMaterials = new Dictionary<Resource, double>
            {
                { GameData.ConcreteResource, 31.5 },
                { GameData.AsphaltResource, 19.8 },
                { GameData.BricksResource, 3.2 },
                { GameData.SteelResource, 7.5 },
                { GameData.GravelResource, 13.8 },
                { GameData.BoardsResource, 1.5 }
            }
        },
        new TransportationBuilding
        {
            Name = "Tram end station (8 parking)",
            Type = TransportationType.Station,
            Workdays = 946,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,
            HeatConsumptionMW = 0,
            ParkingSpots = 8,
            FuelStorageCapacity = null,
            PassengerCapacity = 150,
            ConstructionMaterials = new Dictionary<Resource, double>
            {
                { GameData.ConcreteResource, 44.4 },
                { GameData.AsphaltResource, 29.7 },
                { GameData.BricksResource, 3.2 },
                { GameData.SteelResource, 7.5 },
                { GameData.GravelResource, 20.6 },
                { GameData.BoardsResource, 1.5 }
            }
        },
        new TransportationBuilding
        {
            Name = "Tram end station (15 parking)",
            Type = TransportationType.Station,
            Workdays = 1643,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,
            HeatConsumptionMW = 0,
            ParkingSpots = 15,
            FuelStorageCapacity = null,
            PassengerCapacity = 150,
            ConstructionMaterials = new Dictionary<Resource, double>
            {
                { GameData.ConcreteResource, 77.0 },
                { GameData.AsphaltResource, 49.6 },
                { GameData.BricksResource, 3.2 },
                { GameData.SteelResource, 7.5 },
                { GameData.GravelResource, 34.4 },
                { GameData.BoardsResource, 1.5 }
            }
        },

        // Gas Stations (Refueling)
        new TransportationBuilding
        {
            Name = "Gas station (30t fuel, 2 stations)",
            Type = TransportationType.Refueling,
            Workdays = 98,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,
            HeatConsumptionMW = 0,
            ParkingSpots = null,
            FuelStorageCapacity = 30,
            PassengerCapacity = null,
            ConstructionMaterials = new Dictionary<Resource, double>
            {
                { GameData.ConcreteResource, 7.4 },
                { GameData.AsphaltResource, 5.5 },
                { GameData.BricksResource, 0 },
                { GameData.SteelResource, 0.6 },
                { GameData.GravelResource, 3.8 },
                { GameData.BoardsResource, 0 }
            }
        },
        new TransportationBuilding
        {
            Name = "Gas station (60t fuel, 4 stations)",
            Type = TransportationType.Refueling,
            Workdays = 159,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,
            HeatConsumptionMW = 0,
            ParkingSpots = null,
            FuelStorageCapacity = 60,
            PassengerCapacity = null,
            ConstructionMaterials = new Dictionary<Resource, double>
            {
                { GameData.ConcreteResource, 14.8 },
                { GameData.AsphaltResource, 11.0 },
                { GameData.BricksResource, 0 },
                { GameData.SteelResource, 1.3 },
                { GameData.GravelResource, 7.6 },
                { GameData.BoardsResource, 0 }
            }
        },
        new TransportationBuilding
        {
            Name = "Gas station (120t fuel, 8 stations)",
            Type = TransportationType.Refueling,
            Workdays = 281,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,
            HeatConsumptionMW = 0,
            ParkingSpots = null,
            FuelStorageCapacity = 120,
            PassengerCapacity = null,
            ConstructionMaterials = new Dictionary<Resource, double>
            {
                { GameData.ConcreteResource, 29.6 },
                { GameData.AsphaltResource, 22.1 },
                { GameData.BricksResource, 0 },
                { GameData.SteelResource, 2.5 },
                { GameData.GravelResource, 15.3 },
                { GameData.BoardsResource, 0 }
            }
        },

        // Power Transformers (Trafos)
        new TransportationBuilding
        {
            Name = "Power transformer 1500V (small)",
            Type = TransportationType.Refueling,
            Workdays = 43,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,
            HeatConsumptionMW = 0,
            ParkingSpots = null,
            FuelStorageCapacity = null,
            PassengerCapacity = null,
            ConstructionMaterials = new Dictionary<Resource, double>
            {
                { GameData.ConcreteResource, 3.7 },
                { GameData.AsphaltResource, 0 },
                { GameData.BricksResource, 0 },
                { GameData.SteelResource, 1.9 },
                { GameData.GravelResource, 0 },
                { GameData.BoardsResource, 0 }
            }
        },
        new TransportationBuilding
        {
            Name = "Power transformer 1500V (large)",
            Type = TransportationType.Refueling,
            Workdays = 73,
            PowerConsumptionMWh = 3.0,
            WattageKW = 50,
            WaterConsumptionM3 = 0,
            HeatConsumptionMW = 0,
            ParkingSpots = null,
            FuelStorageCapacity = null,
            PassengerCapacity = null,
            ConstructionMaterials = new Dictionary<Resource, double>
            {
                { GameData.ConcreteResource, 7.4 },
                { GameData.AsphaltResource, 0 },
                { GameData.BricksResource, 0 },
                { GameData.SteelResource, 3.7 },
                { GameData.GravelResource, 0 },
                { GameData.BoardsResource, 0 }
            }
        }
    };

    // All Buildings & Resources
    public static List<Resource> AllResources { get; } = new List<Resource>()
    {
        ClothesResource,
        FabricResource,
        ChemicalsResource,
        CropsResource,
        CoalResource,
        CoalOreResource,
        OilResource,
        WoodResource, BoardsResource,
        GravelResource,
        QuarriedStoneResource,
        SteelResource, MechanicComponentsResource,
        IronResource,
        IronOreResource,
        RawBauxiteResource,
        SolidFertilizerResource,
        LiquidFertilizerResource,
        PowerResource,
        WaterResource,
        HeatResource,
        WasteWaterResource,
        NuclearWasteResource,
        FuelResource,
        FoodResource, MeatResource, AlcoholResource, ElectronicsResource,
        CementResource,
        ConcreteResource,
        BricksResource,
        AsphaltResource,
        PrefabPanelsResource,
        BitumenResource,
        BiologicalWasteResource,
    };
    public static List<ProductionBuilding> AllBuildings { get; } = new List<ProductionBuilding>()
    {
        FoodFactory, Distillery,
        ClothingFactory,
        FabricFactory,
        SmallChemicalPlant,
        BigChemicalPlant, CompostingPlant, SyntheticFertilizerFactory,
        SteelMiil, MechanicalComponentsFactory,
        IronProcessingPlant,
        CoalProcessingPlant,
        CoalMine,
        WoodcuttingPost, Sawmill,
        SmallGravelProcessingPlant,
        BigGravelProcessingPlant,
        Pumpjack,
        SmallGravelQuarry,
        BigGravelQuarry,
        BauxiteMine,
        SmallFarm,
        MediumFarm,
        LargeFarm,
        CreateSmallField(FertilizerType.None),
        CreateMediumField(FertilizerType.None),
        CreateLargeField(FertilizerType.None),
        CoalPowerPlant,
        GasPowerPlant,
        SingleReactorNuclearPowerPlant,
        TwinReactorNuclearPowerPlant,
        SmallWaterTreatment,
        BigWaterTreatment,
        SmallSewageTreatment,
        BigSewageTreatment,
        LiquidPumpingStation, OilLoadingUnloading, BigOilStorage, UndergroundPumpingStation, MediumOilStorage, SmallOilStorage,
        ConveyorEngineTransfer, ConveyorOverpass,
        SmallDistributionOffice, Warehouse,
        WaterLoadingUnloadingStation, BigWaterPumpingStation, SmallWaterPumpingStation,
        LargeCementPlant,
        MediumCementPlant,
        ConcretePlant,
        BrickFactory,
        AsphaltPlant,
        PrefabPanelsFactory,
        HeatExchanger, HeatingPlant, HeatPumpingStation, SmallHeatExchanger, SmallHeatingPlant, SmallHeatPumpingStation,
        AggregateStorage1000, AggregateStorage1950, AggregateStorage2000, AggregateStorage2500, AggregateStorage5000, AggregateStorage870,
        TrainAggregateLoading123m, TrainAggregateLoading123mLarge, TrainAggregateLoading23m, TrainAggregateLoading32m, TrainAggregateLoading98m, TruckAggregateLoadingBig, TruckAggregateLoadingSmall, TrainAggregateLoading100m,
        OpenStorageMedium, OpenStorageSmall250, OpenStorageSmall330,
    };
    public static List<ProductionBuilding> AllSupportBuildings
    {
        get
        {
            return AllBuildings.Where(b => b.IsSupportBuildings).ToList();
        }
    }
    public static List<AmenityBuilding> AllAmenityBuildings
    {
        get {
            return GroceryBuildings
                .Concat(CityServiceBuildings)
                .Concat(EmergencyBuildings)
                .Concat(CultureBuildings)
                .Concat(EducationBuildings)
                .Concat(SportsBuildings)
                .Concat(PubsAndBars)
                .Concat(CrimeJusticeBuildings)
                .Concat(Fountains)
                .ToList();
        }
    }
    public static List<ResidentialBuilding> AllResidentialBuildings { get; } = new List<ResidentialBuilding>()
        .Concat(SmallResidentialBuildings)
        .Concat(MediumResidentialBuildings)
        .Concat(LargeResidentialBuildings).ToList();
}