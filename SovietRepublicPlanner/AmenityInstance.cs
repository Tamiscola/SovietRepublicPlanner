public class AmenityInstance
{
    public AmenityBuilding Building;
    public AmenityType Type => Building.Type;
    public string Name {  get; set; }
    public int Count;
    public int CurNumEmp {  get; set; }
    public int CurCoverage => Type switch
    {
        AmenityType.Education => EducationLevel switch
        {
            EducationSubtype.Kindergarten => (int)(CurCustomCapacity * 15),
            EducationSubtype.School => (int)(CurCustomCapacity * 20),
            EducationSubtype.University => (int)(CurCustomCapacity * 20),
            EducationSubtype.UniversityDorm => CurCustomCapacity,  // Direct housing
            _ => CurCustomCapacity  // Fallback: no multiplier
        },
        AmenityType.Shopping => (int)(CurCustomCapacity * 15),
        AmenityType.Pub => (int)(CurCustomCapacity * 100),
        AmenityType.Healthcare => (int)(CurCustomCapacity * 100),
        AmenityType.Culture => (int)(CurCustomCapacity * 80),
        AmenityType.Sports => (int)(CurCustomCapacity * 80),
        AmenityType.CrimeJustice => (int)(CurCustomCapacity * 100),
        AmenityType.Fireservice => 0,
        AmenityType.CityService => 0,
        AmenityType.Fountain => 0,
        _ => (int)(CurCustomCapacity * 30)  // Fallback
    };
    public int CurCustomCapacity => (int)Math.Round(CurNumEmp * Building.CustomersPerWorker) > Building.MaxVisitors
        ? Building.MaxVisitors
        : (int)Math.Round(CurNumEmp * Building.CustomersPerWorker);
    public int TotalWorkers => CurNumEmp * 3 * Count;
    public int TotalMaxCoverage => Building.MaxCoverage * Count;
    public int TotalCurCoverage => CurCoverage * Count;

    public EducationSubtype EducationLevel { get; set; } = EducationSubtype.None;
}