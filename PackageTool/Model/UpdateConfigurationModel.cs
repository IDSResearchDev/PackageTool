using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageTool.Model
{
    [Serializable()]
    public class UpdateConfigurationModel
    {
        public string RNDServer { get; set; }
        public string FTPServer { get; set; }
        public bool IsRndServer { get; set; }
        public bool IsFtpServer { get; set; }
    }
}
