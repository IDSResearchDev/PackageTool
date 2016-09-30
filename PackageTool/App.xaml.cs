using PackageTool.ViewModel;
using Rnd.Common.Resources;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace PackageTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception.InnerException != null)
            {
                if (e.Exception.InnerException.Message == "Transmittal letter is open.")
                {
                    MessageBox.Show(string.Concat("Please close related documents before creating this model.",
                                                  Environment.NewLine, "Path: ", GlobalVars.OutputTransmittalLetter),
                                                  "Transmittal letter is open", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show(e.Exception.Message, StringResource.ExceptionCaught, MessageBoxButton.OK, MessageBoxImage.Error);

                this.CheckApplicationException(e.Exception.Message);
                new Rnd.TeklaStructure.Helper.Utilities().GetConncectionStatus();
            }

            e.Handled = true;
        }

        private void CheckApplicationException(string message)
        {
            if (message == ErrorCollection.NoOpenModel || message == ErrorCollection.TeklaNotRunning)
                GlobalVars.MainWindow.Close();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            CheckApplicationInstance();
        }

        private void CheckApplicationInstance()
        {
            System.Diagnostics.Process proc = System.Diagnostics.Process.GetCurrentProcess();
            int count = System.Diagnostics.Process.GetProcesses().Where(p =>
                             p.ProcessName == proc.ProcessName).Count();
            if (count > 1)
            {
                MessageBox.Show(StringResource.PackageAlreadyRunning);
                App.Current.Shutdown();
            }
        }


        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            MessageBox.Show(ex.Message, "Uncaught Thread Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }


    }

}
