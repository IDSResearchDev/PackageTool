using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using PackageTool.ViewModel;
using Rnd.TeklaStructure.Helper;

namespace PackageTool.Model
{
    using WorkBook = Microsoft.Office.Interop.Excel;

    public class ExportToExcel
    {
        private WorkBook.Workbook _thisWorkBook;
        private WorkBook.Sheets _thisWorkSheets;
        private WorkBook.Worksheet _thisItemSheets;
        private WorkBook.Application _xls;
        private WorkBook.Range _range;
        private string _path;

        private string _jobnum;
        private string _jcode;

        private ObservableCollection<TransmittalData> data = GlobalVars.TransmittalDatas; //= new ObservableCollection<TransmittalData>(GlobalVars.TransmittalDatas);
        private ObservableCollection<XsrReports> xsrRpt = new ObservableCollection<XsrReports>(GlobalVars.XsrReportList); 

        //IEnumerable<IGrouping<string, TransmittalData>> data1;
        //IEnumerable<IGrouping<string,TransmittalData>> data2;
        //IEnumerable<IGrouping<string, TransmittalData>> dataCollection;

        #region Properties
        private string _fabricator;
        private string _fabaddress;
        void FabParams()
        {
            ProjectProperties projprop = new ProjectProperties();

            _fabricator = projprop.Fabricator;
            _fabaddress = projprop.Fabaddress;
        }
        #endregion

        public ExportToExcel(string jnum, string jcode)
        {
            _jobnum = jnum;
            _jcode = jcode;
            _path = Path.Combine(GlobalVars.LocalAppPackageToolFolder, "TransmittalLetterTemplate.xlt");

            //data = GlobalVars.TransmittalDatas;
            FabParams();
            
        }

        private bool HasReports()
        {
            return GlobalVars.XsrReportList.Count > 0;
        }
        public void ReadXls(string date, string time)
        {
            _xls = new WorkBook.Application
            {
                Visible = false,
                DisplayAlerts = false
            };


            _thisWorkBook = _xls.Workbooks.Open(_path, 0, false, 5, "", "", false, WorkBook.XlPlatform.xlWindows
                                                    , "", true, false, 0, true, false, false);
            _thisWorkSheets = _thisWorkBook.Worksheets;
            _thisItemSheets = (WorkBook.Worksheet)_thisWorkSheets.Item["Sheet1"];
            _range = _thisItemSheets.UsedRange;

            _thisItemSheets.Range["B2", Missing.Value].Value2 = _fabricator;
            _thisItemSheets.Range["B3", Missing.Value].Value2 = _fabaddress;

            _thisItemSheets.Range["I2", Missing.Value].Value2 = GlobalVars.TransmittalNumber;
            _thisItemSheets.Range["I3", Missing.Value].Value2 = DateTime.Now.ToShortDateString();
            _thisItemSheets.Range["I4", Missing.Value].Value2 = GlobalVars.ProjectNumber;
            _thisItemSheets.Range["I5", Missing.Value].Value2 = GlobalVars.Project;
            _thisItemSheets.Range["I7", Missing.Value].Value2 = GlobalVars.Location;

            _thisItemSheets.Range["B8", Missing.Value].Value2 = GlobalVars.Attention;
            _thisItemSheets.Range["E9", Missing.Value].Value2 = GlobalVars.SendingSelection;
            _thisItemSheets.Range["B10", Missing.Value].Value2 = GlobalVars.FileTypes;
            _thisItemSheets.Range["C35", Missing.Value].Value2 = GlobalVars.Purpose;
            _thisItemSheets.Range["C37", Missing.Value].Value2 = GlobalVars.Remarks;
            _thisItemSheets.Range["A43", Missing.Value].Value2 = GlobalVars.Signature;

            //_thisWorkBook.Save();



            //var data = GlobalVars.TransmittalDatas.OrderBy(o=> o.Index).ThenBy(o=> o.SheetName).ThenBy(o=> o.Type).GroupBy(g=> g.Type);
            //data = GlobalVars.TransmittalDatas;
            //xsrRpt = GlobalVars.XsrReportList;


            if (HasReports() || GlobalVars.cfgModel.KSS/*GlobalVars.HasKss*/)
            {
                //var coverrows = 20;
                //var newcoverrows = (coverrows-xsrRpt.Count)-2;
                //var coverrecord = (newcoverrows * 5);
                //var numRecords = data.Count + xsrRpt.Count;

                var coverrows = 20;
                var rptCount = xsrRpt.Count + 1;
                var kssCount = 2;
                var newcoverrows = coverrows - (rptCount + kssCount);
                var coverrecord = (newcoverrows * 5);

                var numRecords = data.Count() + rptCount + kssCount;//(data.Count != null) ? data.Count + rptCount + kssCount : 0 + rptCount + kssCount;


                if (numRecords <= coverrecord)
                {
                    const int baseint = 13;
                    ConvertToExcel(data, baseint);
                }
                else
                {
                    const int topcell = 50;
                    const int bottomcell = 94;
                    _thisItemSheets.Range["A20", Missing.Value].Font.Bold = true;
                    _thisItemSheets.Range["A20", Missing.Value].Value2 = "See Attachment/s";
                    ConvertToExcel(data, topcell, topcell, bottomcell);
                }
            }
            else
            {
                if (data.Count <= 80)
                {
                    const int baseint = 13;
                    ConvertToExcel(data, baseint);
                }
                else
                {
                    const int topcell = 50;
                    const int bottomcell = 94;
                    _thisItemSheets.Range["A20", Missing.Value].Font.Bold = true;
                    _thisItemSheets.Range["A20", Missing.Value].Value2 = "See Attachment/s";
                    ConvertToExcel(data, topcell, topcell, bottomcell);
                }
            }


            #region saveWorkBook

            GlobalVars.OutputTransmittalLetter = string.Empty;
            var transmittal = GlobalVars.TransmittalName;
            var trans = transmittal + "_" + date + "_" + time + ".xls";
            GlobalVars.OutputTransmittalLetter = trans;
            _thisWorkBook.SaveAs(GlobalVars.OutputTransmittalLetter);

            if (_xls != null)
            {
                try
                {
                    _thisWorkBook.Close();
                    Marshal.ReleaseComObject(_thisItemSheets);
                    Marshal.ReleaseComObject(_thisWorkBook);
                    _xls.Quit();
                    Marshal.FinalReleaseComObject(_xls);
                }
                catch (Exception err)
                {
                    throw new ArgumentException(err.Message);
                }
            }

            //_thisItemSheets = null;
            //_thisWorkBook = null;
            //_xls.Quit();
            //GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            //Process.Start(_path); 
            #endregion
        }
        private void ConvertToExcel(IEnumerable<TransmittalData> paramdata, int baseint)
        {
            var stackBaseint = baseint;
            
            var dataCollection = paramdata.OrderBy(o => o.Revision).ThenBy(o => o.SheetName).GroupBy(g => g.Type);//data.OrderBy(o => o.Type).ThenBy(o => o.SheetName).GroupBy(g => g.Type);
            //var data1 = paramdata.Where(t => (t.Type != "KSS" && t.Type != "XSR")).OrderBy(o => o.Revision).ThenBy(o => o.SheetName).GroupBy(g => g.Type);
            //var data2 = paramdata.Where(t => (t.Type == "KSS" || t.Type == "XSR")).GroupBy(g=>g.Type);
            //var dataCollection = data1.Concat(data2);

            //var a = data.Select(s => s.Type == "KSS" || s.Type == "XSR").GroupBy(g=>g.GetType());
            

            
            var dataCount = paramdata.Count();

            string[] arr = { "A", "D", "F", "H", "K" };
            string[] secondary = { "C", "E", "G", "J", "L" };

            var bint = baseint;
            var baseIndex = 0;
            var counter = 1;
            var maxrow = 20;// from 18 to 20 rows
            var lastrow = 0;
            var kssCount =0;

            if (GlobalVars.cfgModel.KSS/*GlobalVars.HasKss*/)
            {
                kssCount = 2;
            }

            if (HasReports() || GlobalVars.cfgModel.KSS) maxrow = (maxrow - (xsrRpt.Count+1+kssCount))-2;
            if (dataCount == 0) lastrow = bint;

            foreach (var items in dataCollection)
            {
                var currentkey = items.Key;

                _thisItemSheets.Range[arr[baseIndex] + bint, Missing.Value].Font.Bold = true;
                _thisItemSheets.Range[arr[baseIndex] + bint, Missing.Value].Value2 = items.Key;

                _thisItemSheets.Range[secondary[baseIndex] + bint, Missing.Value].Font.Bold = true;
                _thisItemSheets.Range[secondary[baseIndex] + bint, Missing.Value].Value2 = "Rev.";
                

                
                foreach (var insideItems in items.AsEnumerable())
                {
                    bint++;
                    counter++;

                    if (counter > maxrow)// maxrow = 18
                    {
                        if (baseIndex <= arr.Length - 1) baseIndex++;
                        counter = 1;
                        bint = stackBaseint;
                    }
                    //else
                    //{
                    //    if (baseIndex == 0) lastrow = bint;
                    //    //lastrow = bint;
                    //}
                    if (baseIndex == 0) lastrow = bint;

                    _thisItemSheets.Range[arr[baseIndex] + bint, Missing.Value].Value2 = insideItems.SheetName;
                    _thisItemSheets.Range[secondary[baseIndex] + bint, Missing.Value].Value2 = insideItems.Revision;

                    #region border
                    //var color = System.Drawing.Color.Black;
                    ////_thisItemSheets.Range[arr[baseIndex] + bint, Missing.Value].Borders[WorkBook.XlBordersIndex.xlEdgeRight].Color = color;
                    //_thisItemSheets.Range[arr[baseIndex] + bint, Missing.Value].Borders[WorkBook.XlBordersIndex.xlEdgeLeft].Color = color;
                    //_thisItemSheets.Range[arr[baseIndex] + bint, Missing.Value].Borders[WorkBook.XlBordersIndex.xlEdgeTop].Color = color;
                    //_thisItemSheets.Range[arr[baseIndex] + bint, Missing.Value].Borders[WorkBook.XlBordersIndex.xlEdgeBottom].Color = color; 
                    #endregion
                }

                
                bint++;
                counter++;
                
                //if (currentkey != items.Key) _thisItemSheets.Range[arr[baseIndex] + baseint, Missing.Value].Value2 = "";
                //checkkey = items.Key;
                bint++;
                counter++;
                if (baseIndex == 0) lastrow = bint;
                if (counter > maxrow)// maxrow = 18
                {
                    if (baseIndex <= arr.Length - 1) baseIndex++;
                    counter = 1;
                    bint = stackBaseint;
                }
            }

            if (GlobalVars.cfgModel.KSS/*GlobalVars.HasKss*/)
            {
                bint = lastrow+1;
                baseIndex = 0;

                //if (items.Key != "KSS") _thisItemSheets.Range[secondary[baseIndex] + bint, Missing.Value].Value2 = "Rev.";

                bint++;
                counter++;

                _thisItemSheets.Range[arr[baseIndex] + bint, Missing.Value].Font.Bold = true;
                _thisItemSheets.Range[arr[baseIndex] + bint, Missing.Value].Value2 = "KSS";

                bint++;
                counter++;

                _thisItemSheets.Range[arr[baseIndex] + bint, Missing.Value].Value2 = GlobalVars.KssName;
                lastrow = bint;
            }

            if (!HasReports()) return;
            bint = lastrow;
            baseIndex = 0;

            bint++;
            counter++;


            _thisItemSheets.Range[arr[baseIndex] + bint, Missing.Value].Font.Bold = true;
            _thisItemSheets.Range[arr[baseIndex] + bint, Missing.Value].Value2 = "REPORTS";
             
            foreach (var item in GlobalVars.XsrReportList.OrderBy(s=>s.ReportName))
            {
                bint++;
                counter++;

                _thisItemSheets.Range[arr[baseIndex] + bint, Missing.Value].Value2 = string.Concat(GlobalVars.JobNumber, "_", GlobalVars.JobCode, "_", item.ReportName);
            }
        }
        private void ConvertToExcel(IEnumerable<TransmittalData> paramdata, int baseint, int topcell,int bottomcell)
        {
            string[] arr = { "A", "D", "F", "H", "K" };
            string[] secondary = { "C", "E", "G", "J", "L" };
            
            var stackBaseint = baseint;
            var bint = baseint;

            var dataCollection = data.OrderBy(o => o.Revision).ThenBy(o => o.SheetName).GroupBy(g => g.Type); //data.OrderBy(o => o.Type).ThenBy(o => o.SheetName).GroupBy(g => g.Type);
            //var data1 = paramdata.Where(t => (t.Type != "KSS" && t.Type != "XSR")).OrderBy(o => o.Revision).ThenBy(o => o.SheetName).GroupBy(g => g.Type);
            //var data2 = paramdata.Where(t => (t.Type == "KSS" || t.Type == "XSR")).GroupBy(g => g.Type);
            //var dataCollection = data1.Concat(data2);
            
            var baseIndex = 0;
            var counter = 1;
            var noOfRecords = 0;
            var attachmentCounter = 1;
            var startdata = 0;

            var maxrow = 42;
            var newcount = (maxrow - GlobalVars.XsrReportList.Count)-2;
            var newattachmentrecord = paramdata.Count();
            var lastrow = 0;

            // _thisItemSheets.Range[arr[baseIndex] + (topcell), Missing.Value].Value2 = string.Concat("Attachment #", attachmentCounter);
            //  
            //if (HasReports() && data.Count()<=210)
            //{
            //    maxrow = (maxrow - GlobalVars.XsrReportList.Count) - 2;

            //}
            
            foreach (var items in dataCollection)
            {
                _thisItemSheets.Range[arr[baseIndex] + bint, Missing.Value].Font.Bold = true;
                _thisItemSheets.Range[arr[baseIndex] + bint, Missing.Value].Value2 = items.Key;

                _thisItemSheets.Range[secondary[baseIndex] + bint, Missing.Value].Font.Bold = true;
                _thisItemSheets.Range[secondary[baseIndex] + bint, Missing.Value].Value2 = "Rev.";

                foreach (var insideItems in items.AsEnumerable())
                {
                    bint ++;
                    counter++;
                    noOfRecords++;
                     
                    if (counter > maxrow)
                    {
                        if (noOfRecords == 210)
                        {
                            var numberOfRecords = bottomcell - topcell;
                            var nextTopcell = bottomcell + 1;
                            var nextLastcell = nextTopcell + numberOfRecords;

                            startdata = nextTopcell + 2;
                            var enddata = nextLastcell - 1;
                            stackBaseint = startdata;
                            attachmentCounter++;
                            noOfRecords = 0;
                            baseIndex = -1;
                            //_thisItemSheets.Range[arr[baseIndex] + (startdata - 1), Missing.Value].Font.Bold = true;
                            //_thisItemSheets.Range[arr[baseIndex] + (startdata - 1), Missing.Value].Value2 = string.Concat("Attachment #", attachmentCounter);
                        }

                        if (baseIndex == 0) lastrow = bint;
                        if (baseIndex <= arr.Length - 1) baseIndex++;
                        counter = 1;
                        bint = stackBaseint;

                        
                    }
                    _thisItemSheets.Range[arr[baseIndex] + bint, Missing.Value].Value2 = insideItems.SheetName;
                    _thisItemSheets.Range[secondary[baseIndex] + bint, Missing.Value].Value2 = insideItems.Revision;



                    #region Commented Code

                    //if (counter > 42)
                    //{
                    //    if (noOfRecords == 210)
                    //    {
                    //        var numberOfRecords = bottomcell - topcell;
                    //        var nextTopcell = bottomcell + 1;
                    //        var nextLastcell = nextTopcell + numberOfRecords;

                    //        startdata = nextTopcell + 2;
                    //        var enddata = nextLastcell - 1;
                    //        stackBaseint = startdata;
                    //        attachmentCounter++;
                    //        noOfRecords = 0;
                    //        baseIndex = -1;
                    //        //_thisItemSheets.Range[arr[baseIndex] + (startdata - 1), Missing.Value].Font.Bold = true;
                    //        //_thisItemSheets.Range[arr[baseIndex] + (startdata - 1), Missing.Value].Value2 = string.Concat("Attachment #", attachmentCounter);
                    //    }

                    //    if (baseIndex <= arr.Length - 1) baseIndex++;
                    //    counter = 1;
                    //    baseint = stackBaseint;    


                    //}
                    //_thisItemSheets.Range[arr[baseIndex] + baseint, Missing.Value].Value2 = insideItems.SheetName;
                    //_thisItemSheets.Range[secondary[baseIndex] + baseint, Missing.Value].Value2 = insideItems.Revision; 
                    #endregion
                }
                bint++;
                counter++;
                noOfRecords++;

                bint++;
                counter++;
                noOfRecords++;

                if (baseIndex == 0) lastrow = bint;
                if (counter > maxrow)// maxrow = 18
                {
                    if (baseIndex <= arr.Length - 1) baseIndex++;
                    counter = 1;
                    bint = stackBaseint;
                }
            }

            //_thisItemSheets.Range[arr[baseIndex] + (startdata - 1), Missing.Value].Font.Bold = true;
            //_thisItemSheets.Range[arr[baseIndex] + (startdata - 1), Missing.Value].Value2 = string.Concat("Attachment #", attachmentCounter);

            if (GlobalVars.cfgModel.KSS/*GlobalVars.HasKss*/)
            {
                bint = lastrow + 3;
                baseIndex = 0;

                //if (items.Key != "KSS") _thisItemSheets.Range[secondary[baseIndex] + bint, Missing.Value].Value2 = "Rev.";

                bint++;
                counter++;

                _thisItemSheets.Range[arr[baseIndex] + bint, Missing.Value].Font.Bold = true;
                _thisItemSheets.Range[arr[baseIndex] + bint, Missing.Value].Value2 = "KSS";

                bint++;
                counter++;

                _thisItemSheets.Range[arr[baseIndex] + bint, Missing.Value].Value2 = GlobalVars.KssName;
                lastrow = bint;
            }

            if (!HasReports()) return;
            bint = lastrow;
            baseIndex = 0;

            bint++;
            counter++;


            /*if (!HasReports()) return;
            bint = lastrow + 2;
            baseIndex = 0;*/
            _thisItemSheets.Range[arr[baseIndex] + bint, Missing.Value].Font.Bold = true;
            _thisItemSheets.Range[arr[baseIndex] + bint, Missing.Value].Value2 = "REPORTS";
            foreach (var item in GlobalVars.XsrReportList)
            {
                bint++;
                counter++;

                _thisItemSheets.Range[arr[baseIndex] + bint, Missing.Value].Value2 = item.ReportName;
            }

        }

        string file = Path.Combine(GlobalVars.LocalAppPackageToolFolder, "STANDARD BASDEN BOLT LIST Template.xlt"); //@"C:\Users\J. Mon\Desktop\STANDARD FIELD BOLT LIST Template.xlt";
        
        public void CreateBasdenReportExcel(string rptname, List<BoltData> list, string path, string project, string multiplier)
        {
            string[] column = new string[] { "B", "C", "G", "H", "I" };
            int numberOfRows = 13;
            int firstRowToWrite = 10;

            WorkBook.Workbook workBook;
            WorkBook.Sheets sheets;
            WorkBook.Worksheet workSheet;

            int remainder = list.Count % numberOfRows > 0 ? 1 : 0;
            int totalSheets = (list.Count / numberOfRows) + remainder;

            var percent = (string.IsNullOrWhiteSpace(multiplier) || string.IsNullOrEmpty(multiplier)) ? "0" : multiplier;

            WorkBook.Application excel = new WorkBook.Application
            {
                Visible = false,
                DisplayAlerts = false
            };
            workBook = excel.Workbooks.Open(file, 0, false, 5, "", "", false, WorkBook.XlPlatform.xlWindows
                                                    , "", true, false, 0, true, false, false);
            sheets = workBook.Worksheets;
            workSheet = (WorkBook.Worksheet)sheets.Item[1];


            for (int c = 1; c <= totalSheets; c++)
            {
                var sheet = (WorkBook.Worksheet)sheets.Item[c];
                sheet.Name = SheetInitials(BasdenRptName(rptname))  + " " + (c).ToString();
                if (workBook.Sheets.Count < totalSheets)
                    sheet.Copy(Type.Missing, workBook.Sheets[workBook.Sheets.Count]);
            }

            int tracker = 1;
            for (int sheetNum = 1; sheetNum <= totalSheets; sheetNum++)
            {
                int row = 0;
                int maxRow = numberOfRows * sheetNum;
                workSheet = (WorkBook.Worksheet)sheets.Item[sheetNum];

                workSheet.Cells[1, 1].Value2 = BasdenRptName(rptname);
                workSheet.Cells[4,3].Value2 = project;
                workSheet.Cells[6,3].Value2 = "manual input";
                workSheet.Cells[4,8].Value2 = _jobnum;
                workSheet.Cells[5,8].Value2 = "manual input";
                workSheet.Cells[6,8].Value2 = "manual input";
                workSheet.Cells[7,8].Value2 = "manual input";

                workSheet.Cells[23, 2].Value2 = string.Concat("ALL QUANTITY REFLECT ", percent, "% ADD ON");


                for (int i = tracker; i <= maxRow; i++)
                {
                    tracker = i;
                    if (i > list.Count)
                    { break; }
                    workSheet.Range[column[0] + (row + firstRowToWrite).ToString(), Missing.Value].Value2 = list[i - 1].Quantity;
                    workSheet.Range[column[1] + (row + firstRowToWrite).ToString(), Missing.Value].Value2 = list[i - 1].BoltSize;
                    workSheet.Range[column[2] + (row + firstRowToWrite).ToString(), Missing.Value].Value2 = list[i - 1].BoltLength;
                    workSheet.Range[column[3] + (row + firstRowToWrite).ToString(), Missing.Value].Value2 = list[i - 1].BoltStandard;
                    workSheet.Range[column[4] + (row + firstRowToWrite).ToString(), Missing.Value].Value2 = list[i - 1].BoltRemarks;
                    //workSheet.Range[column[4] + (row + firstRowToWrite).ToString(), Missing.Value].Value2 = list[i - 1];

                    row++;
                }
                tracker = maxRow + 1;
            }

            workBook.SaveAs(Path.Combine(path, _jobnum + "_" + _jcode + "_" + rptname)
                            /*, WorkBook.XlFileFormat.xlWorkbookDefault, Type.Missing, Type.Missing, false, false
                            , WorkBook.XlSaveAsAccessMode.xlExclusive, WorkBook.XlSaveConflictResolution.xlLocalSessionChanges
                            , Type.Missing, Type.Missing*/);
            
            try
            {
                workBook.Close();
                Marshal.ReleaseComObject(workSheet);
                Marshal.ReleaseComObject(workBook);
                excel.Quit();
                Marshal.FinalReleaseComObject(excel);
            }
            catch (Exception err)
            {
                throw new ArgumentException(err.Message);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        private string BasdenRptName(string rptname)
        {
            var rname = rptname.Replace(".xls", "");
            var rname2 = rname.Replace("BASDEN_Excel_", "");
            return rname2.Replace("_", " ").ToUpper();
        }
        private string SheetInitials(string rptname)
        {
            var str = rptname.Split(' ');
            var inii = "";

            foreach (var item in str)
            {
                inii += item[0];
            }

            return inii;
        }

        #region Has methods

        private void HasReps(IEnumerable<TransmittalData> data, int baseint)
        {
            var stackBaseint = baseint;
            var dataCollection = data.OrderBy(o => o.Type).ThenBy(o => o.SheetName).GroupBy(g => g.Type);

            string[] arr = { "A", "D", "F", "H", "K" };
            string[] secondary = { "C", "E", "G", "J", "L" };
            var baseIndex = 0;
            var counter = 1;
            var maxrecord = 18;

            foreach (var items in dataCollection)
            {
                var currentkey = items.Key;

                _thisItemSheets.Range[arr[baseIndex] + baseint, Missing.Value].Font.Bold = true;
                _thisItemSheets.Range[arr[baseIndex] + baseint, Missing.Value].Value2 = items.Key;

                foreach (var insideItems in items.AsEnumerable())
                {
                    baseint++;
                    counter++;

                    if (HasReports()) maxrecord = maxrecord - GlobalVars.XsrReportList.Count;


                    if (counter > maxrecord)// maxrecord = 18
                    {
                        if (baseIndex <= arr.Length - 1) baseIndex++;
                        counter = 1;
                        baseint = stackBaseint;
                    }
                    _thisItemSheets.Range[arr[baseIndex] + baseint, Missing.Value].Value2 = insideItems.SheetName;
                    _thisItemSheets.Range[secondary[baseIndex] + baseint, Missing.Value].Value2 = insideItems.Revision;
                }
                baseint++;
                counter++;
                //if (currentkey != items.Key) _thisItemSheets.Range[arr[baseIndex] + baseint, Missing.Value].Value2 = "";
                //checkkey = items.Key;
                baseint++;
                counter++;
            }
        }
        private void HasRepsAttachments(IEnumerable<TransmittalData> data, int baseint, int topcell, int bottomcell)
        {
            var stackBaseint = baseint;

            var dataCollection = data.OrderBy(o => o.Type).ThenBy(o => o.SheetName).GroupBy(g => g.Type);

            string[] arr = { "A", "D", "F", "H", "K" };
            string[] secondary = { "C", "E", "G", "J", "L" };
            var baseIndex = 0;
            var counter = 1;
            var noOfRecords = 0;
            var attachmentCounter = 1;
            var startdata = 0;

            //_thisItemSheets.Range[arr[baseIndex] + (topcell), Missing.Value].Value2 = string.Concat("Attachment #", attachmentCounter);

            foreach (var items in dataCollection)
            {
                _thisItemSheets.Range[arr[baseIndex] + baseint, Missing.Value].BorderAround();
                _thisItemSheets.Range[arr[baseIndex] + baseint, Missing.Value].Font.Bold = true;
                _thisItemSheets.Range[arr[baseIndex] + baseint, Missing.Value].Value2 = items.Key;

                foreach (var insideItems in items.AsEnumerable())
                {
                    baseint++;
                    counter++;
                    noOfRecords++;
                    if (counter > 42)
                    {
                        if (noOfRecords == 210)
                        {
                            var numberOfRecords = bottomcell - topcell;
                            var nextTopcell = bottomcell + 1;
                            var nextLastcell = nextTopcell + numberOfRecords;

                            startdata = nextTopcell + 2;
                            var enddata = nextLastcell - 1;
                            stackBaseint = startdata;
                            attachmentCounter++;
                            noOfRecords = 0;
                            baseIndex = -1;
                            //_thisItemSheets.Range[arr[baseIndex] + (startdata - 1), Missing.Value].Font.Bold = true;
                            //_thisItemSheets.Range[arr[baseIndex] + (startdata - 1), Missing.Value].Value2 = string.Concat("Attachment #", attachmentCounter);
                        }

                        if (baseIndex <= arr.Length - 1) baseIndex++;
                        counter = 1;
                        baseint = stackBaseint;


                    }
                    _thisItemSheets.Range[arr[baseIndex] + baseint, Missing.Value].Value2 = insideItems.SheetName;

                    _thisItemSheets.Range[secondary[baseIndex] + baseint, Missing.Value].Value2 = insideItems.Revision;
                }
                baseint++;
                counter++;
                noOfRecords++;
                //if (currentkey != items.Key) _thisItemSheets.Range[arr[baseIndex] + baseint, Missing.Value].Value2 = "-";
                //checkkey = items.Key;
                baseint++;
                counter++;
                noOfRecords++;
            }

            //_thisItemSheets.Range[arr[baseIndex] + (startdata - 1), Missing.Value].Font.Bold = true;
            //_thisItemSheets.Range[arr[baseIndex] + (startdata - 1), Missing.Value].Value2 = string.Concat("Attachment #", attachmentCounter);

        }

        #endregion
    }
}
