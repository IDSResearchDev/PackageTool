using System.Collections.ObjectModel;

namespace PackageTool.Model
{
    public interface ITransmittal
    {
        void Insert(TransmittalData transmittalData);
        void Delete();
    }

    public class TransmittalData
    {
        public string SheetName { get; set; }
        public string Revision { get; set; }
        public string Type { get; set; }

    }

}