public class AmenityInstance
{
    public AmenityBuilding Building;
    public int Count;
    public int CurNumEmp;
    public int TotalWorkers => Building.CurNumEmp * 3 * Count;
    public int TotalCapacity => Building.MaxCoverage * Count;
}