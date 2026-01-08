using System;
using System.Globalization;
using System.Windows.Data;

namespace SidebarSystemMonitoring.Converters;

public class MetricLabelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string result = (string)value;

        return string.Format("{0}:", result);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}