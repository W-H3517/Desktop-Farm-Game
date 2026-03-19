using UnityEngine;
using UnityEngine.InputSystem;

public sealed class DesktopWindowController : MonoBehaviour
{
    public static DesktopWindowController Primary { get; private set; }

    [SerializeField] private Camera targetCamera;
    [SerializeField] private bool enableTransparency = true;
    [SerializeField] private bool keepTopmost = true;
    [SerializeField] private LayerMask interactable2DMask = Physics2D.DefaultRaycastLayers;
    [SerializeField] private bool enableInEditor;
    [SerializeField] private bool logDiagnostics;
    [SerializeField] private bool enableClickThrough = true;

    private IHitTestService _hitTestService;
    private IWindowBackend _windowBackend;
    private int _initializeAttempts;
    private bool _gaveUpOnBackend;

    public bool IsPointerOverUnityInteractive { get; private set; }
    public bool IsPointerOverUI => LastHitResult.IsUI;
    public bool IsPointerOverPhysics2D => LastHitResult.IsPhysics2D;
    public HitTestKind LastHitKind => LastHitResult.Kind;
    public HitTestResult LastHitResult { get; private set; } = HitTestResult.None;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();
        }
    }

    private void OnEnable()
    {
        Primary = this;
        _hitTestService = new UnityHitTestService(targetCamera, interactable2DMask);
        _windowBackend = new WindowsLayeredWindowBackend();
        _initializeAttempts = 0;
        _gaveUpOnBackend = false;
    }

    private void Start()
    {
        LogDiagnosticsIfNeeded();
        TryInitializeBackend();
    }

    private void Update()
    {
        UpdatePointerState();

        if (_windowBackend != null && _windowBackend.IsInitialized)
        {
            _windowBackend.SetPassthroughEnabled(enableClickThrough && !IsPointerOverUnityInteractive);
        }

        if (!_gaveUpOnBackend && !_windowBackend.IsInitialized)
        {
            TryInitializeBackend();
        }
    }

    private void OnDisable()
    {
        if (Primary == this)
        {
            Primary = null;
        }

        ShutdownBackend();
        ResetPointerState();
    }

    private void OnDestroy()
    {
        if (Primary == this)
        {
            Primary = null;
        }
    }

    public HitTestResult EvaluateHitTest(Vector2 screenPosition)
    {
        if (_hitTestService == null)
        {
            LastHitResult = HitTestResult.None;
            IsPointerOverUnityInteractive = false;
            return LastHitResult;
        }

        LastHitResult = _hitTestService.HitTest(screenPosition);
        IsPointerOverUnityInteractive = LastHitResult.IsInteractive;
        return LastHitResult;
    }

    private void UpdatePointerState()
    {
#if UNITY_EDITOR
        if (!enableInEditor)
        {
            ResetPointerState();
            return;
        }
#endif

        if (Mouse.current == null)
        {
            ResetPointerState();
            return;
        }

        EvaluateHitTest(Mouse.current.position.ReadValue());
    }

    private void ResetPointerState()
    {
        LastHitResult = HitTestResult.None;
        IsPointerOverUnityInteractive = false;
    }

    private void TryInitializeBackend()
    {
        if (_windowBackend == null || _windowBackend.IsInitialized || !IsBackendAllowed())
        {
            return;
        }

        _initializeAttempts++;
        bool initialized = false;

        try
        {
            initialized = _windowBackend.Initialize(EvaluateHitTest, enableTransparency, keepTopmost);
        }
        catch (System.Exception ex)
        {
            _gaveUpOnBackend = true;
            Debug.LogWarning($"DesktopWindowController failed to initialize window backend: {ex.Message}", this);
            return;
        }

        if (initialized)
        {
            return;
        }

        if (_initializeAttempts >= 120)
        {
            _gaveUpOnBackend = true;
            Debug.LogWarning("DesktopWindowController could not initialize the Windows window backend. The app will continue without transparency hit-testing.", this);
        }
    }

    private void ShutdownBackend()
    {
        if (_windowBackend == null)
        {
            return;
        }

        _windowBackend.Shutdown();
        _windowBackend.Dispose();
        _windowBackend = null;
    }

    private bool IsBackendAllowed()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        return true;
#else
        return false;
#endif
    }

    private void LogDiagnosticsIfNeeded()
    {
        if (!logDiagnostics)
        {
            return;
        }

        string cameraName = targetCamera != null ? targetCamera.name : "<null>";
        string eventSystemState = UnityEngine.EventSystems.EventSystem.current != null ? "present" : "missing";
        Debug.Log($"DesktopWindowController diagnostics: camera={cameraName}, eventSystem={eventSystemState}, enableTransparency={enableTransparency}, keepTopmost={keepTopmost}", this);
    }
}
