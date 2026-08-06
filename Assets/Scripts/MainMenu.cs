using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button competeButton;

    void Start()
    {
        EnsureSkillzMatchController();
        SetupVisuals();

        // If buttons aren't assigned, build them at runtime
        if (playButton == null || competeButton == null)
        {
            BuildMenuUI();
        }
        else
        {
            playButton.onClick.AddListener(OnPracticePressed);
            competeButton.onClick.AddListener(OnCompetePressed);
        }
    }

    public void PlayGame()
    {
        OnPracticePressed();
    }

    private void OnPracticePressed()
    {
        if (SkillzMatchController.Instance != null)
            SkillzMatchController.Instance.LaunchPractice();
        else
            SceneManager.LoadScene("GameScene");
    }

    private void OnCompetePressed()
    {
        if (SkillzMatchController.Instance != null)
            SkillzMatchController.Instance.LaunchSkillzTournament();
        else
            Debug.LogWarning("[TapRush] SkillzMatchController not found. Can't launch tournament.");
    }

    private void SetupVisuals()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.06f, 1f);
        }
    }

    private void EnsureSkillzMatchController()
    {
        if (SkillzMatchController.Instance != null) return;
        GameObject go = new GameObject("SkillzMatchController");
        go.AddComponent<SkillzMatchController>();
    }

    private void BuildMenuUI()
    {
        // Create a canvas
        GameObject canvasObj = new GameObject("MenuCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Title
        TextMeshProUGUI title = CreateLabel(canvasObj.transform, "Title", 72, new Vector2(0.5f, 0.75f));
        title.text = "TAP RUSH";
        title.fontStyle = FontStyles.Bold;
        title.color = new Color(0.3f, 1f, 0.5f, 1f);
        title.outlineWidth = 0.2f;
        title.outlineColor = new Color(0f, 0.5f, 0.2f, 0.8f);
        StartCoroutine(PulseTitle(title));

        // Subtitle
        TextMeshProUGUI subtitle = CreateLabel(canvasObj.transform, "Subtitle", 28, new Vector2(0.5f, 0.68f));
        subtitle.text = "by Reyeso Studio";
        subtitle.color = new Color(0.6f, 0.6f, 0.7f, 0.8f);

        // Practice button
        playButton = CreateMenuButton(canvasObj.transform, "PracticeBtn", "PRACTICE", new Vector2(0.5f, 0.45f),
            new Color(0.1f, 0.3f, 0.15f, 1f), new Color(0.3f, 1f, 0.5f, 1f));
        playButton.onClick.AddListener(OnPracticePressed);
        StartCoroutine(ButtonHoverPulse(playButton));

        // Compete button
        competeButton = CreateMenuButton(canvasObj.transform, "CompeteBtn", "COMPETE", new Vector2(0.5f, 0.35f),
            new Color(0.3f, 0.15f, 0.05f, 1f), new Color(1f, 0.85f, 0.2f, 1f));
        competeButton.onClick.AddListener(OnCompetePressed);
        StartCoroutine(ButtonHoverPulse(competeButton));

        // Version text
        TextMeshProUGUI version = CreateLabel(canvasObj.transform, "Version", 18, new Vector2(0.5f, 0.05f));
        version.text = "v1.0 — Powered by Skillz";
        version.color = new Color(0.4f, 0.4f, 0.5f, 0.5f);
    }

    private Button CreateMenuButton(Transform parent, string name, string label, Vector2 anchor, Color bgColor, Color textColor)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(600f, 120f);

        Image bg = btnObj.AddComponent<Image>();
        bg.color = bgColor;

        // Rounded look via slight outline
        Outline outline = btnObj.AddComponent<Outline>();
        outline.effectColor = textColor * 0.5f;
        outline.effectDistance = new Vector2(2f, 2f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = bgColor * 1.3f;
        colors.pressedColor = bgColor * 0.7f;
        btn.colors = colors;

        TextMeshProUGUI txt = CreateLabel(btnObj.transform, name + "Label", 42, new Vector2(0.5f, 0.5f));
        txt.text = label;
        txt.color = textColor;
        txt.fontStyle = FontStyles.Bold;
        txt.raycastTarget = false;

        return btn;
    }

    private TextMeshProUGUI CreateLabel(Transform parent, string objectName, int fontSize, Vector2 anchor)
    {
        GameObject textObj = new GameObject(objectName);
        textObj.transform.SetParent(parent, false);
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(900f, 130f);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        return tmp;
    }

    private IEnumerator PulseTitle(TextMeshProUGUI title)
    {
        while (title != null)
        {
            float pulse = 1f + Mathf.Sin(Time.time * 2f) * 0.04f;
            title.transform.localScale = Vector3.one * pulse;

            // Subtle color shift
            float hueShift = Mathf.Sin(Time.time * 0.5f) * 0.1f;
            title.color = Color.HSVToRGB(0.38f + hueShift, 0.7f, 1f);
            yield return null;
        }
    }

    private IEnumerator ButtonHoverPulse(Button btn)
    {
        Transform t = btn.transform;
        Vector3 baseScale = Vector3.one;
        while (t != null)
        {
            float pulse = 1f + Mathf.Sin(Time.time * 3f + t.GetSiblingIndex() * 1.5f) * 0.015f;
            t.localScale = baseScale * pulse;
            yield return null;
        }
    }
}

