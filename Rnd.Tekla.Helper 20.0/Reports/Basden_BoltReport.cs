using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Rnd.TeklaStructure.Helper.Reports
{
    using WorkBook = Microsoft.Office.Interop.Excel;

    public class BasdenBoltReport
    {

        private WorkBook.Application _xls;
        private WorkBook.Workbook _thisWorkBook;
        private WorkBook.Sheets _thisWorkSheets;
        private WorkBook.Worksheet _thisItemSheets;
        private WorkBook.Range _range;
        private string _path;

        public List<BoltData> BoltList { get; set; }
        public string Project { get; set; }
        public string Jobnumber { get; set; }

        public BasdenBoltReport()
        {
            BoltList = new List<BoltData>();
            //_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "IDS_Excel_Bolt Summary.xls"); 
        }

        public void ReadExcel(string rptname, string multiplier, string reportpackagedirectory, string jobnum, string jobcode)
        {
            _path = Path.Combine(reportpackagedirectory, string.Concat(jobnum, "_", jobcode, "_", rptname));

            _xls = new WorkBook.Application
                                            {
                                                Visible = false,
                                                DisplayAlerts = false
                                            };


            _thisWorkBook = _xls.Workbooks.Open(_path, 0, false, 5, "", "", false, WorkBook.XlPlatform.xlWindows
                                                    , "", true, false, 0, true, false, false);
            _thisWorkSheets = _thisWorkBook.Worksheets;
            _thisItemSheets = (WorkBook.Worksheet) _thisWorkBook.Worksheets.Item[1];

            _range = _thisItemSheets.UsedRange;


            var rowCount = _range.Rows.Count;
            var colCount = _range.Columns.Count;

            //var cellValue = (string)(_thisItemSheets.Cells[3,2] as WorkBook.Range).Value.ToString();
            Project = _range.Cells[3,2].Value2.ToString();
            //object obj = _range.Cells[3,7].Value2.ToString();
            //var stre = obj.ToString();
            Jobnumber = _range.Cells[3,7].Value2.ToString();



            var str = "";

            for (var i = 1; i <= rowCount; i++)
            {


                var num=0;
                if (_range.Cells[i, 1].Value2 != null)
                {
                    int.TryParse(_range.Cells[i, 1].Value2.ToString(), out num);
                }
                

                for (var j = 1; j <= colCount; j++)
                {
                    if (_range.Cells[i, j].Value2 == null) continue;
                    if (num > 0) str +=  _range.Cells[i, j].Value2.ToString() + ",";


                    //(_range.Cells[i, j].Value2.ToString())
                }

                if (num > 0)
                {
                    //if (!string.IsNullOrEmpty(multiplier) || !string.IsNullOrWhiteSpace(multiplier))
                    //{
                        //try
                        //{
                        //    double reflector = (Convert.ToDouble(multiplier) + 100) / 100;
                        //    double reflect = num * reflector;
                        //    var toIntMultiplier = reflect.Round0To5();

                        //    AddtoList(String.Concat(str, toIntMultiplier.ToString()).TrimEnd(' ', ','));
                        //}
                        //catch (Exception err)
                        //{        
                        //    throw new Exception(err.Message);
                        //}
                    //}
                    //else
                    //{
                        AddtoList(str.TrimEnd(' ', ','));
                    //}
                }
                    //Console.WriteLine(str.TrimEnd(' ', ','));
                str = string.Empty;
            }

            _thisWorkBook.Close();
            

            releaseObject(_thisWorkSheets);
            releaseObject(_thisWorkBook);
            _xls.Quit();
            releaseObject(_xls);

        }


        private void releaseObject(object obj)
        {

            try
            {
                Marshal.ReleaseComObject(obj);

                obj = null;
            }
            catch (Exception err)
            {
                obj = null;
                throw new ArgumentException("Unable to release the object." + Environment.NewLine + err.ToString());
            }
            finally
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

        }

        private void AddtoList(string str)
        {

            try
            {
                var splitstr = str.Split(',');

                if (splitstr.Count() < 5)
                {
                    BoltList.Add(new BoltData()
                    {
                        
                        Quantity = splitstr[0],
                        BoltSize = splitstr[1],
                        BoltLength = splitstr[2],
                        BoltStandard = "",
                        BoltRemarks = ""
                        //QuantityMultiplier = splitstr[6]

                    });
                }
                else
                {
                    BoltList.Add(new BoltData()
                    {

                        Quantity = splitstr[0],
                        BoltSize = splitstr[1],
                        BoltLength = splitstr[2],
                        BoltStandard = splitstr[3],
                        BoltRemarks = splitstr[4]
                        //QuantityMultiplier = splitstr[6]
                    });
                }
                    
            }
            catch (Exception err)
            {
                
                throw new Exception("nandto error \n"+err.Message);
            }
            

        }


    }
}
