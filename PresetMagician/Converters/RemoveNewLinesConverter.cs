using System;
using System.Windows.Data;

namespace PresetMagician.Converters
{
    public class RemoveNewLinesConverter: IValueConverter
    {
        
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                var val = value as string ?? string.Empty;
                return val.Replace(Environment.NewLine, string.Empty);
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException("Method not implemented");
            }
        
    }
}