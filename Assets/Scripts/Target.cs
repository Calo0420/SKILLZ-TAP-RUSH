using UnityEngine;
using System;

public class Target : MonoBehaviour
{
    [SerializeField] private float lifetime = 2f;

    private static int lastConsumedTapFrame = -1;

    private bool tapped;
    private bool destroyEventSent;
    private float spawnTime;

    public event Action<Target> Destroyed;

    public static bool WasTapConsumedThisFrame()
    {
        return Time.frameCount == lastConsumedTapFrame;
    }

    void Start()
    {
        spawnTime = Time.time;
        Invoke(nameof(Miss), lifetime);
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
