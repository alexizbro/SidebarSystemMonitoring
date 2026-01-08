using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;
using SidebarSystemMonitoring.Windows;

namespace SidebarSystemMonitoring.Converters;

public class HotkeyToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var hotkey = (Hotkey)value;

        if (hotkey == null)
        {
            return "None";
        }

        return
            (hotkey.AltMod ? "Alt + " : "") +
            (hotkey.CtrlMod ? "Ctrl + " : "") +
            (hotkey.ShiftMod ? "Shift + " : "") +
            (hotkey.WinMod ? "Win + " : "") +
            new KeyConverter().ConvertToString(hotkey.WinKey);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}