using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageTool.ViewModel
{
    public enum ConfigurationType
    {
        [Description("For Approval")]
        For_Approval = 0,
        [Description("For Fabrication")]
        For_Fabrication = 1,
        [Description("ABM")]
        ABM = 2
    };

    public static class EnumExtension
    {
        public static string[] GetDescription(Type enumType)
        {
            string[] ret = new string[Enum.GetNames(enumType).Length];
            int counter = 0;
            foreach (ConfigurationType type in Enum.GetValues(enumType))
            {

                var field = enumType.GetField(type.ToString());
                var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute),
                                                           false);

                ret[counter] = attributes.Length == 0
                    ? type.ToString()
                    : ((DescriptionAttribute)attributes[0]).Description;
                counter++;
            }
            return ret;
        }
    }
}
