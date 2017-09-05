using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Rnd.Common.Resources;
using Tekla.Structures;
using Tekla.Structures.Catalogs;
using Tekla.Structures.Model;
using Tekla.Structures.Dialog.UIControls;
using System.Linq;

namespace Rnd.TeklaStructure.Helper
{
    public class Utilities
    {
        string _targetVersion;
        public Utilities() { }

        public Utilities(string targetVersion)
        {
            _targetVersion = targetVersion;
        }
        //private string _dwgMacroFile = @"\macros\modeling\PackageTool DWGConverter.cs";//@"C:\TeklaStructures\21.0\Environments\usimp\macros\drawings\PackageTool DWGConverter.cs";

        //private string _dxfMacroFile = @"\macros\modeling\PackageTool DXFConverter.cs";//@"C:\TeklaStructures\21.0\Environments\usimp\us_roles\steel\macros\modeling\PackageTool DXFConverter.cs";

        //private string _multiReportGenerator = @"\macros\modeling\PackageTool Multi-Report Generator.cs";//@"C:\TeklaStructures\21.0\Environments\usimp\macros\modeling\PackageTool Multi-Report Generator.cs";
        /// <summary>
        ///  Open a new instance of Tekla Structure with initialization 
        /// </summary>
        /// <param name="modelfolder">Model folder directory</param>
        /// <param name="configuration">Current selected configuration</param>
        public void OpenTekla(string modelfolder, string configuration)
        {
            string root = GetTeklaroot(modelfolder).Trim();
            string version = GetVersion(modelfolder).Trim();
            string LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            UpdateBypass(configuration);
            string bypass = Path.Combine(LocalAppData, "ModelLauncher", "Bypass.ini");
            string environment = string.Format(@"{0}\{1}\Environments\USimp\env_US_imperial.ini", root, version);
            string role = string.Format(@"{0}\{1}\Environments\USimp\Role_Steel_Detailing.ini", root, version);

            string arguments = string.Format(@"""{0}""  -I   ""{1}"" -i ""{2}"" -i ""{3}"" ", modelfolder, bypass, environment, role);
            string tekla = string.Format(@"  ""{0}\{1}\nt\bin\TeklaStructures.exe""  ", root, version);

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = tekla;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = arguments;
            Process.Start(startInfo);
        }

        private void UpdateBypass(string configuration)
        {
            var value = "set XS_DEFAULT_LICENSE=";
            string LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            //string path = Assembly.GetExecutingAssembly().Location;
            var strreplace = configuration.Replace(' ', '_');

            var tempfile = Path.GetTempFileName();
            using (var writer = new StreamWriter(tempfile))
            using (var reader = new StreamReader(Path.Combine(LocalAppData, "ModelLauncher", "Bypass.ini")))
            {
                while (!reader.EndOfStream)
                {
                    var readLine = reader.ReadLine();
                    if (readLine != null)
                    {
                        writer.WriteLine((!readLine.Contains(value) ? readLine : value + strreplace));
                    }
                }
            }
            File.Copy(tempfile, Path.Combine(LocalAppData, "ModelLauncher", "Bypass.ini"), true);
        }

        /// <summary>
        /// Provides information Tekla version.
        /// </summary>
        /// <param name="path">Model folder directory</param>
        /// <returns>Version</returns>
        public string GetVersion(string path)
        {
            var utils = new Common.Utilities();
            var value = utils.GetSingleXElementXml(path + "\\TeklaStructuresModel.xml", "Version");
            var splitstr = value.Split(' ');
            return (value != string.Empty) ? splitstr[0] : value;
        }

        /// <summary>
        ///  Provides information on Tekla root directory.
        /// </summary>
        /// <param name="path">Model folder directory</param>
        /// <returns>Tekla Installation directory</returns>
        public string GetTeklaroot(string path)
        {
            var utils = new Common.Utilities();
            var value = utils.GetSingleXElementXml(path + "\\TeklaStructuresModel.xml", "XS_SYSTEM");
            var splitstr = value.Split('\\');

            return (value != string.Empty) ? splitstr[0] + "\\" + splitstr[1] : value;
        }


        public string GetAdvancedOption(string attribute)
        {
            //string xs = "XS_FIRM";
            string val = "";
            GetConncectionStatus();
            TeklaStructuresSettings.GetAdvancedOption(attribute, ref val);
            return val;
        }
        

        public string ModelFolder()
        {
            var model = new Model();
            GetConncectionStatus();
            return model.GetInfo().ModelPath;
        }


        public List<string> PrinterInstance()
        {
            List<string> printerinstance = new List<string>();

            CatalogHandler CatalogHandler = new CatalogHandler();

            if (CatalogHandler.GetConnectionStatus())
            {
                PrinterItemEnumerator PrinterItemEnumerator = CatalogHandler.GetPrinterItems();

                while (PrinterItemEnumerator.MoveNext())
                {
                    PrinterItem printerItem = PrinterItemEnumerator.Current as PrinterItem;

                    printerinstance.Add(printerItem.Name);
                }
            }

            return printerinstance;
        }

        public void GetConncectionStatus()
        {
            var proc = Process.GetProcessesByName("TeklaStructures");
            
            if (proc.Length <= 0) { throw new ArgumentException(ErrorCollection.TeklaNotRunning); }
            else
            {
                var teklaVersion = Process.GetProcessById(proc[0].Id).MainModule.FileVersionInfo.ProductVersion;
                Version v1 = new Version(teklaVersion);
                Version v2 = new Version(_targetVersion);
                if(v1 != v2)
                {
                    throw new ArgumentException(ErrorCollection.RemoteConnectionFailed);
                }
                
            }
        }

        public void CheckSelectedDrawing()
        {
            Drawings drawings = new Drawings();
            List<object> selecteddrawing = drawings.SelectedDrawing;
            int drawingcount = selecteddrawing.Count;

            if (drawingcount <= 0) throw new ArgumentException("Please select a drawing.");

        }
        public void SetDXFOutputDir(string localpath, string dxfOutputDir)
        {
            string fullPath = Path.Combine(localpath, "DXFOutputdir.ini");
            string value = "OUTPUT_FILE_DIR=" + Path.Combine(dxfOutputDir, "DXF");
            new Rnd.Common.Utilities().CreateFileWithText(fullPath, value);

        }

        public void CopyMacrosToFirm(string firmLocation)
        {
            string destination = firmLocation + @"\macros\modeling\";
            CopyMacros(destination);            

            ////CreateDXFMacroFile
            //File.Copy(AppDomain.CurrentDomain.BaseDirectory + @"macro\PackageTool DXFConverter.cs", path + this._dxfMacroFile, true);
            ////CreateDWGMacroFile
            //File.Copy(AppDomain.CurrentDomain.BaseDirectory + @"macro\PackageTool DWGConverter.cs", path + this._dwgMacroFile, true);
            ////CreateMultiReportGenerator
            //File.Copy(AppDomain.CurrentDomain.BaseDirectory + @"macro\PackageTool Multi-Report Generator.cs", path + this._multiReportGenerator, true);
        }

        public void CopyMacroToMacroDirectory()
        {
            /// --- copy to Current Macro location
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var macroDirs = this.GetAdvancedOption("XS_MACRO_DIRECTORY");
            var dirs = macroDirs.Split(';');

            foreach (var dir in dirs)
            {
                if (Directory.Exists(dir + @"modeling\"))
                {
                    CopyMacros(dir + @"modeling\");
                }
            }
        }

        private void CopyMacros(string destination)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var util = new Common.Utilities();

            /// --- copy macros to Firm Location
            util.CopyFilesToLocation(baseDir + @"macro\", destination, "*");

            /// --- copy toolbar macro to Firm location
            util.CopyFilesToLocation(baseDir + @"macrotoolbar\", destination, "*");
        }

        #region KSS File Modification
        public List<string> GetKSSTemplates()
        {
            List<string> fileDirectories = new List<string>();
            List<string> kssFiles = new List<string>(); //;

            GetConncectionStatus();
            var path = new Tekla.Structures.Model.Model().GetInfo().ModelPath;

            if (!string.IsNullOrEmpty(path))
            {
                //AddPaths(fileDirectories, "XS_TEMPLATE_DIRECTORY");
                fileDirectories.Add(path);
                AddPaths(fileDirectories, "XS_PROJECT");
                //AddPaths(fileDirectories, "XS_FIRM");
                //AddPaths(fileDirectories, "XS_TEMPLATE_DIRECTORY_SYSTEM");
                //AddPaths(fileDirectories, "XS_SYSTEM");
                string[] strArray = GetMultiDirectoryList(fileDirectories, "kss.rpt");
                foreach (string str in strArray)
                {
                    kssFiles.Add(str + ".kss");
                }
            }

            return kssFiles;
        }

        public string[] GetMultiDirectoryList(List<string> searchDirectories, string extension)
        {
            return EnvironmentFiles.GetMultiDirectoryFileList(searchDirectories, extension).ToArray();
        }

        public void AddPaths(List<string> fileDirectories, string environmentVariableName)
        {
            char[] separator = new char[] { ';' };
            string[] strArray = EnvironmentVariables.GetEnvironmentVariable(environmentVariableName).Split(separator);
            foreach (string str in strArray)
            {
                string directory = str.Replace(@"\\\\", @"\\").Replace(@"\\\\", @"\\");
                if (IsValidDirectory(directory))
                {
                    fileDirectories.Add(directory);
                }
            }
        }

        private bool IsValidDirectory(string directory)
        {
            if (string.IsNullOrEmpty(directory))
            {
                return false;
            }
            return Directory.Exists(directory);
        }

        #endregion
    }
}
