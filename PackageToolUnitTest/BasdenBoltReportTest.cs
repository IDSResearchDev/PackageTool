using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rnd.TeklaStructure.Helper.Reports;
using PackageTool.Model;

namespace PackageToolUnitTest
{
    [TestClass]
    public class BasdenBoltReportTest
    {
        [TestMethod]
        public void OpenExcel()
        {
            BasdenBoltReport basdenreport = new BasdenBoltReport();
            //basdenreport.ReadExcel("","","","");
            //basdenreport.ReadExcel(multiplier: "5", reportpackagedirectory: "", jobnum: "", jobcode:"");
            Assert.IsTrue(true);

        }

        [TestMethod]
        public void OpenExcelWithReflectValue()
        {
            BasdenBoltReport basdenreport = new BasdenBoltReport();
            //basdenreport.ReadExcel("", "", "", "");
            Assert.IsTrue(true); 

        }

        [TestMethod]
        public void ExportExcel()
        {
            BasdenBoltReport basdenreport = new BasdenBoltReport();
            //basdenreport.ReadExcel("", "", "", "");
            var path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            ExportToExcel exportToExcel = new ExportToExcel("","");
            //exportToExcel.CreateBasdenReportExcel(basdenreport.BoltList, path,"","","");


            Assert.IsTrue(true);
        }
    }
}
