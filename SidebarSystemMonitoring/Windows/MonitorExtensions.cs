using System.Linq;

namespace SidebarSystemMonitoring.Windows;

public static class MonitorExtensions
{
    public static Monitor GetPrimary(this Monitor[] monitors)
    {
        return monitors.Single(m => m.IsPrimary);
    }
}