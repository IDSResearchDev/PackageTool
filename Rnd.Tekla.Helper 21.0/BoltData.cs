using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures.Model;


namespace Rnd.TeklaStructure.Helper
{
    public class BoltData
    {

        public string Quantity { get; set; }
        public string BoltStandard { get; set; }
        public Enums.BoltType BoltType { get; set; }
        public string BoltLength { get; set; }
        public string BoltSize { get; set; }
        public string BoltRemarks { get; set; }
        public string SupplyWith { get; set; }
        public string QuantityMultiplier { get; set; }

        #region SupplyWith
        public bool Washer1 { get; set; }
        public bool Washer2 { get; set; }
        public bool Washer3 { get; set; }
        public bool Nut1 { get; set; }
        public bool Nut2 { get; set; }
        #endregion

        private readonly List<BoltData> _boltList;
        public List<BoltData> BoltDataList { get; set; }

        //private readonly BoltReportProperties _boltprop;
        
        public BoltData()
        {
            //_boltprop = new BoltReportProperties {BoltList = new List<BoltReportProperties>()};
            _boltList = new List<BoltData>();
            BoltDataList = new List<BoltData>();

        }

   

        public void GetProperties()
        {
            Tekla.Structures.Model.UI.ModelObjectSelector modelObjectSelector = new Tekla.Structures.Model.UI.ModelObjectSelector();
            ModelObjectEnumerator modelObjectEnum = modelObjectSelector.GetSelectedObjects();

            

            var count = 0;
            foreach (var item in modelObjectEnum)
            {
                

                #region item as Beam
                if (item is Beam)
                {
                    
                    Beam b = (Beam)modelObjectEnum.Current;
                    

                 


                    //int id = 0;
                    List<int> connectionId = new List<int>();
                    foreach (Connection connection in b.GetComponents())
                    {
                        connectionId.Add(connection.Identifier.ID);
                    }


                    foreach (var conId in connectionId)
                    {
                        Connection conx = new Connection { Identifier = { ID = conId} };
                        conx.Select();
                        int boltrow = 0;
                        conx.GetAttribute("nb", ref boltrow);
                        conx.GetAttribute("nb2", ref boltrow);    
                    }





                    var boltenum = b.GetBolts();
                    int total = boltenum.GetSize();
                    Console.WriteLine(total);
                    var items = b.GetComponents();

          
                    foreach (var boltitem in 
                        boltenum)
                    {

                        BoltGroup bo = (BoltGroup)boltitem;

                        if (bo.BoltType == BoltGroup.BoltTypeEnum.BOLT_TYPE_SITE)
                            BoltType = Enums.BoltType.Site;
                        else if (bo.BoltType == BoltGroup.BoltTypeEnum.BOLT_TYPE_WORKSHOP)
                            BoltType = Enums.BoltType.WorkShop;

                        BoltStandard = bo.BoltStandard;


                        var boltSizeDouble = bo.BoltSize;

                        BoltSize = boltSizeDouble.ConvertDecimaltoFraction();

                        var boltLength = 0.0;
                        bo.GetReportProperty("LENGTH", ref boltLength);

                        BoltLength = boltLength.ConvertDecimaltoFraction();

                        var boltComment = "";
                        bo.GetUserProperty("BOLT_COMMENT", ref boltComment);

                        BoltRemarks = boltComment;

                        #region BoltStandard Property
                        //string name = "";
                        //boltGroup.GetReportProperty("BOLT_STANDARD", ref name); 
                        #endregion


                        Washer1 = bo.Washer1;
                        Washer2 = bo.Washer2;
                        Washer3 = bo.Washer3;


                        Nut1 = bo.Nut1;
                        Nut2 = bo.Nut2;

                        count++;

                        _boltList.Add(new BoltData()
                        {
                            BoltStandard = /*_boltprop.*/BoltStandard,
                            BoltType = /*_boltprop.*/BoltType,
                            BoltLength = /*_boltprop.*/BoltLength,
                            BoltSize = /*_boltprop.*/BoltSize,
                            BoltRemarks = /*_boltprop.*/BoltRemarks
                        });
                    }

                    Console.WriteLine("My counter: {0}", count);
                    count = 0;


                } 
                #endregion

                
                
                #region Comment

                //if (item is BoltGroup)
                //{
                    
                //    BoltGroup boltGroup = (BoltGroup) item;//modelObjectEnum.Current;
                //    count++;
                //    var boltassemblt = boltGroup.PartToBoltTo.GetAssembly();
                //    BoltStandard = boltGroup.BoltStandard;

                //    if (boltGroup.BoltType == BoltGroup.BoltTypeEnum.BOLT_TYPE_SITE)
                //        BoltType = Enums.BoltType.Site;
                //    else if (boltGroup.BoltType == BoltGroup.BoltTypeEnum.BOLT_TYPE_WORKSHOP)
                //        BoltType = Enums.BoltType.WorkShop;

                //    //cutLength = boltGroup.CutLength.ToString();
                //    //extraLength = boltGroup.ExtraLength.ToString();
                //    //boltGroupShape = boltGroup.GetType().Name;


                //    var boltSizeDouble = boltGroup.BoltSize;
                //    BoltSize = boltSizeDouble.ConvertDecimaltoFraction();

                //    var boltLength = 0.0;
                //    boltGroup.GetReportProperty("LENGTH", ref boltLength);
                //    BoltLength = boltLength.ConvertDecimaltoFraction();

                //    var boltComment = "";
                //    boltGroup.GetUserProperty("BOLT_COMMENT", ref boltComment);
                //    BoltRemarks = boltComment;

                //    #region BoltStandard Property
                //    //string name = "";
                //    //boltGroup.GetReportProperty("BOLT_STANDARD", ref name); 
                //    #endregion


                //    Washer1 = boltGroup.Washer1;
                //    Washer2 = boltGroup.Washer2;
                //    Washer3 = boltGroup.Washer3;

                //    Nut1 = boltGroup.Nut1;
                //    Nut2 = boltGroup.Nut2;


                //    //SupplyWith = GetSupplyWith(BoltStandard,Nut1, Nut2, Washer1,Washer2, Washer3);

                //    //string value = string.Concat("DIAMETER: ", toFractionSize, Environment.NewLine, "BOLT STANDARD: ", boltStandard, Environment.NewLine, "Supply With: ",SupplyWith, Environment.NewLine, "BOLT TYPE: ", boltType,
                //    //                                   Environment.NewLine, "LENGTH: ", toFractionLength, Environment.NewLine, "Remarks:", boltComment, Environment.NewLine);

                //    //Console.WriteLine(value);

                    
                //    _boltList.Add(new BoltData()
                //    {
                //        BoltStandard = BoltStandard,
                //        BoltType = BoltType,
                //        BoltLength = BoltLength,
                //        BoltSize = BoltSize,
                //        BoltRemarks = BoltRemarks
                //    });

                //} 

                #endregion
            }
            Console.WriteLine("bolt num: {0}",count);

            GetGroupedList(_boltList);
        }


        public string GetSupplyWith(string boltstandard, bool nut1, bool nut2, bool washer1, bool washer2, bool washer3)
        {
            if (boltstandard.Contains("TC") || boltstandard.Contains("HILTI")) return SupplyWith = "SET";
            if ((!washer1 && !washer2 && washer3 && nut1 && !nut2) && (!boltstandard.Contains("TC") || !boltstandard.Contains("HILTI"))) return SupplyWith = "1 NUT & 1 WASHER";
            if ((!washer1 && washer2 && !washer3 && nut1 && !nut2) && (!boltstandard.Contains("TC") || !boltstandard.Contains("HILTI"))) return SupplyWith = "1 NUT & 1 TAPERED WASHER";
            if ((washer1 && !washer2 && washer3 && nut1 && !nut2) && (!boltstandard.Contains("TC") || !boltstandard.Contains("HILTI"))) return SupplyWith = "1 NUT & 2 WASHER";
            if ((!washer1 && !washer2 && washer3 && nut1 && nut2) && (!boltstandard.Contains("TC") || !boltstandard.Contains("HILTI"))) return SupplyWith = "1 NUT, 1 LOCK NUT & 1 WASHER";
            if ((washer1 && !washer2 && washer3 && nut1 && nut2) && (!boltstandard.Contains("TC") || !boltstandard.Contains("HILTI"))) return SupplyWith = "1 NUT, 1 LOCK NUT & 2 WASHER";

            return SupplyWith = "1 NUT & 1 WASHER";
        }

        public void GetGroupedList(List<BoltData> boltlist)
        {
            var ungrouped = boltlist;
            var groupList = boltlist.GroupBy(g => new { g.BoltStandard, g.BoltType, g.BoltSize, g.BoltLength });
            int counter = 0;

            foreach (var item in groupList)
            {
                var standard = item.Key.BoltStandard;
                var type = item.Key.BoltType.ToString();
                var size = item.Key.BoltSize;
                var length = item.Key.BoltLength;

                foreach (var item2 in ungrouped)
                {
                    if (item2.BoltStandard == standard && item2.BoltType.ToString() == type && item2.BoltSize == size && item2.BoltLength == length)
                    {
                        counter++;
                    }
                }

                //string value = string.Concat("QUANTITY: ", counter.ToString(), Environment.NewLine, "DIAMETER: ", size, Environment.NewLine, "BOLT STANDARD: ", standard, Environment.NewLine, "BOLT TYPE: ", type,
                //                                   Environment.NewLine, "LENGTH: ", length, Environment.NewLine);

                BoltDataList = GetFinalList(counter.ToString(), size, standard, type, length);


                //Console.WriteLine(value);
                counter = 0;

            }

            foreach (var boltData in BoltDataList)
            {
                string value = string.Concat("QUANTITY: ", boltData.Quantity, Environment.NewLine, "DIAMETER: ", boltData.BoltSize, Environment.NewLine, "BOLT STANDARD: ",
                                                   boltData.BoltStandard, Environment.NewLine, "BOLT TYPE: ", boltData.BoltType,
                                                   Environment.NewLine, "LENGTH: ", boltData.BoltLength, Environment.NewLine);

                Console.WriteLine(value);
            }
        }

        public List<BoltData> GetFinalList(string quantity, string diameter, string standard, string type, string length)
        {
            //var list = new List<BoltData>();

            Enums.BoltType btype = (Enums.BoltType)Enum.Parse(typeof(Enums.BoltType), type); 
            BoltDataList.Add(new BoltData()
            {
                Quantity = quantity,
                BoltSize = diameter,
                BoltStandard = standard,
                BoltType = btype,
                BoltLength = length
            });


            return BoltDataList;
        }
    }
}
