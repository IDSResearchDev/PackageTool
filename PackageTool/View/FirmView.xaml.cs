using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PackageTool.View
{
    /// <summary>
    /// Interaction logic for FirmView.xaml
    /// </summary>
    public partial class FirmView : RNDWindow
    {
        public FirmView()
        {
            InitializeComponent();
            //Style = (Style)FindResource(typeof(Window));
        }
    }
}
