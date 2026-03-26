using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SavedCity
{
    public string Name { get; set; }
    public List<SavedIndustryPlan> IndustryPlans { get; set; }
    public List<MicroDistrict> ResidentialArea { get; set; }
}

