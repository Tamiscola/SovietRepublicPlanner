public class Resource
{
    public Resource(string name, int processingLevel, bool isImportable, bool isUtility) 
    {
        this.Name = name;
        this.ProcessingLevel = processingLevel;
        this.IsImportable = isImportable;
        this.IsUtility = isUtility;
    }
    public string Name { get; set; }
    public int ProcessingLevel { get; set; }
    public bool IsImportable { get; set; }
    public bool IsUtility { get; set; } // for water, electricity, etc
    public double PerCapitalConsumption { get; set; } = 0;  // t/citizen/day
    public bool IsConsumable { get; set; } = false;

    // Infrastructure Requirements
    public bool RequiresLiquidInfrastructure { get; set; } = false;
    public bool RequiresBulkHandling { get; set; } = false;
    public bool RequiresDryBulkHandling { get; set; } = false;
    public bool RequiresSolidHandling { get; set; } = false;
    public bool RequiresGeneralDistribution { get; set; } = false;
    public bool RequiresElectricalInfrastructure { get; set; } = false;
    public bool RequiresWaterInfrastructure { get; set; } = false;
    public bool RequiresSewageInfrastructure { get; set; } = false;
    public bool RequiresHeatInfrastructure { get; set; } = false;

    // Helper Methods
    public override bool Equals(object? obj)
    {
        if (obj is Resource other)
        {
            return this.Name == other.Name; // Compare by name, not memory address
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();  // Same name = same hash code
    }
}