using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace PresetMagicianGUI
{
    [ValueConversion(typeof(String), typeof(String))]

    public class EscapeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            String rawString = (String)value;
            MessageBox.Show(rawString.Replace(@"\", @"\\"));
            
            return rawString.Replace(@"\", @"\\");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            String rawString = (String)value;
            return rawString.Replace(@"\\", @"\");
        }
    }
}
