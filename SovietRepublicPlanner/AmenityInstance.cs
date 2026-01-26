class AmenityInstance
{
    public AmenityBuilding Building;
    public int Count;
    public int TotalWorkers => Building.EffectiveWorkersPerShift * 3 * Count;
    public int TotalCapacity => Building.CitizenCapacity * Count;
}