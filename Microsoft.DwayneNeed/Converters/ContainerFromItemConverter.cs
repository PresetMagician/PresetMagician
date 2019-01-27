using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace Microsoft.DwayneNeed.Converters
{
    /// <summary>
    ///     Returns the container in the specified ItemsControl for the specified item.
    /// </summary>
    /// <remarks>
    ///     <MultiBinding Converter="{StaticResource converter}">
    ///         <Binding ElementName="MainMdiView"/>
    ///         <Binding ElementName="MainMdiView" Path="SelectedItem"/>
    ///     </MultiBinding>
    /// </remarks>
    public class ContainerFromItemConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
            {
                throw new ArgumentException("values is expected to contain 2 values: the items control and the item");
            }

            ItemsControl itemsControl = (ItemsControl) values[0];
            if (itemsControl == null)
            {
                throw new ArgumentException(
                    "The ItemsControl must be specified as the first value and must not be null.");
            }

            object item = values[1];
            if (item != null)
            {
                return itemsControl.ItemContainerGenerator.ContainerFromItem(item);
            }
            else
            {
                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}