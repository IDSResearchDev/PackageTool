using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace PackageTool.Converters
{
    public class BoolToVisibilitConverter :IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var invert = (parameter as string ?? "") == "Invert";

            var b = (bool)value ^ invert;
            return b ? Visibility.Visible : Visibility.Collapsed;
        }




        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
