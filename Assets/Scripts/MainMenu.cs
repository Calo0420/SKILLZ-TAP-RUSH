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

#if UNITY_WEBGL
        // Skillz WebSDK: the player already picked a tournament on games.skillz.com
        // before this build ever loaded, and the SDK expects the game to be ready to
        // receive OnMatchWillBegin immediately on launch. Tap Rush has no separate
        // FTUE/tutorial screen to worry about, but our own Practice/Compete menu is
        // exactly the kind of pre-launch UX Skillz's web docs say must be skipped, so
        // we bypass it here and launch straight into Skillz instead of waiting for a
        // Compete button press.
        SkillzMatchController.Instance.LaunchSkillzTournament();
        return;
#endif

        SetupVisuals();
        HideSceneCanvas();

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

        // Add ambient particles like in-game
        if (FindAnyObjectByType<AmbientParticles>() == null)
        {
            GameObject particlesGO = new GameObject("MenuParticles");
            particlesGO.AddComponent<AmbientParticles>();
        }
    }

    private void EnsureSkillzMatchController()
    {
        if (SkillzMatchController.Instance != null) return;
        GameObject go = new GameObject("SkillzMatchController");
        go.AddComponent<SkillzMatchController>();
    }

    private void HideSceneCanvas()
    {
        // Hide the old scene Canvas that has the stray "Play" button
        GameObject sceneCanvas = GameObject.Find("Canvas");
        if (sceneCanvas != null) sceneCanvas.SetActive(false);
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
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // No background image — use game's dark bg + ambient particles (already added in SetupVisuals)

        // Logo image (no background box)
        GameObject logoObj = new GameObject("Logo");
        logoObj.transform.SetParent(canvasObj.transform, false);
        RectTransform logoRect = logoObj.AddComponent<RectTransform>();
        logoRect.anchorMin = new Vector2(0.5f, 0.72f);
        logoRect.anchorMax = new Vector2(0.5f, 0.72f);
        logoRect.pivot = new Vector2(0.5f, 0.5f);
        logoRect.sizeDelta = new Vector2(850f, 400f);
        Image logoImage = logoObj.AddComponent<Image>();
        Sprite logoSprite = Resources.Load<Sprite>("TapRushLogo");
        if (logoSprite != null)
        {
            logoImage.sprite = logoSprite;
            logoImage.preserveAspect = true;
        }

        // Subtitle — brighter, more visible
        TextMeshProUGUI subtitle = CreateLabel(canvasObj.transform, "Subtitle", 34, new Vector2(0.5f, 0.55f));
        subtitle.text = "by Reyeso Studio";
        subtitle.color = new Color(0.85f, 0.9f, 0.95f, 0.95f);
        subtitle.fontStyle = FontStyles.Italic;
        subtitle.outlineWidth = 0.15f;
        subtitle.outlineColor = new Color(0.2f, 0.5f, 0.4f, 0.5f);

        // Practice button — using image asset
        playButton = CreateImageButton(canvasObj.transform, "PracticeBtn", "PracticeButton", new Vector2(0.5f, 0.36f), new Vector2(680f, 110f));
        playButton.onClick.AddListener(OnPracticePressed);
        StartCoroutine(ButtonHoverPulse(playButton));

        // Compete button — using image asset
        competeButton = CreateImageButton(canvasObj.transform, "CompeteBtn", "CompeteButton", new Vector2(0.5f, 0.24f), new Vector2(680f, 110f));
        competeButton.onClick.AddListener(OnCompetePressed);
        StartCoroutine(ButtonHoverPulse(competeButton));

        // Version text — brighter, more visible
        TextMeshProUGUI version = CreateLabel(canvasObj.transform, "Version", 24, new Vector2(0.5f, 0.06f));
        version.text = "v1.0 — Powered by Skillz";
        version.color = new Color(0.7f, 0.75f, 0.8f, 0.8f);
        version.outlineWidth = 0.1f;
        version.outlineColor = new Color(0.1f, 0.3f, 0.2f, 0.4f);
    }

    private Button CreateImageButton(Transform parent, string name, string spriteName, Vector2 anchor, Vector2 size)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;

        Image bg = btnObj.AddComponent<Image>();
        Sprite btnSprite = Resources.Load<Sprite>(spriteName);
        if (btnSprite != null)
        {
            bg.sprite = btnSprite;
            bg.preserveAspect = true;
            bg.type = Image.Type.Simple;
        }

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        colors.selectedColor = Color.white;
        btn.colors = colors;

        return btn;
    }

    private Button CreateMenuButton(Transform parent, string name, string label, Vector2 anchor, Color bgColor, Color textColor)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(680f, 130f);

        Image bg = btnObj.AddComponent<Image>();
        bg.color = bgColor;

        // Neon glow border
        Outline outline = btnObj.AddComponent<Outline>();
        outline.effectColor = new Color(textColor.r, textColor.g, textColor.b, 0.7f);
        outline.effectDistance = new Vector2(3f, 3f);

        // Second outline for extra glow
        Outline outline2 = btnObj.AddComponent<Outline>();
        outline2.effectColor = new Color(textColor.r, textColor.g, textColor.b, 0.3f);
        outline2.effectDistance = new Vector2(6f, 6f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        colors.selectedColor = Color.white;
        btn.colors = colors;

        TextMeshProUGUI txt = CreateLabel(btnObj.transform, name + "Label", 48, new Vector2(0.5f, 0.5f));
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

