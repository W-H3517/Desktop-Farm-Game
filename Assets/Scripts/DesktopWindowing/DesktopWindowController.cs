using UnityEngine;
using UnityEngine.InputSystem;

namespace DesktopWindowing
{
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

        private PointerHitTester _pointerHitTester;
        private WindowsTransparentWindow _window;
        private int _initializeAttempts;
        private bool _gaveUpOnWindow;

        public bool IsPointerOverUnityInteractive { get; private set; }
        public bool IsPointerOverUI => LastHitKind == HitTestKind.UI;
        public bool IsPointerOverPhysics2D => LastHitKind == HitTestKind.Physics2D;
        public HitTestKind LastHitKind { get; private set; } = HitTestKind.None;

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
            _pointerHitTester = new PointerHitTester(targetCamera, interactable2DMask);
            _window = new WindowsTransparentWindow();
            _initializeAttempts = 0;
            _gaveUpOnWindow = false;
        }

        private void Start()
        {
            LogDiagnosticsIfNeeded();
            TryInitializeWindow();
        }

        private void Update()
        {
            UpdatePointerState();

            if (_window != null && _window.IsInitialized)
            {
                _window.SetPassthroughEnabled(enableClickThrough && !IsPointerOverUnityInteractive);
            }

            if (!_gaveUpOnWindow && _window != null && !_window.IsInitialized)
            {
                TryInitializeWindow();
            }
        }

        private void OnDisable()
        {
            if (Primary == this)
            {
                Primary = null;
            }

            ShutdownWindow();
            ResetPointerState();
        }

        private void OnDestroy()
        {
            if (Primary == this)
            {
                Primary = null;
            }
        }

        public HitTestKind EvaluateHitTest(Vector2 screenPosition)
        {
            if (_pointerHitTester == null)
            {
                ResetPointerState();
                return LastHitKind;
            }

            LastHitKind = _pointerHitTester.GetHitKind(screenPosition);
            IsPointerOverUnityInteractive = LastHitKind != HitTestKind.None;
            return LastHitKind;
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
            LastHitKind = HitTestKind.None;
            IsPointerOverUnityInteractive = false;
        }

        private void TryInitializeWindow()
        {
            if (_window == null || _window.IsInitialized || !IsWindowAllowed())
            {
                return;
            }

            _initializeAttempts++;
            bool initialized;

            try
            {
                initialized = _window.Initialize(EvaluateHitTest, enableTransparency, keepTopmost);
            }
            catch (System.Exception ex)
            {
                _gaveUpOnWindow = true;
                Debug.LogWarning($"DesktopWindowController failed to initialize the window: {ex.Message}", this);
                return;
            }

            if (initialized)
            {
                return;
            }

            if (_initializeAttempts >= 120)
            {
                _gaveUpOnWindow = true;
                Debug.LogWarning("DesktopWindowController could not initialize the Windows transparent window. The app will continue without click-through.", this);
            }
        }

        private void ShutdownWindow()
        {
            if (_window == null)
            {
                return;
            }

            _window.Shutdown();
            _window.Dispose();
            _window = null;
        }

        private bool IsWindowAllowed()
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
}
