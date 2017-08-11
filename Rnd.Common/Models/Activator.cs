using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rnd.Common.Models
{
    [Serializable()]
    public class Activator
    {
        public string MacAddress { get; set; } = string.Empty;
        public string ActivationCode { get; set; } = string.Empty;
    }
}
