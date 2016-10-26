using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using PackageTool.BaseClass;
using PackageTool.Model;
using PackageTool.View;
using Rnd.TeklaStructure.Helper;
using Rnd.TeklaStructure.Helper.Enums;
using Utilities = Rnd.Common.Utilities;
using TeklaUtilities = Rnd.TeklaStructure.Helper.Utilities;
using System.Runtime.Remoting.Messaging;
using System.Windows.Controls;
using Microsoft.Office.Interop.Excel;
using Rnd.TeklaStructure.Helper.Reports;
using Action = System.Action;
using Drawings = Rnd.TeklaStructure.Helper.Drawings;

namespace PackageTool.ViewModel
{
    public delegate void ExportDelegate();
    public class ProgressDialogViewModel : ViewModelBase
    {
        private List<string> _dirList = new List<string>();
        private TeklaUtilities _teklaUtilities;
        readonly Utilities _commonUtilities;

        public ProgressDialogViewModel()
        {
            ShowControls(false);
            _commonUtilities = new Utilities();
            _teklaUtilities = new TeklaUtilities();
            Start();
        }

        #region Properties
        private bool _isIndeterminate;

        public bool IsIndeterminate
        {
            get { return _isIndeterminate; }
            set
            {
                _isIndeterminate = value;
                OnPropertyChanged("IsIndeterminate");
            }
        }
        private Visibility _visibility;
        public Visibility Visibility
        {
            get { return _visibility; }
            set
            {
                if (_visibility == value) return;
                _visibility = value;
                OnPropertyChanged("Visibility");
            }
        }

        private bool _isPreviewTransmittal;
        public bool IsPreviewTransmittal
        {
            get
            {
                return _isPreviewTransmittal;
            }
            set
            {
                _isPreviewTransmittal = value;
                OnPropertyChanged("IsPreviewTransmittal");
            }
        }

        private bool _isPackageFolder;
        public bool IsPackageFolder
        {
            get
            {
                return _isPackageFolder;
            }
            set
            {
                _isPackageFolder = value;
                OnPropertyChanged("IsPackageFolder");
            }
        }
        private int _currentProgress;
        public int CurrentProgress
        {
            get { return _currentProgress; }
            set
            {
                //if (_currentProgress == value) return;
                _currentProgress = value;
                IsIndeterminate = _currentProgress == 0;
                OnPropertyChanged("CurrentProgress");
            }
        }
        private string _newpkgdir;

        private string _pdfdir
        {
            get
            {
                var dir = Path.Combine(GlobalVars.PackageDirectory, "PDF");

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                return dir;
            }
        }

        private string _dwgdir
        {
            get
            {
                var dir = Path.Combine(GlobalVars.PackageDirectory, "DWG");

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                return dir;
            }
        }

        private string _dxfdir
        {
            get
            {
                var dir = Path.Combine(GlobalVars.PackageDirectory, "DXF");

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                return dir;
            }
        }

        private string _ncdir
        {
            get
            {
                var dir = Path.Combine(GlobalVars.PackageDirectory, "NC");

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                if (!Directory.Exists(dir + "\\Angles"))
                    Directory.CreateDirectory(dir + "\\Angles");

                if (!Directory.Exists(dir + "\\Plates"))
                    Directory.CreateDirectory(dir + "\\Plates");


                return dir;
            }
        }

        private string TeklaReportOutputDirectory
        {
            get
            {
                // transfer to tekla helper
                string dir = "";
                dir = _teklaUtilities.GetAdvancedOption("XS_REPORT_OUTPUT_DIRECTORY");
                if (dir.Equals(@".\Reports")) dir = Path.Combine(GlobalVars.ModelFolder, "Reports");
                return dir;
            }
        }

        private bool _isEnableOk;
        public bool IsEnableOk
        {
            get { return _isEnableOk; }
            set
            {
                if (_isEnableOk == value) return;
                _isEnableOk = value;
                OnPropertyChanged("IsEnableOk");
            }
        }

        private bool _isEnableClose;
        public bool IsEnableClose
        {
            get { return _isEnableClose; }
            set
            {
                if (_isEnableClose == value) return;
                _isEnableClose = value;
                OnPropertyChanged("IsEnableClose");
            }
        }

        private string _lblwaitcontent;
        public string LblWaitContent
        {
            get { return _lblwaitcontent; }
            set
            {
                if (_lblwaitcontent == value) return;
                _lblwaitcontent = value;
                OnPropertyChanged("LblWaitContent");
            }
        }

        private string _lblDetailsContent;

        public string LblDetailsContent
        {
            get { return _lblDetailsContent; }
            set
            {
                if (_lblDetailsContent == value) return;
                _lblDetailsContent = value;
                OnPropertyChanged("LblDetailsContent");
            }
        }
        #endregion

        #region Commands
        public ICommand CloseDialog
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    Close();
                });
            }
        }
        public ICommand CreatePackageOK
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (IsPreviewTransmittal)
                    {
                        ShowReportViewer();
                    }

                    if (IsPackageFolder)
                    {

                        PackageFolder();
                    }

                    Close();
                });
            }
        }

        #endregion

        #region Functions
        public void Start()
        {
            ExportDelegate action = new ExportDelegate(Export);
            //IAsyncResult async = action.BeginInvoke(new AsyncCallback(ExportCallBack), null);
            action.BeginInvoke(ExportCallBack, null);
        }

        private void ExportCallBack(IAsyncResult async)
        {
            AsyncResult ar = (AsyncResult)async;
            ExportDelegate del = (ExportDelegate)ar.AsyncDelegate;
            bool hasError = false;
            string errorMessage = string.Empty;

            try
            {
                del.EndInvoke(async);
            }
            catch (Exception ex)
            {
                hasError = true;
                errorMessage = ex.Message;
            }
            finally
            {
                MoveFilesToNewPkgDirectory();
                if (!hasError)
                {
                    //MoveFilesToNewPkgDirectory();
                    ShowControls(true);
                }
                else
                {
                    if (errorMessage.Equals("Failed to read from an IPC Port: The pipe has been ended.\r\n"))
                    {
                        GlobalVars.MainWindow.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            MessageBox.Show(this.GetCurrentWindow(), "Error details: " + errorMessage + "\nPlease restart the Tekla Structures. This application", "Tekla Structures unexpectedly terminated", MessageBoxButton.OK, MessageBoxImage.Error);

                            GlobalVars.MainWindow.Close();
                        }));
                    }
                    else if (errorMessage.Equals("Print operation failed because of unknown reason!"))
                    {
                        GlobalVars.MainWindow.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            //throw new Exception(string.Concat("Error details: ", errorMessage, "\nPlease restart the Tekla Structures. This application", "Tekla Structures unexpectedly terminated"));
                            MessageBox.Show(this.GetCurrentWindow(), "Error details: " + errorMessage + "\nUnable to create package. Please check if model is modified.", "Tekla Structures unexpectedly terminated", MessageBoxButton.OK, MessageBoxImage.Error);
                            this.GetCurrentWindow().Close();
                        }));
                    }
                    else
                    {
                        GlobalVars.MainWindow.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            MessageBox.Show(this.GetCurrentWindow(), "Error details: " + errorMessage + "\nPlease re-run the application or re-create the package", "Error occur while exporting files", MessageBoxButton.OK, MessageBoxImage.Error);
                            this.GetCurrentWindow().Close();
                        }));
                    }
                }


            }
        }

        public void Export()
        {
            try
            {
                CurrentProgress = 0;
                ValidatePrinterInstance();


                var date = DateTime.Now.ToShortDateString().Trim().Replace("/", "");
                var time = DateTime.Now.ToLongTimeString().Trim().Replace(":", ".");
                //var oldpkgdir = GlobalVars.PackageDirectory;
                _newpkgdir = GlobalVars.PackageDirectory + "\\" + GlobalVars.JobCode + "_" + date + "_" + time;
                GlobalVars.ReportPackageDirectory = Path.Combine(_newpkgdir, "Reports");
                //GlobalVars.PackageDirectory = newpkgdir;
                //Directory.CreateDirectory(_newpkgdir);

                Drawings drawings = new Drawings();
                List<object> selecteddrawing = drawings.SelectedDrawing;
                int drawingcount = selecteddrawing.Count;
                var drwtype = "";


                GlobalVars.TransmittalDatas = new ObservableCollection<TransmittalData>();
                ExportToExcel exportExcel = new ExportToExcel(GlobalVars.JobNumber, GlobalVars.JobCode);
                int i;


                if (GlobalVars.cfgModel.PDF)
                {
                    CurrentProgress = 0;

                    LblDetailsContent = "Exporting PDF files...";
                    i = 0;
                    foreach (var drw in selecteddrawing)
                    {
                        i++;
                        var printerinstance = GlobalVars.ApplyPrinterInstance ? GlobalVars.PrinterInstance : string.Empty;


                        var autoScaling = GlobalVars.AutoScaling;
                        var scaleValue = GlobalVars.ScaleValue;

                        var size = drawings.Size(drw);

                        if (printerinstance.Equals(string.Empty))
                        {
                            this.GetPrinterSelectionInstance(size, ref printerinstance);
                        }

                        if (!GlobalVars.ApplyScaling)
                        {
                            PrinterSelectionModel scaling = null;
                            this.GetPrinterSelectionInstance(size, ref scaling);
                            autoScaling = !scaling.ManualScaling;
                            scaleValue = scaling.ScaleValue.Trim();
                        }
                        
                        var filenamewithoutrevision = drawings.GetFilenamewithoutRevision(drw);

                        //drawings.Export(_pdfdir, ExportType.PDF, printerinstance, drw);
                        drawings.ExportPDF(_pdfdir, printerinstance, autoScaling, scaleValue, drw);

                        drwtype = drawings.Type(drw);
                        

                        GlobalVars.TransmittalDatas.Add(new TransmittalData()
                        {

                            SheetName = filenamewithoutrevision,
                            Revision = drawings.RevisionMark(drw),
                            Type = ExportType.PDF.ToString()
                        });

                        CurrentProgress = Convert.ToInt32(((double)i / drawingcount) * 100);
                    }
                    CurrentProgress = 100;
                }

                //export dwg using macro
                if (GlobalVars.cfgModel.DWG)
                {
                    CurrentProgress = 0;
                    i = 0;
                    LblDetailsContent = "Exporting DWG files...";

                    drawings.ExportDWG(_dwgdir);

                    foreach (var drw in selecteddrawing)
                    {
                        i++;
                        drwtype = drawings.Type(drw);
                        var filenamewithoutrevision = drawings.GetFilenamewithoutRevision(drw);
                        GlobalVars.TransmittalDatas.Add(new TransmittalData()
                        {
                            SheetName = filenamewithoutrevision,
                            Revision = drawings.RevisionMark(drw),
                            Type = ExportType.DWG.ToString()
                        });

                        CurrentProgress = Convert.ToInt32(((double)i / drawingcount) * 100);
                    }
                    CurrentProgress = 100;
                }

                //try
                //{
                if (GlobalVars.cfgModel.NC)
                {
                    CurrentProgress = 0;
                    LblDetailsContent = "Exporting NC files...";

                    bool IsAngles = false;
                    bool IsPlates = false;
                    bool IsProfiles = false;

                    foreach (var item in GlobalVars.cfgModel.NCItems)
                    {
                        if (item.Name.Equals("DSTV for Angles"))
                            IsAngles = item.IsChecked;
                        if (item.Name.Equals("DSTV for Plates"))
                            IsPlates = item.IsChecked;
                        if (item.Name.Equals("DSTV for Profiles"))
                            IsProfiles = item.IsChecked;

                    }

                    drawings.CreateNCFiles(_ncdir, IsAngles, IsPlates, IsProfiles);

                    var path = Path.Combine(GlobalVars.PackageDirectory, "NC");
                    NcDxfToList(path, "NC", "nc1");

                    CurrentProgress = 100;
                }

                //}
                //catch (Exception err)
                //{

                //    MessageBox.Show(err.Message);
                //}

                if (GlobalVars.cfgModel.DXF)
                {
                    CurrentProgress = 0;
                    LblDetailsContent = "Exporting DXF files...";
                    GlobalVars.MainWindow.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var diagresult =
                        MessageBox.Show(this.GetCurrentWindow(), "You're about to export DXF files. Please specify the NC Files on the next dialog to continue.",
                                    "DXF Export", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (diagresult == MessageBoxResult.OK)
                        {
                            var x = _dxfdir;
                            drawings.ConvertDXF();
                        }

                        //GlobalVars.SuspendProcess = false;
                        SuspendProcess(false);
                    }));
                    SuspendProcess(true);

                    var path = Path.Combine(GlobalVars.PackageDirectory, "DXF");
                    NcDxfToList(path, "DXF", "dxf");

                    CurrentProgress = 100;
                }

                if (GlobalVars.cfgModel.KSS)
                {
                    CurrentProgress = 0;
                    LblDetailsContent = "Exporting KSS file...";

                    if (!GlobalVars.UseKssTemplate)
                    {
                        drawings.CreateKssFiles(GlobalVars.KssName);
                    }
                    else
                    {
                        //kss template
                        drawings.CreateKssFileFromTemplate(GlobalVars.KssTemplateFilename, GlobalVars.KssName);
                    }

                    KssFile();
                    CurrentProgress = 100;
                }




                if (GlobalVars.cfgModel.FABTROL || GlobalVars.cfgModel.BOLTLIST || GlobalVars.cfgModel.XSR)
                {
                    CurrentProgress = 0;
                    ReportGenerator rptgenGenerator = new ReportGenerator();
                    LblDetailsContent = "Creating XSR/BoltList/Fabtrol";
                    rptgenGenerator.ShowReportGenerator();
                    LblDetailsContent = "Copying files to output directory...";


                    XsrReportFiles();//moving xsr reports

                    foreach (var rpt in GlobalVars.XsrReportList)
                    {
                        if (rpt.ReportName == "BASDEN_Excel_Bolt_Summary.xls" || rpt.ReportName == "BASDEN_Excel_Field_Stud_List.xls"
                            || rpt.ReportName == "BASDEN_Excel_Field_Bolt_List.xls" || rpt.ReportName == "BASDEN_Excel_Shop_Applied_List.xls"
                            || rpt.ReportName == "BASDEN_Excel_Shop_Bolt_List.xls")
                        {
                            BasdenBoltReport basdenreport = new BasdenBoltReport();
                            basdenreport.ReadExcel(rpt.ReportName, GlobalVars.Title1, GlobalVars.ReportPackageDirectory, GlobalVars.JobNumber, GlobalVars.JobCode);


                            exportExcel.CreateBasdenReportExcel(rpt.ReportName, basdenreport.BoltList, Path.Combine(GlobalVars.PackageDirectory, _newpkgdir, "Reports"), basdenreport.Project, GlobalVars.Title1);
                        }
                    }



                    CurrentProgress = 100;
                }


                CurrentProgress = 0;
                LblDetailsContent = "Generating transmittal reports...";
                //ExportToExcel exportExcel = new ExportToExcel(GlobalVars.JobNumber, GlobalVars.JobCode);


                exportExcel.ReadXls(date, time);
                CurrentProgress = 100;



            }
            catch (Exception ex)
            {
                throw ex;

            }
        }

        //private string BasdenRptName(string rptname)
        //{
        //    var rname = rptname.Replace(".xls","");
        //    var rname2 = rname.Replace("BASDEN_Excel_"," ");
        //    return rname2.Replace("_", " ").ToUpper();  
        //}

        private bool GetPrinterSelectionInstance(string size, ref string printerinstance)
        {
            if (GlobalVars.PrinterSelection != null)
            {
                printerinstance = (from x in GlobalVars.PrinterSelection
                                   where x.PaperSize.Equals(size)
                                   select x.PrinterInstance).FirstOrDefault();
            }

            return !String.IsNullOrEmpty(printerinstance);
        }

        private bool GetPrinterSelectionInstance(string size, ref PrinterSelectionModel printerinstance)
        {
            if (GlobalVars.PrinterSelection != null)
            {
                printerinstance = (from x in GlobalVars.PrinterSelection
                                   where x.PaperSize.Equals(size)
                                   select x).FirstOrDefault();
            }

            return printerinstance != null;
        }

        private void MoveFilesToNewPkgDirectory()
        {

            Thread.Sleep(5000);

            var source = GlobalVars.PackageDirectory;
            var target = _newpkgdir;
            var directories = Directory.GetDirectories(GlobalVars.PackageDirectory);

            Directory.CreateDirectory(_newpkgdir);
            foreach (var directory in directories)
            {
                try
                {
                    var dirsplit = directory.Split('\\');

                    if (directory.Equals(Path.Combine(source, "PDF"))
                        || directory.Equals(Path.Combine(source, "DWG"))
                        || directory.Equals(Path.Combine(source, "DXF"))
                        || directory.Equals(Path.Combine(source, "KSS"))
                        || directory.Equals(Path.Combine(source, "NC"))
                        || directory.Equals(Path.Combine(source, "Reports"))
                      ) Directory.Move(directory, Path.Combine(target, dirsplit[dirsplit.Count() - 1]));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Please close any open documents related to package. Cannot merge files into folder." + Environment.NewLine + ex.Message, "File Open.", MessageBoxButton.OK, MessageBoxImage.Error);
                    Process.Start(GlobalVars.PackageDirectory);
                }
            }

        }

        private void KssFile()
        {
            if (!_commonUtilities.CheckIfDirectoryExists(Path.Combine(GlobalVars.PackageDirectory, "KSS")))
            {
                _commonUtilities.CreateDirectory(Path.Combine(GlobalVars.PackageDirectory, _newpkgdir, "KSS"));
            }
            MoveKssFiletoPackageDirectory();


        }

        private void MoveKssFiletoPackageDirectory()
        {
            var teklaUtilities = new Rnd.TeklaStructure.Helper.Utilities();
            var modelpath = teklaUtilities.ModelFolder();

            var kssFiles = Directory.GetFiles(modelpath, "*.kss", SearchOption.TopDirectoryOnly);

            foreach (var file in kssFiles)
            {
                var kssname = file.Split('\\');


                File.Move(file, Path.Combine(GlobalVars.PackageDirectory, _newpkgdir, "KSS", kssname[kssname.Length - 1]));



                //GlobalVars.TransmittalDatas.Add(new TransmittalData()
                //{
                //    SheetName = kssname[kssname.Length-1],
                //    Revision = "",
                //    Type = "KSS"
                //});

                GlobalVars.KssName = kssname[kssname.Length - 1];
            }

        }

        private void XsrReportFiles()
        {
            if (!_commonUtilities.CheckIfDirectoryExists(Path.Combine(GlobalVars.PackageDirectory, "Reports")))
            {
                _commonUtilities.CreateDirectory(Path.Combine(GlobalVars.PackageDirectory, _newpkgdir, "Reports"));
            }
            MoveXsrFilesToPackageDirectory();
        }

        private void MoveXsrFilesToPackageDirectory()
        {
            var path = System.IO.Path.Combine(GlobalVars.LocalAppPackageToolFolder, "reports.txt");
            var str = string.Concat(GlobalVars.JobNumber, "_", GlobalVars.JobCode, "_");
            using (var reader = new StreamReader(path))
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


                    if (_commonUtilities.CheckIfFileExists(Path.Combine(TeklaReportOutputDirectory, string.Concat(readLine, ".xsr"))))
                        File.Move(Path.Combine(TeklaReportOutputDirectory, string.Concat(readLine, ".xsr")), Path.Combine(GlobalVars.PackageDirectory, _newpkgdir, "Reports", string.Concat(str, newreadLine/*readLine*/, ".xsr")));
                    // then add to HasReports Lists

                    if (_commonUtilities.CheckIfFileExists(Path.Combine(TeklaReportOutputDirectory, string.Concat(readLine, ".csv"))))
                        File.Move(Path.Combine(TeklaReportOutputDirectory, string.Concat(readLine, ".csv")), Path.Combine(GlobalVars.PackageDirectory, _newpkgdir, "Reports", string.Concat(str, newreadLine/*readLine*/, ".csv")));
                    // then add to HasReports Lists

                    if (readLine != null && _commonUtilities.CheckIfFileExists(Path.Combine(TeklaReportOutputDirectory, readLine)))
                        File.Move(Path.Combine(TeklaReportOutputDirectory, readLine), Path.Combine(GlobalVars.PackageDirectory, _newpkgdir, "Reports", string.Concat(str, newreadLine)));

                }
            }

        }

        private void NcDxfToList(string path, string type, string extension)
        {
            if (GlobalVars.CommonUtilities.CheckIfDirectoryExists(path))
            {
                GetDirectories(path, type, extension);
            }
        }

        private void GetDirectories(string dir, string type, string extension)
        {
            var directory = Directory.GetDirectories(dir);

            var files = GlobalVars.CommonUtilities.GetFiles(dir, extension);
            foreach (var fileInfo in files)
            {
                var filename = Path.GetFileNameWithoutExtension(fileInfo.Name);
                var filename_rev = filename.Split('_');
                var name = string.Empty;
                var revision = string.Empty;

                if (filename_rev.Count() >= 2)
                {
                    name = filename_rev[0];
                    revision = filename_rev[1];
                }
                else
                {
                    name = filename;
                }

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrEmpty(name)) return;

                GlobalVars.TransmittalDatas.Add(new TransmittalData()
                {
                    SheetName = name,
                    Type = type,
                    Revision = revision
                });
            }


            foreach (var items in directory)
            {
                //LoadDir(items, type, extension);
                GetDirectories(items, type, extension);
            }
        }

        private void LoadDir(string dir, string type, string extension)
        {
            var subdirectories = Directory.GetDirectories(dir);
            var files = GlobalVars.CommonUtilities.GetFiles(dir, extension);
            foreach (var fileInfo in files)
            {
                var filename = Path.GetFileNameWithoutExtension(fileInfo.Name);
                var filename_rev = filename.Split('_');
                var name = string.Empty;
                var revision = string.Empty;

                if (filename_rev.Count() >= 2)
                {
                    name = filename_rev[0];
                    revision = filename_rev[1];
                }
                else
                {
                    name = filename;
                }

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrEmpty(name)) return;

                GlobalVars.TransmittalDatas.Add(new TransmittalData()
                {
                    SheetName = name,
                    Type = type,
                    Revision = revision
                });
            }
            foreach (var subdirectory in subdirectories)
            {
                LoadDir(subdirectory, type, extension);
            }
        }

        private void ShowLogFile(int errorCount)
        {
            if (errorCount > 0)
            {
                var diagresult = MessageBox.Show(errorCount.ToString() + " error encountered while inserting data.\nDo you want to view the log file?", "Error",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (diagresult == MessageBoxResult.Yes)
                {
                    Process.Start(Path.Combine(GlobalVars.LocalAppPackageToolFolder, DateTime.Now.Date.ToString("yyyy.MM.d") + ".log"));
                }
            }
        }

        private void ShowControls(bool value)
        {
            IsEnableOk = value;
            IsEnableClose = value;
            if (value)
            {
                LblWaitContent = "Package Tool Completed";
                LblDetailsContent = "It's done.";
                Visibility = Visibility.Visible;
            }
            else
            {
                LblWaitContent = "Please Wait...";
                LblDetailsContent = "Exporting files...";
                Visibility = Visibility.Hidden;
            }
        }

        private void ShowReportViewer()
        {
            Process.Start(GlobalVars.OutputTransmittalLetter);
        }

        private void PackageFolder()
        {
            Process.Start(GlobalVars.PackageDirectory);
        }

        public bool CheckPrinterInstanceExists(ref List<string> printerInstances)
        {
            bool isExist = true;
            Drawings drawings = new Drawings();
            List<object> selecteddrawing = drawings.SelectedDrawing;
            int drawingcount = selecteddrawing.Count;
            var instances = new Rnd.TeklaStructure.Helper.Utilities().PrinterInstance();
            foreach (var drw in selecteddrawing)
            {
                string instance = drawings.Size(drw);
                if (!instances.Contains(instance))
                {
                    isExist = false;
                    if (!printerInstances.Contains(instance))
                        printerInstances.Add(instance);
                }
            }
            return isExist;
        }

        private void ValidatePrinterInstance()
        {
            if (GlobalVars.cfgModel.PDF)
            {
                LblDetailsContent = "Checking Printer Instances...";
                List<string> instances = new List<string>();
                if (!CheckPrinterInstanceExists(ref instances))
                {

                    bool terminateProcess = false;
                    GlobalVars.MainWindow.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        PrinterInstanceSelectionView view = new PrinterInstanceSelectionView();
                        view.vm.IsViewing = false;
                        foreach (var size in instances)
                        {
                            string printerinstance = string.Empty;
                            if (GlobalVars.PrinterSelection == null)
                                GlobalVars.PrinterSelection = new ObservableCollection<PrinterSelectionModel>();
                            /// Check if size exist in PrinterSelection Collection
                            if (!GetPrinterSelectionInstance(size, ref printerinstance))
                            {
                                /// add size to PrinterSelection Collection if does not exist
                                view.vm.PrinterSelection.Add
                                (
                                   new PrinterSelectionModel
                                   {
                                       PaperSize = size,
                                       ManualScaling = false,
                                       ScaleValue = "1.00"
                                   }
                                );
                            }
                        }

                        if (view.vm.PrinterSelection.Count <= 0)
                        {
                            //GlobalVars.SuspendProcess = false;
                            SuspendProcess(false);
                        }
                        else
                        {
                            // if any of the sizes does not exist in PrinterSelection Collection
                            view.Owner = this.GetCurrentWindow();
                            view.ShowDialog();
                            // flag to terminate process if printer setting does not save
                            terminateProcess = !view.vm.IsSaving;
                        }
                    }));


                    /// Suspends the Working thread
                    SuspendProcess(true);

                    if (terminateProcess)
                    { throw new ArgumentException("Cannot proceed packaging without setting Printer Instance Setting."); }

                }
            }
        }

        private void SuspendProcess(bool value)
        {
            GlobalVars.SuspendProcess = value;
            while (GlobalVars.SuspendProcess)
            {
                Thread.Sleep(100);
            }
        }

        #endregion
    }
}
