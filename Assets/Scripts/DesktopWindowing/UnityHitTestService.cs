using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class UnityHitTestService : IHitTestService
{
    private readonly Camera _targetCamera;
    private readonly int _interactable2DMask;
    private readonly List<RaycastResult> _uiHits = new List<RaycastResult>();

    public UnityHitTestService(Camera targetCamera, LayerMask interactable2DMask)
    {
        _targetCamera = targetCamera;
        _interactable2DMask = interactable2DMask.value;
    }

    public HitTestResult HitTest(Vector2 screenPosition)
    {
        if (IsPointerOverUI(screenPosition))
        {
            return new HitTestResult(HitTestKind.UI, screenPosition);
        }

        if (IsPointerOverPhysics2D(screenPosition))
        {
            return new HitTestResult(HitTestKind.Physics2D, screenPosition);
        }

        return new HitTestResult(HitTestKind.None, screenPosition);
    }

    private bool IsPointerOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        var eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        _uiHits.Clear();
        EventSystem.current.RaycastAll(eventData, _uiHits);
        return _uiHits.Count > 0;
    }

    private bool IsPointerOverPhysics2D(Vector2 screenPosition)
    {
        if (_targetCamera == null)
        {
            return false;
        }

        Vector3 worldPosition = _targetCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0f));
        Vector2 point = new Vector2(worldPosition.x, worldPosition.y);
        return Physics2D.OverlapPoint(point, _interactable2DMask) != null;
    }
}
