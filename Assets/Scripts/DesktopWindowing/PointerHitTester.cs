using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DesktopWindowing
{
    public enum HitTestKind
    {
        None = 0,
        UI = 1,
        Physics2D = 2,
    }
    
    public sealed class PointerHitTester
    {
        private readonly Camera _targetCamera;
        private readonly int _interactable2DMask;
        private readonly List<RaycastResult> _uiHits = new List<RaycastResult>();

        public PointerHitTester(Camera targetCamera, LayerMask interactable2DMask)
        {
            _targetCamera = targetCamera;
            _interactable2DMask = interactable2DMask.value;
        }

        public HitTestKind GetHitKind(Vector2 screenPosition)
        {
            if (IsPointerOverUI(screenPosition))
            {
                return HitTestKind.UI;
            }

            if (IsPointerOverPhysics2D(screenPosition))
            {
                return HitTestKind.Physics2D;
            }

            return HitTestKind.None;
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
}
