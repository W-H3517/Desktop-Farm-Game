using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;     // ← 新增
using System.Collections.Generic;  // ← 新增

public class TransparentWindow : MonoBehaviour
{
    public static TransparentWindow Instance => _instance;
    public static TransparentWindow _instance;

    private void Awake()
    {
        _instance = this;
    }

    [DllImport("user32.dll")]  static extern int  GetWindowLong(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")]  static extern int  SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    [DllImport("Dwmapi.dll")] static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

    const int  GWL_EXSTYLE       = -20;
    const int  WS_EX_LAYERED     = 0x00080000;
    const int  WS_EX_TRANSPARENT = 0x00000020;

    const uint SWP_NOMOVE = 0x0002, SWP_NOSIZE = 0x0001, SWP_SHOWWINDOW = 0x0040;

    struct MARGINS { public int cxLeftWidth, cxRightWidth, cyTopHeight, cyBottomHeight; }

    IntPtr hWnd;   // 缓存句柄
    public bool clickThrough;

    private Camera _camera;
    private readonly List<RaycastResult> _uiHits = new List<RaycastResult>(); // 复用列表，避免 GC

    void Start()
    {
        // 1) 缓存窗口句柄（打包后的 Player 最稳）
        hWnd = Process.GetCurrentProcess().MainWindowHandle;

        // 2) 置顶（带上 no-move/no-size 标志更稳）
        SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, (int)(SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW));

        // 3) 先只开分层，不开穿透
        int ex = GetWindowLong(hWnd, GWL_EXSTYLE);
        ex |= WS_EX_LAYERED;
        ex &= ~WS_EX_TRANSPARENT;
        SetWindowLong(hWnd, GWL_EXSTYLE, ex);

        // 4) 可选：DWM 玻璃扩展（不是像素透明）
        var margins = new MARGINS { cxLeftWidth = -1 };
        DwmExtendFrameIntoClientArea(hWnd, ref margins);

        // 如果你用色键透明，请在这里调用 SetLayeredWindowAttributes（这段代码里没写）
        _camera = Camera.main;
    }

    public bool uiHit;
    public bool physicsHit;
    
    
    void Update()
    {
        if (!_camera || Mouse.current == null) return;

        // 稳定的屏幕->世界坐标（2D）
        Vector2 mouseScreen = Mouse.current.position.ReadValue();

        // --- UI 命中检测 ---
        uiHit = IsPointerOverUI(mouseScreen);

        // --- 2D 物理命中检测（保留你原来的逻辑）---
        Vector3 world = _camera.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, 0f));
        Vector2 p2d = new Vector2(world.x, world.y);
        physicsHit = Physics2D.OverlapPoint(p2d);

        // UI 或 物理任意命中 → 关闭穿透；否则开启穿透
        SetClickthrough(!(uiHit || physicsHit));
    }

    bool IsPointerOverUI(Vector2 screenPos)
    {
        if (!EventSystem.current) return false;

        var ped = new PointerEventData(EventSystem.current)
        {
            position = screenPos
        };

        _uiHits.Clear();
        EventSystem.current.RaycastAll(ped, _uiHits);
        return _uiHits.Count > 0;
    }

    void SetClickthrough(bool enable)
    {
        if (clickThrough == enable || hWnd == IntPtr.Zero) return;
        clickThrough = enable;

        int ex = GetWindowLong(hWnd, GWL_EXSTYLE);
        if (enable) ex |= WS_EX_TRANSPARENT;
        else        ex &= ~WS_EX_TRANSPARENT;
        SetWindowLong(hWnd, GWL_EXSTYLE, ex);
    }
}
