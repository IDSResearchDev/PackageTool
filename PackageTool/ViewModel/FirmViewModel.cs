using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using PackageTool.BaseClass;
using PackageTool.Properties;
using Rnd.Common;
using Rnd.Common.Resources;
using MessageBox = System.Windows.MessageBox;

namespace PackageTool.ViewModel
{
    public class FirmViewModel: ViewModelBase, IDataErrorInfo
    {
        private readonly Utilities _utilities;

    #region Properties
        private string _firmfolder;
        public string FirmFolder
        {
            get { return _firmfolder; }
            set
            {
                if (_firmfolder == value) return;
                _firmfolder = value;                
                OnPropertyChanged("FirmFolder");
            }
        } 
    #endregion

        public FirmViewModel()
        {
            _utilities = new Utilities();
            FirmFolder = Settings.Default.strFirmFolderLocation;
        }

    #region Commands 

        public ICommand CloseFolderSettings
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    Close();

                });
            }
        }

        public ICommand BrowseFirmFolder
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    var dialog = _utilities.FolderDialog();

                    var dialogresult = dialog.Item1;
                    var selectedpath = dialog.Item2;

                    if (dialogresult != DialogResult.OK)
                    {
                        FirmFolder = Settings.Default.strFirmFolderLocation;
                        return;
                    }

                    FirmFolder = selectedpath;
                   

                });
            }
        }

        public ICommand CancelFirmFolderSettings
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    Close();
                });
            }
        }

        public ICommand SaveFirmFolderSettings
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (!_utilities.CheckIfDirectoryExists(FirmFolder)) return;
                    
                        Settings.Default.strFirmFolderLocation = FirmFolder;
                        var msgResult = MessageBox.Show(StringResource.SaveFirmDirectoryQuestion, StringResource.FolderSetting, MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (msgResult == MessageBoxResult.Yes)
                        {
                            Settings.Default.Save();
                            this.Close();
                        }
                });
            }
        }
        
     #endregion



        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get
            {
                if (columnName == "FirmFolder")
                {
                    if ((string.IsNullOrEmpty(this.FirmFolder) || (string.IsNullOrWhiteSpace(this.FirmFolder))))
                    {
                        return StringResource.FirmFolderNotSet;
                    }
                }

                return string.Empty;
            }

        }
    }
}
