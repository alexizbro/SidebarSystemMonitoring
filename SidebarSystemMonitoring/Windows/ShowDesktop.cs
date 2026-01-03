using System;
using System.Text;
using System.Windows.Interop;

namespace SidebarSystemMonitoring.Windows;

public static class ShowDesktop
    {
        private const uint WINEVENT_OUTOFCONTEXT = 0u;
        private const uint EVENT_SYSTEM_FOREGROUND = 3u;

        private const string WORKERW = "WorkerW";
        private const string PROGMAN = "Progman";

        public static void AddHook(Sidebar sidebar)
        {
            if (IsHooked)
            {
                return;
            }

            IsHooked = true;

            _delegate = new WinEventDelegate(WinEventHook);
            _hookIntPtr = NativeMethods.SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, _delegate, 0, 0, WINEVENT_OUTOFCONTEXT);
            _sidebar = sidebar;
            _sidebarHwnd = new WindowInteropHelper(sidebar).Handle;
        }

        public static void RemoveHook()
        {
            if (!IsHooked)
            {
                return;
            }

            IsHooked = false;

            NativeMethods.UnhookWinEvent(_hookIntPtr.Value);

            _delegate = null;
            _hookIntPtr = null;
            _sidebar = null;
            _sidebarHwnd = null;
        }

        private static string GetWindowClass(IntPtr hwnd)
        {
            StringBuilder _sb = new StringBuilder(32);
            NativeMethods.GetClassName(hwnd, _sb, _sb.Capacity);
            return _sb.ToString();
        }

        internal delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        private static void WinEventHook(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (eventType == EVENT_SYSTEM_FOREGROUND)
            {
                string _class = GetWindowClass(hwnd);

                if (string.Equals(_class, WORKERW, StringComparison.Ordinal) /*|| string.Equals(_class, PROGMAN, StringComparison.Ordinal)*/ )
                {
                    _sidebar.SetTopMost(false);
                }
                else if (_sidebar.IsTopMost)
                {
                    _sidebar.SetBottom(false);
                }
            }
        }

        public static bool IsHooked { get; private set; } = false;

        private static IntPtr? _hookIntPtr { get; set; }

        private static WinEventDelegate _delegate { get; set; }

        private static Sidebar _sidebar { get; set; }

        private static IntPtr? _sidebarHwnd { get; set; }
    }