using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class MicroDistrict
{
    public void Display()
    {
        Console.WriteLine("\n┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓");
        Console.WriteLine("┃         MICRODISTRICT PLAN             ┃");
        Console.WriteLine("┞━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┦");
        // Display Status section
        Console.WriteLine("├────────────────────────────────────────┤");
        Console.WriteLine($"│ District Name: {Name}                      ");
        Console.WriteLine("├────────────────────────────────────────┤");
        Console.WriteLine("│ Utilities Status:                      │");
        Console.WriteLine("├────────────────────────────────────────┤");
        Console.WriteLine("│                   Needed    ");
        Console.WriteLine($"│ Capacity   :    {TotalHousingCapacity,6}     ");
        Console.WriteLine($"│ Power (MW) :    {PowerConsumption,6:F2}     ");
        Console.WriteLine($"│ Water (m³) :    {WaterConsumption,6:F2}     ");
        Console.WriteLine($"│ Sewage (m³):    {SewageProduction,6:F2}     ");
        Console.WriteLine($"│ Heat (MW)  :    {HeatConsumption,6:F2}     ");
        Console.WriteLine($"│ Garbage    :    {GarbageProduction,6:F6} t/day              ");
        Console.WriteLine("├────────────────────────────────────────┤");
        Console.WriteLine("│ Citizen Consumption:                   │");
        Console.WriteLine("├────────────────────────────────────────┤");
        Console.WriteLine("│                   Needed");
        foreach (var kv in CitizenConsumption)
        {
            Console.WriteLine($"│ {kv.Key.Name, 11}    :    {kv.Value,6}");
        }
        if (SupportBuildings.Count() > 0)
        {
            Console.WriteLine("├────────────────────────────────────────┤");
            Console.WriteLine("│ Support Infrastructures:               │");
            Console.WriteLine("├────────────────────────────────────────┤");

            var supportByCategory = SupportBuildings
                .GroupBy(i => i.Building.SupportCategory)
                .OrderBy(g => (int)g.Key);

            foreach (var supGroup in supportByCategory)
            {
                string categoryName = City.GetSupCategoryDisplayName(supGroup.Key);
                Console.WriteLine($"│ [ {categoryName} ]");

                foreach (var kv in supGroup.OrderBy(x => x.Building.Name))
                    Console.WriteLine($"│ · {kv.Count} × {kv.Building.Name}");
                Console.WriteLine("│");
            }
        }
        if (ResidentialBuildings.Count > 0)
        {
            Console.WriteLine("├────────────────────────────────────────┤");
            Console.WriteLine("│ Residential Buildings:                 ");
            Console.WriteLine("├────────────────────────────────────────┤");

            foreach (var ins in ResidentialBuildings)
            {
                Console.WriteLine($"│ · {ins.Count} × {ins.Building.Name}");
            }
        }
    }
}
