using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class UtilityInstance
{
    public UtilityBuilding Building { get; set; }
    public string Name { get; set; }
    public int CurNumEmp { get; set; }
    public double CurProductivity => (CurNumEmp * Building.ProdPerWorker) > 100
    ? 100
    : CurNumEmp * Building.ProdPerWorker;
    public int Count { get; set; } = 0;
}
