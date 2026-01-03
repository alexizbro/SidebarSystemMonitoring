using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Windows.Media;
using SidebarSystemMonitoring.Style;

namespace SidebarSystemMonitoring.Windows
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public int Width
        {
            get
            {
                return Right - Left;
            }
        }

        public int Height
        {
            get
            {
                return Bottom - Top;
            }
        }
    }

    public class WorkArea
    {
        public double Left { get; set; }

        public double Top { get; set; }

        public double Right { get; set; }

        public double Bottom { get; set; }

        public double Width
        {
            get
            {
                return Right - Left;
            }
        }

        public double Height
        {
            get
            {
                return Bottom - Top;
            }
        }

        public void Scale(double x, double y)
        {
            Left *= x;
            Top *= y;
            Right *= x;
            Bottom *= y;
        }

        public void Offset(double x, double y)
        {
            Left += x;
            Top += y;
            Right += x;
            Bottom += y;
        }

        public void SetWidth(DockEdge edge, double width)
        {
            switch (edge)
            {
                case DockEdge.Left:
                    Right = Left + width;
                    break;

                case DockEdge.Right:
                    Left = Right - width;
                    break;
            }
        }

        public static WorkArea FromRECT(RECT rect)
        {
            return new WorkArea()
            {
                Left = rect.Left,
                Top = rect.Top,
                Right = rect.Right,
                Bottom = rect.Bottom
            };
        }
    }

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

    public static class MonitorExtensions
    {
        public static Monitor GetPrimary(this Monitor[] monitors)
        {
            return monitors.Where(m => m.IsPrimary).Single();
        }
    }

    public partial class DPIAwareWindow : FlatWindow
    {
        private static class WM_MESSAGES
        {
            public const int WM_DPICHANGED = 0x02E0;
            public const int WM_GETMINMAXINFO = 0x0024;
            public const int WM_SIZE = 0x0005;
            public const int WM_WINDOWPOSCHANGING = 0x0046;
            public const int WM_WINDOWPOSCHANGED = 0x0047;
        }

        public override void BeginInit()
        {
            Utilities.Culture.SetCurrent(false);

            base.BeginInit();
        }

        public override void EndInit()
        {
            base.EndInit();

            _originalWidth = base.Width;
            _originalHeight = base.Height;

            if (AutoDPI && OS.SupportDPI)
            {
                Loaded += DPIAwareWindow_Loaded;
            }
        }

        public void HandleDPI()
        {
            //IntPtr _hwnd = new WindowInteropHelper(this).Handle;

            //IntPtr _hmonitor = NativeMethods.MonitorFromWindow(_hwnd, 0);

            //Monitor _monitorInfo = Monitor.GetMonitor(_hmonitor);

            double _uiScale = Framework.Settings.Instance.UIScale;

            UpdateScale(_uiScale, _uiScale, true);
        }

        public void UpdateScale(double scaleX, double scaleY, bool resize)
        {
            if (VisualChildrenCount > 0)
            {
                GetVisualChild(0).SetValue(LayoutTransformProperty, new ScaleTransform(scaleX, scaleY));
            }

            if (resize)
            {
                SizeToContent _autosize = SizeToContent;
                SizeToContent = SizeToContent.Manual;

                base.Width = _originalWidth * scaleX;
                base.Height = _originalHeight * scaleY;

                SizeToContent = _autosize;
            }
        }

        private void DPIAwareWindow_Loaded(object sender, RoutedEventArgs e)
        {
            HandleDPI();

            Framework.Settings.Instance.PropertyChanged += UIScale_PropertyChanged;

            //HwndSource.AddHook(WindowHook);
        }

        private void UIScale_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "UIScale")
            {
                HandleDPI();
            }
        }

        //private IntPtr WindowHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        //{
        //    if (msg == WM_MESSAGES.WM_DPICHANGED)
        //    {
        //        HandleDPI();

        //        handled = true;
        //    }

        //    return IntPtr.Zero;
        //}

        public HwndSource HwndSource
        {
            get
            {
                return (HwndSource)PresentationSource.FromVisual(this);
            }
        }

        public static readonly DependencyProperty AutoDPIProperty = DependencyProperty.Register("AutoDPI", typeof(bool), typeof(DPIAwareWindow), new UIPropertyMetadata(true));

        public bool AutoDPI
        {
            get
            {
                return (bool)GetValue(AutoDPIProperty);
            }
            set
            {
                SetValue(AutoDPIProperty, value);
            }
        }

        public new double Width
        {
            get
            {
                return base.Width;
            }
            set
            {
                _originalWidth = base.Width = value;
            }
        }

        public new double Height
        {
            get
            {
                return base.Height;
            }
            set
            {
                _originalHeight = base.Height = value;
            }
        }

        private double _originalWidth { get; set; }

        private double _originalHeight { get; set; }
    }

    [Serializable]
    public enum DockEdge : byte
    {
        Left,
        Top,
        Right,
        Bottom,
        None
    }

    public partial class AppBarWindow : DPIAwareWindow
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct APPBARDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uCallbackMessage;
            public int uEdge;
            public RECT rc;
            public IntPtr lParam;
        }

        private static class APPBARMSG
        {
            public const int ABM_NEW = 0;
            public const int ABM_REMOVE = 1;
            public const int ABM_QUERYPOS = 2;
            public const int ABM_SETPOS = 3;
            public const int ABM_GETSTATE = 4;
            public const int ABM_GETTASKBARPOS = 5;
            public const int ABM_ACTIVATE = 6;
            public const int ABM_GETAUTOHIDEBAR = 7;
            public const int ABM_SETAUTOHIDEBAR = 8;
            public const int ABM_WINDOWPOSCHANGED = 9;
            public const int ABM_SETSTATE = 10;
        }

        private static class APPBARNOTIFY
        {
            public const int ABN_STATECHANGE = 0;
            public const int ABN_POSCHANGED = 1;
            public const int ABN_FULLSCREENAPP = 2;
            public const int ABN_WINDOWARRANGE = 3;
        }

        internal enum DWMWINDOWATTRIBUTE : int
        {
            DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY = 2,
            DWMWA_TRANSITIONS_FORCEDISABLED = 3,
            DWMWA_ALLOW_NCPAINT = 4,
            DWMWA_CAPTION_BUTTON_BOUNDS = 5,
            DWMWA_NONCLIENT_RTL_LAYOUT = 6,
            DWMWA_FORCE_ICONIC_REPRESENTATION = 7,
            DWMWA_FLIP3D_POLICY = 8,
            DWMWA_EXTENDED_FRAME_BOUNDS = 9,
            DWMWA_HAS_ICONIC_BITMAP = 10,
            DWMWA_DISALLOW_PEEK = 11,
            DWMWA_EXCLUDED_FROM_PEEK = 12,
            DWMWA_CLOAK = 13,
            DWMWA_CLOAKED = 14,
            DWMWA_FREEZE_REPRESENTATION = 15,
            DWMWA_LAST = 16
        }

        private static class HWND_FLAG
        {
            public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
            public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
            public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

            public const uint SWP_NOSIZE = 0x0001;
            public const uint SWP_NOMOVE = 0x0002;
            public const uint SWP_NOACTIVATE = 0x0010;
        }

        private static class WND_STYLE
        {
            public const int GWL_EXSTYLE = -20;

            public const long WS_EX_TRANSPARENT = 32;
            public const long WS_EX_TOOLWINDOW = 128;
        }

        private static class WM_WINDOWPOSCHANGING
        {
            public const int MSG = 0x0046;
            public const int SWP_NOMOVE = 0x0002;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WINDOWPOS
        {
            public IntPtr hWnd;
            public IntPtr hWndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public uint flags;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Loaded += AppBarWindow_Loaded;
        }

        private void AppBarWindow_Loaded(object sender, RoutedEventArgs e)
        {
            PreventMove();
        }

        public void Move(WorkArea workArea)
        {
            AllowMove();

            Left = workArea.Left;
            Top = workArea.Top;
            Width = workArea.Width;
            Height = workArea.Height;

            PreventMove();
        }

        private void PreventMove()
        {
            if (!_canMove)
            {
                return;
            }

            _canMove = false;

            HwndSource.AddHook(MoveHook);
        }

        private void AllowMove()
        {
            if (_canMove)
            {
                return;
            }

            _canMove = true;

            HwndSource.RemoveHook(MoveHook);
        }

        private IntPtr MoveHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_WINDOWPOSCHANGING.MSG)
            {
                WINDOWPOS _pos = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));

                _pos.flags |= WM_WINDOWPOSCHANGING.SWP_NOMOVE;

                Marshal.StructureToPtr(_pos, lParam, true);

                handled = true;
            }

            return IntPtr.Zero;
        }

        public void SetTopMost(bool activate)
        {
            if (IsTopMost)
            {
                return;
            }

            IsTopMost = true;

            SetPos(HWND_FLAG.HWND_TOPMOST, activate);
        }

        public void ClearTopMost(bool activate)
        {
            if (!IsTopMost)
            {
                return;
            }

            IsTopMost = false;

            SetPos(HWND_FLAG.HWND_NOTOPMOST, activate);
        }

        public void SetBottom(bool activate)
        {
            IsTopMost = false;

            SetPos(HWND_FLAG.HWND_BOTTOM, activate);
        }

        private void SetPos(IntPtr hwnd_after, bool activate)
        {
            uint _uflags = HWND_FLAG.SWP_NOMOVE | HWND_FLAG.SWP_NOSIZE;

            if (!activate)
            {
                _uflags |= HWND_FLAG.SWP_NOACTIVATE;
            }

            NativeMethods.SetWindowPos(
                new WindowInteropHelper(this).Handle,
                hwnd_after,
                0,
                0,
                0,
                0,
                _uflags
                );
        }

        public void SetClickThrough()
        {
            if (IsClickThrough)
            {
                return;
            }

            IsClickThrough = true;

            SetWindowLong(WND_STYLE.WS_EX_TRANSPARENT, null);
        }

        public void ClearClickThrough()
        {
            if (!IsClickThrough)
            {
                return;
            }

            IsClickThrough = false;

            SetWindowLong(null, WND_STYLE.WS_EX_TRANSPARENT);
        }

        public void ShowInAltTab()
        {
            if (IsInAltTab)
            {
                return;
            }

            IsInAltTab = true;

            SetWindowLong(null, WND_STYLE.WS_EX_TOOLWINDOW);
        }

        public void HideInAltTab()
        {
            if (!IsInAltTab)
            {
                return;
            }

            IsInAltTab = false;

            SetWindowLong(WND_STYLE.WS_EX_TOOLWINDOW, null);
        }

        public void DisableAeroPeek()
        {
            IntPtr _hwnd = new WindowInteropHelper(this).Handle;

            IntPtr _status = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(_status, 1);

            NativeMethods.DwmSetWindowAttribute(_hwnd, DWMWINDOWATTRIBUTE.DWMWA_EXCLUDED_FROM_PEEK, _status, sizeof(int));
        }

        private void SetWindowLong(long? add, long? remove)
        {
            IntPtr _hwnd = new WindowInteropHelper(this).Handle;

            bool _32bit = IntPtr.Size == 4;

            long _style;

            if (_32bit)
            {
                _style = NativeMethods.GetWindowLong(_hwnd, WND_STYLE.GWL_EXSTYLE);
            }
            else
            {
                _style = NativeMethods.GetWindowLongPtr(_hwnd, WND_STYLE.GWL_EXSTYLE);
            }

            if (add.HasValue)
            {
                _style |= add.Value;
            }

            if (remove.HasValue)
            {
                _style &= ~remove.Value;
            }

            if (_32bit)
            {
                NativeMethods.SetWindowLong(_hwnd, WND_STYLE.GWL_EXSTYLE, _style);
            }
            else
            {
                NativeMethods.SetWindowLongPtr(_hwnd, WND_STYLE.GWL_EXSTYLE, _style);
            }
        }

        public async Task SetAppBar()
        {
            ClearAppBar();

            await Task.Delay(100).ContinueWith(async (_) =>
            {
                await Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action)(async () =>
                {
                    await BindAppBar();
                }));
            });

        }

        private async Task BindAppBar()
        {
            Monitor.GetWorkArea(this, out int screen, out DockEdge edge, out WorkArea initPos, out WorkArea windowWA, out WorkArea appbarWA);

            Move(initPos);

            APPBARDATA _data = NewData();

            _callbackID = _data.uCallbackMessage = NativeMethods.RegisterWindowMessage("AppBarMessage");

            NativeMethods.SHAppBarMessage(APPBARMSG.ABM_NEW, ref _data);

            Screen = screen;
            DockEdge = edge;

            _data.uEdge = (int)edge;
            _data.rc = new RECT()
            {
                Left = (int)Math.Round(appbarWA.Left),
                Top = (int)Math.Round(appbarWA.Top),
                Right = (int)Math.Round(appbarWA.Right),
                Bottom = (int)Math.Round(appbarWA.Bottom)
            };

            NativeMethods.SHAppBarMessage(APPBARMSG.ABM_QUERYPOS, ref _data);

            NativeMethods.SHAppBarMessage(APPBARMSG.ABM_SETPOS, ref _data);

            IsAppBar = true;

            appbarWA.Left = _data.rc.Left;
            appbarWA.Top = _data.rc.Top;
            appbarWA.Right = _data.rc.Right;
            appbarWA.Bottom = _data.rc.Bottom;

            AppBarWidth = appbarWA.Width;

            await Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action)(() =>
            {
                Move(windowWA);
            }));

            await Task.Delay(500).ContinueWith(async (_) =>
            {
                await Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action)(() =>
                {
                    HwndSource.AddHook(AppBarHook);
                }));
            });
        }

        public void ClearAppBar()
        {
            if (!IsAppBar)
            {
                return;
            }

            HwndSource.RemoveHook(AppBarHook);

            APPBARDATA _data = NewData();

            NativeMethods.SHAppBarMessage(APPBARMSG.ABM_REMOVE, ref _data);

            IsAppBar = false;
        }

        public virtual async Task AppBarShow()
        {
            if (Framework.Settings.Instance.UseAppBar)
            {
                await SetAppBar();
            }

            Show();
        }

        public virtual void AppBarHide()
        {
            Hide();

            if (IsAppBar)
            {
                ClearAppBar();
            }
        }

        private APPBARDATA NewData()
        {
            APPBARDATA _data = new APPBARDATA();
            _data.cbSize = Marshal.SizeOf(_data);
            _data.hWnd = new WindowInteropHelper(this).Handle;

            return _data;
        }

        private IntPtr AppBarHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == _callbackID)
            {
                switch (wParam.ToInt32())
                {
                    case APPBARNOTIFY.ABN_POSCHANGED:
                        //SetAppBar(); removed due to constant refreshing bug
                        break;

                    case APPBARNOTIFY.ABN_FULLSCREENAPP:
                        if (lParam.ToInt32() == 1)
                        {
                            _wasTopMost = IsTopMost;

                            if (IsTopMost)
                            {
                                SetBottom(false);
                            }
                        }
                        else if (_wasTopMost)
                        {
                            SetTopMost(false);
                        }
                        break;
                }

                handled = true;
            }

            return IntPtr.Zero;
        }

        public bool IsTopMost { get; private set; } = false;

        public bool IsClickThrough { get; private set; } = false;

        public bool IsInAltTab { get; private set; } = true;

        public bool IsAppBar { get; private set; } = false;

        public int Screen { get; private set; } = 0;

        public DockEdge DockEdge { get; private set; } = DockEdge.None;

        public double AppBarWidth { get; private set; } = 0;

        private bool _canMove { get; set; } = true;

        private bool _wasTopMost { get; set; } = false;

        private int _callbackID { get; set; }

        private CancellationTokenSource _cancelReposition { get; set; }
    }
}