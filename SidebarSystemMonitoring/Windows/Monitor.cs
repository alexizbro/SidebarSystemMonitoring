using System.Runtime.InteropServices;
using SidebarSystemMonitoring.Windows.Structs;
using SidebarSystemMonitoring.Windows.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SidebarSystemMonitoring.Windows;

public class Monitor
{
    private const uint DPICONST = 96u;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MONITORINFO
    {
        public int cbSize;
        public RECT Size;
        public RECT WorkArea;
        public bool IsPrimary;
    }

    internal enum MONITOR_DPI_TYPE : int
    {
        MDT_EFFECTIVE_DPI = 0,
        MDT_ANGULAR_DPI = 1,
        MDT_RAW_DPI = 2,
        MDT_DEFAULT = MDT_EFFECTIVE_DPI
    }

    public RECT Size { get; set; }

    public RECT WorkArea { get; set; }

    public double DPIx { get; set; }

    public double ScaleX
    {
        get
        {
            return DPIx / DPICONST;
        }
    }

    public double InverseScaleX
    {
        get
        {
            return 1 / ScaleX;
        }
    }

    public double DPIy { get; set; }

    public double ScaleY
    {
        get
        {
            return DPIy / DPICONST;
        }
    }

    public double InverseScaleY
    {
        get
        {
            return 1 / ScaleY;
        }
    }

    public bool IsPrimary { get; set; }

    internal delegate bool EnumCallback(IntPtr hDesktop, IntPtr hdc, ref RECT pRect, int dwData);

    public static Monitor GetMonitor(IntPtr hMonitor)
    {
        MONITORINFO _info = new MONITORINFO();
        _info.cbSize = Marshal.SizeOf(_info);

        NativeMethods.GetMonitorInfo(hMonitor, ref _info);

        uint _dpiX = Monitor.DPICONST;
        uint _dpiY = Monitor.DPICONST;

        if (OS.SupportDPI)
        {
            NativeMethods.GetDpiForMonitor(hMonitor, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out _dpiX, out _dpiY);
        }

        return new Monitor()
        {
            Size = _info.Size,
            WorkArea = _info.WorkArea,
            DPIx = _dpiX,
            DPIy = _dpiY,
            IsPrimary = _info.IsPrimary
        };
    }

    public static Monitor[] GetMonitors()
    {
        List<Monitor> _monitors = new List<Monitor>();

        EnumCallback _callback = (IntPtr hMonitor, IntPtr hdc, ref RECT pRect, int dwData) =>
        {
            _monitors.Add(GetMonitor(hMonitor));

            return true;
        };

        NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, _callback, 0);

        return _monitors.OrderByDescending(m => m.IsPrimary).ToArray();
    }

    public static Monitor GetMonitorFromIndex(int index)
    {
        return GetMonitorFromIndex(index, GetMonitors());
    }

    private static Monitor GetMonitorFromIndex(int index, Monitor[] monitors)
    {
        if (index < monitors.Length)
            return monitors[index];
        else
            return monitors.GetPrimary();
    }

    public static void GetWorkArea(AppBarWindow window, out int screen, out DockEdge edge, out WorkArea initPos, out WorkArea windowWA, out WorkArea appbarWA)
    {
        screen = Framework.Settings.Instance.ScreenIndex;
        edge = Framework.Settings.Instance.DockEdge;

        double _uiScale = Framework.Settings.Instance.UIScale;

        if (OS.SupportDPI)
        {
            window.UpdateScale(_uiScale, _uiScale, false);
        }

        Monitor[] _monitors = GetMonitors();

        Monitor _primary = _monitors.GetPrimary();
        Monitor _active = GetMonitorFromIndex(screen, _monitors);

        initPos = new WorkArea()
        {
            Top = _active.WorkArea.Top,
            Left = _active.WorkArea.Left,
            Bottom = _active.WorkArea.Top + 10,
            Right = _active.WorkArea.Left + 10
        };

        windowWA = Windows.WorkArea.FromRECT(_active.WorkArea);
        windowWA.Scale(_active.InverseScaleX, _active.InverseScaleY);

        double _modifyX = 0d;
        double _modifyY = 0d;

        windowWA.Offset(_modifyX, _modifyY);

        double _windowWidth = Framework.Settings.Instance.SidebarWidth * _uiScale;

        windowWA.SetWidth(edge, _windowWidth);

        int _offsetX = Framework.Settings.Instance.XOffset;
        int _offsetY = Framework.Settings.Instance.YOffset;

        windowWA.Offset(_offsetX, _offsetY);

        appbarWA = Windows.WorkArea.FromRECT(_active.WorkArea);

        appbarWA.Offset(_modifyX, _modifyY);

        double _appbarWidth = Framework.Settings.Instance.UseAppBar ? windowWA.Width * _active.ScaleX : 0;

        appbarWA.SetWidth(edge, _appbarWidth);

        appbarWA.Offset(_offsetX, _offsetY);
    }
}