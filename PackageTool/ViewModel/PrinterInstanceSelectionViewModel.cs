using PackageTool.BaseClass;
using PackageTool.Model;
using Rnd.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PackageTool.ViewModel
{
    public class PrinterInstanceSelectionViewModel : ViewModelBase
    {


        public PrinterInstanceSelectionViewModel()
        {
            PrinterSelection = new ObservableCollection<PrinterSelectionModel>();
            IsSaving = false;
        }

        private void UpdateViewState()
        {
            if (IsViewing)
            {
                Message = "The following are the configured Printer Instance Setting";
                Icon = Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Information.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            else
            {
                Message = "Printer Instance not found. Please select Printer Instance for the following Paper Sizes:";
                Icon = Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Exclamation.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
        }

        #region Properties

        private bool _isSaving;

        public bool IsSaving
        {
            get { return _isSaving; }
            set
            {
                _isSaving = value;
                OnPropertyChanged("IsSaving");
            }
        }

        private bool _isViewing;
        public bool IsViewing
        {
            get { return _isViewing; }
            set
            {
                _isViewing = value;
                OnPropertyChanged("IsViewing");
                this.UpdateViewState();
            }
        }

        private string _message;
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
                OnPropertyChanged("Message");
            }
        }

        private ImageSource _icon;
        public ImageSource Icon
        {
            get
            {
                return _icon;
            }
            set
            {
                _icon = value;
                OnPropertyChanged("Icon");
            }
        }

        private ObservableCollection<PrinterSelectionModel> _printerSelection;
        public ObservableCollection<PrinterSelectionModel> PrinterSelection
        {
            get
            {
                return this._printerSelection;
            }
            set
            {
                this._printerSelection = value;
                OnPropertyChanged("PrinterSelection");
            }
        }
        #endregion

        #region Command
        public ICommand AcceptButton_OnClick
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    IsSaving = true;
                    if (IsViewing)
                    { GlobalVars.PrinterSelection.Clear(); }
                    foreach (var item in PrinterSelection)
                        GlobalVars.PrinterSelection.Add(item);
                    new Utilities().SerializeBinFile(GlobalVars.LocalPrinterInstanceBinFile, GlobalVars.PrinterSelection);
                    this.Close();
                });
            }
        }

        public ICommand Window_Closed
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    GlobalVars.SuspendProcess = false;
                });
            }
        }
        #endregion

    }
}
