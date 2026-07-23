using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;
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

        if (!TryGetPrimaryPressThisFrame(out Vector2 screenPos))
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

        if (TryTapTargetAtPointer(screenPos, cam))
        {
            return;
        }

        if (Target.WasTapConsumedThisFrame())
        {
            return;
        }

        if (IsPointerOverBlockingUi(screenPos))
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

    private static bool IsPointerOverBlockingUi(Vector2 screenPos)
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

        for (int i = 0; i < results.Count; i++)
        {
            if (results[i].gameObject.GetComponentInParent<Selectable>() != null)
            {
                return true;
            }
        }

        return false;
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

    private static bool TryGetPrimaryPressThisFrame(out Vector2 screenPos)
    {
        screenPos = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            screenPos = Mouse.current.position.ReadValue();
            return true;
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
            return true;
        }
#endif

        try
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetMouseButtonDown(0))
            {
                screenPos = Input.mousePosition;
                return true;
            }
#endif
        }
        catch
        {
            // Ignore legacy input exceptions when project is configured for Input System only.
        }

        return false;
    }
}
