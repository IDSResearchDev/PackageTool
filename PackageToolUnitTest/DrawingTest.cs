using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rnd.TeklaStructure.Helper;
using Rnd.TeklaStructure.Helper.Enums;


namespace PackageToolUnitTest
{
    [TestClass]
    public class DrawingTest
    {
        [TestMethod]
        public void DrawingList()
        {
            //Drawings drawings = new Drawings(ExportType.PDF, @"C:\Users\pc\Desktop\package");
            ////drawings.ExportDrawings();

            //drawings = new Drawings(ExportType.DWG, @"C:\Users\pc\Desktop\package");
            ////drawings.ExportDrawings();

            //drawings = new Drawings(ExportType.DXF, @"C:\Users\pc\Desktop\package");
            ////drawings.ExportDrawings();

            Assert.IsFalse(false);
        }

        [TestMethod]
        public void NCFIles()
        {
            //Drawings drawings = new Drawings();
            //drawings.CreateNCFiles(@"C:\Users\pc\Desktop\package\NC\");
        }
    }
}

