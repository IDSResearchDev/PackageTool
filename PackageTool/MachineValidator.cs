using PackageTool.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PackageTool
{
    public static class MachineValidator
    {
        static string localAppFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
        static string activationFilePath = Path.Combine(localAppFolder, "activation.bin");
        static string appFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.exe");


        static string apptivatorFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"IDS Inc\Apptivator", "Apptivator.exe");
        //Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Apptivator", "Apptivator.exe");
        //@"C:\Users\J. Mon\Documents\apptivator\Apptivator\Apptivator\bin\Debug\Apptivator.exe"; //Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Apptivator.exe");  
        static string spaceSaver = "%20%";

        public static void Run()
        {
            bool isActivated = false;
            if (!File.Exists(activationFilePath))
            {
                if (File.Exists(apptivatorFilePath))
                {
                    Process p = new Process();
                    p.StartInfo.FileName = apptivatorFilePath;
                    p.StartInfo.Arguments = $"{activationFilePath.Replace(" ", spaceSaver)} {appFilePath.Replace(" ", spaceSaver)} {spaceSaver} {PackageTool.Properties.Resources.ActivationUrl}";
                    p.Start();
                }
                else
                { MessageBox.Show("Apptivator not found.", "App", MessageBoxButton.OK, MessageBoxImage.Information); }
            }
            else
            {
                Rnd.Common.Utilities util = new Rnd.Common.Utilities();
                string mac = util.GetPhysicalAddress();
                var activation = util.DeserializeBinFile<Rnd.Common.Models.Activator>(activationFilePath);
                if (activation.MacAddress.Equals(mac))
                {
                    isActivated = true;
                    MainView main = new MainView();
                    main.Show();
                }
                else
                {
                    MessageBox.Show("Activation code is needed in this machine.", "Activation required", MessageBoxButton.OK, MessageBoxImage.Information);
                    File.Delete(activationFilePath);
                }
            }

            if (!isActivated)
                App.Current.Shutdown();
        }
    }
}
