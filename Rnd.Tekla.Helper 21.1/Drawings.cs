using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Reflection;
using Rnd.TeklaStructure.Helper.Enums;
using Tekla.Structures;
using Tekla.Structures.Catalogs;
using Tekla.Structures.Drawing;
using Tekla.Structures.Drawing.UI;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
using ModelObject = Tekla.Structures.Model.ModelObject;
using ModelObjectSelector = Tekla.Structures.Model.UI.ModelObjectSelector;
using System.Threading;

namespace Rnd.TeklaStructure.Helper
{
    public class Drawings
    {

        private string _outputdir;

        private string _modelPath;

        private DrawingHandler _drawingHandler;

        private ExportType _exportType;

        private DrawingType _drawingType;

        //private string _dwgmacro = @"C:\TeklaStructures\21.0\Environments\usimp\macros\drawings\";

        //private string _dxfmacro = @"C:\TeklaStructures\21.0\Environments\usimp\us_roles\steel\macros\modeling\";

        private string _plotFilesDirectory
        {
            get
            {
                string flotfiledir = null;
                TeklaStructuresSettings.GetAdvancedOption("XS_DRAWING_PLOT_FILE_DIRECTORY", ref flotfiledir);

                if (flotfiledir.StartsWith(@".\"))
                {
                    flotfiledir = flotfiledir.Remove(0, 2);
                    flotfiledir = Path.Combine(_modelPath, flotfiledir);
                }
                else if (string.IsNullOrEmpty(flotfiledir))
                {
                    flotfiledir = _modelPath;
                }

                return flotfiledir;
            }
        }

        private string _filenameformat
        {
            get
            {
                string xs = "";
                string format = "";
                switch (_drawingType)
                {
                    case DrawingType.A:
                        xs = "XS_DRAWING_PLOT_FILE_NAME_A";
                        break;
                    case DrawingType.C:
                        xs = "XS_DRAWING_PLOT_FILE_NAME_C";
                        break;
                    case DrawingType.G:
                        xs = "XS_DRAWING_PLOT_FILE_NAME_G";
                        break;
                    case DrawingType.M:
                        xs = "XS_DRAWING_PLOT_FILE_NAME_M";
                        break;
                    case DrawingType.W:
                        xs = "XS_DRAWING_PLOT_FILE_NAME_W";
                        break;
                }

                TeklaStructuresSettings.GetAdvancedOption(xs, ref format);

                return format;
            }
        }

        public DrawingSelector DrawingSelector
        {
            get { return _drawingHandler.GetDrawingSelector(); }
        }

        public List<object> SelectedDrawing
        {

            get
            {
                List<object> drw = new List<object>();
                foreach (Drawing drawing in DrawingSelector.GetSelected())
                {

                    drw.Add(drawing);

                }

                return drw;
            }

        }

        public Drawings()
        {
            _drawingHandler = new DrawingHandler();
            var model = new Model();
            _modelPath = model.GetInfo().ModelPath;
        }

        public void ExportDrawings(string outputdir, ExportType exportType, List<object> drawings)
        {
            _exportType = exportType;
            _outputdir = outputdir;

            if (_drawingHandler.GetConnectionStatus())
            {
                ConvertDrawing(drawings);
            }
        }

        public string GetMark(object drawing)
        {
            var drw = (Drawing)drawing;
            return drw.Mark.Replace("[", "").Replace("]", "");
        }

        public string GetName(object drawing)
        {
            var drw = (Drawing)drawing;
            return drw.Name;
        }


        public string Type(object drawing)
        {
            string result = "";
            GetReportProperty((Drawing)drawing, "DRAWING.TYPE", ref result);
            return result;
        }

        
        public string GetFilename(object drawing)
        {
            var drw = (Drawing)drawing;
            _drawingType = GetDrawingType(type: Type(drw));
            var format = _filenameformat.Replace(@"%%", ",").Replace(".", "");
            string[] x = format.Split(',');
            

            //string DRAWING_MARK = "DRAWING_MARK";

            string DRAWING_NAME = "DRAWING_NAME";

            string REVISION_MARK = "REVISION_MARK";

            string DRAWING_REVISION = "DRAWING_REVISION?";

            string DRAWING_TITLE = "DRAWING_TITLE";

            string ASSEMBLY_SERIAL_NUMBER = "TPL:ASSEMBLYASSEMBLY_SERIAL_NUMBER";


            for (int i = 0; i < x.Count(); i++)
            {
                if (x[i].Contains(DRAWING_NAME))
                    x[i] =
                        x[i].Replace(DRAWING_NAME, drw.Mark)
                            .Replace("-", "")
                            .Replace(".", "")
                            .Replace("[", "")
                            .Replace("]", "")
                            .Replace(DRAWING_TITLE, drw.Name)
                            .Replace("%", "")
                            .Trim();

                else if (x[i].Contains(ASSEMBLY_SERIAL_NUMBER))
                    x[i] = x[i].Replace(ASSEMBLY_SERIAL_NUMBER, drw.Mark.Where(digit => char.IsDigit(digit)).ToArray().ToString());

                else if (x[i].Contains(REVISION_MARK))
                    x[i] = x[i].Replace(REVISION_MARK, RevisionMark(drawing));

                else if (x[i].Contains(DRAWING_REVISION))
                    x[i] = x[i].Replace(DRAWING_REVISION, "");

                else if (x[i].Contains(DRAWING_TITLE))
                    x[i] = x[i].Replace(DRAWING_TITLE, drw.Name);
            }


            if (string.IsNullOrEmpty(RevisionMark(drawing)))
                return string.Join("", x).Replace("%", string.Empty).Replace("-", "").Replace("Rev", "").Trim();
            return string.Join("", x).Replace("%", string.Empty);

        }

        public string GetFilenamewithoutRevision(object drawing)
        {
            var drw = (Drawing)drawing;
            _drawingType = GetDrawingType(Type(drw));
            var format = _filenameformat.Replace(@"%%", ",").Replace(".", "")
                                        .Replace("REVISION_MARK",string.Empty)
                                        .Replace("DRAWING_REVISION?", string.Empty);

            string[] x = format.Split(',');
            
            string DRAWING_NAME = "DRAWING_NAME";

            string DRAWING_TITLE = "DRAWING_TITLE";

            string ASSEMBLY_SERIAL_NUMBER = "TPL:ASSEMBLYASSEMBLY_SERIAL_NUMBER";

            var str = new string (drw.Mark.Where(digit => char.IsDigit(digit)).ToArray());

            for (int i = 0; i < x.Count(); i++)
            {
                if (x[i].Contains(DRAWING_NAME))
                    x[i] =
                        x[i].Replace(DRAWING_NAME, drw.Mark)
                            .Replace("-", "")
                            .Replace(".", "")
                            .Replace("[", "")
                            .Replace("]", "")
                            .Replace(DRAWING_TITLE, drw.Name)
                            .Replace("%", "")
                            .Trim();

                else if (x[i].Contains(ASSEMBLY_SERIAL_NUMBER))
                    x[i] = x[i].Replace(ASSEMBLY_SERIAL_NUMBER, str);
                
                else if (x[i].Contains(DRAWING_TITLE))
                    x[i] = x[i].Replace(DRAWING_TITLE, drw.Name);
            }

            
            return string.Join("", x).Replace("%", string.Empty).Replace("_", string.Empty);
            
        }



        //public string Size(Drawing drawing)
        public string Size(object drawing)
        {
            string result = "";
            GetReportProperty((Drawing)drawing, "SIZE", ref result);

            string[] x = result.ToUpper().Split('X');

            //double num = (Convert.ToDouble(x[0]) / 25.4) * 16.0;
            //double num2 = (Convert.ToDouble(x[1]) / 25.4) * 16.0;

            

            //x[0] = (Math.Round(num, MidpointRounding.AwayFromZero) / 16.0).ConvertDecimaltoFraction();
            //x[1] = (Math.Round(num2, MidpointRounding.AwayFromZero) / 16.0).ConvertDecimaltoFraction();

            ////x[0] = (Math.Round(Convert.ToDouble(x[0]) / 25.4, 1, MidpointRounding.AwayFromZero)).ToString();
            ////x[1] = (Math.Round(Convert.ToDouble(x[1]) / 25.4, 1, MidpointRounding.AwayFromZero)).ToString();


            double num = Convert.ToDouble(x[0]);
            double num2 = Convert.ToDouble(x[1]);
            x[0] = num.ConvertDecimaltoFraction();
            x[1] = num2.ConvertDecimaltoFraction();


            
            return string.Join("*", x);
        }
        
     

        private string DrawingName(Drawing drawing)
        {
            string result = "";
            GetReportProperty(drawing, "NAME", ref result);
            return result;
        }

        private string Title(Drawing drawing)
        {
            string result = "";
            GetReportProperty(drawing, "DRAWING.TITLE", ref result);
            return result;
        }

        public string RevisionMark(object drawing)
        {
            string result = "";
            GetReportProperty((Drawing)drawing, "DRAWING.REVISION.MARK", ref result);
            return result;
        }

        private string RevisionNumber(Drawing drawing)
        {
            string result = "";
            GetReportProperty(drawing, "DRAWING.REVISION.NUMBER#1", ref result);
            return result;
        }


        private string Type(Drawing drawing)
        {
            string result = "";
            GetReportProperty(drawing, "DRAWING.TYPE", ref result);
            return result;
        }


        private DrawingType GetDrawingType(string type)
        {
            return (DrawingType)Enum.Parse(typeof(DrawingType), type);
        }


        public ModelObject GetDrawingModelObject(Drawing drawing)
        {
            Identifier identifier =
                (Identifier)
                    drawing.GetType()
                        .GetProperty("Identifier", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(drawing, null);
            return new Beam { Identifier = identifier };
        }

        private void GetReportProperty(Drawing drawing, string param, ref string result)
        {
            ModelObject modelObject = GetDrawingModelObject(drawing);
            modelObject.GetReportProperty(param, ref result);
        }


        private void GetReportProperty(Drawing drawing, string param, ref int result)
        {
            var typeDrawing = (AssemblyDrawing)drawing;
            var modelPart = new Model().SelectModelObject(typeDrawing.AssemblyIdentifier);
            modelPart.GetReportProperty(param, ref result);
        }

        private void DeleteFiles()
        {
            var directory = new DirectoryInfo(_plotFilesDirectory);
            foreach (FileInfo file in directory.GetFiles()) file.Delete();
        }

        public void ConvertDrawing(List<object> drawings)
        {
            string str2 = string.Format("PackagingTool_ExportTo{0}.cs", _exportType.ToString());
            string path = string.Empty, destFileName = string.Empty, mark = string.Empty;
            DeleteFiles();

            if (Operation.RunMacro(@"..\drawings\" + str2))
            {
                foreach (string file in Directory.GetFiles(_plotFilesDirectory))
                {

                    path = Path.Combine(_plotFilesDirectory, file);
                    mark = Path.GetFileNameWithoutExtension(file);
                    destFileName = Path.Combine(_outputdir, DrawingName(drawings, mark) + GetExtensionForDrawingType(_exportType));
                    File.Copy(path, destFileName, true);
                }
            }

        }



        private string DrawingName(List<object> selecteddrawing, string mark)
        {
            foreach (Drawing drw in selecteddrawing)
            {
                if (drw.Name.Contains(mark))
                {
                    _drawingType = GetDrawingType(Type(drw));
                    return GetFilename(drw);
                }
            }
            return "";
        }


        public string GetExtensionForDrawingType(ExportType drawingType)
        {
            switch (drawingType)
            {
                case ExportType.DXF:
                    return ".dxf";

                case ExportType.DWG:
                    return ".dwg";
            }
            return ".pdf";
        }
       
        //private string _dxfmacro = @"C:\TeklaStructures\21.0\Environments\usimp\us_roles\steel\macros\modeling\";
        public void ConvertDXF()
        {
            Operation.RunMacro(@"..\modeling\PackageTool DXFConverter.cs");
        }


        public void ExportPDF(string outputdir, string printerinstance, bool autoScaling, string scaleValue, object drawing = null)
        {
            _exportType = ExportType.PDF;
            _outputdir = outputdir;

            var _scaleValue = 1.00;
            Double.TryParse(scaleValue, out _scaleValue);

            if (_drawingHandler.GetConnectionStatus())
            {
                ConvertPDF((Drawing)drawing, printerinstance, autoScaling, _scaleValue);
            }

        }

        public void ConvertPDF(Drawing drawing, string printerinstance, bool autoScaling, double scaleValue)
        {
            CatalogHandler CatalogHandler = new CatalogHandler();

            if (CatalogHandler.GetConnectionStatus())
            {
                PrintAttributes printAttributes = new PrintAttributes();
                printAttributes.PrinterInstance = string.IsNullOrEmpty(printerinstance) ? Size(drawing) : printerinstance;
                printAttributes.ScalingType = autoScaling ? DotPrintScalingType.Auto : DotPrintScalingType.Scale;
                printAttributes.Scale = scaleValue;

                _drawingHandler.PrintDrawing(drawing, printAttributes);
            }
        }

        public void ExportDWG(string outputdir)
        {
            _exportType = ExportType.DWG;
            _outputdir = outputdir;

            if (_drawingHandler.GetConnectionStatus())
            {
                ConvertDWG();
            }

        }

        public void ConvertDWG()
        {
            string destination = string.Empty, source = string.Empty;
            DeleteFiles();

            if (Operation.RunMacro(@"..\modeling\PackageTool DWGConverter.cs"))
            {
                foreach (string file in Directory.GetFiles(_plotFilesDirectory))
                {
                    source = Path.Combine(_plotFilesDirectory, file);
                    destination = Path.Combine(_outputdir, Path.GetFileNameWithoutExtension(file + ".DWG"));
                    File.Copy(source, destination, true);
                }
            }

        }

        public void CreateNCFiles(string dxfdir, bool IsAngles, bool IsPlates, bool IsProfiles)
        {
            ModelObjectSelector MOS = new ModelObjectSelector();

            bool hasselected = false;
            foreach (var obj in MOS.GetSelectedObjects())
            {
                hasselected = true;
                break;
            }

            if (!hasselected) throw new Exception("Please select object.");

            if (IsAngles)
                Operation.CreateNCFilesFromSelected("DSTV for angles", dxfdir + "\\Angles\\");
            if (IsPlates)
                Operation.CreateNCFilesFromSelected("DSTV for plates", dxfdir + "\\Plates\\");
            if (IsProfiles)
                Operation.CreateNCFilesFromSelected("DSTV for profiles", dxfdir +"\\");
        }

        public void CreateKssFiles(string filename)
        {

            if (string.IsNullOrWhiteSpace(filename) || string.IsNullOrEmpty(filename))
                filename = "MIS_List";
            
            ModelObjectSelector modObjSelector = new ModelObjectSelector(); 

            bool isSelected = false;

            foreach (var item in modObjSelector.GetSelectedObjects())
            {
                isSelected = true;
                break;
            }

            if (!isSelected) throw new Exception("Please select object.");
            Operation.CreateMISFileFromSelected(Operation.MISExportTypeEnum.KISS, filename);
        }

        #region KSS File Modification

        public void CreateKssFileFromTemplate(string kissReportTemplate, string kissOutputFilePath)
        {
            if (string.IsNullOrWhiteSpace(kissOutputFilePath) || string.IsNullOrEmpty(kissOutputFilePath))
                kissOutputFilePath = kissReportTemplate;

            string modelPath = _modelPath;
            if (!kissOutputFilePath.Contains(".kss"))
                kissOutputFilePath = kissOutputFilePath + ".kss";

            kissOutputFilePath = Path.Combine(modelPath, kissOutputFilePath);

            FileInfo outputInfo = new FileInfo(Path.Combine(modelPath, "KISS1_1_Export.mdd"));
            FileInfo importTemplate = new FileInfo(Path.Combine(modelPath, "KISS1_1_Export.mdd.rpt"));
            if (CreateImportInformation(importTemplate, outputInfo) && ImportMultiDrawingData(outputInfo))
            {
                if (outputInfo.Exists)
                {
                    outputInfo.Delete();
                }
                FileInfo info3 = new FileInfo(kissOutputFilePath);
                Operation.CreateReportFromSelected(kissReportTemplate, kissOutputFilePath, string.Empty, string.Empty, string.Empty);
                
                int num = 0;
                while (!info3.Exists && (num < 120))
                {
                    num++;
                    Thread.Sleep(500);
                }
            }
        }

        private bool CreateImportInformation(FileInfo importTemplate, FileInfo outputInfo)
        {
            try
            {
                FileStream stream = importTemplate.Create();
                stream.Write(Rnd.TeklaStructure.Helper.Properties.Resources.KISS1_1_Export_mdd, 0, Rnd.TeklaStructure.Helper.Properties.Resources.KISS1_1_Export_mdd.Length);
                stream.Flush();
                stream.Close();
                if (Operation.CreateReportFromAll("KISS1_1_Export.mdd", outputInfo.FullName, string.Empty, string.Empty, string.Empty))
                {
                    int num = 0;
                    while (!outputInfo.Exists && (num < 100))
                    {
                        num++;
                        Thread.Sleep(500);
                    }
                }
                if (importTemplate.Exists)
                {
                    importTemplate.Delete();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message + exception.StackTrace);
            }
            return outputInfo.Exists;
        }

        private bool ImportMultiDrawingData(FileInfo importFileInfo)
        {
            ClearMultiDrawingUdas();
            StreamReader reader = importFileInfo.OpenText();
            KissMultiDrawingInfo info = null;
            List<Tuple<KissMultiDrawingInfo, List<int>>> list = new List<Tuple<KissMultiDrawingInfo, List<int>>>();
            List<int> list2 = new List<int>();
            while (!reader.EndOfStream)
            {
                string str = reader.ReadLine();
                if (str != null)
                {
                    string[] strArray = str.Split(new char[] { ',' }, StringSplitOptions.None);
                    if ((strArray[0].Trim() != "*") && (strArray.Length > 1))
                    {
                        if (strArray[0].Trim() == "M")
                        {
                            if (info != null)
                            {
                                list.Add(new Tuple<KissMultiDrawingInfo, List<int>>(info, list2));
                                list2 = new List<int>();
                            }
                            info = new KissMultiDrawingInfo
                            {
                                Name = strArray[1].Trim(new char[] { ' ', '[', ']' }),
                                Revision = strArray[2].Trim(),
                                Title = strArray[3].Trim(),
                                ModifiedDate = strArray[4].Trim(),
                                DrawnBy = strArray[5].Trim()
                            };
                        }
                        else
                        {
                            int num;
                            if ((strArray[0].Trim() == "A") && int.TryParse(strArray[1].Trim(), out num))
                            {
                                list2.Add(num);
                            }
                        }
                    }
                }
            }
            if (info != null)
            {
                list.Add(new Tuple<KissMultiDrawingInfo, List<int>>(info, list2));
            }
            reader.Close();
            Beam beam = new Beam();
            foreach (Tuple<KissMultiDrawingInfo, List<int>> tuple in list)
            {
                foreach (int num in tuple.Item2)
                {
                    try
                    {
                        beam.Identifier = new Identifier(num);
                        beam.SetUserProperty("MDWG_NAME", tuple.Item1.Name);
                        beam.SetUserProperty("MDWG_REVISION_LAST_MARK", tuple.Item1.Revision);
                        beam.SetUserProperty("MDWG_TITLE", tuple.Item1.Title);
                        beam.SetUserProperty("MDWG_DATE_MODIFY", tuple.Item1.ModifiedDate);
                        beam.SetUserProperty("MDWG_DRAWN_BY", tuple.Item1.DrawnBy);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception.Message + exception.StackTrace);
                        throw;
                    }
                }
            }
            return true;
        }

        private void ClearMultiDrawingUdas()
        {
            ModelObjectEnumerator allObjectsWithType = new Tekla.Structures.Model.Model().GetModelObjectSelector().GetAllObjectsWithType(ModelObject.ModelObjectEnum.ASSEMBLY);
            allObjectsWithType.SelectInstances = false;
            while (allObjectsWithType.MoveNext())
            {
                allObjectsWithType.Current.SetUserProperty("MDWG_NAME", string.Empty);
                allObjectsWithType.Current.SetUserProperty("MDWG_REVISION_LAST_MARK", string.Empty);
                allObjectsWithType.Current.SetUserProperty("MDWG_TITLE", string.Empty);
                allObjectsWithType.Current.SetUserProperty("MDWG_DATE_MODIFY", string.Empty);
                allObjectsWithType.Current.SetUserProperty("MDWG_DRAWN_BY", string.Empty);
            }
        }

        #endregion

        #region Unused methods
        public void Export(string outputdir, ExportType exportType, string printerinstance, object drawing = null)
        {
            _exportType = exportType;
            _outputdir = outputdir;

            if (_drawingHandler.GetConnectionStatus())
            {
                switch (exportType)
                {
                    case ExportType.PDF:
                        ConvertPDF((Drawing)drawing, printerinstance);
                        break;
                    case ExportType.DWG:
                        ConvertDWG();
                        break;
                }
            }

        }        

        public void ConvertPDF(Drawing drawing, string printerinstance)
        {
            CatalogHandler CatalogHandler = new CatalogHandler();
            //string instance = string.IsNullOrEmpty(printerinstance) ? Size(drawing) : printerinstance;
            //var instances = new Utilities().PrinterInstance();

            //if (!instances.Contains(instance))
            //    throw new Exception("Create a " + instance + " first in Printer Instances.");

            if (CatalogHandler.GetConnectionStatus())
            {
                PrintAttributes printAttributes = new PrintAttributes();
                printAttributes.PrinterInstance = string.IsNullOrEmpty(printerinstance) ? Size(drawing) : printerinstance;
                printAttributes.ScalingType = DotPrintScalingType.Scale;

                _drawingHandler.PrintDrawing(drawing, printAttributes);
            }
        }
        #endregion
    }
}
