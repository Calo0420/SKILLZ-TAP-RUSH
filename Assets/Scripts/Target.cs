using UnityEngine;
using System;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Target : MonoBehaviour
{
    [SerializeField] private float lifetime = 2f;

    private static int lastConsumedTapFrame = -1;

    private bool tapped;
    private bool destroyEventSent;
    private float spawnTime;
    private Collider2D cachedCollider2D;
    private Collider cachedCollider3D;

    public event Action<Target> Destroyed;

    public static bool WasTapConsumedThisFrame()
    {
        return Time.frameCount == lastConsumedTapFrame;
    }

    void Start()
    {
        cachedCollider2D = GetComponent<Collider2D>();
        cachedCollider3D = GetComponent<Collider>();
        spawnTime = Time.time;
        Invoke(nameof(Miss), lifetime);
    }

    void Update()
    {
        if (!TryGetPrimaryPressThisFrame(out Vector2 screenPos))
        {
            return;
        }

        TryTapFromScreenPosition(screenPos);
    }

    void OnMouseDown()
    {
        TryTap();
    }

    public bool TryTap()
    {
        if (tapped || GameManager.Instance == null || !GameManager.Instance.IsGameActive)
        {
            return false;
        }

        tapped = true;
        lastConsumedTapFrame = Time.frameCount;
        CancelInvoke(nameof(Miss));

        float elapsed = Time.time - spawnTime;
        float speedScore01 = 1f - Mathf.Clamp01(elapsed / lifetime);
        GameManager.Instance?.RegisterHit(speedScore01);

        Destroy(gameObject);
        return true;
    }

    private void TryTapFromScreenPosition(Vector2 screenPos)
    {
        if (tapped || GameManager.Instance == null || !GameManager.Instance.IsGameActive)
        {
            return;
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        Vector3 worldPoint = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
        Vector2 worldPoint2D = new Vector2(worldPoint.x, worldPoint.y);

        if (cachedCollider2D != null && cachedCollider2D.OverlapPoint(worldPoint2D))
        {
            TryTap();
            return;
        }

        if (cachedCollider3D != null)
        {
            Ray ray = cam.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y, 0f));
            if (cachedCollider3D.Raycast(ray, out RaycastHit hit, Mathf.Infinity) && hit.collider == cachedCollider3D)
            {
                TryTap();
            }
        }
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
        }

        return false;
    }

    private void Miss()
    {
        if (tapped)
        {
            return;
        }

        GameManager.Instance?.RegisterMiss();
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (destroyEventSent)
        {
            return;
        }

        destroyEventSent = true;
        Destroyed?.Invoke(this);
    }
}
