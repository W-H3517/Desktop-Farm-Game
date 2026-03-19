using UnityEngine;

public enum HitTestKind
{
    None = 0,
    UI = 1,
    Physics2D = 2,
}

public struct HitTestResult
{
    public static readonly HitTestResult None = new HitTestResult(HitTestKind.None, Vector2.zero);

    public HitTestResult(HitTestKind kind, Vector2 screenPosition)
    {
        Kind = kind;
        ScreenPosition = screenPosition;
    }

    public HitTestKind Kind { get; }
    public Vector2 ScreenPosition { get; }

    public bool IsInteractive => Kind != HitTestKind.None;
    public bool IsUI => Kind == HitTestKind.UI;
    public bool IsPhysics2D => Kind == HitTestKind.Physics2D;
}
