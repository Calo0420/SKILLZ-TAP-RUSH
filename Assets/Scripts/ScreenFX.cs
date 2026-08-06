using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

/// <summary>
/// Full-screen visual effects driven by gameplay state:
/// - Hit flash (brief white pulse)
/// - Combo glow (bloom intensity scales with combo)
/// - Chaos vignette (darkens edges during chaos mode)
/// - Wrong-tap chromatic aberration pulse
/// </summary>
public class ScreenFX : MonoBehaviour
{
    public static ScreenFX Instance { get; private set; }

    [Header("Volume Reference")]
    [SerializeField] private Volume postProcessVolume;

    [Header("Hit Flash")]
    [SerializeField] private float hitFlashIntensity = 0.15f;
    [SerializeField] private float hitFlashDecay = 8f;

    [Header("Combo Bloom")]
    [SerializeField] private float baseBloomIntensity = 1.2f;
    [SerializeField] private float maxBloomIntensity = 4.5f;
    [SerializeField] private int comboForMaxBloom = 25;
    [SerializeField] private float bloomSmoothing = 3f;

    [Header("Chaos Vignette")]
    [SerializeField] private float chaosVignetteIntensity = 0.45f;
    [SerializeField] private float chaosVignetteSmoothing = 2f;
    [SerializeField] private Color chaosVignetteColor = new Color(0.8f, 0f, 0f, 1f);

    [Header("Wrong Tap Aberration")]
    [SerializeField] private float aberrationIntensity = 0.8f;
    [SerializeField] private float aberrationDecay = 6f;

    private Bloom bloom;
    private Vignette vignette;
    private ChromaticAberration chromaticAberration;

    private float targetBloom;
    private float currentFlash;
    private float currentAberration;
    private float targetVignette;
    private bool chaosActive;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (postProcessVolume == null)
        {
            // Try to find the NeonBloomVolume in scene
            Volume[] volumes = FindObjectsByType<Volume>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (volumes.Length > 0) postProcessVolume = volumes[0];
        }

        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            postProcessVolume.profile.TryGet(out bloom);
            postProcessVolume.profile.TryGet(out vignette);

            if (!postProcessVolume.profile.TryGet(out chromaticAberration))
            {
                chromaticAberration = postProcessVolume.profile.Add<ChromaticAberration>(true);
            }

            if (vignette == null)
            {
                vignette = postProcessVolume.profile.Add<Vignette>(true);
            }
        }

        targetBloom = baseBloomIntensity;
    }

    void Update()
    {
        // Bloom
        if (bloom != null)
        {
            float current = bloom.intensity.value;
            float target = targetBloom + currentFlash * 3f;
            bloom.intensity.value = Mathf.Lerp(current, target, Time.deltaTime * bloomSmoothing);
        }

        // Flash decay
        if (currentFlash > 0f)
        {
            currentFlash = Mathf.MoveTowards(currentFlash, 0f, Time.deltaTime * hitFlashDecay);
        }

        // Chromatic aberration decay
        if (chromaticAberration != null)
        {
            if (currentAberration > 0f)
            {
                currentAberration = Mathf.MoveTowards(currentAberration, 0f, Time.deltaTime * aberrationDecay);
            }
            chromaticAberration.intensity.value = currentAberration;
        }

        // Vignette
        if (vignette != null)
        {
            float vigTarget = chaosActive ? chaosVignetteIntensity : 0f;
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, vigTarget, Time.deltaTime * chaosVignetteSmoothing);
            if (chaosActive)
            {
                vignette.color.value = chaosVignetteColor;
            }
        }
    }

    public void OnHit()
    {
        currentFlash = hitFlashIntensity;
        UpdateComboBloom();
    }

    public void OnWrongTap()
    {
        currentAberration = aberrationIntensity;
        // Brief vignette pulse even outside chaos
        if (vignette != null)
        {
            vignette.intensity.value = 0.35f;
        }
    }

    public void OnComboReset()
    {
        targetBloom = baseBloomIntensity;
    }

    public void SetChaosMode(bool active)
    {
        chaosActive = active;
    }

    private void UpdateComboBloom()
    {
        if (GameManager.Instance == null) return;
        float comboNorm = Mathf.Clamp01((float)GameManager.Instance.ComboCount / comboForMaxBloom);
        targetBloom = Mathf.Lerp(baseBloomIntensity, maxBloomIntensity, comboNorm);
    }
}
