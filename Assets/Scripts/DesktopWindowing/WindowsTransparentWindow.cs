using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

namespace DesktopWindowing
{
public sealed class WindowsTransparentWindow : IDisposable
{
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    private const int GWL_EXSTYLE = -20;
    private const int GWLP_WNDPROC = -4;

    private const int WS_EX_LAYERED = 0x00080000;
    private const int WS_EX_TRANSPARENT = 0x00000020;

    private const int WM_NCHITTEST = 0x0084;

    private const int HTCLIENT = 1;

    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_SHOWWINDOW = 0x0040;

    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

    [StructLayout(LayoutKind.Sequential)]
    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll")]
    private static extern void SetLastError(uint dwErrCode);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW", SetLastError = true)]
    private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW", SetLastError = true)]
    private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [DllImport("dwmapi.dll", SetLastError = true)]
    private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

    private IntPtr _windowHandle;
    private IntPtr _originalWndProc;
    private int _originalExStyle;
    private Func<Vector2, HitTestKind> _hitTestCallback;
    private WndProcDelegate _wndProcDelegate;
    private bool _passthroughEnabled;

    public bool IsInitialized { get; private set; }

    public bool Initialize(Func<Vector2, HitTestKind> hitTestCallback, bool enableTransparency, bool keepTopmost)
    {
        if (IsInitialized)
        {
            return true;
        }

        _hitTestCallback = hitTestCallback;
        _windowHandle = Process.GetCurrentProcess().MainWindowHandle;
        if (_windowHandle == IntPtr.Zero)
        {
            return false;
        }

        _originalExStyle = GetWindowLongPtr(_windowHandle, GWL_EXSTYLE).ToInt32();
        int updatedExStyle = _originalExStyle;
        if (enableTransparency)
        {
            updatedExStyle |= WS_EX_LAYERED;
        }
        else
        {
            updatedExStyle &= ~WS_EX_LAYERED;
        }

        updatedExStyle &= ~WS_EX_TRANSPARENT;

        if (!TrySetWindowLongPtr(_windowHandle, GWL_EXSTYLE, new IntPtr(updatedExStyle)))
        {
            return false;
        }

        if (keepTopmost)
        {
            SetWindowPos(_windowHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
        }

        if (enableTransparency)
        {
            MARGINS margins = new MARGINS { cxLeftWidth = -1 };
            DwmExtendFrameIntoClientArea(_windowHandle, ref margins);
        }

        _wndProcDelegate = CustomWndProc;
        IntPtr wndProcPtr = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate);
        _originalWndProc = SetWindowLongPtr(_windowHandle, GWLP_WNDPROC, wndProcPtr);
        if (_originalWndProc == IntPtr.Zero && Marshal.GetLastWin32Error() != 0)
        {
            TrySetWindowLongPtr(_windowHandle, GWL_EXSTYLE, new IntPtr(_originalExStyle));
            return false;
        }

        IsInitialized = true;
        _passthroughEnabled = false;
        return true;
    }

    public void SetPassthroughEnabled(bool enabled)
    {
        if (!IsInitialized || _windowHandle == IntPtr.Zero || _passthroughEnabled == enabled)
        {
            return;
        }

        IntPtr currentStylePtr = GetWindowLongPtr(_windowHandle, GWL_EXSTYLE);
        int currentStyle = currentStylePtr.ToInt32();
        int updatedStyle = enabled
            ? currentStyle | WS_EX_TRANSPARENT
            : currentStyle & ~WS_EX_TRANSPARENT;

        if (TrySetWindowLongPtr(_windowHandle, GWL_EXSTYLE, new IntPtr(updatedStyle)))
        {
            _passthroughEnabled = enabled;
        }
    }

    public void Shutdown()
    {
        if (!IsInitialized)
        {
            return;
        }

        if (_windowHandle != IntPtr.Zero)
        {
            if (_originalWndProc != IntPtr.Zero)
            {
                TrySetWindowLongPtr(_windowHandle, GWLP_WNDPROC, _originalWndProc);
            }

            TrySetWindowLongPtr(_windowHandle, GWL_EXSTYLE, new IntPtr(_originalExStyle));
        }

        _hitTestCallback = null;
        _wndProcDelegate = null;
        _originalWndProc = IntPtr.Zero;
        _windowHandle = IntPtr.Zero;
        IsInitialized = false;
        _passthroughEnabled = false;
    }

    public void Dispose()
    {
        Shutdown();
    }

    private IntPtr CustomWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == WM_NCHITTEST && _hitTestCallback != null)
        {
            Vector2 screenPosition = DecodeScreenPoint(lParam);
            Vector2 unityScreenPosition = ConvertToUnityScreenPosition(hWnd, screenPosition);
            HitTestKind hitKind = _hitTestCallback(unityScreenPosition);
            return hitKind != HitTestKind.None ? new IntPtr(HTCLIENT) : CallWindowProc(_originalWndProc, hWnd, msg, wParam, lParam);
        }

        return CallWindowProc(_originalWndProc, hWnd, msg, wParam, lParam);
    }

    private static Vector2 DecodeScreenPoint(IntPtr lParam)
    {
        long value = lParam.ToInt64();
        short x = unchecked((short)(value & 0xFFFF));
        short y = unchecked((short)((value >> 16) & 0xFFFF));
        return new Vector2(x, y);
    }

    private static Vector2 ConvertToUnityScreenPosition(IntPtr hWnd, Vector2 nativeScreenPosition)
    {
        POINT clientPoint = new POINT
        {
            X = Mathf.RoundToInt(nativeScreenPosition.x),
            Y = Mathf.RoundToInt(nativeScreenPosition.y)
        };

        if (!ScreenToClient(hWnd, ref clientPoint))
        {
            return nativeScreenPosition;
        }

        if (!GetClientRect(hWnd, out RECT clientRect))
        {
            return new Vector2(clientPoint.X, clientPoint.Y);
        }

        float unityY = clientRect.Bottom - clientPoint.Y;
        return new Vector2(clientPoint.X, unityY);
    }

    private static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
    {
        return IntPtr.Size == 8
            ? GetWindowLongPtr64(hWnd, nIndex)
            : new IntPtr(GetWindowLong32(hWnd, nIndex));
    }

    private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr newValue)
    {
        return IntPtr.Size == 8
            ? SetWindowLongPtr64(hWnd, nIndex, newValue)
            : new IntPtr(SetWindowLong32(hWnd, nIndex, newValue.ToInt32()));
    }

    private static bool TrySetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr newValue)
    {
        SetLastError(0);
        IntPtr previousValue = SetWindowLongPtr(hWnd, nIndex, newValue);
        return previousValue != IntPtr.Zero || Marshal.GetLastWin32Error() == 0;
    }
#else
    public bool IsInitialized { get; private set; }

    public bool Initialize(Func<Vector2, HitTestKind> hitTestCallback, bool enableTransparency, bool keepTopmost)
    {
        return false;
    }

    public void SetPassthroughEnabled(bool enabled)
    {
    }

    public void Shutdown()
    {
        IsInitialized = false;
    }

    public void Dispose()
    {
        Shutdown();
    }
#endif
}
}
