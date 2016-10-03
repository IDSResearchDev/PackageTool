using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Xml.Linq;
using PackageTool.BaseClass;
using PackageTool.View;
using Helper = Rnd.TeklaStructure.Helper;
using Utilities = Rnd.Common.Utilities;
using Rnd.Common.Resources;
using PackageTool.Model;
using System.Windows.Controls;

namespace PackageTool.ViewModel
{
    public class MainViewModel : ViewModelBase, IDataErrorInfo
    {
        private Utilities _utilities;
        private Helper.Utilities _helper;

        public MainViewModel()
        {

            _utilities = new Utilities();
            _helper = new Helper.Utilities();
            CreateReportTxt();

            _TransmittalDate = DateTime.Now.ToShortDateString();

            GlobalVars.GetXsrReports();

            GlobalVars.cfgModel = this._utilities.DeserializeBinFile<CfgModel>(GlobalVars.LocalCfgBinFile);
            GlobalVars.PrinterSelection = this._utilities.DeserializeBinFile<ObservableCollection<PrinterSelectionModel>>(GlobalVars.LocalPrinterInstanceBinFile);
            GlobalVars.UpdateConfigModel = this._utilities.DeserializeBinFile<UpdateConfigurationModel>(GlobalVars.LocalUpdateConfigurationFile);
            CheckLatestUpdate();
        }

        private void GetProjectInfo()
        {
            Helper.ProjectProperties project = new Helper.ProjectProperties();
            JobCode = project.JobCode;
            JobNumber = project.JobNumber;
            Fabricator = project.Fabricator;
        }

        private XDocument _xDoc;
        private XElement _rooXElement;
        private const string _xmlname = "pkgrecentdata.xml";
        private string _xmlfile;

        #region Properties

        public string AppTitle
        {
            get { return "Package Tool " + GlobalVars.AppVersion; }
        }

        private bool _islightbox;
        public bool IsLightBox
        {
            get { return _islightbox; }
            set
            {
                if (_islightbox == value) return;
                _islightbox = value;
                OnPropertyChanged("IsLightBox");
            }
        }

        private string _TransmittalDate;
        public string TransmittalDate
        {
            get { return _TransmittalDate; }
            set
            {
                if (_TransmittalDate == value) return;
                _TransmittalDate = value;
                OnPropertyChanged("TransmittalDate");
            }
        }

        private string _fabricator;
        public string Fabricator
        {
            get { return _fabricator; }
            set
            {
                if (_fabricator == value) return;
                _fabricator = value;
                GlobalVars.Fabricator = _fabricator;
                OnPropertyChanged("Fabricator");
            }
        }

        private string _jobnumber;
        public string JobNumber
        {
            get { return _jobnumber; }
            set
            {
                if (_jobnumber == value) return;
                _jobnumber = value;
                GlobalVars.JobNumber = _jobnumber;
                OnPropertyChanged("JobNumber");
            }
        }

        private string _jobCode;
        public string JobCode
        {
            get { return _jobCode; }
            set
            {
                if (_jobCode == value) return;
                _jobCode = value;
                GlobalVars.JobCode = _jobCode;
                OnPropertyChanged("JobCode");
            }
        }

        private string _version;
        public string Version
        {
            get { return _version; }
            set
            {
                if (_version == value) return;
                _version = value;
                OnPropertyChanged("Version");
            }
        }

        private string _packageDirectory;
        public string PackageDirectory
        {
            get { return _packageDirectory; }
            set
            {
                if (value == null) return;
                this._packageDirectory = value;
                GlobalVars.PackageDirectory = _packageDirectory;
                this._helper.SetDXFOutputDir(GlobalVars.LocalAppPackageToolFolder, PackageDirectory);
                OnPropertyChanged("PackageDirectory");
            }
        }

        private string _outputdirectory;
        public string OutputDirectory
        {
            get { return _outputdirectory; }
            set
            {
                if (value == null) return;
                _outputdirectory = value;
                GlobalVars.OutputDirectory = _outputdirectory;
                GlobalVars.TransmittalName = Path.Combine(OutputDirectory, string.Concat(JobNumber, "_", JobCode, "_", "TRANSMITTAL#", TransmittalNumber));
                OnPropertyChanged("OutputDirectory");
            }
        }

        private string _configurationtype;
        public string ConfigurationType
        {
            get { return _configurationtype; }
            set
            {
                if (value == null || value.Equals(string.Empty)) return;
                _configurationtype = value;
                OnPropertyChanged("ConfigurationType");
            }
        }

        private string _projectnumber;
        public string ProjectNumber
        {
            get { return _projectnumber; }
            set
            {
                if (value == null) return;
                _projectnumber = value;
                GlobalVars.ProjectNumber = _projectnumber;
                OnPropertyChanged("ProjectNumber");
            }
        }

        private string _transmittalnumber;
        public string TransmittalNumber
        {
            get { return _transmittalnumber; }
            set
            {
                if (value == null) return;
                _transmittalnumber = value;
                GlobalVars.TransmittalNumber = _transmittalnumber;
                OnPropertyChanged("TransmittalNumber");
            }
        }

        private string _projectname;
        public string ProjectName
        {
            get { return _projectname; }
            set
            {
                if (value == null) return;
                _projectname = value;
                GlobalVars.Project = _projectname;
                OnPropertyChanged("ProjectName");
            }
        }

        private string _location;
        public string Location
        {
            get { return _location; }
            set
            {
                if (value == null) return;
                _location = value;
                GlobalVars.Location = _location;
                OnPropertyChanged("Location");
            }
        }

        private string _attention;
        public string Attention
        {
            get { return _attention; }
            set
            {
                if (value == null) return;
                _attention = value;
                GlobalVars.Attention = _attention;
                OnPropertyChanged("Attention");
            }
        }

        private string _signatory;
        public string Signatory
        {
            get { return _signatory; }
            set
            {
                if (value == null) return;
                _signatory = value;
                GlobalVars.Signature = _signatory;
                OnPropertyChanged("Signatory");
            }
        }

        private string _remarks;
        public string Remarks
        {
            get { return _remarks; }
            set
            {
                if (value == null) return;
                _remarks = value;
                GlobalVars.Remarks = _remarks;
                OnPropertyChanged("Remarks");
            }
        }

        private bool _applyPrinterInstance;

        public bool ApplyPrinterInstance
        {
            get { return _applyPrinterInstance; }
            set
            {
                _applyPrinterInstance = value;
                GlobalVars.ApplyPrinterInstance = _applyPrinterInstance;
                OnPropertyChanged("ApplyPrinterInstance");
            }
        }

        private string _cfgFilename;

        public string CfgFilename
        {
            get
            {
                return (_cfgFilename != string.Empty && GlobalVars.cfgModel != null) ? _cfgFilename
                    : (_cfgFilename == string.Empty && GlobalVars.cfgModel != null) ? StringResource.Default
                    : "Included Files";
            }
            set
            {
                _cfgFilename = value;
                OnPropertyChanged("CfgFileName");
            }
        }

        private int _mainTabIndex;

        public int MainTabIndex
        {
            get { return _mainTabIndex; }
            set
            {
                _mainTabIndex = value;
                OnPropertyChanged("MainTabIndex");
            }
        }

        private bool _includeFiles;

        public bool IncludeFiles
        {
            get { return _includeFiles; }
            set
            {
                _includeFiles = value;
                OnPropertyChanged("IncludeFiles");
            }
        }

        private object _cmbTypeValue;

        public object CmbTypeValue
        {
            get { return _cmbTypeValue; }
            set
            {
                _cmbTypeValue = value;
                OnPropertyChanged("CmbTypeValue");
            }
        }

        private bool _click;
        private bool _isKeyLeftControl;

        public bool IsKeyLeftControl
        {
            get { return _isKeyLeftControl; }
            set
            {
                _isKeyLeftControl = value;

                OnPropertyChanged("IsKeyLeftControl");
            }
        }

        private string _title1;
        public string Title1
        {
            get { return _title1; }
            set
            {
                if (_title1 == value) return;
                _title1 = value;
                GlobalVars.Title1 = _title1;
                OnPropertyChanged("Title1");
            }
        }

        private string _title2;
        public string Title2
        {
            get { return _title2; }
            set
            {
                if (_title2 == value) return;
                _title2 = value;
                OnPropertyChanged("Title2");
            }
        }
        private string _title3;
        public string Title3
        {
            get { return _title3; }
            set
            {
                if (_title3 == value) return;
                _title3 = value;
                OnPropertyChanged("Title3");
            }
        }

        private string _kssname;
        public string KssName
        {
            get { return _kssname; }
            set
            {
                if (_kssname == value) return;
                _kssname = value;
                GlobalVars.KssName = _kssname;
                OnPropertyChanged("KssName");
            }
        }

        private bool _useksstemplate;
        public bool UseKssTemplate
        {
            get { return _useksstemplate; }
            set
            {
                _useksstemplate = value;
                GlobalVars.UseKssTemplate = _useksstemplate;
                OnPropertyChanged("UseKssTemplate");
            }
        }

        private string _getUpdate;
        public string GetUpdate
        {
            get { return _getUpdate; }
            set
            {
                _getUpdate = value;
                OnPropertyChanged("GetUpdate");
            }
        }

        private string _checkForUpdate;
        public string CheckForUpdate
        {
            get { return _checkForUpdate; }
            set
            {
                _checkForUpdate = value;
                OnPropertyChanged("CheckForUpdate");
            }
        }


        #region Sending Selection
        private string _sendingselection;
        public string SendingSelection
        {
            get { return _sendingselection; }
            set
            {
                if (value == null) return;
                _sendingselection = value;
                GlobalVars.SendingSelection = _sendingselection;
                OnPropertyChanged("SendingSelection");
            }
        }

        private bool _herewith;
        public bool Herewith
        {
            get { return _herewith; }
            set
            {
                if (UnderSeparateCover)
                {
                    UnderSeparateCover = false;
                }
                if (_herewith == value) return;
                _herewith = value;
                GetSelection();
                OnPropertyChanged("Herewith");

            }
        }

        private bool _underseparatecover;
        public bool UnderSeparateCover
        {
            get { return _underseparatecover; }
            set
            {
                if (Herewith)
                {
                    Herewith = false;
                }
                if (_underseparatecover == value) return;
                _underseparatecover = value;
                GetSelection();
                OnPropertyChanged("UnderSeparateCover");

            }
        }

        private string _printerInstance;

        public string PrinterInstance
        {
            get { return _printerInstance; }
            set
            {
                if (value == string.Empty) return;
                _printerInstance = value;
                GlobalVars.PrinterInstance = _printerInstance;
                OnPropertyChanged("PrinterInstance");
            }
        }

        private string _kssTemplateFilename;
        public string KssTemplateFilename
        {
            get { return _kssTemplateFilename; }
            set
            {
                if (value == string.Empty) return;
                _kssTemplateFilename = value;
                GlobalVars.KssTemplateFilename = _kssTemplateFilename;
                OnPropertyChanged("KssTemplateFilename");
            }
        }

        #endregion

        #region Types

        private bool _shopdrawing;
        public bool ShopDrawing
        {
            get { return _shopdrawing; }
            set
            {
                _shopdrawing = value;
                OnPropertyChanged("ShopDrawing");
            }
        }
        private bool _copyofletter;
        public bool CopyOfLetter
        {
            get { return _copyofletter; }
            set
            {
                _copyofletter = value;
                OnPropertyChanged("CopyOfLetter");
            }
        }
        private bool _drawings;
        public bool Drawings
        {
            get { return _drawings; }
            set
            {
                _drawings = value;
                OnPropertyChanged("Drawings");
            }
        }
        private bool _samples;
        public bool Samples
        {
            get { return _samples; }
            set
            {
                if (_samples == value) return;
                _samples = value;
                OnPropertyChanged("Samples");
            }
        }
        private bool _prints;
        public bool Prints
        {
            get { return _prints; }
            set
            {
                if (_prints == value) return;
                _prints = value;
                OnPropertyChanged("Prints");
            }
        }
        private bool _changeorder;
        public bool ChangeOrder
        {
            get { return _changeorder; }
            set
            {
                if (_changeorder == value) return;
                _changeorder = value;
                OnPropertyChanged("ChangeOrder");
            }
        }
        private bool _specifications;
        public bool Specifications
        {
            get { return _specifications; }
            set
            {
                if (_specifications == value) return;
                _specifications = value;
                OnPropertyChanged("Specifications");
            }
        }
        private bool _brochure;
        public bool Brochure
        {
            get { return _brochure; }
            set
            {
                if (_brochure == value) return;
                _brochure = value;
                OnPropertyChanged("Brochure");
            }
        }
        private bool _datasheet;
        public bool DataSheet
        {
            get { return _datasheet; }
            set
            {
                if (_datasheet == value) return;
                _datasheet = value;
                OnPropertyChanged("DataSheet");
            }
        }
        private bool _schedule;
        public bool Schedule
        {
            get { return _schedule; }
            set
            {
                if (_schedule == value) return;
                _schedule = value;
                OnPropertyChanged("Schedule");
            }
        }
        private bool _addendum;
        public bool Addendum
        {
            get { return _addendum; }
            set
            {
                if (_addendum == value) return;
                _addendum = value;
                OnPropertyChanged("Addendum");
            }
        }
        private bool _catalogcuts;
        public bool CatalogCuts
        {
            get { return _catalogcuts; }
            set
            {
                if (_catalogcuts == value) return;
                _catalogcuts = value;
                OnPropertyChanged("CatalogCuts");
            }
        }
        private string _types;
        public string Types
        {
            get { return _types; }
            set
            {
                if (_types == value) return;
                _types = value;
                GlobalVars.FileTypes = _types;
                OnPropertyChanged("Types");
            }
        }

        #endregion

        #region Purpose
        private string _purpose;
        public string Purpose
        {
            get { return _purpose; }
            set
            {
                if (value == null) return;
                _purpose = value;
                GlobalVars.Purpose = _purpose;
                OnPropertyChanged("Purpose");
            }
        }

        private bool _forapproval;
        public bool ForApproval
        {
            get { return _forapproval; }
            set
            {
                if (_forapproval == value) return;
                _forapproval = value;
                OnPropertyChanged("ForApproval");
            }
        }
        private bool _forfabricator;
        public bool ForFabricator
        {
            get { return _forfabricator; }
            set
            {
                if (_forfabricator == value) return;
                _forfabricator = value;
                OnPropertyChanged("ForFabricator");
            }
        }
        private bool _fyi;
        public bool FYI
        {
            get { return _fyi; }
            set
            {
                if (_fyi == value) return;
                _fyi = value;
                OnPropertyChanged("FYI");
            }
        }
        private bool _approved;
        public bool Approved
        {
            get { return _approved; }
            set
            {
                if (_approved == value) return;
                _approved = value;
                OnPropertyChanged("Approved");
            }
        }
        private bool _reviseadsubmit;
        public bool ReviseandSubmit
        {
            get { return _reviseadsubmit; }
            set
            {
                if (_reviseadsubmit == value) return;
                _reviseadsubmit = value;
                OnPropertyChanged("ReviseandSubmit");
            }
        }
        private bool _specification;
        public bool Specification
        {
            get { return _specification; }
            set
            {
                if (_specification == value) return;
                _specification = value;
                OnPropertyChanged("Specification");
            }
        }
        private bool _revisedforconstruction;
        public bool RevisedForConstruction
        {
            get { return _revisedforconstruction; }
            set
            {
                if (_revisedforconstruction == value) return;
                _revisedforconstruction = value;
                OnPropertyChanged("RevisedForConstruction");
            }
        }
        private bool _asrequested;
        public bool AsRequested
        {
            get { return _asrequested; }
            set
            {
                if (_asrequested == value) return;
                _asrequested = value;
                OnPropertyChanged("AsRequested");
            }
        }
        private bool _addendum2;
        public bool Addendum2
        {
            get { return _addendum2; }
            set
            {
                if (_addendum2 == value) return;
                _addendum2 = value;
                OnPropertyChanged("Addendum2");
            }
        }
        private bool _revisedforapproval;
        public bool RevisedForApproval
        {
            get { return _revisedforapproval; }
            set
            {
                if (_revisedforapproval == value) return;
                _revisedforapproval = value;
                OnPropertyChanged("RevisedForApproval");
            }
        }
        private bool _revisedforreviewandcomment;
        public bool ReviseForReviewAndComment
        {
            get { return _revisedforreviewandcomment; }
            set
            {
                if (_revisedforreviewandcomment == value) return;
                _revisedforreviewandcomment = value;
                OnPropertyChanged("ReviseForReviewAndComment");
            }
        }
        private bool _forapprovalfabricator;
        public bool ForApprovalFabricator
        {
            get { return _forapprovalfabricator; }
            set
            {
                if (_forapprovalfabricator == value) return;
                _forapprovalfabricator = value;
                OnPropertyChanged("ForApprovalFabricator");
            }
        }
        private bool _foryouruse;
        public bool ForYourUse
        {
            get { return _foryouruse; }
            set
            {
                if (_foryouruse == value) return;
                _foryouruse = value;
                OnPropertyChanged("ForYourUse");
            }
        }
        private bool _revisedforshopandapproval;
        public bool RevisedForShopAndApproval
        {
            get { return _revisedforshopandapproval; }
            set
            {
                if (_revisedforshopandapproval == value) return;
                _revisedforshopandapproval = value;
                OnPropertyChanged("RevisedForShopAndApproval");
            }
        }
        private bool _forquotation;
        public bool ForQuotationDue
        {
            get { return _forquotation; }
            set
            {
                if (_forquotation == value) return;
                _forquotation = value;
                OnPropertyChanged("ForQuotationDue");
            }
        }
        private bool _forconstruction;
        public bool ForConstruction
        {
            get { return _forconstruction; }
            set
            {
                if (_forconstruction == value) return;
                _forconstruction = value;
                OnPropertyChanged("ForConstruction");
            }
        }
        private bool _forfabrication;
        public bool ForFabrication
        {
            get { return _forfabrication; }
            set
            {
                if (_forfabrication == value) return;
                _forfabrication = value;
                OnPropertyChanged("ForFabrication");
            }
        }

        #endregion


        #endregion

        #region Miscellenous

        private void CreateReportTxt()
        {
            var file = Path.Combine(GlobalVars.LocalAppPackageToolFolder, "reports.txt");

            if (!_utilities.CheckIfFileExists(file))
            {
                _utilities.CreateFile(GlobalVars.LocalAppPackageToolFolder, "reports", "txt");
            }
        }

        private void GetSelection()
        {

            if (Herewith)
            {
                SendingSelection = "Herewith";
            }

            if (UnderSeparateCover)
            {
                SendingSelection = "Under separate cover";
            }
        }
        private void GetTypes()
        {
            Types = string.Empty;
            if (ShopDrawing) Types += "Shop Drawings, ";
            if (CopyOfLetter) Types += "Copy of Letter, ";
            if (Drawings) Types += "Drawings, ";
            if (Samples) Types += "Samples, ";
            if (Prints) Types += "Prints, ";
            if (ChangeOrder) Types += "Change Order, ";
            if (Specifications) Types += "Specifications, ";
            if (Brochure) Types += "Brochure, ";
            if (DataSheet) Types += "Data Sheet, ";
            if (Schedule) Types += "Schedule, ";
            if (Addendum) Types += "Addendum, ";
            if (CatalogCuts) Types += "Catalog Cuts, ";

            if (Types.Length <= 0) return;
            var removelast = Types.Substring(0, Types.Length - 2);
            Types = removelast;
        }
        private void GetPurpose()
        {
            Purpose = string.Empty;
            if (ForApproval) Purpose += "For Approval, ";
            if (ForFabricator) Purpose += "For Fabricator, ";
            if (FYI) Purpose += "FYI, ";
            if (Approved) Purpose += "Approved, ";
            if (ReviseandSubmit) Purpose += "Revise and Submit, ";
            if (Specification) Purpose += "Specification, ";
            if (RevisedForConstruction) Purpose += "Revised for construction, ";
            if (AsRequested) Purpose += "As requested, ";
            if (Addendum2) Purpose += "Addendum #, ";
            if (RevisedForApproval) Purpose += "Revised for approval, ";
            if (ReviseForReviewAndComment) Purpose += "Revised for review and comment, ";
            if (ForApprovalFabricator) Purpose += "For Approval/Fabricator, ";
            if (ForYourUse) Purpose += "For your use, ";
            if (RevisedForShopAndApproval) Purpose += "Revised for shop &; approval, ";
            if (ForQuotationDue) Purpose += "For Quotation Due, ";
            if (ForConstruction) Purpose += "For construction, ";
            if (ForFabrication) Purpose += "For Fabrication, ";

            if (Purpose.Length <= 0) return;
            var removelast = Purpose.Substring(0, Purpose.Length - 2);
            Purpose = removelast;

        }


        /// <summary>
        /// returns true boolean expression if the required fields are Null or Empty
        /// </summary>
        /// <returns></returns>
        private bool CheckerIsNullOrEmpty()
        {
            return (string.IsNullOrEmpty(ProjectNumber) || string.IsNullOrEmpty(TransmittalNumber)
                    || string.IsNullOrEmpty(ProjectName) || string.IsNullOrEmpty(Location)
                    || string.IsNullOrEmpty(Attention) || string.IsNullOrEmpty(Signatory)
                    || string.IsNullOrEmpty(OutputDirectory) || string.IsNullOrEmpty(PackageDirectory)
                    || string.IsNullOrEmpty(ConfigurationType)
                    || !_utilities.CheckIfDirectoryExists(OutputDirectory)
                    || !_utilities.CheckIfDirectoryExists(PackageDirectory)
                );
        }
        /// <summary>
        /// returns True boolean expression if the required fields are Null or Whitespaces
        /// </summary>
        /// <returns></returns>
        private bool CheckerIsNullOrWhiteSpace()
        {
            return (string.IsNullOrWhiteSpace(ProjectNumber) || string.IsNullOrWhiteSpace(TransmittalNumber)
                    || string.IsNullOrWhiteSpace(ProjectName) || string.IsNullOrWhiteSpace(Location)
                    || string.IsNullOrWhiteSpace(Attention) || string.IsNullOrWhiteSpace(Signatory)
                    || string.IsNullOrWhiteSpace(OutputDirectory) || string.IsNullOrWhiteSpace(PackageDirectory)
                    || string.IsNullOrWhiteSpace(ConfigurationType)

                )
            ;
        }
        private void LoadXml()
        {
            //check if directory exist if not create new
            //_utilities.CreateDirectory(_xmlpath);
            _utilities.CreateDirectory(GlobalVars.ModelFolder);

            //check if existing if not create new
            //else load xml
            if (!File.Exists(_xmlfile))
            {
                SaveXmlFile(_xmlfile, CreateXml());
            }
            else
            {
                CompareVersion();
            }
            //_xDoc = XDocument.Load(Path.Combine(_xmlpath, _xmlname));
            _xDoc = XDocument.Load(Path.Combine(GlobalVars.ModelFolder, _xmlname));
            _rooXElement = _xDoc.Root;
            GetXElements();
        }
        private XDocument CreateXml()
        {
            _xDoc = new XDocument
            (
                new XDeclaration("1.0", "utf-8", ""),
                new XElement("recentdata",
                    new XComment(" Export tab "),
                        new XElement("version"),
                        new XElement("type"),
                        new XElement("cfg"),
                        new XElement("packagedirectory"),
                        new XElement("date"),
                        new XElement("applyprinterinstance"),
                        new XElement("printerinstance"),
                    new XComment(" Transmittal tab "),
                        new XElement("projectnumber"),
                        new XElement("transmittalnumber"),
                        new XElement("projectname"),
                        new XElement("location"),
                        new XElement("remarks"),
                        new XElement("attention"),
                        new XElement("signatory"),
                        new XElement("outputdirectory")
                    )
            );

            if (_xDoc.Root != null)
            {
                var xElement = _xDoc.Root.Element("version");
                if (xElement != null)
                    xElement.Value = Properties.Settings.Default.strXmlLastestVersion;
            }

            return _xDoc;
        }
        private void SaveXmlFile(string xmlfile, XDocument xmlDoc)
        {
            xmlDoc.Save(xmlfile);
        }
        private string GetCurrentXmlVersion()
        {
            //var xmldoc = XDocument.Load(Path.Combine(_xmlpath, _xmlname));
            var xmldoc = XDocument.Load(Path.Combine(GlobalVars.ModelFolder, _xmlname));
            var root = xmldoc.Root;
            var curversion = string.Empty;

            if (root != null)
            {
                var xElement = root.Element("version");
                if (xElement != null) return xElement.Value;
            }

            return curversion;
        }
        private void CompareVersion()
        {
            double newversion;
            double currentversion;

            var _newversion = Properties.Settings.Default.strXmlLastestVersion;

            double.TryParse(_newversion, out newversion);
            double.TryParse(GetCurrentXmlVersion(), out currentversion);

            //if (newversion == currentversion) MessageBox.Show("%localappdata%~packagetool.xml is up to date.");
            if (_newversion != GetCurrentXmlVersion())
            {
                if (newversion < currentversion) throw new ArgumentException("Current xml version is higher than new version settings. Please check \"Properties.Settings.Default ~strXmlLatestVersion value.\" ");
                if (newversion > currentversion) SaveXmlFile(_xmlfile, CreateXml());
            }
        }
        public void GetXElements()
        {
            foreach (var itemElements in _rooXElement.Elements())
            {
                if (itemElements.Name == "version") Version = itemElements.Value;
                if (itemElements.Name == "type") ConfigurationType = itemElements.Value;
                if (itemElements.Name == "packagedirectory")
                {
                    PackageDirectory = itemElements.Value;
                    if (!_utilities.CheckIfDirectoryExists(PackageDirectory))
                    {
                        PackageDirectory = string.Empty;
                    }
                }
                if (itemElements.Name == "projectnumber") ProjectNumber = itemElements.Value;
                if (itemElements.Name == "transmittalnumber") TransmittalNumber = itemElements.Value;
                if (itemElements.Name == "projectname") ProjectName = itemElements.Value;
                if (itemElements.Name == "location") Location = itemElements.Value;
                if (itemElements.Name == "remarks") Remarks = itemElements.Value;
                if (itemElements.Name == "attention") Attention = itemElements.Value;
                if (itemElements.Name == "signatory") Signatory = itemElements.Value;
                if (itemElements.Name == "outputdirectory")
                {
                    OutputDirectory = itemElements.Value;
                    if (!_utilities.CheckIfDirectoryExists(OutputDirectory))
                    {
                        OutputDirectory = string.Empty;
                    }
                }
                if (itemElements.Name == "printerinstance") PrinterInstance = itemElements.Value;
                if (itemElements.Name == "applyprinterinstance") ApplyPrinterInstance = !string.IsNullOrEmpty(itemElements.Value) ? Convert.ToBoolean(itemElements.Value) : false;
                if (itemElements.Name == "cfg") CfgFilename = itemElements.Value;
            }
        }
        public XDocument UpdateXElements()
        {
            foreach (var itemElements in _rooXElement.Elements())
            {
                if (itemElements.Name == "packagedirectory") itemElements.Value = _packageDirectory;
                if (itemElements.Name == "projectnumber") itemElements.Value = _projectnumber;
                if (itemElements.Name == "transmittalnumber") itemElements.Value = _transmittalnumber;
                if (itemElements.Name == "projectname") itemElements.Value = _projectname;
                if (itemElements.Name == "location") itemElements.Value = _location;

                if (itemElements.Name == "remarks") itemElements.Value = _remarks;
                if (itemElements.Name == "attention") itemElements.Value = _attention;
                if (itemElements.Name == "signatory") itemElements.Value = _signatory;
                if (itemElements.Name == "outputdirectory") itemElements.Value = _outputdirectory;
                if (itemElements.Name == "printerinstance") itemElements.Value = _printerInstance;
                if (itemElements.Name == "applyprinterinstance") itemElements.Value = _applyPrinterInstance.ToString();
                if (itemElements.Name == "cfg") itemElements.Value = CfgFilename;
            }
            return _xDoc;
        }
        private string ShowBrowserDialog(string path)
        {
            var dialog = _utilities.FolderDialog();
            var dialogresult = dialog.Item1;
            var selectedpath = dialog.Item2;
            if (dialogresult != System.Windows.Forms.DialogResult.OK)
                return path;
            return selectedpath;
        }
        private void Initialize()
        {
            CanValidate = false;
            GetProjectInfo();
            GlobalVars.FirmFolder = this._helper.GetAdvancedOption("XS_FIRM");
            GlobalVars.ModelFolder = this._helper.ModelFolder();
            //GlobalVars.FirmReportDir = Path.Combine(GlobalVars.FirmFolder, @"Report");
            GlobalVars.FirmReportDir = Path.Combine(GlobalVars.FirmFolder, @"Template");
            GlobalVars.ModelReportTemplateDir = Path.Combine(GlobalVars.ModelFolder, @"ReportTemplate");
            GlobalVars.FirmReportTemplateDir = Path.Combine(GlobalVars.FirmReportDir, @"ReportTemplate");



            this._helper.CopyMacrosToFirm(GlobalVars.FirmFolder);
            this._helper.CopyMacroToMacroDirectory();

            this._xmlfile = Path.Combine(GlobalVars.ModelFolder, _xmlname);
            LoadXml();
            GlobalVars.TransmittalName = Path.Combine(OutputDirectory, string.Concat(JobNumber, "_", JobCode, "_", "TRANSMITTAL#", TransmittalNumber));

        }

        public bool CheckPrinterInstanceExists(ref List<string> printerInstances)
        {
            bool isExist = true;
            Helper.Drawings drawings = new Helper.Drawings();
            List<object> selecteddrawing = drawings.SelectedDrawing;
            int drawingcount = selecteddrawing.Count;
            var instances = _helper.PrinterInstance();
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

        private void GetXsrReports()
        {
            GlobalVars.XsrReportList = new ObservableCollection<XsrReports>();

            using (var reader = new StreamReader(Path.Combine(GlobalVars.LocalAppPackageToolFolder, "reports.txt")))
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

                    GlobalVars.XsrReportList.Add(new XsrReports()
                    {
                        ReportName = newreadLine
                    });

                }
            }

        }

        private bool ValidatePrinterInstance()
        {
            List<string> instances = new List<string>();
            if (!CheckPrinterInstanceExists(ref instances))
            {
                string sizes = string.Empty;
                foreach (var item in instances) sizes += "● " + item + Environment.NewLine;
                MessageBox.Show(this.GetCurrentWindow(), "No Printer Instances found for the \n following Paper Size:\n\n" + sizes + "\nPlease add these Printer Instances to Continue.", "Printer Instance not found", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            return true;
        }

        public void CloseOpenCfgDialog()
        {
            if (GlobalVars.cfgDialog != null)
            { GlobalVars.cfgDialog.Close(); }
            if (IncludeFiles == true)
            {
                GlobalVars.cfgDialog = new CfgDialog();
                this.UpdateCFGText();
                GlobalVars.cfgDialog.Owner = this.GetCurrentWindow();
                this.UpdateWindowLocation();
                GlobalVars.cfgDialog.Show();
            }
        }
        private void UpdateCFGText()
        {
            if (GlobalVars.cfgDialog != null)
            {
                GlobalVars.cfgDialog.TxtBlockType.Text = this.CmbTypeValue.ToString();
            }
        }
        private void UpdateWindowLocation()
        {
            if (GlobalVars.cfgDialog != null)
            {
                if (this.MainTabIndex == 0)
                {
                    var location = GlobalVars.MainWindow.BtnIncludFiles.PointToScreen(new Point(0, 0));
                    System.Windows.Media.Matrix matrix = PresentationSource.FromVisual(GlobalVars.MainWindow).CompositionTarget.TransformToDevice;
                    GlobalVars.cfgDialog.Left = (location.X - 30) / matrix.M11;
                    GlobalVars.cfgDialog.Top = (location.Y - 2) / matrix.M22;
                }
            }
        }
        private void ShowPackageDir(string OpenDirectory/*, string PropertyName*/)
        {
            if (_click && IsKeyLeftControl)
            {
                if (!string.IsNullOrEmpty(OpenDirectory) || !string.IsNullOrWhiteSpace(OpenDirectory))
                {
                    Utilities util = new Utilities();
                    if (util.CheckIfDirectoryExists(OpenDirectory))
                    {
                        Process.Start(OpenDirectory);
                    }
                    else
                    {
                        MessageBox.Show("Please select existing directory.", "Directory not found", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }
                else
                {
                    MessageBox.Show("Directory cannot be empty or whitespace. Please select existing directory.", "Null or Whitespace", MessageBoxButton.OK, MessageBoxImage.Exclamation);

                }
                _click = false;
                IsKeyLeftControl = false;
            }
        }

        private List<string> XsrTitleList()
        {
            return new List<string>
            {
                string.Concat("XS_TITLE1=", Title1),
                string.Concat("XS_TITLE2=", Title2),
                string.Concat("XS_TITLE3=", Title3)
            };
        }



        #endregion

        private void UpdateXsrTitle()
        {
            var file = Path.Combine(GlobalVars.LocalAppPackageToolFolder, "xsrtitle.txt");
            if (!_utilities.CheckIfFileExists(file)) _utilities.CreateFile(GlobalVars.LocalAppPackageToolFolder, "xsrtitle", "txt");

            using (StreamWriter wr = new StreamWriter(file))
            {
                foreach (var item in XsrTitleList())
                {
                    wr.WriteLine(item);
                }
            }

        }

        private bool CheckLatestUpdate()
        {
            bool value = false;
            if (File.Exists(GlobalVars.LocalUpdaterFile))
            {
                var aiuFile = "package_tool_update.aiu";
                var util = new Rnd.Common.Utilities();
                var updatePath = Path.Combine(util.GetTextFileValue(GlobalVars.LocalUpdaterFile, '=', "DownloadsFolder"), aiuFile);
                if (File.Exists(updatePath))
                {
                    var updateVersion = new Version(util.GetTextFileValue(updatePath, '=', "Version")).ToString(3);

                    if (VersionComparer.IsUptoDate(updateVersion, GlobalVars.AppVersion))
                    {
                        value = true;
                        GetUpdate = string.Empty;
                        CheckForUpdate = "Check for Update";
                    }
                    else
                    {
                        GetUpdate = "Get latest version ";
                        CheckForUpdate = updateVersion;
                    }
                }
                else
                {
                    MessageBox.Show("Model Launcher update file (" + aiuFile + ") doesn't exist.", "Update not found", MessageBoxButton.OK, MessageBoxImage.Information);
                    value = true;
                }
            }
            return value;
        }


        #region Command
        public ICommand Closing
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    Environment.Exit(0);
                });
            }
        }

        public ICommand ViewLoad
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    this.Initialize();
                });
            }
        }

        public ICommand CreatePackage
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    GetTypes();
                    GetPurpose();

                    XsrTitleList();
                    UpdateXsrTitle();

                    CanValidate = true;
                    new Helper.Utilities().GetConncectionStatus();

                    if (!string.IsNullOrEmpty(PackageDirectory) && _utilities.CheckIfDirectoryExists(PackageDirectory) && !string.IsNullOrEmpty(ConfigurationType)
                        && (string.IsNullOrEmpty(ProjectNumber) || string.IsNullOrEmpty(TransmittalNumber) ||
                            string.IsNullOrEmpty(ProjectName) || string.IsNullOrEmpty(Location) ||
                            string.IsNullOrEmpty(Attention) || string.IsNullOrEmpty(Signatory) ||
                            string.IsNullOrEmpty(OutputDirectory) || (!_utilities.CheckIfDirectoryExists(OutputDirectory)))
                           ) (GlobalVars.MainWindow).TabControlMain.SelectedIndex = 1;

                    if ((!string.IsNullOrEmpty(ProjectNumber) && !string.IsNullOrEmpty(TransmittalNumber) &&
                         !string.IsNullOrEmpty(ProjectName) && !string.IsNullOrEmpty(Location) &&
                         !string.IsNullOrEmpty(Attention) && !string.IsNullOrEmpty(Signatory) &&
                         !string.IsNullOrEmpty(OutputDirectory) && _utilities.CheckIfDirectoryExists(OutputDirectory)
                         )
                            && (string.IsNullOrEmpty(PackageDirectory) || !_utilities.CheckIfDirectoryExists(PackageDirectory)
                                || string.IsNullOrEmpty(ConfigurationType)
                               )
                        ) GlobalVars.MainWindow.TabControlMain.SelectedIndex = 0;

                    if (CheckerIsNullOrEmpty() /*|| CheckerIsNullOrWhiteSpace()*/) return;

                    //this._helper.CheckSelectedDrawing();

                    //insert save recent required fields
                    SaveXmlFile(_xmlfile, UpdateXElements());

                    //if (!ValidatePrinterInstance())
                    //    return;

                    var diagresult = MessageBox.Show(StringResource.ProceedCreatePackageQuestion, StringResource.CreatePackage,
                        MessageBoxButton.YesNo, MessageBoxImage.Question);


                    if (diagresult == MessageBoxResult.Yes)
                    {
                        var progressdiag = new ProgressDialogView();
                        IsLightBox = true;

                        progressdiag.ShowDialog(GlobalVars.MainWindow);
                    }

                });
            }
        }

        public ICommand BrowsePackageDir
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    PackageDirectory = ShowBrowserDialog(PackageDirectory);
                });
            }
        }

        public ICommand BrowseOutputDir
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    OutputDirectory = ShowBrowserDialog(OutputDirectory);
                    GlobalVars.TransmittalName = Path.Combine(OutputDirectory, string.Concat(JobNumber, "_", JobCode, "_", "TRANSMITTAL#", TransmittalNumber));
                });
            }
        }

        public ICommand FirmFolderSettings
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    var firmview = new FirmView();
                    firmview.ShowDialog(GlobalVars.MainWindow);
                });
            }
        }

        public ICommand PrinterSettings
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (GlobalVars.PrinterSelection == null)
                    {
                        MessageBox.Show(this.GetCurrentWindow(), "There are no configured printer settings available at this moment.", "Printer Setting", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    PrinterInstanceSelectionView view = new PrinterInstanceSelectionView();
                    view.vm.IsViewing = true;
                    view.Owner = this.GetCurrentWindow();

                    foreach (var item in GlobalVars.PrinterSelection)
                    { view.vm.PrinterSelection.Add(item); }
                    view.ShowDialog();

                });
            }
        }

        public ICommand BrowseKssTemplate
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    //kss template code here
                });
            }
        }

        public ICommand BtnIncludFiles_Click
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    this.CloseOpenCfgDialog();
                });
            }
        }

        public ICommand CmbType_SelectionChanged
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    this.UpdateCFGText();
                });
            }
        }

        public ICommand Window_LocationChanged
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    this.UpdateWindowLocation();
                });
            }
        }

        public ICommand TabControl_SelectionChanged
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (MainTabIndex != 0 && MainTabIndex >= 0)
                    {
                        IncludeFiles = false;
                        this.CloseOpenCfgDialog();
                    }
                });
            }
        }

        public ICommand Window_KeyUp
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    IsKeyLeftControl = false;
                });
            }
        }

        public ICommand TxtDir_OnPreviewMouseLeftButtonDown
        {
            get
            {
                return new DelegateCommand((sender) =>
                {
                    var textbox = sender as TextBox;
                    if (textbox != null)
                    {
                        _click = true;
                        if (IsKeyLeftControl)
                        {
                            if (textbox.Name.Equals("TxtPackageDir"))
                                ShowPackageDir(GlobalVars.PackageDirectory/*, "Package Directory"*/);
                            else
                                ShowPackageDir(GlobalVars.OutputDirectory/*, "Output Directory"*/);
                        }

                    }
                });
            }
        }

        public ICommand TxtDir_OnPreviewMouseLeftButtonUp
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _click = false;
                });
            }
        }

        public ICommand CheckUpdate
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    string updater = Path.Combine(GlobalVars.LocalAppPackageToolFolder, @"updater.exe");
                    if (!File.Exists(updater))
                    {
                        MessageBox.Show(this.GetCurrentWindow(), "Updater not found.", "Update Failed", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    if (File.Exists(GlobalVars.LocalUpdaterFile))
                    {
                        if (CheckLatestUpdate())
                        {
                            UpdateSettingView update = new UpdateSettingView();
                            update.Owner = this.GetCurrentWindow();
                            update.ShowDialog();
                        }
                        else
                        {
                            Process.Start(updater);
                        }
                    }
                });
            }
        }

        public ICommand Help
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    string appDir = AppDomain.CurrentDomain.BaseDirectory;
                    if (!File.Exists(appDir + @"PackageTool_Help.pdf"))
                    {
                        MessageBox.Show(this.GetCurrentWindow(), "Help file doesn't exist.", "Help file not found", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    Process.Start(appDir + @"PackageTool_Help.pdf");
                });
            }
        }
        #endregion

        #region Validation Implementation of IDataErrorInfo

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get
            {
                if (CanValidate)
                {
                    //combobox type
                    if (columnName == "ConfigurationType")
                    {
                        if (string.IsNullOrEmpty(this.ConfigurationType))
                        {
                            return StringResource.ConfigurationTypeNotSet;
                        }

                    }

                    if (columnName == "ProjectNumber")
                    {
                        if (string.IsNullOrEmpty(this.ProjectNumber))
                        {

                            return StringResource.ProjectNumberNotSet;
                        }

                    }

                    if (columnName == "TransmittalNumber")
                    {
                        if (string.IsNullOrEmpty(this.TransmittalNumber))
                        {
                            return StringResource.TransmittalNumberNotSet;
                        }

                    }

                    if (columnName == "ProjectName")
                    {
                        if (string.IsNullOrEmpty(this.ProjectName))
                        {
                            return StringResource.ProjectNameNotSet;
                        }
                    }

                    if (columnName == "Location")
                    {
                        if (string.IsNullOrEmpty(this.Location))
                        {
                            return StringResource.LocationNotSet;
                        }
                    }

                    if (columnName == "Attention")
                    {
                        if (string.IsNullOrEmpty(this.Attention))
                        {
                            return StringResource.AttentionNotSet;
                        }
                    }


                    if (columnName == "Signatory")
                    {
                        if (string.IsNullOrEmpty(this.Signatory))
                        {
                            return StringResource.SignatoryNotSet;
                        }
                    }

                    if (columnName == "OutputDirectory")
                    {
                        if (string.IsNullOrEmpty(this.OutputDirectory))
                        {
                            return StringResource.OutputDirectoryNotSet;
                        }
                        if (!_utilities.CheckIfDirectoryExists(OutputDirectory))
                        {
                            //OutputDirectory = string.Empty;
                            return "Output directory doesn't exists.";
                        }
                    }

                    if (columnName == "PackageDirectory")
                    {
                        if (string.IsNullOrEmpty(this.PackageDirectory))
                        {
                            return StringResource.PackageDirectoryNotSet;
                        }
                        if (!_utilities.CheckIfDirectoryExists(PackageDirectory)) return "Package directory doesn't exists.";

                    }

                    if (columnName == "PrinterInstance")
                    {
                        if (string.IsNullOrEmpty(this.PrinterInstance))
                        {
                            return StringResource.PrinterInstanceNotSet;
                        }
                    }
                }
                return string.Empty;
            }
        }

        private bool canValidate;

        public bool CanValidate
        {
            get { return canValidate; }
            set
            {
                canValidate = value;
                OnPropertyChanged("CanValidate");
                OnPropertyChanged("ConfigurationType");
                OnPropertyChanged("ProjectNumber");
                OnPropertyChanged("TransmittalNumber");
                OnPropertyChanged("ProjectName");
                OnPropertyChanged("Location");
                OnPropertyChanged("Attention");
                OnPropertyChanged("Signatory");
                OnPropertyChanged("OutputDirectory");
                OnPropertyChanged("PackageDirectory");
                OnPropertyChanged("PrinterInstance");
            }
        }

        #endregion
    }

}
