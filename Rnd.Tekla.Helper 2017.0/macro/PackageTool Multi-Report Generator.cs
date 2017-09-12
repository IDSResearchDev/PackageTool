using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using Tekla.Structures;
using Tekla.Structures.Model;
using Tekla.Technology.Akit.UserScript;
using Tekla.Technology.Scripting;

[assembly: Tekla.Technology.Scripting.Compiler.Reference(@"%XSBIN%\plugins\Interop.Excel.dll")]

namespace Tekla.Technology.Akit.UserScript
{
    /// <summary>
    /// A scripted macro tool to easily generate multiple reports.
    /// </summary>
    /// <remarks>Version 1.2 2008.08.04
    /// Eric Beyer
    /// eric.beyer@tekla.com
    /// </remarks>
    /// 
    /// 
    public class Checker
    {
        // ------- LOCALIZATION STARTS HERE -------- //
        // Change this variable to write normal text reports to something other than ".xsr" files
        public const string DEFAULT_REPORT_FILE_EXTENSION = ".xsr";
        // ------- LOCALIZATION ENDS HERE -------- //

        // *********************************************************************************
        // **Changing code beyond this point may cause the macro to fail.                 **
        // **Changes should only be made by persons with sufficient programming knowledge **
        // *********************************************************************************
        public static string[] REPORT_SEARCH_DIRECTORIES;
        private static string m_TSLanguage;
        private string lbl_EditReportConfirmation = "You are about to edit the following report:";

        private Excel.Application oExcelApplication = null;
        private Excel.Workbook oExcelWorkbook = null;
        public static bool bRadioButtonDialogValue;

        private static object missingValue = System.Reflection.Missing.Value;

        #region CONSTANT VARIABLES

        private const string PROCESS_NAME_EXCEL = "Excel";
        private const string EXTENSION_EXCEL = "Excel";
        private const string EXCEL_WORKBOOK_AND_INTERFACE_FUNCTION_NAME = "ThisWorkbook.ReadTeklaStructuresReport";
        private const string REPORTS_FOLDER = "Reports";
        private const string EXCELTEMPLATES_FOLDER = "Excel_templates";
        private const string XLS_EXTENSION = ".xls";
        private const string EXCEL_EXTENSION = ".Excel";

        private const string USING_THREE_DIGITS = "000";
        private const string BACKSLASH = "\\";
        private const string DOT = ".";
        private const string UNDERSCORE = "_";
        private const char CHAR_DOT = '.';
        private const char CHAR_SEMICOLON = ';';

        private const string AKIT_SELECTED = "1";
        private const string AKIT_NOTSELECTED = "0";

        private const string RUUKKI_SAP_REPORT_NAME = "R-TS_SAP_transferfile_ShapeCode";

        private const string RUUKKI_SAP_PLUGIN_EXE_MISSING = "Error running SAPShapeCodePlugin.exe! Shape codes will not be set";
        private const string RUUKKI_SAP_SHAPE_CODES_NOT_SET_WITH_ALL_OBJECTS = "Shape codes cannot be set when Report Objects - All Objects is selected";
        public const string RUUKKI_SAP_PROGRAM_EXE = "SAPShapeCodePlugin.exe";

        #endregion
        private object osFileName;

        #region OPEN WORKBOOK VARIABLES
        private object oiUpdateLinks = 0;
        private object obReadOnly = false;
        private object oiFormat = 1;
        private object oPassword = missingValue;
        private object oWriteResPassword = missingValue;
        private object obIgnoreReadOnlyRecommend = true;
        private object oOrigin = missingValue;
        private object oDelimiter = missingValue;
        private object obEditable = false;
        private object obNotify = false;
        private object oConverter = missingValue;
        private object obAddToMru = false;
        private object obLocal = false;
        #endregion
        #region CLOSE WORKBOOK VARIABLES
        private object obSaveChanges = false;
        private object obRouteWorkbook = false;
        #endregion

        #region PROJECT PROPERTIES
        private string _jobCode;
        private string _jobnumber;
        private string _fabricator;
        private string _fabaddress;

        public string Fabricator
        {
            get { return _fabricator; }
            set { _fabricator = value; }
        }
        public string JobNumber
        {
            get { return _jobnumber; }
            set
            {
                _jobnumber = value;
            }
        }
        public string JobCode
        {
            get { return _jobCode; }
            set { _jobCode = value; }
        }
        public string Fabaddress
        {
            get { return _fabaddress; }

            set
            {
                _fabaddress = value;
            }
        }
        #endregion

        private string _title1;
        private string _title2;
        private string _title3;


        private void GetProjectProperties()
        {
            Model model = new Model();
            if (!model.GetConnectionStatus()) { throw new ArgumentException("TeklaStructure is not running: Package Tool should be executed in Tekla Toolbar Macro."); }

            ProjectInfo projectInfo = model.GetProjectInfo();
            if (projectInfo == null) { throw new ArgumentException("There's no open Tekla Model."); }

            string fab = "", addrs = "";
            projectInfo.GetUserProperty("FAB_NAME", ref fab);
            projectInfo.GetUserProperty("FAB_ADDRESS", ref addrs);

            _fabaddress = addrs;
            _fabricator = fab;
            _jobCode = projectInfo.Info2;
            _jobnumber = projectInfo.ProjectNumber;

            //var str = "Fabricator: " + _fabricator + "\n" + "FabAddress: " + _fabaddress + "\n" + "JobCode: " + _jobCode +"\n" + "JobNumber: " + _jobnumber;
            //System.Windows.Forms.MessageBox.Show(str);
        }

        public Checker()
        {

            List<string> reportSearchDirectories = new List<string>();
            TeklaControls.EnvironmentFiles.AddPaths(reportSearchDirectories, "XS_TEMPLATE_DIRECTORY");
            reportSearchDirectories.Add(@"./");
            TeklaControls.EnvironmentFiles.AddPaths(reportSearchDirectories, "XS_PROJECT");
            TeklaControls.EnvironmentFiles.AddPaths(reportSearchDirectories, "XS_FIRM");
            TeklaControls.EnvironmentFiles.AddPaths(reportSearchDirectories, "XS_TEMPLATE_DIRECTORY_SYSTEM");
            TeklaControls.EnvironmentFiles.AddPaths(reportSearchDirectories, "XS_SYSTEM");
            TeklaControls.EnvironmentFiles.PropertyFileDirectories = reportSearchDirectories;

            //TeklaControls.EnvironmentVariables.Add("XS_REPORT_OUTPUT_DIRECTORY");
            TeklaControls.EnvironmentVariables.Add("XS_START_PARAMETERS");
            TeklaControls.EnvironmentVariables.Add("XS_TPLED_DIRECTORY");
            TeklaControls.EnvironmentVariables.Add("XS_DIR");

            GetProjectProperties();

            //var slash = "./";
            //var  = @"G:\IDS_Firm\Beauce Atlas, Inc";
            //var = @"C:\TeklaStructures\21.0\environments\usimp\template\";
            //var = @"C:\TeklaStructures\21.0\environments\usimp\rolse\steel\inp\";
            //var = @"C:\TeklaStructures\21.0\environments\usimp\rolse\steel\system\";
            //var = @"C:\TeklaStructures\21.0\environments\usimp\system\";
            //var = @".\Reports";
            //var = @"";
            //var = @"C:\TeklaStructures\21.0\nt\tpled\";
            //var = @"C:\TeklaStructures\21.0\";


            //var str = string.Empty;
            //foreach (var reportSearchDirectory in reportSearchDirectories)
            //{
            //    str = string.Concat(str, "\n", reportSearchDirectory);
            //}
            //var str2 = String.Concat(TeklaControls.EnvironmentVariables.Get("XS_REPORT_OUTPUT_DIRECTORY"), "\n",
            //TeklaControls.EnvironmentVariables.Get("XS_START_PARAMETERS"), "\n",
            //TeklaControls.EnvironmentVariables.Get("XS_TPLED_DIRECTORY"), "\n",
            //TeklaControls.EnvironmentVariables.Get("XS_DIR"));

            //MessageBox.Show(str + "\n" + str2);

            Create();
        }

        private List<string> xsrtitlelList;
        private void xsrtitle()
        {
            xsrtitlelList = new List<string>();

            string TS_Version = GetCurrentVersion();
            var xsrtitlepath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "PackageTool", TS_Version, "xsrtitle.txt");

            using (var reader = new System.IO.StreamReader(xsrtitlepath))
            {
                while (!reader.EndOfStream)
                {
                    var readLine = reader.ReadLine();
                    xsrtitlelList.Add(readLine);
                }
            }

            _title1 = xsrtitlelList[0].Replace("XS_TITLE1=", "");
            _title2 = xsrtitlelList[1].Replace("XS_TITLE2=", "");
            _title3 = xsrtitlelList[2].Replace("XS_TITLE3=", "");
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

        private void Create()
        {

            List<string> lFilenames = new List<string>();

            string sCreatedReportName = "";

            //bRadioButtonDialogValue = this.radioButtonDialog.Checked;

            List<string> namesList = new List<string>();
            //namesList.Add("350   Bolt list");
            //namesList.Add("350   Assembly list BOM");
            int counter = 0;
            string TS_Version = GetCurrentVersion();
            var reportpath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "PackageTool", TS_Version, "reports.txt");



            using (var reader = new System.IO.StreamReader(reportpath))
            {
                while (!reader.EndOfStream)
                {
                    var readLine = reader.ReadLine();
                    namesList.Add(readLine);
                }
            }

            xsrtitle();

            //System.Windows.Forms.MessageBox.Show(_title1 + "\n" + _title2 +"\n"+_title3);

            try
            {
                //Checker chk = new Checker();
                //var str = chk.JobCode + "_" + sReportName;
                string bIsDisplayReportsSelected = "1";
                string bIsAssociatedApplicationSelected = "0";
                //string bIsDisplayReportsSelected = (this.radioButtonDontShow.Checked ? AKIT_NOTSELECTED : AKIT_SELECTED);
                //string bIsAssociatedApplicationSelected = (this.radioButtonApplication.Checked ? AKIT_SELECTED : AKIT_NOTSELECTED);

                Tekla.Structures.Model.Model MyModel = new Model();
                Tekla.Structures.Model.ModelInfo modelInfo = MyModel.GetInfo();



                //Script.OpenReportDialog(bIsDisplayReportsSelected, bIsAssociatedApplicationSelected, this.textBoxTitle1.Text, this.textBoxTitle2.Text, this.textBoxTitle3.Text);
                //doesnt show UI multi-report generator
                Script.OpenReportDialog(bIsDisplayReportsSelected, bIsAssociatedApplicationSelected, _title1, _title2, _title3);

                foreach (var item in namesList)
                {

                    //SetShapeCodesIfSAPReport(counter,item);
                    //this.checkedListBox1.Items[indexChecked].ToString(), appendDate,this.checkBoxOverwrite.Checked,this.radioSelected.Checked 

                    sCreatedReportName = Script.CreateReport(item, false, true, true);


                    if (bIsAssociatedApplicationSelected == "1")//AKIT_SELECTED
                    {
                        //checkedListBox1.Items[indexChecked].ToString(),//CHAR_DOT

                        SlitStringIntoArrayOfStrings(item, '.', false, ref lFilenames);
                        if (lFilenames.Count > 1)
                        {
                            if (lFilenames[1] == EXTENSION_EXCEL)
                            {
                                string sFileName = sCreatedReportName.Substring(0, sCreatedReportName.Length - EXTENSION_EXCEL.Length);
                                this.CheckDirectory(modelInfo.ModelPath + BACKSLASH + REPORTS_FOLDER + BACKSLASH + sFileName, lFilenames[0]);

                            }
                            else
                            {
                                string sFilePath = modelInfo.ModelPath + BACKSLASH + REPORTS_FOLDER + BACKSLASH + sCreatedReportName;
                                //string sFilePath = @"C:\Users\J. Man\Desktop\package\";

                                //Process proc = new Process();
                                //proc.StartInfo = new ProcessStartInfo(@sFilePath);
                                //proc.Start();
                            }
                        }
                        else
                        {
                            string sFilePath = modelInfo.ModelPath + BACKSLASH + REPORTS_FOLDER + BACKSLASH + sCreatedReportName;
                            //string sFilePath = @"C:\Users\J. Man\Desktop\package\";
                            //string sFilePath = path + sCreatedReportName;
                            //Process proc = new Process();
                            //proc.StartInfo = new ProcessStartInfo(@sFilePath);
                            //proc.Start();

                        }

                    }
                    counter++;
                }
                counter = 0;
                Script.CloseReportDialog();

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        #region Methods for Ruukki SAP report

        private void SetShapeCodesIfSAPReport(int indexChecked, string item)
        {

            try
            {
                //    if (item.ToLower()/*this.checkedListBox1.Items[indexChecked].ToString().ToLower()*/ == RUUKKI_SAP_REPORT_NAME.ToLower())
                //    {
                //        if (true/*this.radioAll.Checked*/)
                //        {
                //            MessageBox.Show(RUUKKI_SAP_SHAPE_CODES_NOT_SET_WITH_ALL_OBJECTS);
                //        }
                //        else
                //        {
                try
                {
                    // This will run SAP shape codes via .exe
                    Script.RunSAPSetterExe();
                }
                catch
                {
                    MessageBox.Show(RUUKKI_SAP_PLUGIN_EXE_MISSING);
                }
                //        }
                //    }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }
        #endregion
        #region Code to open Excel file, load the ASCII file into the opened Excel file
        /// <summary>
        /// Code to open Excel file, load the Text file into the opened Excel file
        /// </summary>
        /// 

        private void startExcel()
        {
            if (this.oExcelApplication == null)
            {
                this.oExcelApplication = new Excel.Application();
            }

            this.oExcelApplication.Visible = false; //Visible;
        }

        public void stopExcel()
        {
            if (this.oExcelApplication != null)
            {
                Process[] oProcess;
                oProcess = System.Diagnostics.Process.GetProcessesByName(PROCESS_NAME_EXCEL);
                oProcess[0].Kill();
            }
        }

        public bool OpenExcelFile(string sFileName, string sPassword)
        {
            bool bResult = true;

            this.osFileName = sFileName;

            if (sPassword.Length > 0)
            {
                this.oPassword = sPassword;
            }

            try
            {
                System.Globalization.CultureInfo oOldCulture;

                oOldCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
                System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");

                if (File.Exists(sFileName)) // Removes ReadOnly attribute if it is TRUE
                {
                    FileAttributes fileAttributes = File.GetAttributes(sFileName);
                    if ((fileAttributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        File.SetAttributes(sFileName, FileAttributes.Normal);
                }

                // Open a workbook in Excel
                this.oExcelWorkbook = this.oExcelApplication.Workbooks.Open(
                    sFileName, oiUpdateLinks, obReadOnly, oiFormat, oPassword,
                    oWriteResPassword, obIgnoreReadOnlyRecommend, oOrigin,
                    oDelimiter, obEditable, obNotify, oConverter, obAddToMru);

                System.Threading.Thread.CurrentThread.CurrentCulture = oOldCulture;
            }
            catch
            {
                this.CloseExcelFile();
                bResult = false;
            }
            return bResult;
        }

        public void CloseExcelFile()
        {
            oExcelWorkbook.Close(obSaveChanges, osFileName, obRouteWorkbook);
        }

        public bool CallExcelsReadTeklaStructuresReportFunction(string sCreatedReportPathName, string sTextFilename)
        {
            bool bResult = true;

            try
            {
                object oNull = null;
                object osFullMacroPath = EXCEL_WORKBOOK_AND_INTERFACE_FUNCTION_NAME;

                Script.AmendReportNameIfNeeded(ref sTextFilename, /*this.checkBoxAppendDate.Checked*/false);
                this.AddRunningNumberToPathNameWithDateIfNeeded(ref sTextFilename);

                string sProjectPath = Directory.GetCurrentDirectory();

                object oTextFilePathname = sProjectPath + BACKSLASH + REPORTS_FOLDER + BACKSLASH + sTextFilename;
                //object oTextFilePathname = @"C:\Users\J. Man\Desktop\package\" + sTextFilename;


                System.Globalization.CultureInfo oOldCulture;

                oOldCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
                System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");

                //Run the ReadTeklaStructuresReport macro to load the *.Excel file created by the report
                this.oExcelApplication.Run(osFullMacroPath, oTextFilePathname,
                                  oNull, oNull, oNull, oNull, oNull, oNull, oNull,
                                  oNull, oNull, oNull, oNull, oNull, oNull, oNull,
                                  oNull, oNull, oNull, oNull, oNull, oNull, oNull,
                                  oNull, oNull, oNull, oNull, oNull, oNull, oNull, oNull);

                System.Threading.Thread.CurrentThread.CurrentCulture = oOldCulture;
            }
            catch
            {
                MessageBox.Show("Macro opening Failed");
                bResult = false;
            }
            return bResult;
        }

        private bool AddRunningNumberToPathNameWithDateIfNeeded(ref string sTextFilename)
        {
            bool bResult = false;
            //var output = @"C:\Users\J. Man\Desktop\package\";
            string sReportPath = String.Concat(TeklaControls.EnvironmentVariables.Get("XS_REPORT_OUTPUT_DIRECTORY"), "\\");
            //MessageBox.Show(sReportPath);
            bool bFileExists = (System.IO.File.Exists(sReportPath + sTextFilename));
            bool overwrite = true;

            if (bFileExists)
            {
                if (overwrite/*this.checkBoxOverwrite.Checked*/)
                {
                    bResult = true;
                }
                else
                {
                    int iIndex = 1;
                    int dotLocation = sTextFilename.LastIndexOf(DOT);
                    string sReportRoot = sTextFilename.Substring(0, dotLocation);
                    string sFileExtension = sTextFilename.Substring(dotLocation, sTextFilename.Length - dotLocation);
                    string sNewTextFilename = String.Concat(sReportRoot, UNDERSCORE, iIndex.ToString(USING_THREE_DIGITS), sFileExtension);

                    while (System.IO.File.Exists(sReportPath + sNewTextFilename))
                    {
                        sTextFilename = String.Concat(sReportRoot, UNDERSCORE, iIndex.ToString(USING_THREE_DIGITS), sFileExtension);
                        iIndex++;
                        sNewTextFilename = String.Concat(sReportRoot, UNDERSCORE, iIndex.ToString(USING_THREE_DIGITS), sFileExtension);
                    }
                    bResult = true;
                }
            }
            return bResult;
        }
        // Separates the N path names from 1 string, character is the char that separates each pathname
        private bool SlitStringIntoArrayOfStrings(string sInput, char cCharacter,
                         bool bAddModelPathName, ref List<string> lOutput)
        {
            bool bResult = true;

            try
            {
                lOutput.Clear();

                if (sInput != "")
                {
                    //Counts the number of ; characters to alocate enough memory in the array
                    int iSize = 0;
                    foreach (char cChar in sInput)
                    {
                        if (cChar == cCharacter)
                            iSize++;
                    }
                    string[] sSplittedString = new string[iSize + 1];

                    //Finds and separates the Paths
                    char[] aCharSpliter = { cCharacter };
                    sSplittedString = sInput.Split(aCharSpliter);

                    for (int iIndex = 0; iIndex < sSplittedString.Length; iIndex++)
                    {
                        if (bAddModelPathName)
                        {
                            //Adds the full Path name to the strings
                            System.IO.DirectoryInfo oDi = new DirectoryInfo(sSplittedString[iIndex]);
                            lOutput.Add(@oDi.FullName);
                        }
                        else
                        {
                            lOutput.Add(sSplittedString[iIndex]);

                        }
                    }
                }
                else
                {
                    bResult = false;
                }
            }
            catch
            {
                bResult = false;
            }
            return bResult;
        }

        // Adds the extension \Excel_templates to the inputed path
        private bool AddExcelTemplateSubfolderToPath(ref List<string> aPaths)
        {
            if (aPaths != null)
            {
                for (int iIndex = 0; iIndex < aPaths.Count; iIndex++)
                    aPaths[iIndex] = aPaths[iIndex] + BACKSLASH + EXCELTEMPLATES_FOLDER;
                return true;
            }
            return false;
        }

        private bool CopyExcelFileToCreatedReportFile(string sCreatedReportPathName, string sExcelFilePathName)
        {
            bool bResult = true;
            try
            {
                if (File.Exists(sExcelFilePathName))
                    File.Copy(sExcelFilePathName, sCreatedReportPathName + "xls", true);
            }
            catch
            {
                bResult = false;
            }

            return bResult;
        }

        // Finds the Excel files in the computer and loads the ASCII files into the correct Excel file
        private bool OpenScriptInExcelLoadMacro(string sCreatedReportPathName, List<string> aPaths, string sFilename)
        {
            bool bFound = false;
            if (aPaths != null)
            {
                int iPathIterator = 0;

                while ((iPathIterator <= aPaths.Count - 1))
                {
                    // Start Excel, open the file corresponding to the selections in the UI and starts the Macro

                    if (File.Exists(aPaths[iPathIterator] + BACKSLASH + sFilename + XLS_EXTENSION))
                    {
                        this.startExcel();

                        this.CopyExcelFileToCreatedReportFile(sCreatedReportPathName, aPaths[iPathIterator] + BACKSLASH + sFilename + XLS_EXTENSION);

                        this.OpenExcelFile(sCreatedReportPathName + "xls", "");

                        this.CallExcelsReadTeklaStructuresReportFunction(sCreatedReportPathName, sFilename + EXCEL_EXTENSION);

                        bFound = true;
                    }

                    iPathIterator++;
                }
            }
            return bFound;
        }

        private void CheckDirectory(string sCreatedReportPathName, string sFilename)
        {
            try
            {
                bool bFound = false;
                string sReportDirectory;
                List<string> aPaths = new List<string>(); ;

                // Checks the ..\XS_TEMPLATE_DIRECTORY for the Excel files
                sReportDirectory = @TeklaControls.EnvironmentVariables.Get("XS_TEMPLATE_DIRECTORY");
                SlitStringIntoArrayOfStrings(sReportDirectory, CHAR_SEMICOLON, true, ref aPaths);
                bFound = this.OpenScriptInExcelLoadMacro(sCreatedReportPathName, aPaths, sFilename);
                MessageBox.Show(sCreatedReportPathName);
                // Checks the ..\XS_TEMPLATE_DIRECTORY\Excel_templates for the Excel files
                if (!bFound)
                {
                    if (AddExcelTemplateSubfolderToPath(ref aPaths))
                        bFound = this.OpenScriptInExcelLoadMacro(sCreatedReportPathName, aPaths, sFilename);
                }

                // Checks the ..\Model folder for the Excel files
                if (!bFound)
                {
                    // Model Directory
                    Tekla.Structures.Model.Model MyModel = new Model();
                    Tekla.Structures.Model.ModelInfo modelInfo = MyModel.GetInfo();

                    aPaths.Clear();
                    aPaths.Add(modelInfo.ModelPath);

                    // Checks if the selected excel file exist and opens it
                    bFound = this.OpenScriptInExcelLoadMacro(sCreatedReportPathName, aPaths, sFilename);
                }

                // Checks the ..\Model folder\Excel_templates for the Excel files
                if (!bFound)
                {
                    if (AddExcelTemplateSubfolderToPath(ref aPaths))
                        bFound = this.OpenScriptInExcelLoadMacro(sCreatedReportPathName, aPaths, sFilename);
                }

                // Checks the ..\XS_PROJECT for the Excel files
                if (!bFound)
                {
                    sReportDirectory = @TeklaControls.EnvironmentVariables.Get("XS_PROJECT");
                    SlitStringIntoArrayOfStrings(sReportDirectory, CHAR_SEMICOLON, true, ref aPaths);
                    bFound = this.OpenScriptInExcelLoadMacro(sCreatedReportPathName, aPaths, sFilename);
                }

                // Checks the ..\XS_PROJECT\Excel_templates for the Excel files
                if (!bFound)
                {
                    if (AddExcelTemplateSubfolderToPath(ref aPaths))
                        bFound = this.OpenScriptInExcelLoadMacro(sCreatedReportPathName, aPaths, sFilename);
                }

                // Checks the ..\XS_FIRM for the Excel files
                if (!bFound)
                {
                    sReportDirectory = @TeklaControls.EnvironmentVariables.Get("XS_FIRM");
                    SlitStringIntoArrayOfStrings(sReportDirectory, CHAR_SEMICOLON, true, ref aPaths);
                    bFound = this.OpenScriptInExcelLoadMacro(sCreatedReportPathName, aPaths, sFilename);
                }

                // Checks the ..\XS_FIRM\Excel_templates for the Excel files
                if (!bFound)
                {
                    if (AddExcelTemplateSubfolderToPath(ref aPaths))
                        bFound = this.OpenScriptInExcelLoadMacro(sCreatedReportPathName, aPaths, sFilename);
                }

                // Checks the ..\XS_TEMPLATE_DIRECTORY_SYSTEM for the Excel files
                if (!bFound)
                {
                    sReportDirectory = @TeklaControls.EnvironmentVariables.Get("XS_TEMPLATE_DIRECTORY_SYSTEM");
                    SlitStringIntoArrayOfStrings(sReportDirectory, CHAR_SEMICOLON, true, ref aPaths);
                    bFound = this.OpenScriptInExcelLoadMacro(sCreatedReportPathName, aPaths, sFilename);
                }

                // Checks the ..\XS_TEMPLATE_DIRECTORY_SYSTEM\Excel_templates for the Excel files
                if (!bFound)
                {
                    if (AddExcelTemplateSubfolderToPath(ref aPaths))
                        bFound = this.OpenScriptInExcelLoadMacro(sCreatedReportPathName, aPaths, sFilename);
                }

                // Checks the ..\XS_SYSTEM for the Excel files
                if (!bFound)
                {
                    sReportDirectory = @TeklaControls.EnvironmentVariables.Get("XS_SYSTEM");
                    SlitStringIntoArrayOfStrings(sReportDirectory, CHAR_SEMICOLON, true, ref aPaths);
                    bFound = this.OpenScriptInExcelLoadMacro(sCreatedReportPathName, aPaths, sFilename);
                }

                // Checks the ..\XS_SYSTEM\Excel_templates for the Excel files
                if (!bFound)
                {
                    if (AddExcelTemplateSubfolderToPath(ref aPaths))
                        bFound = this.OpenScriptInExcelLoadMacro(sCreatedReportPathName, aPaths, sFilename);
                }

                if (!bFound)
                    MessageBox.Show("The excel file of the report '" + sFilename + "' does not exist.");
            }
            catch
            {
                MessageBox.Show("Contact system administrator", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
    }

    #region Tekla Macro Script
    public class Script //Tekla Macro Script
    {
        private const string USING_THREE_DIGITS = "000";
        private const string BACKSLASH = "\\";
        private const string DOT = ".";
        private const string UNDERSCORE = "_";

        static Tekla.Technology.Akit.IScript akit;
        public static void Run(Tekla.Technology.Akit.IScript akit_in)
        {
            System.Runtime.Remoting.Lifetime.ClientSponsor sponsor = null;
            try
            {
                sponsor = new System.Runtime.Remoting.Lifetime.ClientSponsor();
                akit = akit_in;
                sponsor.Register((System.MarshalByRefObject)akit);
                //Application.Run(new ReportGenerator());
                //Application.Run(new Checker());
                new Checker();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
            finally
            {
                if (sponsor != null)
                {
                    sponsor.Close();
                }
            }
        }

        public static void OpenReportDialog(string reportDisplay, string externalBrowser, string title1, string title2, string title3)
        {
            //Checker chk = new Checker();
            //var str = chk.JobCode + "_" + sReportName;

            try
            {
                akit.Callback("acmd_display_report_dialog", "", "main_frame");
                akit.TabChange("xs_report_dialog", "Container_516", "Container_519");
                akit.ValueChange("xs_report_dialog", "display_created_report", reportDisplay);
                akit.ValueChange("xs_report_dialog", "report_display_type", externalBrowser);
                akit.TabChange("xs_report_dialog", "Container_516", "Container_517");
                akit.ValueChange("xs_report_dialog", "user_title1", title1);
                akit.ValueChange("xs_report_dialog", "user_title2", title2);
                akit.ValueChange("xs_report_dialog", "user_title3", title3);

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        public static void CloseReportDialog()
        {
            try
            {
                akit.PushButton("xs_report_cancel", "xs_report_dialog");
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        public static string CreateReport(string reportName, bool appendDate, bool overwriteReport, bool selectedParts)
        {


            //System.Windows.Forms.MessageBox.Show("create report :"+reportName);
            string sReportNameCopy = reportName;
            string fullpath = "";
            var rptgenerator_bradioButtonDialogValue = true;


            try
            {
                AmendReportNameIfNeeded(ref sReportNameCopy, appendDate);
                FileOverWriteCheck(ref sReportNameCopy, ref fullpath, overwriteReport);

                //if(ReportGenerator.bRadioButtonDialogValue)
                if (rptgenerator_bradioButtonDialogValue)
                    akit.ValueChange("xs_report_dialog", "display_created_report", "1");
                else
                    akit.ValueChange("xs_report_dialog", "display_created_report", "0");

                akit.ListSelect("xs_report_dialog", "xs_report_list", reportName);
                akit.ValueChange("xs_report_dialog", "xs_report_file", sReportNameCopy);
                if (selectedParts)
                {
                    akit.PushButton("xs_report_selected", "xs_report_dialog");
                }
                else
                {
                    akit.PushButton("xs_report_all", "xs_report_dialog");
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
            return sReportNameCopy;
            //return fullpath;
        }

        public static bool AmendReportNameIfNeeded(ref string sReportName, bool bAppendDate)
        {
            bool bResult = true;

            try
            {

                string fileExtension = Checker.DEFAULT_REPORT_FILE_EXTENSION;//ReportGenerator.DEFAULT_REPORT_FILE_EXTENSION;
                int dotLocation = sReportName.LastIndexOf(DOT);
                if (dotLocation > 0)
                {
                    fileExtension = sReportName.Substring(dotLocation, sReportName.Length - dotLocation);
                    sReportName = sReportName.Substring(0, dotLocation);
                }
                if (bAppendDate)
                {
                    sReportName = string.Concat(sReportName, UNDERSCORE, System.DateTime.Today.Date.ToShortDateString().Replace("/", ""));
                }
                sReportName = string.Concat(sReportName, fileExtension);
            }
            catch
            {
                bResult = false;
            }

            return bResult;
        }

        public static bool FileOverWriteCheck(ref string sReportName, ref string fullpath, bool bOverwriteReport)
        {
            bool bResult = true;
            try
            {

                string reportPath = String.Concat(TeklaControls.EnvironmentVariables.Get("XS_REPORT_OUTPUT_DIRECTORY"), "\\");
                fullpath = reportPath + sReportName;
                bool fileExists = (System.IO.File.Exists(reportPath + sReportName));
                if (fileExists)
                {
                    if (bOverwriteReport)
                    {
                        //System.Windows.Forms.MessageBox.Show("file overwrite check:(delete) "+ reportPath + " " + sReportName);
                        System.IO.File.Delete(reportPath + sReportName);
                    }
                    else
                    {
                        int i = 1;
                        int dotLocation = sReportName.LastIndexOf(DOT);
                        string reportRoot = sReportName.Substring(0, dotLocation);

                        string fileExtension = sReportName.Substring(dotLocation, sReportName.Length - dotLocation);
                        sReportName = String.Concat(reportRoot, UNDERSCORE, i.ToString(USING_THREE_DIGITS), fileExtension);

                        System.Windows.Forms.MessageBox.Show("file: " + reportPath + " " + sReportName);
                        while (System.IO.File.Exists(reportPath + sReportName))
                        {
                            i++;
                            int endLocation = (sReportName.LastIndexOf(DOT) - 4);
                            sReportName = String.Concat(reportRoot, UNDERSCORE, i.ToString(USING_THREE_DIGITS), fileExtension);
                        }
                    }
                }

                //fullpath = reportPath + str;
                //bool fileExists = (System.IO.File.Exists(reportPath + sReportName));
                //if (fileExists)
                //{
                //    if (bOverwriteReport)
                //    {
                //        //System.Windows.Forms.MessageBox.Show("file overwrite check:(delete) "+ reportPath + " " + sReportName);
                //        System.IO.File.Delete(reportPath + str);
                //    }
                //    else
                //    {
                //        int i = 1;
                //        int dotLocation = str.LastIndexOf(DOT);
                //        string reportRoot = str.Substring(0, dotLocation);

                //        string fileExtension = str.Substring(dotLocation, sReportName.Length - dotLocation);
                //        str = String.Concat(reportRoot, UNDERSCORE, i.ToString(USING_THREE_DIGITS), fileExtension);
                //        sReportName = String.Concat(reportRoot, UNDERSCORE, i.ToString(USING_THREE_DIGITS), fileExtension);

                //        while (System.IO.File.Exists(reportPath + sReportName))
                //        {
                //            i++;
                //            int endLocation = (str.LastIndexOf(DOT) - 4);
                //            str = String.Concat(reportRoot, UNDERSCORE, i.ToString(USING_THREE_DIGITS), fileExtension);
                //            sReportName = String.Concat(reportRoot, UNDERSCORE, i.ToString(USING_THREE_DIGITS), fileExtension);
                //        }
                //    }
                //}
            }
            catch
            {
                bResult = false;
            }


            return bResult;
        }

        #region Scripts for Ruukki SAP report
        public static bool RunSAPSetterExe()
        {
            bool bResult = true;

            try
            {
                string XS_Variable = System.Environment.GetEnvironmentVariable("XSBIN");
                string TS_Plugin = @"\applications\Ruukki\";
                string TS_Application = Checker.RUUKKI_SAP_PROGRAM_EXE;//ReportGenerator.RUUKKI_SAP_PROGRAM_EXE;

                if (System.IO.File.Exists(XS_Variable + TS_Plugin + TS_Application))
                {
                    System.Diagnostics.Process oProcess = new System.Diagnostics.Process();
                    oProcess.EnableRaisingEvents = false;
                    oProcess.StartInfo.FileName = XS_Variable + TS_Plugin + TS_Application;
                    oProcess.Start();
                    while (!oProcess.HasExited)
                    {
                    }
                    oProcess.Close();
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(TS_Application + " not found, Ruukki SAP Shape codes will not be set!\n\nCheck the files in " + XS_Variable + TS_Plugin,
                                                         "Tekla Structures",
                                                         System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
            }
            catch
            {
                bResult = false;
            }
            return bResult;
        }
    }
    #endregion
    #endregion

}


namespace TeklaControls
{
    /// Version 2.0
    /// <summary>
    /// A generic save and load control which mimics the functionality of the standard Tekla Structures save and load control.
    /// Note that it is necessary to call the method InitializeFileList() after the parent dialog or component has been created.
    /// </summary>
    /// <remarks>For optimum performance, place this control in the top of a dilaog setting its docking type to Top.
    /// </remarks>
    public class TeklaSaveLoadControl : System.Windows.Forms.UserControl
    {
        #region Constructors
        /// <summary>
        /// Initiates the control with the specified file extension.
        /// </summary>
        /// <param name="FileExtension">The file extension the control will load parameters from and save to</param>
        /// <param name="Language">The language to use in the user interface of the control</param>
        public TeklaSaveLoadControl(string fileExtension, string language)
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            SetLanguage(language);
            this.FileExtension = fileExtension;
            InitializeAttributeFileDirectories();
        }

        /// <summary>
        /// Initializes the list of serach directories to the standard Tekla search directories (model, XS_PROJECT, XS_FIRM, system)
        /// </summary>
        private void InitializeAttributeFileDirectories()
        {
            try
            {
                m_AttributeFileDirectories = EnvironmentFiles.GetStandardPropertyFileDirectories();
            }
            catch (System.Exception e)
            {
                MessageBox.Show("Exception:\n" + e.Message + "\n" + e.StackTrace);
            }
        }

        /// <summary>
        /// Sets the language for the SaveLoad control button labels
        /// </summary>
        /// <param name="lang"></param>
        private void SetLanguage(string lang)
        {
            switch (lang)
            {
                case ("JAPANESE"):
                    this.buttonLoad.Text = "読込み(&L)";
                    this.buttonSave.Text = "上書保存(&S)";
                    this.buttonSaveAs.Text = "名前付けて保存";
                    break;
                case ("GERMAN"):
                    this.buttonLoad.Text = "&Laden";
                    this.buttonSave.Text = "&Sichern";
                    this.buttonSaveAs.Text = "Sichern als";
                    break;
                case ("FRENCH"):
                    this.buttonLoad.Text = "&Charger";
                    this.buttonSave.Text = "&Enregistrer";
                    this.buttonSaveAs.Text = "Enregistrer sous";
                    break;
                case ("ENGLISH"):
                default:
                    this.buttonLoad.Text = "&Load";
                    this.buttonSave.Text = "&Save";
                    this.buttonSaveAs.Text = "Save as";
                    break;
            }
        }
        #endregion

        #region Private Properties
        private string m_FileExtension = "myExtension";
        private List<string> m_AttributeFileDirectories;
        private const char CHAR_DOT = '.';
        private const string DOT = ".";
        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the file extension for attribute files loaded and saved by this control.
        /// By default the file extension is "myExtension"
        /// </summary>
        /// <value>the file extension</value>
        /// <remarks>Note that any "." characters will be removed from the beginning and end of the file extension</remarks>
        public string FileExtension
        {
            get
            {
                return m_FileExtension;
            }
            set
            {
                m_FileExtension = value.Trim(CHAR_DOT);
            }
        }

        /// <summary>
        /// Gets or sets the text in the SaveAs text box
        /// </summary>
        public string SaveAsText
        {
            get
            {
                return this.textBoxSaveAs.Text;
            }
            set
            {
                if (value != null)
                {
                    this.textBoxSaveAs.Text = value;
                }
                else
                {
                    this.textBoxSaveAs.Text = "";
                }
            }
        }

        /// <summary>
        /// Gets the currently selected file name in the Save/Load combo box. 
        /// </summary>
        public string SaveLoadText
        {
            get
            {
                return this.comboBoxFileList.Text;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Updates (populates) the file list and if a "standard" file exists, loads it.
        /// </summary>
        public void InitializeFileList()
        {
            try
            {
                // Initialize local variables for save and load as necessary
                this.comboBoxFileList_update();
                if (this.comboBoxFileList.Items.Contains("standard"))
                {
                    this.comboBoxFileList.Text = "standard";
                    buttonLoad_Click(null, null);
                }
                this.Focus();
            }
            catch (System.Exception e)
            {
                MessageBox.Show("Exception:\n" + e.Message + "\n" + e.StackTrace);
            }
        }

        #endregion

        #region Public Events
        /// <summary>
        /// This event is triggered just before attributes are loaded into the dialog
        /// </summary>
        public event EventHandler AttributesLoading;

        /// <summary>
        /// This event is triggered just after attributes have been loaded into the dialog
        /// </summary>
        public event EventHandler AttributesLoaded;

        /// <summary>
        /// This event is triggered just before attributes are saved to a file
        /// </summary>
        public event EventHandler AttributesSaving;

        /// <summary>
        /// This event is triggered just after attributes are saved to a file
        /// </summary>
        public event EventHandler AttributesSaved;

        #endregion

        #region Save, Load, and SaveAs functions

        /// <summary>
        /// Updates or re-populates the file list
        /// </summary>
        private void comboBoxFileList_update()
        {
            try
            {
                List<string> fileList = EnvironmentFiles.GetMultiDirectoryFileList(m_AttributeFileDirectories, m_FileExtension);
                this.comboBoxFileList.Items.Clear();
                foreach (string nextFile in fileList)
                {
                    this.comboBoxFileList.Items.Add(nextFile);
                }
                this.comboBoxFileList.Sorted = true;
                this.comboBoxFileList.Update();
            }
            catch (System.Exception e)
            {
                MessageBox.Show("Exception:\n" + e.Message + "\n" + e.StackTrace);
            }
        }

        /// <summary>
        /// Saves the dialog properties to the currently selected file name and calls any AttributesSaved events for additional save related routines
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSave_Click(object sender, System.EventArgs e)
        {
            SaveAttributeFile(this.comboBoxFileList.Text);
            if (AttributesSaving != null)
            {
                try
                {
                    AttributesSaving(null, null);
                }
                catch { }
            }
        }

        /// <summary>
        /// Loads the dialog properties from the currently selected file name and calls any AttributesLoading and AttributesLoaded events for additional load related routines
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonLoad_Click(object sender, System.EventArgs e)
        {
            try
            {
                if (AttributesLoading != null)
                {
                    AttributesLoading(null, null);
                }
                FileInfo fi = EnvironmentFiles.GetAttributeFile(m_AttributeFileDirectories, this.comboBoxFileList.Text + DOT + m_FileExtension);
                if (fi == null)
                {
                    MessageBox.Show("The file [" + this.comboBoxFileList.Text + DOT + m_FileExtension + "] could not be found.");
                }
                else
                {
                    // Should add a routine to clear all controls
                    ClearFields(this.Parent);
                    using (StreamReader sr = new StreamReader(fi.FullName))
                    {
                        string line = sr.ReadLine();
                        while (line != null)
                        {
                            if (line.IndexOf(" ") > 0)
                            {
                                string control = line.Substring(0, line.IndexOf(" "));
                                string setting = line.Substring(line.IndexOf(" ")).Trim();
                                SetFormAttribute(this.Parent, control, setting);
                            }
                            line = sr.ReadLine();
                        }
                    }
                }
                if (AttributesLoaded != null)
                {
                    AttributesLoaded(null, null);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        /// <summary>
        /// Saves the dialog properties to the save as file name and calls any AttributesSaved events for additional save related routines
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSaveAs_Click(object sender, System.EventArgs e)
        {
            if (AttributesSaving != null)
            {
                try
                {
                    AttributesSaving(null, null);
                }
                catch { }
            }
            if (SaveAttributeFile(this.textBoxSaveAs.Text))
            {
                comboBoxFileList_update();
                this.comboBoxFileList.Text = this.textBoxSaveAs.Text;
                if (AttributesSaved != null)
                {
                    try
                    {
                        AttributesSaved(null, null);
                    }
                    catch { }
                }
            }
        }

        private bool SaveAttributeFile(string fileName)
        {
            if (fileName == null || fileName == "")
                return false;

            try
            {
                using (StreamWriter sw = new StreamWriter(m_AttributeFileDirectories[0] + fileName + DOT + m_FileExtension))
                {
                    foreach (Control ct in this.Parent.Controls)
                    {
                        WriteControlToFile(ct, sw);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Couldn't save file.\n" + e.Message);
                return false;
            }
            return true;
        }

        private void WriteControlToFile(Control ct, StreamWriter sw)
        {
            try
            {
                // Skip the SaveAs and Load controls
                if (ct.Equals(this.comboBoxFileList) ||
                    ct.Equals(this.textBoxSaveAs))
                {
                    return;
                }
                else if (ct.HasChildren)
                {
                    foreach (Control subControl in ct.Controls)
                    {
                        WriteControlToFile(subControl, sw);
                    }
                }
                else
                {
                    if (ct is System.Windows.Forms.TextBox)
                    {
                        System.Windows.Forms.TextBox ctOut = (System.Windows.Forms.TextBox)ct;
                        sw.WriteLine(ctOut.Name + " " + ctOut.Text);
                    }
                    else if (ct is System.Windows.Forms.ComboBox)
                    {
                        System.Windows.Forms.ComboBox ctOut = (System.Windows.Forms.ComboBox)ct;
                        sw.WriteLine(ctOut.Name + " " + ctOut.Text);
                    }
                    else if (ct is System.Windows.Forms.RadioButton)
                    {
                        System.Windows.Forms.RadioButton ctOut = (System.Windows.Forms.RadioButton)ct;
                        sw.WriteLine(ctOut.Name + " " + ctOut.Checked.ToString());
                    }
                    else if (ct is System.Windows.Forms.CheckBox)
                    {
                        System.Windows.Forms.CheckBox ctOut = (System.Windows.Forms.CheckBox)ct;
                        sw.WriteLine(ctOut.Name + " " + ctOut.CheckState.ToString());
                    }
                    else if (ct is System.Windows.Forms.CheckedListBox)
                    {
                        System.Windows.Forms.CheckedListBox ctOut = (System.Windows.Forms.CheckedListBox)ct;
                        foreach (int i in ctOut.CheckedIndices)
                        {
                            sw.WriteLine(ctOut.Name + " " + ctOut.Items[i].ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message + "\n" + e.StackTrace);
            }
        }

        private void ClearFields(Control ct)
        {
            try
            {
                // Skip the SaveAs and Load controls
                if (ct.Equals(this.comboBoxFileList) ||
                    ct.Equals(this.textBoxSaveAs))
                {
                    return;
                }
                else if (ct.HasChildren)
                {
                    foreach (Control subControl in ct.Controls)
                    {
                        ClearFields(subControl);
                    }
                }
                else
                {
                    switch (ct.GetType().Name)
                    {
                        case ("TextBox"):
                            {
                                System.Windows.Forms.TextBox ctSet = (System.Windows.Forms.TextBox)ct;
                                ctSet.Text = "";
                                break;
                            }
                        case ("ComboBox"):
                            {
                                System.Windows.Forms.ComboBox ctSet = (System.Windows.Forms.ComboBox)ct;
                                ctSet.Text = "";
                                break;
                            }
                        case ("RadioButton"):
                            {
                                System.Windows.Forms.RadioButton ctSet = (System.Windows.Forms.RadioButton)ct;
                                ctSet.Checked = false;
                                break;
                            }
                        case ("CheckBox"):
                            {
                                System.Windows.Forms.CheckBox ctSet = (System.Windows.Forms.CheckBox)ct;
                                ctSet.CheckState = CheckState.Unchecked;
                                break;
                            }
                        case ("CheckedListBox"):
                            {
                                System.Windows.Forms.CheckedListBox ctSet = (System.Windows.Forms.CheckedListBox)ct;
                                for (int i = ctSet.Items.Count - 1; i >= 0; i--)
                                {
                                    ctSet.SetItemCheckState(i, CheckState.Unchecked);
                                }
                                break;
                            }
                    }
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message + "\n" + e.StackTrace);
            }
        }

        private void SetFormAttribute(Control ct, string control, string setting)
        {
            try
            {
                if (ct.HasChildren)
                {
                    foreach (Control subCt in ct.Controls)
                    {
                        SetFormAttribute(subCt, control, setting);
                    }
                }
                else
                {
                    if (ct.Name == control)
                    {
                        switch (ct.GetType().Name)
                        {
                            case ("TextBox"):
                                {
                                    System.Windows.Forms.TextBox ctSet = (System.Windows.Forms.TextBox)ct;
                                    ctSet.Text = setting;
                                    break;
                                }
                            case ("ComboBox"):
                                {
                                    System.Windows.Forms.ComboBox ctSet = (System.Windows.Forms.ComboBox)ct;
                                    ctSet.Text = setting;
                                    break;
                                }
                            case ("RadioButton"):
                                {
                                    System.Windows.Forms.RadioButton ctSet = (System.Windows.Forms.RadioButton)ct;
                                    ctSet.Checked = bool.Parse(setting);
                                    break;
                                }
                            case ("CheckBox"):
                                {
                                    System.Windows.Forms.CheckBox ctSet = (System.Windows.Forms.CheckBox)ct;
                                    ctSet.CheckState = parseCheckState(setting);
                                    break;
                                }
                            case ("CheckedListBox"):
                                {
                                    System.Windows.Forms.CheckedListBox ctSet = (System.Windows.Forms.CheckedListBox)ct;
                                    for (int i = ctSet.Items.Count - 1; i >= 0; i--)
                                    {
                                        if (ctSet.Items[i].ToString().Equals(setting))
                                        {
                                            ctSet.SetItemCheckState(i, CheckState.Checked);
                                        }
                                    }
                                    break;
                                }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message + "\n" + e.StackTrace);
            }
        }

        private CheckState parseCheckState(string state)
        {
            switch (state)
            {
                case "Unchecked":
                    return CheckState.Unchecked;
                case "Checked":
                    return CheckState.Checked;
                default:
                    return CheckState.Indeterminate;
            }
        }
        #endregion

        #region Designer Code
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TeklaSaveLoadControl));
            this.groupBoxSaveLoad = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.textBoxSaveAs = new System.Windows.Forms.TextBox();
            this.buttonSaveAs = new System.Windows.Forms.Button();
            this.comboBoxFileList = new System.Windows.Forms.ComboBox();
            this.buttonLoad = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.groupBoxSaveLoad.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxSaveLoad
            // 
            this.groupBoxSaveLoad.Controls.Add(this.tableLayoutPanel1);
            this.groupBoxSaveLoad.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxSaveLoad.Name = "groupBoxSaveLoad";
            this.groupBoxSaveLoad.TabStop = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Controls.Add(this.textBoxSaveAs, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonSaveAs, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.comboBoxFileList, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonLoad, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonSave, 0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // textBoxSaveAs
            // 
            this.textBoxSaveAs.Name = "textBoxSaveAs";
            this.textBoxSaveAs.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Left;
            this.textBoxSaveAs.Location = new Point(310, 3);
            this.textBoxSaveAs.Size = new Size(149, 19);
            this.textBoxSaveAs.TabIndex = 5;
            this.textBoxSaveAs.Parent = this.tableLayoutPanel1;
            // 
            // buttonSaveAs
            // 
            this.buttonSaveAs.Name = "buttonSaveAs";
            this.buttonSaveAs.Click += new System.EventHandler(this.buttonSaveAs_Click);
            this.buttonSaveAs.AutoSize = true;
            this.buttonSaveAs.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.buttonSaveAs.Location = new Point(248, 3);
            this.buttonSaveAs.Size = new Size(56, 22);
            this.buttonSaveAs.TabIndex = 4; this.buttonSaveAs.Text = "S&amp;ave as";
            this.buttonSaveAs.Parent = this.tableLayoutPanel1;
            // 
            // comboBoxFileList
            // 
            this.comboBoxFileList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFileList.Name = "comboBoxFileList";
            this.comboBoxFileList.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.comboBoxFileList.Location = new Point(94, 3);
            this.comboBoxFileList.Size = new Size(148, 20);
            this.comboBoxFileList.TabIndex = 3;
            this.comboBoxFileList.Name = "comboBoxFileList";
            this.comboBoxFileList.Parent = this.tableLayoutPanel1;
            // 
            // buttonLoad
            // 
            this.buttonLoad.Name = "buttonLoad";
            this.buttonLoad.Click += new System.EventHandler(this.buttonLoad_Click);
            this.buttonLoad.AutoSize = true;
            this.buttonLoad.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.buttonLoad.Location = new Point(49, 3);
            this.buttonLoad.Size = new Size(39, 22);
            this.buttonLoad.TabIndex = 2;
            this.buttonLoad.Text = "&amp;Load";
            this.buttonLoad.Name = "buttonLoad";
            this.buttonLoad.Parent = this.tableLayoutPanel1;
            // 
            // buttonSave
            // 
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // TeklaSaveLoadControl
            // 
            this.Controls.Add(this.groupBoxSaveLoad);
            this.MinimumSize = new System.Drawing.Size(474, 48);
            this.Name = "TeklaSaveLoadControl";

            this.groupBoxSaveLoad.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

            this.buttonSave.AutoSize = true;
            this.buttonSave.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.buttonSave.Location = new Point(3, 3);
            this.buttonSave.Size = new Size(40, 22);
            this.buttonSave.TabIndex = 1;
            this.buttonSave.Text = "&Save";
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Parent = this.tableLayoutPanel1;
            this.tableLayoutPanel1.Dock = DockStyle.Fill;
            this.tableLayoutPanel1.Location = new Point(3, 12);
            this.tableLayoutPanel1.Margin = new Padding(3, 0, 3, 3);
            this.tableLayoutPanel1.Size = new Size(462, 30);
            this.tableLayoutPanel1.TabIndex = 0;
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Parent = this.groupBoxSaveLoad;
            this.groupBoxSaveLoad.Dock = DockStyle.Fill;
            this.groupBoxSaveLoad.Location = new Point(3, 0);
            this.groupBoxSaveLoad.Padding = new Padding(3, 0, 3, 3);
            this.groupBoxSaveLoad.Size = new Size(468, 45);
            this.groupBoxSaveLoad.TabIndex = 0;
            this.groupBoxSaveLoad.Name = "groupBoxSaveLoad";
            this.groupBoxSaveLoad.Parent = this;
            this.Padding = new Padding(3, 0, 3, 3);
            this.Size = new Size(474, 48);
        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxSaveLoad;
        internal System.Windows.Forms.ComboBox comboBoxFileList;
        internal System.Windows.Forms.TextBox textBoxSaveAs;
        internal System.Windows.Forms.Button buttonSaveAs;
        internal System.Windows.Forms.Button buttonLoad;
        internal System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;

        #endregion
    }

    public class Language
    {
        public static string GetTeklaStructuresLanguage()
        {
            // Check if we can identify the language that Tekla Structures was started in
            string language = EnvironmentVariables.GetEnvironmentVariable("XS_LANGUAGE");
            if (language == null || language == "")
            {
                language = "XXX";
            }
            return language;
        }

        /// <summary>
        /// Gets the three letter language code for the given language.
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public static string GetThreeLetterLangaugeCode(string language)
        {
            string languageCode = "enu";
            switch (language)
            {
                case ("CHS"):
                case ("CHINESE SIMPLIFIED"):
                    languageCode = "chs";
                    break;
                case ("CHINESE TRADITIONAL"):
                    languageCode = "cht";
                    break;
                case ("CZECH"):
                    languageCode = "csy";
                    break;
                case ("GERMAN"):
                    languageCode = "deu";
                    break;
                case ("SPANISH"):
                    languageCode = "esp";
                    break;
                case ("FRENCH"):
                    languageCode = "fra";
                    break;
                case ("HUNGARIAN"):
                    languageCode = "hun";
                    break;
                case ("ITALIAN"):
                    languageCode = "ita";
                    break;
                case ("JAPANESE"):
                    languageCode = "jpn";
                    break;
                case ("DUTCH"):
                    languageCode = "nld";
                    break;
                case ("POLISH"):
                    languageCode = "plk";
                    break;
                case ("PORTUGUESE BRAZILIAN"):
                    languageCode = "ptb";
                    break;
                case ("PORTUGUESE"):
                    languageCode = "ptg";
                    break;
                case ("RUSSIAN"):
                    languageCode = "rus";
                    break;
                case ("ENGLISH"):
                default:
                    languageCode = "enu";
                    break;
            }
            return languageCode;
        }

    }

    /// <summary>
    /// A set of tools for parsing text to "Tekla" values
    /// </summary>
    public class Parsers
    {
        /// <summary>
        /// Stores the Tekla numberFormatinfo
        /// </summary>
        private static NumberFormatInfo m_TeklaNumberFormat;
        private const string DecimalSeparator = ".";
        private const string GroupSeparator = ",";

        /// <summary>
        /// Initializes the class so we have the "Tekla" numberFormatInfo available
        /// </summary>
        static Parsers()
        {
            m_TeklaNumberFormat = new NumberFormatInfo();
            m_TeklaNumberFormat.NumberDecimalSeparator = DecimalSeparator;
            m_TeklaNumberFormat.NumberGroupSeparator = GroupSeparator;
            m_TeklaNumberFormat.NumberDecimalSeparator = DecimalSeparator;
            m_TeklaNumberFormat.NumberGroupSeparator = GroupSeparator;
            m_TeklaNumberFormat.CurrencyDecimalSeparator = DecimalSeparator;
            m_TeklaNumberFormat.CurrencyGroupSeparator = GroupSeparator;
            m_TeklaNumberFormat.PercentDecimalSeparator = DecimalSeparator;
            m_TeklaNumberFormat.PercentGroupSeparator = GroupSeparator;
        }

        public static List<double> DistanceListToList(string text)
        {
            string[] chunks = text.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            List<double> distList = new List<double>();
            for (int i = 0; i < chunks.GetLength(0); i++)
            {
                if (chunks[i].IndexOf("*") > -1)
                {
                    int multiplier = ParseInt(chunks[i].Split(new char[] { '*' })[0], 1);
                    double distance = ParseDouble(chunks[i].Split(new char[] { '*' })[1]);
                    for (int j = 0; j < multiplier; j++)
                    {
                        distList.Add(distance);
                    }
                }
                else
                {
                    double val = ParseDouble(chunks[i], double.MinValue);
                    if (val != double.MinValue)
                        distList.Add(val);
                }
            }
            return distList;
        }

        public static double[] DistanceListToArray(string text)
        {
            return DistanceListToList(text).ToArray();
        }

        public static double ParseDouble(string text)
        {
            return ParseDouble(text, 0);
        }

        public static double ParseDouble(string text, double def)
        {
            try
            {
                return double.Parse(text, m_TeklaNumberFormat);
            }
            catch
            {
                return def;
            }
        }

        public static int ParseInt(string text)
        {
            return ParseInt(text, 0);
        }

        public static int ParseInt(string text, int def)
        {
            try
            {
                return int.Parse(text, m_TeklaNumberFormat);
            }
            catch
            {
                return def;
            }
        }
    }

    public class EnvironmentFiles
    {
        private static List<string> m_PropertyFileDirectories = GetStandardPropertyFileDirectories();
        private const string DOT = ".";

        public static List<string> PropertyFileDirectories
        {
            get
            {
                return m_PropertyFileDirectories;
            }
            set
            {
                m_PropertyFileDirectories = value;
            }
        }

        public static List<string> GetStandardPropertyFileDirectories()
        {
            List<string> fileDirectories = new List<string>();
            try
            {
                // First attempt to add the model/attributes/ directory
                Tekla.Structures.Model.Model model;
                if (ModelAccess.ConnectToModel(out model))
                {
                    string modelPath = model.GetInfo().ModelPath;
                    // Check first for an "./attributes/" directory. If one is not found use the local directory.
                    if (IsValidDirectory(modelPath + @"/attributes/"))
                    {
                        fileDirectories.Add(modelPath + @"/attributes/");
                    }
                    else if (IsValidDirectory(modelPath + @"/"))
                    {
                        fileDirectories.Add(modelPath + @"/");
                    }
                }
                if (fileDirectories.Count == 0 && IsValidDirectory(@"./"))
                {
                    fileDirectories.Add(@"./");
                }

                // Now add any Tekla Structures standard environment directories
                AddPaths(fileDirectories, "XS_PROJECT");
                AddPaths(fileDirectories, "XS_FIRM");
                AddPaths(fileDirectories, "XS_SYSTEM");
            }
            catch (System.Exception e)
            {
                Console.WriteLine("Exception:\n{0}\n{1}", e.Message, e.StackTrace);
            }
            return fileDirectories;
        }

        public static void AddPaths(List<string> fileDirectories, string environmentVariableName)
        {
            char[] semiColon = new char[] { ';' };
            string[] xs_Project = EnvironmentVariables.GetEnvironmentVariable(environmentVariableName).Split(semiColon);
            foreach (string path in xs_Project)
            {
                string cleanPath = path.Replace(@"\\\\", @"\\");
                if (IsValidDirectory(cleanPath))
                    fileDirectories.Add(cleanPath);
            }
        }

        public static bool IsValidDirectory(string directory)
        {
            if (directory == null || directory == "")
                return false;
            return Directory.Exists(directory);
        }

        public static List<string> GetMultiDirectoryFileList(string fileExtension)
        {
            return GetMultiDirectoryFileList(PropertyFileDirectories, fileExtension);
        }

        /// <summary>
        /// Gets a list of files with the given extension from the list of directories.
        /// </summary>
        /// <param name="searchDirectories"></param>
        /// <param name="fileExtension"></param>
        /// <returns>A list of found files names without the file extension</returns>
        public static List<string> GetMultiDirectoryFileList(List<string> searchDirectories, string fileExtension)
        {
            List<string> fileList = new List<string>();
            fileExtension = DOT + fileExtension;
            foreach (string nextDirectory in searchDirectories)
            {
                try
                {
                    if (Directory.Exists(nextDirectory))
                    {
                        DirectoryInfo di = new DirectoryInfo(nextDirectory);
                        FileInfo[] fi = di.GetFiles("*" + fileExtension);
                        foreach (FileInfo nextFile in fi)
                        {
                            if (nextFile != null && nextFile.Exists && nextFile.Extension == fileExtension)
                            {
                                string fileName = nextFile.Name.Substring(0, nextFile.Name.Length - fileExtension.Length);
                                if (!fileList.Contains(fileName))
                                {
                                    fileList.Add(fileName);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.Message + "\n" + e.StackTrace);
                }
            }
            fileList.Sort(Sorting.TeklaSort.CompareLikeTekla);
            return fileList;
        }

        /// <summary>
        /// Gets FileInfo representing the first match in the standard property file directories.
        /// </summary>
        /// <param name="fileName">the name of the file including file extension</param>
        /// <returns>a FileInfo for the first match in the directory list, null if no match was found.</returns>
        public static FileInfo GetAttributeFile(string fileName)
        {
            return GetAttributeFile(PropertyFileDirectories, fileName);
        }

        /// <summary>
        /// Gets FileInfo representing the first match in the search directories.
        /// </summary>
        /// <param name="searchDirectories">the list of directories to search for the file</param>
        /// <param name="fileName">the name of the file including file extension</param>
        /// <returns>a FileInfo for the first match in the directory list, null if no match was found.</returns>
        public static FileInfo GetAttributeFile(List<string> searchDirectories, string fileName)
        {
            try
            {
                foreach (string di in searchDirectories)
                {
                    if (File.Exists(di + fileName))
                    {
                        return new FileInfo(di + fileName);
                    }
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine("Exception:\n{0}\n{1}", e.Message, e.StackTrace);
            }
            return null;
        }

    }
    /// <summary>
    /// A class of helper methods for connecting to and accessing the model and objects in the model.
    /// Attempts to provide efficient but robust methods for connecting to and verifying connection to the model.
    /// Also provides efficient methods for enumeration and generation of lists strongly typed to specific ModelObject types.
    /// </summary>
    public class ModelAccess
    {
        private static Tekla.Structures.Model.Model m_ModelConnection = null;

        /// <summary>
        /// Checks if a model connection is established, and attempts to create one if needed.
        /// </summary>
        /// <returns>true if successfully connected to the  model, false otherwise</returns>
        private static bool ConnectedToModel
        {
            get
            {
                bool connection = false;
                ConnectToModel(out connection);
                return connection;
            }
        }

        /// <summary>
        /// Gets a Model connnection
        /// </summary>
        /// <returns>The Model or null if unable to connect</returns>
        public static Tekla.Structures.Model.Model ConnectToModel()
        {
            bool connection = false;
            return ConnectToModel(out connection);
        }

        /// <summary>
        /// Gets a model connection
        /// </summary>
        /// <param name="model">the Model connection</param>
        /// <returns>true on success, false otherwise</returns>
        public static bool ConnectToModel(out Tekla.Structures.Model.Model model)
        {
            bool connection = false;
            ConnectToModel(out connection);
            model = m_ModelConnection;
            return connection;
        }

        /// <summary>
        /// Gets a Model connection
        /// </summary>
        /// <param name="ConnectedToModel">set to true if a model connection was made, false otherwise.</param>
        /// <returns>The Model or null if unable to connect</returns>
        public static Tekla.Structures.Model.Model ConnectToModel(out bool ConnectedToModel)
        {
            ConnectedToModel = false;
            if (m_ModelConnection == null)
            {
                try
                {
                    m_ModelConnection = new Tekla.Structures.Model.Model();
                }
                catch
                {
                    ConnectedToModel = false;
                }
            }
            if (m_ModelConnection.GetConnectionStatus())
            {
                if (m_ModelConnection.GetInfo().ModelName == "")
                {
                    ConnectedToModel = false;
                }
                else
                {
                    ConnectedToModel = true;
                }
            }
            return m_ModelConnection;
        }
    }

    class Sorting
    {
        /// <summary>
        /// An IComparer interface to sort lists in the same way that Tekla attribute and report lists are sorted.
        /// </summary>
        /// <remarks>Tekla Structures sorting places lowercase letters before uppercase letters.
        /// In all other cases, the local cultural info is used for sorting.
        /// Note that using this comparer will tread upper and lowercase letters as different.</remarks>
        public class TeklaSort : IComparable<string>, IComparer
        {
            public static int CompareLikeTekla(string x, string y)
            {
                char[] s1 = x.ToCharArray();
                char[] s2 = y.ToCharArray();
                int max = ((s1.Length < s2.Length) ? s1.Length : s2.Length);
                for (int i = 0; i < max; i++)
                {
                    if (s1[i] != s2[i])
                    {
                        if ((char.IsUpper(s1[i]) && char.IsLower(s2[i])))
                        {
                            return -1;
                        }
                        else if ((char.IsUpper(s2[i]) && char.IsLower(s1[i])))
                        {
                            return 1;
                        }
                        else
                        {
                            return (new Comparer(System.Threading.Thread.CurrentThread.CurrentCulture).Compare(x, y));
                        }
                    }
                }
                return (new Comparer(System.Threading.Thread.CurrentThread.CurrentCulture).Compare(x, y));
            }

            #region IComparer Members
            public int CompareTo(string value)
            {
                return Compare(this, value);
            }

            public int Compare(object x, object y)
            {
                if ((x.GetType().Name.ToLower() == "string") && (y.GetType().Name.ToLower() == "string"))
                {
                    char[] s1 = ((string)x).ToCharArray();
                    char[] s2 = ((string)y).ToCharArray();
                    int max = ((s1.Length < s2.Length) ? s1.Length : s2.Length);
                    for (int i = 0; i < max; i++)
                    {
                        if (s1[i] != s2[i])
                        {
                            if ((char.IsUpper(s1[i]) && char.IsLower(s2[i])))
                            {
                                return -1;
                            }
                            else if ((char.IsUpper(s2[i]) && char.IsLower(s1[i])))
                            {
                                return 1;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                return ((new Comparer(System.Threading.Thread.CurrentThread.CurrentCulture).Compare(x, y)));
            }
            #endregion
        }

    }


    /// <summary>
    /// A SortedList specializing in getting active environment variables and advanced option settings.
    /// It also checks options.ini files in the active model folder as well as options_user.ini files.
    /// </summary>
    public static class EnvironmentVariables
    {
        private static SortedList<string, string> m_List = new SortedList<string, string>();

        static EnvironmentVariables()
        {
            Add("XSBIN");
            Add("USERNAME");
        }

        public static string GetEnvironmentVariable(string variableName)
        {
            if (m_List.ContainsKey(variableName))
            {
                return m_List[variableName];
            }
            else
            {
                Add(variableName);
                return Get(variableName);
            }
        }

        public static void Add(string key)
        {
            if (!m_List.ContainsKey(key))
            {
                string val = string.Empty;
                try
                {
                    bool connected = false;
                    bool added = false;
                    Tekla.Structures.Model.Model model = ModelAccess.ConnectToModel(out connected);
                    if (connected)
                    {
                        added = TeklaStructuresSettings.GetAdvancedOption(key, ref val);
                    }

                    if (!connected || !added)
                    {
                        val = @System.Environment.GetEnvironmentVariable(key);

                        if (File.Exists("options.ini"))
                        {
                            StreamReader fs = File.OpenText("options.ini");
                            string line = fs.ReadLine();
                            while (line != null)
                            {
                                if (line.StartsWith(key))
                                {
                                    val = line.Substring(line.IndexOf("=") + 1);
                                    break;
                                }
                                line = fs.ReadLine();
                            }
                            fs.Close();
                        }
                        if (m_List.ContainsKey("XSBIN") && m_List.ContainsKey("USERNAME"))
                        {
                            string options_ini = m_List["XSBIN"] + "options_" + m_List["USERNAME"] + ".ini";
                            if (File.Exists(options_ini))
                            {
                                StreamReader fs = File.OpenText(options_ini);
                                string line = fs.ReadLine();
                                while (line != null)
                                {
                                    if (line.StartsWith(key))
                                    {
                                        val = line.Substring(line.IndexOf("=") + 1);
                                        break;
                                    }
                                    line = fs.ReadLine();
                                }
                                fs.Close();
                            }
                        }
                    }
                    if (val == null)
                        val = "";
                    m_List.Add(key, val);
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.Message + "\n" + e.StackTrace);
                    m_List.Add(key, "");
                }
            }
        }

        public static string Get(string key)
        {
            if (m_List.ContainsKey(key))
            {
                return m_List[key];
            }
            return string.Empty;
        }
    }
}