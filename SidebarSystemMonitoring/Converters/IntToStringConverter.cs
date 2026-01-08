using System;
using System.Globalization;
using System.Windows.Data;

namespace SidebarSystemMonitoring.Converters;

public class IntToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Need to check if throwing this breaks anything
        //ArgumentNullException.ThrowIfNull(value, nameof(value));
        var format = (string)parameter;

        if (string.IsNullOrEmpty(format))
        {
            return value.ToString();
        }
        
        return string.Format(culture, format, value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        //ArgumentNullException.ThrowIfNull(value, nameof(value));
        int result = 0;

        int.TryParse(value.ToString(), out result);

        return result;
    }
}