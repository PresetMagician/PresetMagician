using System;
using System.Collections.Generic;
using System.Windows.Data;
using Catel.Collections;
using PresetMagician.Core.Models;
using Type = System.Type;

namespace PresetMagician.Converters
{
    public class NodeSortConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            FastObservableCollection<PresetBank> nodes = value as FastObservableCollection<PresetBank>;
            if (nodes != null)
            {
                FastObservableCollection<PresetBank> sorted = new FastObservableCollection<PresetBank>(nodes);
                sorted.Sort((x, y) => string.Compare(x.BankName, y.BankName, StringComparison.Ordinal));
                return sorted;
            }
            else
                throw new ArgumentException();
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

}