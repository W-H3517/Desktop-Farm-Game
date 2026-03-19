using System;
using UnityEngine;

public interface IWindowBackend : IDisposable
{
    bool IsInitialized { get; }

    bool Initialize(Func<Vector2, HitTestResult> hitTestCallback, bool enableTransparency, bool keepTopmost);
    void SetPassthroughEnabled(bool enabled);
    void Shutdown();
}
