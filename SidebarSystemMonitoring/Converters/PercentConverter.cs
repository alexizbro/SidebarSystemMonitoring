using System;
using System.Globalization;
using System.Windows.Data;

namespace SidebarSystemMonitoring.Converters;

public class PercentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var result = (double)value;

        return string.Format("{0:0}%", result);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}