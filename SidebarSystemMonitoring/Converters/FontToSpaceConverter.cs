using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SidebarSystemMonitoring.Converters;

public class FontToSpaceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        int result = (int)value;

        return new Thickness(0, 0, result * 0.4d, 0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}