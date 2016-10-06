using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PackageTool.ViewModel;
using Rnd.Common;

namespace PackageTool.View
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            this.VM.IsKeyLeftControl = e.Key == Key.LeftCtrl;            
        }
    }
}
