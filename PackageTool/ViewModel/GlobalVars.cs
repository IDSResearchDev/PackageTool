using PackageTool.Model;
using PackageTool.View;
using Common = Rnd.Common;
using Rnd.Common.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using Helper = Rnd.TeklaStructure.Helper;

namespace PackageTool.ViewModel
{
    public static class GlobalVars
    {
        public static string AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
        public static volatile bool SuspendProcess = false;
        public static string FirmFolder;
        public static string ModelFolder;
        public static string FirmReportDir;
        public static string ModelReportTemplateDir;
        public static string FirmReportTemplateDir;
        
        public static Common.Utilities CommonUtilities = new Common.Utilities();
        public static MainView MainWindow = App.Current.MainWindow as MainView;
        public static CfgDialog cfgDialog;
        public static CfgModel cfgModel;
        public static UpdateConfigurationModel UpdateConfigModel;
        public static string PackageDirectory = @"";
        public static string OutputDirectory = @"";
        public static string LocalAppPackageToolFolder = Path.Combine(new Common.Utilities().LocalAppData, StringResource.PackageTool);
        public static string LocalCfgBinFile = Path.Combine(LocalAppPackageToolFolder, "currentcfg.bin");
        public static string LocalPrinterInstanceBinFile = Path.Combine(LocalAppPackageToolFolder, "printerinstance.bin");
        public static string LocalUpdateConfigurationFile = Path.Combine(LocalAppPackageToolFolder, "updater.bin");
        public static string LocalUpdaterFile = Path.Combine(LocalAppPackageToolFolder, "updater.ini");
        public static string ReportPackageDirectory = "";


        public static string Fabricator;
        public static string JobCode;
        public static string JobNumber;
        public static string TransmittalNumber;
        public static string ProjectNumber;
        public static string Project;
        public static string Location;
        public static string Attention;
        public static string SendingSelection;
        public static string Remarks;
        public static string Signature;
        public static string FileTypes;
        public static string Purpose;
        public static string LastPageDetails;
        public static string PrinterInstance;
        public static bool ApplyPrinterInstance;
        public static string Title1;
        public static string KssTemplateFilename;

        public static string TransmittalName;
        public static string OutputTransmittalLetter;

        public static ObservableCollection<TransmittalData> TransmittalDatas;

        public static ObservableCollection<PrinterSelectionModel> PrinterSelection;

        public static ObservableCollection<XsrReports> XsrReportList;

        public static void GetXsrReports()
        {
            XsrReportList = new ObservableCollection<XsrReports>();
            using (var reader = new StreamReader(Path.Combine(LocalAppPackageToolFolder, "reports.txt")))
            {
                while (!reader.EndOfStream)
                {
                    var readLine = reader.ReadLine();

                    var newreadLine = "";
                    var splitstr = readLine.Split(' ');
                    var firstvalue = splitstr[0];
                    int num;


                    if (int.TryParse(firstvalue, out num))
                    {
                        var removestr = string.Concat(splitstr[0], "   ");
                        newreadLine = readLine.Replace(removestr, "");
                    }
                    else
                    {
                        newreadLine = readLine;
                    }

                    XsrReportList.Add(new XsrReports()
                    {
                        ReportName = newreadLine
                    });

                }
            }
        }


        public static string KssName { get; set; }
        public static bool UseKssTemplate;
    }
}
