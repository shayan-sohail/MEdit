using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MEdit.Converters
{
    public class HeaderToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Assuming the input is a boolean
            if (value is string header)
            {
                return header != "Name";
            }
            return true;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
