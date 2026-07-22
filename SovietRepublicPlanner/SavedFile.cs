public class SavedFile
{
    public string Name { get; set; }
    public List<SavedCity> Cities { get; set; }
    public int CurrentYear { get; set; }
    public HashSet<string> UnlockedTech { get; set; }
}