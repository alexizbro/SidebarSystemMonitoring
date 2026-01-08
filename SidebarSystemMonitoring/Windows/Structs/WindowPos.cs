using System;
using System.Runtime.InteropServices;

namespace SidebarSystemMonitoring.Windows.Structs;

[StructLayout(LayoutKind.Sequential)]
public struct WindowPos
{
    public IntPtr hWnd;
    public IntPtr hWndInsertAfter;
    public int x;
    public int y;
    public int cx;
    public int cy;
    public uint flags;
}