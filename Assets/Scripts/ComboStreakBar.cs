using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Horizontal combo bar at bottom of screen. Fills with combo count,
/// changes color through green → yellow → magenta → white as combo climbs.
/// Resets on combo break with a brief flash.
/// </summary>
public class ComboStreakBar : MonoBehaviour
{
    public static ComboStreakBar Instance { get; private set; }

    private Image barFill;
    private Image barGlow;
    private RectTransform barRect;
    private float targetFill;
    private float currentFill;
    private int maxComboForFull = 30;

    private static readonly Color[] StreakColors = {
        new Color(0.2f, 1f, 0.4f, 0.8f),   // green
        new Color(1f, 0.9f, 0.2f, 0.9f),    // yellow
        new Color(1f, 0.4f, 0.8f, 0.95f),   // magenta
        new Color(1f, 1f, 1f, 1f)            // white (god mode)
    };

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        CreateBar();
    }

    void Update()
    {
        // Smooth fill
        currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * 8f);
        if (barFill != null)
        {
            barFill.fillAmount = currentFill;
        }

        // Glow pulse
        if (barGlow != null && currentFill > 0.01f)
        {
            Color c = barGlow.color;
            c.a = 0.3f + Mathf.Sin(Time.time * 6f) * 0.15f * currentFill;
            barGlow.color = c;
        }
    }

    public void UpdateCombo(int combo)
    {
        targetFill = Mathf.Clamp01((float)combo / maxComboForFull);

        // Color based on combo tier
        float colorT = Mathf.Clamp01((float)combo / maxComboForFull) * (StreakColors.Length - 1);
        int colorIndex = Mathf.FloorToInt(colorT);
        float colorLerp = colorT - colorIndex;
        int nextIndex = Mathf.Min(colorIndex + 1, StreakColors.Length - 1);
        Color barColor = Color.Lerp(StreakColors[colorIndex], StreakColors[nextIndex], colorLerp);

        if (barFill != null) barFill.color = barColor;
        if (barGlow != null)
        {
            Color glowC = barColor;
            glowC.a = 0.4f;
            barGlow.color = glowC;
        }
    }

    public void OnComboReset()
    {
        targetFill = 0f;
        // Instant flash then fade
        if (barFill != null)
        {
            barFill.color = new Color(1f, 0.2f, 0.2f, 1f);
        }
    }

    private void CreateBar()
    {
        // Create a canvas for the bar
        GameObject canvasObj = new GameObject("ComboBarCanvas");
        canvasObj.transform.SetParent(transform, false);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 8;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);

        // Background bar (dark)
        GameObject bgObj = new GameObject("BarBG");
        bgObj.transform.SetParent(canvasObj.transform, false);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.15f, 0.6f);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0f, 0f);
        bgRect.anchorMax = new Vector2(1f, 0f);
        bgRect.pivot = new Vector2(0.5f, 0f);
        bgRect.anchoredPosition = new Vector2(0f, 0f);
        bgRect.sizeDelta = new Vector2(0f, 8f);

        // Glow (slightly larger, behind fill)
        GameObject glowObj = new GameObject("BarGlow");
        glowObj.transform.SetParent(canvasObj.transform, false);
        barGlow = glowObj.AddComponent<Image>();
        barGlow.color = new Color(0.2f, 1f, 0.4f, 0f);
        RectTransform glowRect = glowObj.GetComponent<RectTransform>();
        glowRect.anchorMin = new Vector2(0f, 0f);
        glowRect.anchorMax = new Vector2(1f, 0f);
        glowRect.pivot = new Vector2(0.5f, 0f);
        glowRect.anchoredPosition = new Vector2(0f, -2f);
        glowRect.sizeDelta = new Vector2(0f, 14f);

        // Fill bar
        GameObject fillObj = new GameObject("BarFill");
        fillObj.transform.SetParent(canvasObj.transform, false);
        barFill = fillObj.AddComponent<Image>();
        barFill.color = StreakColors[0];
        barFill.type = Image.Type.Filled;
        barFill.fillMethod = Image.FillMethod.Horizontal;
        barFill.fillAmount = 0f;
        barRect = fillObj.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0f, 0f);
        barRect.anchorMax = new Vector2(1f, 0f);
        barRect.pivot = new Vector2(0.5f, 0f);
        barRect.anchoredPosition = new Vector2(0f, 0f);
        barRect.sizeDelta = new Vector2(0f, 8f);
    }
}
