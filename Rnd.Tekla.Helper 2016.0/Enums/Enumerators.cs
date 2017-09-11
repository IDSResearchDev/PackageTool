using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rnd.TeklaStructure.Helper.Enums
{
    public enum ExportType
    {
        PDF,
        DXF,
        DWG
    }

    public enum DrawingType
    {
        A,
        C,
        G,
        M,
        W
    }

    public enum BoltType
    {
        Site /*= 1*/,
        WorkShop /*= 2*/,
    }
}
