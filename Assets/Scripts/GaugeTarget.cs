using UnityEngine;
using System.Collections;

public class GaugeTarget : MonoBehaviour
{
    [SerializeField] private int tapsRequired = 8;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float powerUpDuration = 8f;

    private int tapCount;
    private SpriteRenderer sr;
    private Vector3 baseScale;
    private bool consumed;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = new Color(0.2f, 0.8f, 1f, 1f);

        // Add neon glow
        NeonTargetFX nfx = gameObject.AddComponent<NeonTargetFX>();
        nfx.SetType(NeonTargetFX.FXType.Gauge);

        baseScale = transform.localScale * 1.5f;
        transform.localScale = baseScale;

        StartCoroutine(PulseLoop());
        Invoke(nameof(Expire), lifetime);
    }

    void OnMouseDown()
    {
        RegisterTap();
    }

public bool RegisterTap()
    {
        if (consumed || GameManager.Instance == null || !GameManager.Instance.IsGameActive)
            return false;

        // Mark this frame so WrongTapDetector ignores it
        Target.MarkTapConsumedThisFrame();

        tapCount++;
        float progress = (float)tapCount / tapsRequired;

        // Visual feedback: fill color toward white as it charges
        if (sr != null)
            sr.color = Color.Lerp(new Color(0.2f, 0.8f, 1f, 1f), Color.white, progress);

        // Scale pulse on each tap
        StopCoroutine(nameof(TapPulse));
        StartCoroutine(TapPulse());

        AudioManager.Instance?.PlayGaugeTap();
        UIManager.Instance?.ShowGaugeProgress(tapCount, tapsRequired);

        if (tapCount >= tapsRequired)
        {
            consumed = true;
            CancelInvoke(nameof(Expire));
            AudioManager.Instance?.PlayGaugeComplete();
            GameManager.Instance?.ActivatePowerUp(powerUpDuration);
            UIManager.Instance?.ShowPowerUpActive(powerUpDuration);
            Destroy(gameObject);
            return true;
        }

        return true;
    }

    private IEnumerator TapPulse()
    {
        float t = 0f;
        while (t < 0.1f)
        {
            t += Time.deltaTime;
            float scale = 1f + Mathf.Sin(t / 0.1f * Mathf.PI) * 0.25f;
            transform.localScale = baseScale * scale;
            yield return null;
        }
        transform.localScale = baseScale;
    }

    private IEnumerator PulseLoop()
    {
        while (true)
        {
            float t = 0f;
            while (t < 0.6f)
            {
                t += Time.deltaTime;
                float s = 1f + Mathf.Sin(t / 0.6f * Mathf.PI) * 0.08f;
                transform.localScale = baseScale * s;
                yield return null;
            }
        }
    }

    private void Expire()
    {
        if (!consumed)
        {
            UIManager.Instance?.HideGaugeProgress();
            Destroy(gameObject);
        }
    }
}
