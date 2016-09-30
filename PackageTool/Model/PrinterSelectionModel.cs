using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageTool.Model
{
    [Serializable()]
    public class PrinterSelectionModel
    {
        public string PaperSize { get; set; }
        public string PrinterInstance { get; set; }
    }
}
