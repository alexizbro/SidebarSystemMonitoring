using System;
using System.Runtime.InteropServices;

namespace SidebarSystemMonitoring.Windows.Structs;

[StructLayout(LayoutKind.Sequential)]
public struct AppBarData
{
    public int cbSize;
    public IntPtr hWnd;
    public int uCallbackMessage;
    public int uEdge;
    public RECT rc;
    public IntPtr lParam;
}