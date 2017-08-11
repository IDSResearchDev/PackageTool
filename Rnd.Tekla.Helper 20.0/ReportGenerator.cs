using Tekla.Structures.Model.Operations;

namespace Rnd.TeklaStructure.Helper
{
    public class ReportGenerator
    {
        public void ShowReportGenerator()
        {
            Operation.RunMacro(@"..\modeling\PackageTool Multi-Report Generator.cs");
        }
    }
}
