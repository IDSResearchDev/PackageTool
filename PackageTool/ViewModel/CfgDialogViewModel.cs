using PackageTool.BaseClass;
using PackageTool.View;
using Rnd.Common;
using System;
using System.Collections.Generic;
//
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using System.Linq;
using Rnd.Common.Resources;
using PackageTool.Model;
using System.Windows;

namespace PackageTool.ViewModel
{
    public class CfgViewModel : ViewModelBase, IDataErrorInfo
    {
        Utilities _utilities;
        bool _isCheckSourceChanged = false;
        public CfgViewModel()
        {
            _utilities = new Utilities();
            CanValidate = false;
        }

        #region Properties

        private string _cfgFilename;
        public string CfgFilename
        {
            get { return _cfgFilename; }
            set
            {
                _cfgFilename = value;
                OnPropertyChanged("CfgFilename");
                this.CheckIcon = Visibility.Hidden;
            }
        }

        private string _type;
        public string Type
        {
            get { return _type; }
            set
            {
                _type = value;

                UpdateCfgItemSource();
                CanValidate = false;
                OnPropertyChanged("Type");
            }
        }

        private bool _pdf;
        public bool PDF
        {
            get { return _pdf; }
            set
            {
                _pdf = value;
                OnPropertyChanged("PDF");
                this.CheckIcon = Visibility.Hidden;
            }

        }

        private bool _kss;

        public bool KSS
        {
            get { return _kss; }
            set
            {
                _kss = value;
                OnPropertyChanged("KSS");
                this.CheckIcon = Visibility.Hidden;
            }
        }

        private bool _dwg;

        public bool DWG
        {
            get { return _dwg; }
            set
            {
                _dwg = value;
                OnPropertyChanged("DWG");
                this.CheckIcon = Visibility.Hidden;
            }
        }

        private bool _dxf;

        public bool DXF
        {
            get { return _dxf; }
            set
            {
                _dxf = value;
                OnPropertyChanged("DXF");
                this.CheckIcon = Visibility.Hidden;
            }
        }

        private bool _ifc;

        public bool IFC
        {
            get { return _ifc; }
            set
            {
                _ifc = value;
                OnPropertyChanged("IFC");
                this.CheckIcon = Visibility.Hidden;
            }
        }

        private bool? _fabtrol;
        public bool? FABTROL
        {
            get { return _fabtrol; }
            set
            {
                _fabtrol = value;
                OnPropertyChanged("FABTROL");
                this.CheckIcon = Visibility.Hidden;
            }
        }
        private ObservableCollection<ItemTemplate> _fabtrolItems;
        public ObservableCollection<ItemTemplate> FabTrolItems
        {
            get
            {
                return this._fabtrolItems;
            }
            set
            {
                this._fabtrolItems = value;
                OnPropertyChanged("FabTrolItems");
                this.CheckIcon = Visibility.Hidden;
            }
        }
        private bool? _boltList;
        public bool? BOLTLIST
        {
            get { return _boltList; }
            set
            {
                _boltList = value;
                OnPropertyChanged("BOLTLIST");
                this.CheckIcon = Visibility.Hidden;
            }
        }
        private ObservableCollection<ItemTemplate> _boltListItems;
        public ObservableCollection<ItemTemplate> BoltListItems
        {
            get
            {
                return this._boltListItems;
            }
            set
            {
                this._boltListItems = value;
                OnPropertyChanged("BoltListItems");
                this.CheckIcon = Visibility.Hidden;
            }
        }
        private bool? _xsr;
        public bool? XSR
        {
            get { return _xsr; }
            set
            {
                _xsr = value;
                OnPropertyChanged("XSR");
                this.CheckIcon = Visibility.Hidden;
            }
        }
        private ObservableCollection<ItemTemplate> _xsrItems;
        public ObservableCollection<ItemTemplate> XSRItems
        {
            get
            {
                return this._xsrItems;
            }
            set
            {
                this._xsrItems = value;
                OnPropertyChanged("XSRItems");
                this.CheckIcon = Visibility.Hidden;
            }
        }

        private bool? _nc;
        public bool? NC
        {
            get { return _nc; }
            set
            {
                _nc = value;
                OnPropertyChanged("NC");
                this.CheckIcon = Visibility.Hidden;
            }
        }
        private ObservableCollection<ItemTemplate> _ncItems;
        public ObservableCollection<ItemTemplate> NCItems
        {
            get
            {
                return this._ncItems;
            }
            set
            {
                this._ncItems = value;
                OnPropertyChanged("NCItems");
            }
        }

        private ObservableCollection<ItemTemplate> _popUpItem;
        public ObservableCollection<ItemTemplate> PopUpItems
        {
            get
            {
                return this._popUpItem;
            }
            set
            {
                this._popUpItem = value;
                OnPropertyChanged("PopUpItems");
                this.CheckIcon = Visibility.Hidden;
            }
        }

        private string _popUpName;

        public string PopUpName
        {
            get { return _popUpName; }
            set
            {
                _popUpName = value;
                this.UpdatePopUpLabel();
                OnPropertyChanged("PopUpName");
            }
        }

        private Visibility _checkIcon;
        public Visibility CheckIcon
        {
            get { return _checkIcon; }
            set
            {
                _checkIcon = value;
                OnPropertyChanged("CheckIcon");
            }
        }

        private string _popUpLabel;

        public string PopUpLabel
        {
            get { return _popUpLabel; }
            set
            {
                _popUpLabel = value;
                OnPropertyChanged("PopUpLabel");
            }
        }

        private UIElement _popUpPlacementTarget;

        public UIElement PopUpPlacementTarget
        {
            get { return _popUpPlacementTarget; }
            set
            {
                _popUpPlacementTarget = value;
                OnPropertyChanged("PopUpPlacementTarget");
            }
        }

        private bool _popUpIsOpen;

        public bool PopUpIsOpen
        {
            get { return _popUpIsOpen; }
            set
            {
                _popUpIsOpen = value;
                OnPropertyChanged("PopUpIsOpen");
            }
        }

        private string[] _cmbCfgItemSource;

        public string[] CmbCfgItemSource
        {
            get { return _cmbCfgItemSource; }
            set
            {
                _cmbCfgItemSource = value;
                OnPropertyChanged("CmbCfgItemSource");
            }
        }

        #endregion

        #region Commands

        public ICommand SaveCfg
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    CanValidate = true;
                    if (String.IsNullOrEmpty(this.CfgFilename)) return;

                    GlobalVars.MainWindow.VM.CfgFilename = this.CfgFilename;
                    this.CheckIcon = Visibility.Visible;
                    this.Save(filename: this.CfgFilename, configType: Type);
                    this.UpdateCfgRecentData(isSaving: true);
                    this.SaveCurrentCfgSetting();
                });
            }
        }

        public ICommand LoadDefaultCfg
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    CanValidate = false;
                    GlobalVars.MainWindow.VM.CfgFilename = StringResource.Default;
                    this.LoadCfgState(filename: StringResource.Default, isDefault: true);
                    this.CfgFilename = string.Empty;
                    this.UpdateCfgRecentData(isSaving: true);
                    this.SaveCurrentCfgSetting();
                });
            }
        }

        public ICommand CloseCfg
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    this.CloseWindow();
                });
            }
        }

        public ICommand ToggleButton_Click
        {
            get
            {
                return new DelegateCommand((sender) =>
                {
                    var toggle = sender as Button;

                    if (toggle != null)
                    {
                        PopUpPlacementTarget = toggle;
                        PopUpName = toggle.Name;
                        PopUpItems = this.GetItemSource(sender);
                        PopUpIsOpen = true;
                    }
                });
            }
        }

        public ICommand CmbCFG_SelectionChanged
        {
            get
            {
                return new DelegateCommand((sender) =>
                {
                    var cmb = sender as ComboBox;

                    if (cmb != null)
                    {
                        if (cmb.SelectedValue != null)
                        {
                            LoadCfgState(filename: cmb.SelectedValue.ToString(), isDefault: false);
                        }
                    }
                });
            }
        }

        public ICommand ChkList_CheckChanged
        {
            get
            {
                return new DelegateCommand((sender) =>
                {
                    var checkbox = sender as CheckBox;
                    if (checkbox != null)
                    {
                        if (!_isCheckSourceChanged)
                        {
                            var source = GetItemSource(checkbox);
                            if (source != null)
                            {
                                foreach (var item in source)
                                {
                                    item.IsChecked = checkbox.IsChecked.Value;
                                }
                                PopUpItems = null;
                            }
                        }
                    }
                });
            }
        }

        public ICommand Window_Loaded
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    this.UpdateCfgRecentData(isSaving: false);
                });
            }
        }

        public ICommand Window_Closed
        {
            get
            {
                return new DelegateCommand(() =>
                {

                });
            }
        }

        public ICommand PopUpCheckBoxList_CheckedChanged
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _isCheckSourceChanged = true;
                    this.CheckSourceChanged();
                    _isCheckSourceChanged = false;
                });
            }
        }

        #endregion

        #region CFG Utility
        /// <summary>
        /// Get User-defined CFG's on Model Report Template Directory
        /// </summary>
        /// <param name="configType"></param>
        /// <returns></returns>
        public string[] GetCfgFiles(string configType)
        {
            string path = Path.Combine(GlobalVars.ModelReportTemplateDir, configType);
            this._utilities.CreateDirectory(path);

            var files = this._utilities.GetFiles(path, StringResource.cfg);
            var list = new string[files.Length];

            int counter = 0;
            foreach (var file in files)
            {
                list[counter] = Path.GetFileNameWithoutExtension(file.Name);
                counter++;
            }

            return list;
        }

        public void UpdateCfgRecentData(bool isSaving)
        {
            //this._utilities.CreateDirectory(GlobalVars.LocalAppFolder);
            this._utilities.CreateDirectory(GlobalVars.ModelFolder);

            string filePath = Path.Combine(GlobalVars.ModelFolder, "pkgrecentdata.xml");
            XDocument xDoc = XDocument.Load(filePath);

            var root = xDoc.Root;
            foreach (var item in root.Elements())
            {
                if (item.Name == StringResource.cfg)
                {
                    if (isSaving) item.Value = this.CfgFilename;
                    else
                    {
                        this.CfgFilename = item.Value;
                        if (GlobalVars.cfgModel == null)
                        {
                            this.LoadCfgState(this.CfgFilename, isDefault: false);
                        }
                        else
                        {
                            // Load default if CfgFilename is empty and GlobalVars.cfgModel is not null
                            this.LoadCfgState
                            (
                                this.CfgFilename == string.Empty ? StringResource.Default : this.CfgFilename,
                                isDefault: this.CfgFilename == string.Empty
                            );
                        }

                    }
                }
                if (item.Name == StringResource.type)
                { if (isSaving) item.Value = this.Type; }
            }

            if (isSaving) xDoc.Save(filePath);
        }

        public void LoadCfgState(string filename, bool isDefault)
        {
            // Load Cfg Check State: Get XDocument
            XDocument xDoc = GetCfgXDocument
                (
                    filename,
                    this.Type,
                    (isDefault ?
                        (Directory.Exists(GlobalVars.FirmReportTemplateDir) ?
                            GlobalVars.FirmReportTemplateDir
                            : Path.Combine(GlobalVars.LocalAppPackageToolFolder, @"ReportTemplate"))
                    : GlobalVars.ModelReportTemplateDir)
                );
            // -- get root elements
            var root = xDoc.Root;
            if (root != null)
            {
                foreach (var itemElements in root.Elements())
                {
                    switch (itemElements.Name.ToString())
                    {
                        case "pdf":
                            this.PDF = !string.IsNullOrEmpty(itemElements.Value) ? Convert.ToBoolean(itemElements.Value) : false;
                            break;
                        case "nc1":
                            //this.NC1 = !string.IsNullOrEmpty(itemElements.Value) ? Convert.ToBoolean(itemElements.Value) : false;
                            this.NCItems = this.UpdateNCItems(itemElements);
                            this.UpdateCheckState(itemElements, GlobalVars.cfgDialog.ChkNC);
                            break;
                        case "kss":
                            this.KSS = !string.IsNullOrEmpty(itemElements.Value) ? Convert.ToBoolean(itemElements.Value) : false;
                            break;
                        case "dwg":
                            this.DWG = !string.IsNullOrEmpty(itemElements.Value) ? Convert.ToBoolean(itemElements.Value) : false;
                            break;
                        case "dxf":
                            this.DXF = !string.IsNullOrEmpty(itemElements.Value) ? Convert.ToBoolean(itemElements.Value) : false;
                            break;
                        case "ifc":
                            this.IFC = !string.IsNullOrEmpty(itemElements.Value) ? Convert.ToBoolean(itemElements.Value) : false;
                            break;
                        case "fabtrol":
                            this.FabTrolItems = this.UpdateReportTemplate(itemElements);
                            this.UpdateCheckState(itemElements, GlobalVars.cfgDialog.ChkFABTROL);
                            break;
                        case "boltlist":
                            this.BoltListItems = this.UpdateReportTemplate(itemElements);
                            this.UpdateCheckState(itemElements, GlobalVars.cfgDialog.ChkBOLT);
                            break;
                        case "xsr":
                            this.XSRItems = this.UpdateReportTemplate(itemElements);
                            this.UpdateCheckState(itemElements, GlobalVars.cfgDialog.ChkXSR);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void UpdateCheckState(XElement itemElements, object obj)
        {
            if (itemElements.Value.Contains("False") && itemElements.Value.Contains("True"))
                (obj as CheckBox).IsChecked = null;
            if ((itemElements.Value.Contains("False") && !itemElements.Value.Contains("True")) || itemElements.Value == string.Empty)
                (obj as CheckBox).IsChecked = false;
            if (!itemElements.Value.Contains("False") && itemElements.Value.Contains("True"))
                (obj as CheckBox).IsChecked = true;
        }

        private bool? UpdateSourceCheckState(ObservableCollection<ItemTemplate> items)
        {
            string temp = "";
            bool? value = null;
            foreach (var item in items) temp += item.IsChecked.ToString();

            if (temp.Contains("False") && temp.Contains("True"))
                value = null;
            if (temp.Contains("False") && !temp.Contains("True"))
                value = false;
            if (!temp.Contains("False") && temp.Contains("True"))
                value = true;

            return value;
        }

        public void CheckSourceChanged()
        {
            if (this.PopUpName != null)
            {
                if (this.PopUpName.Contains("FABTROL"))
                {
                    GlobalVars.cfgDialog.ChkFABTROL.IsChecked = UpdateSourceCheckState(this.FabTrolItems);
                }
                if (this.PopUpName.Contains("BOLT"))
                {
                    GlobalVars.cfgDialog.ChkBOLT.IsChecked = UpdateSourceCheckState(this.BoltListItems);
                }
                if (this.PopUpName.Contains("XSR"))
                {
                    GlobalVars.cfgDialog.ChkXSR.IsChecked = UpdateSourceCheckState(this.XSRItems);
                }
                if (this.PopUpName.Contains("NC"))
                {
                    GlobalVars.cfgDialog.ChkNC.IsChecked = UpdateSourceCheckState(this.NCItems);
                }
            }
        }

        private ObservableCollection<ItemTemplate> UpdateReportTemplate(XElement itemElements)
        {
            var items = new ObservableCollection<ItemTemplate>();

            var helper = new Rnd.TeklaStructure.Helper.Utilities();
            List<string> fileDirectories = new List<string>();
            helper.AddPaths(fileDirectories, "XS_TEMPLATE_DIRECTORY");
            fileDirectories.Add(@"./");
            helper.AddPaths(fileDirectories, "XS_PROJECT");
            helper.AddPaths(fileDirectories, "XS_FIRM");
            helper.AddPaths(fileDirectories, "XS_TEMPLATE_DIRECTORY_SYSTEM");
            helper.AddPaths(fileDirectories, "XS_SYSTEM");
            string[] reportNames = helper.GetMultiDirectoryList(fileDirectories, "rpt");

            foreach (var file in reportNames)
            {
                if (FilterReportFiles(itemElements.Name.ToString(), file.ToLower()))
                {
                    items.Add(
                    new ItemTemplate
                    {
                        Name = file,
                        IsChecked = Convert.ToBoolean((from e in itemElements.Elements("item")
                                                       where e.Attribute("name").Value == file
                                                       select e.Value).FirstOrDefault())
                    });
                }
            }

            #region old code
            //string firm = Path.Combine(GlobalVars.FirmReportDir, itemElements.Name.ToString());
            //string firm = Path.Combine(GlobalVars.FirmReportDir);

            //if (Directory.Exists(firm))
            //{
            //    //var firmItems = this._utilities.GetFiles(firm, "*");
            //    var firmItems = this._utilities.GetFiles(firm, "rpt");

            //    foreach (var file in firmItems)
            //    {
            //        if (FilterReportFiles(itemElements.Name.ToString(), file.Name.ToLower()))
            //        {
            //            items.Add(
            //            new ItemTemplate
            //            {
            //                Name = Path.GetFileNameWithoutExtension(file.Name),
            //                IsChecked = Convert.ToBoolean((from e in itemElements.Elements("item")
            //                                               where e.Attribute("name").Value == Path.GetFileNameWithoutExtension(file.Name)
            //                                               select e.Value).FirstOrDefault())
            //            });
            //        }
            //    }
            //}
            #endregion


            return items;
        }

        private bool FilterReportFiles(string source, string filename)
        {
            bool addItem = false;
            switch (source)
            {
                case "xsr":
                    if (!(filename.Contains("bolt") && filename.Contains("list")) && !filename.Contains("fabtrol"))
                    { addItem = true; }
                    break;
                case "boltlist":
                    if (filename.Contains("bolt") && filename.Contains("list") && !filename.Contains("fabtrol"))
                    { addItem = true; }
                    break;
                case "fabtrol":
                    if (filename.Contains("fabtrol"))
                    { addItem = true; }
                    break;
                default:
                    break;
            }
            return addItem;
        }

        private ObservableCollection<ItemTemplate> UpdateNCItems(XElement itemElements)
        {
            var items = new ObservableCollection<ItemTemplate>();

            items.Add(
                    new ItemTemplate
                    {
                        Name = "DSTV for Angles",
                        IsChecked = Convert.ToBoolean((from e in itemElements.Elements("item")
                                                       where e.Attribute("name").Value == Path.GetFileNameWithoutExtension("DSTV for Angles")
                                                       select e.Value).FirstOrDefault())
                    });
            items.Add(
                    new ItemTemplate
                    {
                        Name = "DSTV for Plates",
                        IsChecked = Convert.ToBoolean((from e in itemElements.Elements("item")
                                                       where e.Attribute("name").Value == Path.GetFileNameWithoutExtension("DSTV for Plates")
                                                       select e.Value).FirstOrDefault())
                    });
            items.Add(
                    new ItemTemplate
                    {
                        Name = "DSTV for Profiles",
                        IsChecked = Convert.ToBoolean((from e in itemElements.Elements("item")
                                                       where e.Attribute("name").Value == Path.GetFileNameWithoutExtension("DSTV for Profiles")
                                                       select e.Value).FirstOrDefault())
                    });

            return items;

        }

        private void UpdatePopUpLabel()
        {
            if (PopUpName.Contains("NC"))
                this.PopUpLabel = "NC File Settings:";
            else
                this.PopUpLabel = "Report templates:";
        }

        private XDocument GetCfgXDocument(string filename, string configType, string dir)
        {
            string path = Path.Combine(dir, configType);
            XDocument xDoc = new XDocument();

            this._utilities.CreateDirectory(path);
            string xFile = this._utilities.PathFilename(path, filename, StringResource.cfg);
            if (File.Exists(xFile))
            { xDoc = this._utilities.XdocReadXml(xFile); }
            return xDoc;
        }

        private void Save(string filename, string configType)
        {
            XDocument xDoc = GetCfgXmlTemplate();

            var root = xDoc.Root;
            foreach (var itemElements in root.Elements())
            {
                switch (itemElements.Name.ToString())
                {
                    case "pdf":
                        itemElements.Value = this.PDF.ToString();
                        break;
                    case "nc1":
                        //itemElements.Value = this.NC1.ToString();
                        this.UpdateListItems(itemElements, this.NCItems);
                        break;
                    case "kss":
                        itemElements.Value = this.KSS.ToString();
                        break;
                    case "dwg":
                        itemElements.Value = this.DWG.ToString();
                        break;
                    case "dxf":
                        itemElements.Value = this.DXF.ToString();
                        break;
                    case "ifc":
                        itemElements.Value = this.IFC.ToString();
                        break;
                    case "fabtrol":
                        this.UpdateListItems(itemElements, this.FabTrolItems);
                        break;
                    case "boltlist":
                        this.UpdateListItems(itemElements, this.BoltListItems);
                        break;
                    case "xsr":
                        this.UpdateListItems(itemElements, this.XSRItems);
                        break;
                    default:
                        break;
                }

            }
            string path = Path.Combine(GlobalVars.ModelReportTemplateDir, configType);
            _utilities.CreateDirectory(path);

            // overwrite saving
            xDoc.Save(_utilities.PathFilename(path, filename, StringResource.cfg));
        }

        private XDocument GetCfgXmlTemplate()
        {
            var xDoc = new XDocument
            (
                new XDeclaration("1.0", "utf-8", "yes"),
                new XComment("CFG Report Templates"),
                new XElement
                (
                    "cfg",
                    new XElement("pdf"),
                    new XElement("nc1"),
                    new XElement("kss"),
                    new XElement("dwg"),
                    new XElement("dxf"),
                    new XElement("ifc"),
                    new XElement("fabtrol"),
                    new XElement("boltlist"),
                    new XElement("xsr")
                )
            );
            return xDoc;
        }

        private void UpdateListItems(XElement itemElements, ObservableCollection<ItemTemplate> source)
        {
            itemElements.RemoveAll();
            if (source != null)
            {
                foreach (var item in source)
                {
                    XElement x = new XElement("item", item.IsChecked.ToString(), new XAttribute("name", item.Name));
                    itemElements.Add(x);
                }
            }
        }

        public ObservableCollection<ItemTemplate> GetItemSource(object sender)
        {
            Control control;
            if (sender is CheckBox)
            { control = sender as CheckBox; }
            else
            { control = sender as Button; }
            return control.Name.Contains("FABTROL") ? this.FabTrolItems
                : control.Name.Contains("BOLT") ? this.BoltListItems
                : control.Name.Contains("XSR") ? this.XSRItems
                : this.NCItems;
        }

        private void CloseWindow()
        {
            GlobalVars.MainWindow.BtnIncludFiles.IsChecked = false;
            GlobalVars.cfgDialog.Close();
        }

        private void SaveCurrentCfgSetting()
        {
            GlobalVars.cfgModel = new CfgModel
            {
                PDF = this.PDF,
                NC = this.NC == null || (bool)this.NC,
                NCItems = this.NCItems,
                KSS = this.KSS,
                DWG = this.DWG,
                DXF = this.DXF,
                IFC = this.IFC,
                FABTROL = this.FABTROL == null || (bool)this.FABTROL,
                FabTrolItems = this.FabTrolItems,
                BOLTLIST = this.BOLTLIST == null || (bool)this.BOLTLIST,
                BoltListItems = this.BoltListItems,
                XSR = this.XSR == null || (bool)this.XSR,
                XSRItems = this.XSRItems
            };

            var file = Path.Combine(GlobalVars.LocalAppPackageToolFolder, "reports.txt");

            using (StreamWriter wr = new StreamWriter(file))
            {
                foreach (var item in ReportList() /*GlobalVars.cfgModel.FabTrolItems.Where(o => o.IsChecked).Select(s => s.Name)*/)
                {
                    wr.WriteLine(item);
                }
            }

            GlobalVars.GetXsrReports();

            this._utilities.SerializeBinFile(GlobalVars.LocalCfgBinFile, GlobalVars.cfgModel);
        }

        public List<string> ReportList()
        {
            var rpt1 = GlobalVars.cfgModel.FabTrolItems;
            var rpt2 = GlobalVars.cfgModel.BoltListItems;
            var rpt3 = GlobalVars.cfgModel.XSRItems;

            if (rpt1 == null && rpt2 == null && rpt3 == null)
                return new List<string>();
            else
            {
                return rpt1.Where(i => i.IsChecked).Select(s => s.Name)
               .Concat(rpt2.Where(i => i.IsChecked).Select(s => s.Name))
               .Concat(rpt3.Where(i => i.IsChecked).Select(s => s.Name)).ToList();
            }

        }

        private void UpdateCfgItemSource()
        {
            if (Type != string.Empty)
            {
                CmbCfgItemSource = null;
                CmbCfgItemSource = this.GetCfgFiles(configType: Type);
            }
        }

        #endregion

        #region Validation

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
                    if (columnName == StringResource.CfgFilename)
                    {

                        if (string.IsNullOrEmpty(this.CfgFilename))
                        {
                            return StringResource.CfgFilenameNotSet;
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
                OnPropertyChanged(StringResource.CfgFilename);
            }
        }
        #endregion

    }


}
