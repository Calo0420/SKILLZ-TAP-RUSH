using UnityEngine;

/// <summary>
/// Smoothly shifts the camera background color based on gameplay state.
/// Base: deep dark blue. Combo builds → subtle neon tint. Chaos → dark red pulse.
/// </summary>
public class ReactiveBackground : MonoBehaviour
{
    private static readonly Color BaseColor = new Color(0.02f, 0.02f, 0.06f, 1f);
    private static readonly Color ComboTint = new Color(0.03f, 0.06f, 0.12f, 1f);
    private static readonly Color ChaosColor = new Color(0.08f, 0.01f, 0.02f, 1f);

    private Camera cam;
    private bool chaosMode;
    private float chaosFlicker;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (cam == null) return;

        Color target;
        if (chaosMode)
        {
            // Pulsing dark red
            chaosFlicker = Mathf.Sin(Time.time * 4f) * 0.3f + 0.7f;
            target = Color.Lerp(BaseColor, ChaosColor, chaosFlicker);
        }
        else
        {
            float comboNorm = 0f;
            if (GameManager.Instance != null)
                comboNorm = Mathf.Clamp01(GameManager.Instance.ComboCount / 20f);
            target = Color.Lerp(BaseColor, ComboTint, comboNorm);
        }

        cam.backgroundColor = Color.Lerp(cam.backgroundColor, target, Time.deltaTime * 3f);
    }

    public void SetChaosMode(bool active)
    {
        chaosMode = active;
    }

    // Auto-hook into chaos via TimerManager event
    void OnEnable()
    {
        // ScreenFX handles chaos state — we piggyback via Update check
    }
}
