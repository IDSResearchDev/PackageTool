using PackageTool.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageTool.Model
{
    [Serializable()]
    public class CfgModel
    {
        private bool _pdf;
        public bool PDF
        {
            get { return _pdf; }
            set
            {
                _pdf = value;
            }

        }

        

        private bool _kss;

        public bool KSS
        {
            get { return _kss; }
            set
            {
                _kss = value;
            }
        }

        private bool _dwg;

        public bool DWG
        {
            get { return _dwg; }
            set
            {
                _dwg = value;
            }
        }

        private bool _dxf;

        public bool DXF
        {
            get { return _dxf; }
            set
            {
                _dxf = value;
            }
        }

        private bool _ifc;

        public bool IFC
        {
            get { return _ifc; }
            set
            {
                _ifc = value;
            }
        }

        private bool _fabtrol;
        public bool FABTROL
        {
            get { return _fabtrol; }
            set
            {
                _fabtrol = value;
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
            }
        }
        private bool _boltList;
        public bool BOLTLIST
        {
            get { return _boltList; }
            set
            {
                _boltList = value;
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
            }
        }
        private bool _xsr;
        public bool XSR
        {
            get { return _xsr; }
            set
            {
                _xsr = value;
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
            }
        }

        private bool _nc;

        public bool NC
        {
            get { return _nc; }
            set
            {
                _nc = value;
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
            }
        }
    }

    [Serializable()]
    public class ItemTemplate
    {
        public bool IsChecked { get; set; }
        public string Name { get; set; }
    }

    
}
