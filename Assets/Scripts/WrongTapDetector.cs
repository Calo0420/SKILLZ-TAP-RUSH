using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Handles forgiving tap hit detection and registers wrong taps on empty screen space.
/// Fully supports multi-touch (two-thumb / multi-finger play) with zero GC allocations.
/// </summary>
public class WrongTapDetector : MonoBehaviour
{
    [SerializeField] private bool debugLogs = false;

    private static readonly Collider2D[] Hits2DBuffer = new Collider2D[24];
    private static readonly RaycastHit[] Hits3DBuffer = new RaycastHit[16];

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameActive)
        {
            return;
        }

        Camera cam = Camera.main;
        if (cam == null) return;

        // Process all distinct tap presses began this frame (multi-touch supported)
#if ENABLE_INPUT_SYSTEM
        if (Touchscreen.current != null)
        {
            var touches = Touchscreen.current.touches;
            int count = touches.Count;
            for (int i = 0; i < count; i++)
            {
                var t = touches[i];
                if (t.press.wasPressedThisFrame)
                {
                    ProcessTapAtScreenPosition(t.position.ReadValue(), cam);
                }
            }
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            ProcessTapAtScreenPosition(Mouse.current.position.ReadValue(), cam);
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == UnityEngine.TouchPhase.Began)
                {
                    ProcessTapAtScreenPosition(touch.position, cam);
                }
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            ProcessTapAtScreenPosition(Input.mousePosition, cam);
        }
#endif
    }

    private void ProcessTapAtScreenPosition(Vector2 screenPos, Camera cam)
    {
        // If a target was already tapped this frame (e.g. by OnMouseDown or a previous touch), ignore
        if (Target.WasTapConsumedThisFrame())
        {
            return;
        }

        // Try forgiving proximity hit on targets first
        if (TryTapTargetAtPointer(screenPos, cam))
        {
            return;
        }

        if (Target.WasTapConsumedThisFrame())
        {
            return;
        }

        float zDist = Mathf.Abs(cam.transform.position.z);
        Vector3 worldPoint = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, zDist));
        Vector2 point2D = new Vector2(worldPoint.x, worldPoint.y);

        if (debugLogs)
        {
            Debug.Log("[TapRush] Wrong tap detected at " + point2D);
        }

        GameManager.Instance.RegisterWrongTap();

        // Show floating penalty text
        FloatingTextManager.Instance?.ShowFloatingScore(worldPoint, -75, isBonus: false, isPenalty: true);
    }

    private static bool TryTapTargetAtPointer(Vector2 screenPos, Camera cam)
    {
        float zDist = Mathf.Abs(cam.transform.position.z);
        Vector3 worldPoint = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, zDist));
        Vector2 worldPoint2D = new Vector2(worldPoint.x, worldPoint.y);

        // Forgiving tap radius for mobile fingers (prevents near-miss frustrations)
        const float tapRadius = 0.4f;
        int hitCount2D = Physics2D.OverlapCircleNonAlloc(worldPoint2D, tapRadius, Hits2DBuffer);

        Target closestTarget = null;
        float closestDist = float.MaxValue;
        GaugeTarget closestGauge = null;
        float closestGaugeDist = float.MaxValue;

        for (int i = 0; i < hitCount2D; i++)
        {
            Collider2D col = Hits2DBuffer[i];
            if (col == null) continue;

            float dist = Vector2.Distance(worldPoint2D, (Vector2)col.transform.position);

            Target target = col.GetComponentInParent<Target>();
            if (target != null)
            {
                // If this target was already tapped this frame, this touch was part of that valid hit
                if (target.IsTapped)
                {
                    ClearHits2DBuffer(hitCount2D);
                    return true;
                }

                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestTarget = target;
                }
            }

            GaugeTarget gauge = col.GetComponentInParent<GaugeTarget>();
            if (gauge != null && dist < closestGaugeDist)
            {
                closestGaugeDist = dist;
                closestGauge = gauge;
            }
        }

        ClearHits2DBuffer(hitCount2D);

        // Gauge takes priority (it's a power-up, player is actively seeking it)
        if (closestGauge != null && closestGauge.RegisterTap())
        {
            return true;
        }

        if (closestTarget != null && closestTarget.TryTap())
        {
            return true;
        }

        // 3D raycast fallback if 3D colliders are present
        Ray ray = cam.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y, 0f));
        int hitCount3D = Physics.RaycastNonAlloc(ray, Hits3DBuffer);
        for (int i = 0; i < hitCount3D; i++)
        {
            RaycastHit hit = Hits3DBuffer[i];
            if (hit.collider == null) continue;

            Target target = hit.collider.GetComponentInParent<Target>();
            if (target != null && target.TryTap())
            {
                return true;
            }

            GaugeTarget gauge = hit.collider.GetComponentInParent<GaugeTarget>();
            if (gauge != null && gauge.RegisterTap())
            {
                return true;
            }
        }

        return false;
    }

    private static void ClearHits2DBuffer(int count)
    {
        for (int i = 0; i < count && i < Hits2DBuffer.Length; i++)
        {
            Hits2DBuffer[i] = null;
        }
    }
}
