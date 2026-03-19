using UnityEngine;

public interface IHitTestService
{
    HitTestResult HitTest(Vector2 screenPosition);
}
