using Tekla.Structures;
namespace Tekla.Technology.Akit.UserScript
{
    public class Script
    {
        public static void Run(Tekla.Technology.Akit.IScript akit)
        {
            string TS_Version = GetCurrentVersion();//"21.1";
            //string TS_BinaryDir = "";
            //TeklaStructuresSettings.GetAdvancedOption("XSBIN", ref TSBinaryDir);
            string TS_Application = "PackageTool.exe";
            //string TS_Path = System.IO.Path.Combine(TS_BinaryDir, "applications\\tekla\\Model\\PackageTool\\");
            string TS_Path32 = @"C:\\Program Files (x86)\\IDS INC\\Package Tool\\" + TS_Version + "\\";
            string TS_Path64 = @"C:\\Program Files\\IDS INC\\Package Tool\\" + TS_Version + "\\";

            System.Diagnostics.Process Process = new System.Diagnostics.Process();
            Process.EnableRaisingEvents = false;

            if (System.IO.File.Exists(TS_Path32 + TS_Application))
            {
                Process.StartInfo.FileName = TS_Path32 + TS_Application;
            }
            else if (System.IO.File.Exists(TS_Path64 + TS_Application))
            {
                Process.StartInfo.FileName = TS_Path64 + TS_Application;                
            }
            else
            {
                System.Windows.Forms.MessageBox.Show(TS_Application + " not found, application stopped!\n\nCheck the PackageTool Installation Folder.", "Package Tool", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }

            Process.Start();
            Process.Close();
        }

        private static string GetCurrentVersion()
        {
            string currentVersion = TeklaStructuresInfo.GetCurrentProgramVersion();
            var version = currentVersion.Split(' ');

            switch (version)
            {
                case "2016":
                    return "2016.0";
                case "2016i":
                    return "2016.1";
                case "2017":
                    return "2017.0";
                default:
                    break;
            }

            return version[0];
        }
    }
}
