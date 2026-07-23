using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class WrongTapDetector : MonoBehaviour
{
    [SerializeField] private bool debugLogs = false;

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

        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        Vector2 screenPos = GetPointerScreenPosition();
        if (IsPointerOverUi(screenPos))
        {
            return;
        }

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

    private static bool IsPointerOverUi(Vector2 screenPos)
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = screenPos
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        return results.Count > 0;
    }

    private static bool TryTapTargetAtPointer(Vector2 screenPos, Camera cam)
    {
        Vector3 worldPoint = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
        Vector2 worldPoint2D = new Vector2(worldPoint.x, worldPoint.y);

        Collider2D[] hits2D = Physics2D.OverlapPointAll(worldPoint2D);
        for (int i = 0; i < hits2D.Length; i++)
        {
            Target target = hits2D[i].GetComponentInParent<Target>();
            if (target != null && target.TryTap())
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
