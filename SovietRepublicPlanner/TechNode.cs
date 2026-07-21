using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TechNode
{
    public string Name { get; set; }
    public int UnlockYear { get; set; }
    public int ResearchDays { get; set; }
    public List<string> Prerequisites { get; set; } = new List<string>();
    public List<string> UnlocksBuildings { get; set; } = new List<string>();
}
