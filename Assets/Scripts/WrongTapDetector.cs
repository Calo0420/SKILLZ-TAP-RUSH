using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class WrongTapDetector : MonoBehaviour
{
    [SerializeField] private bool debugLogs = true;

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameActive)
        {
            return;
        }

        if (!WasPrimaryPressThisFrame())
        {
            return;
        }

        if (Target.WasTapConsumedThisFrame())
        {
            return;
        }

        // NOTE: IsPointerOverGameObject check removed — no interactive buttons
        // exist during gameplay, and HUD text had raycastTarget disabled.
        // This prevents dead zones where taps were silently swallowed.

        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        Vector2 screenPos = GetPointerScreenPosition();
        if (TryTapTargetAtPointer(screenPos, cam))
        {
            return;
        }

        if (Target.WasTapConsumedThisFrame())
        {
            return;
        }

        Vector3 worldPoint = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
        Vector2 point2D = new Vector2(worldPoint.x, worldPoint.y);

        if (debugLogs)
        {
            Debug.Log("[TapRush] Wrong tap detected at " + point2D);
        }

        GameManager.Instance.RegisterWrongTap();
    }

private static bool TryTapTargetAtPointer(Vector2 screenPos, Camera cam)
    {
        Vector3 worldPoint = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
        Vector2 worldPoint2D = new Vector2(worldPoint.x, worldPoint.y);

        // Tap forgiveness radius — match collider precisely for skill-based accuracy.
        const float tapRadius = 0.01f;
        Collider2D[] hits2D = Physics2D.OverlapCircleAll(worldPoint2D, tapRadius);
        for (int i = 0; i < hits2D.Length; i++)
        {
            Target target = hits2D[i].GetComponentInParent<Target>();
            if (target != null && target.TryTap())
            {
                return true;
            }

            GaugeTarget gauge = hits2D[i].GetComponentInParent<GaugeTarget>();
            if (gauge != null && gauge.RegisterTap())
            {
                return true;
            }
        }

        Ray ray = cam.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y, 0f));
        RaycastHit[] hits3D = Physics.RaycastAll(ray);
        for (int i = 0; i < hits3D.Length; i++)
        {
            Target target = hits3D[i].collider.GetComponentInParent<Target>();
            if (target != null && target.TryTap())
            {
                return true;
            }

            GaugeTarget gauge = hits3D[i].collider.GetComponentInParent<GaugeTarget>();
            if (gauge != null && gauge.RegisterTap())
            {
                return true;
            }
        }

        return false;
    }

    private static bool WasPrimaryPressThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            return true;
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetMouseButtonDown(0);
#else
        return false;
#endif
    }

    private static Vector2 GetPointerScreenPosition()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }

        if (Touchscreen.current != null)
        {
            return Touchscreen.current.primaryTouch.position.ReadValue();
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.mousePosition;
#else
        return Vector2.zero;
#endif
    }
}
