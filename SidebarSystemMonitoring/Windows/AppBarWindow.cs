using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using SidebarSystemMonitoring.Windows.Enums;
using SidebarSystemMonitoring.Windows.Structs;

namespace SidebarSystemMonitoring.Windows;

public partial class AppBarWindow : DpiAwareWindow
{
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
            WindowPos _pos = (WindowPos)Marshal.PtrToStructure(lParam, typeof(WindowPos));

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

        NativeMethods.DwmSetWindowAttribute(_hwnd, DwmWindowAttribute.DWMWA_EXCLUDED_FROM_PEEK, _status, sizeof(int));
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

        AppBarData _data = NewData();

        _callbackID = _data.uCallbackMessage = NativeMethods.RegisterWindowMessage("AppBarMessage");

        NativeMethods.SHAppBarMessage((int) AppBarMsg.ABM_NEW, ref _data);

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

        NativeMethods.SHAppBarMessage((int) AppBarMsg.ABM_QUERYPOS, ref _data);

        NativeMethods.SHAppBarMessage((int) AppBarMsg.ABM_SETPOS, ref _data);

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

        AppBarData _data = NewData();

        NativeMethods.SHAppBarMessage((int) AppBarMsg.ABM_REMOVE, ref _data);

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

    private AppBarData NewData()
    {
        AppBarData _data = new AppBarData();
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
                case (int) AppBarNotify.ABN_POSCHANGED:
                    //SetAppBar(); removed due to constant refreshing bug
                    break;

                case (int) AppBarNotify.ABN_FULLSCREENAPP:
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