using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Loading screen shown during Skillz SDK transitions.
/// Shows logo + animated spinner + "Loading..." text.
/// Call LoadingScreen.Show() / LoadingScreen.Hide() from anywhere.
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance { get; private set; }

    private GameObject root;
    private Image spinnerImage;
    private TextMeshProUGUI loadingText;
    private bool isVisible;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CreateUI();
        root.SetActive(false);
    }

    void Update()
    {
        // No spinner — just animated dots
    }

    public static void Show(string message = "Loading...")
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("LoadingScreen");
            go.AddComponent<LoadingScreen>();
        }
        Instance.ShowInternal(message);
    }

    public static void Hide()
    {
        if (Instance != null) Instance.HideInternal();
    }

    private void ShowInternal(string message)
    {
        if (root == null) CreateUI();
        if (loadingText != null) loadingText.text = message;
        root.SetActive(true);
        isVisible = true;
        StartCoroutine(AnimateDots());
    }

    private void HideInternal()
    {
        isVisible = false;
        if (root != null) root.SetActive(false);
        StopAllCoroutines();
    }

    private IEnumerator AnimateDots()
    {
        string baseText = loadingText != null ? loadingText.text.TrimEnd('.') : "Loading";
        int dots = 0;
        while (isVisible)
        {
            dots = (dots + 1) % 4;
            if (loadingText != null)
                loadingText.text = baseText + new string('.', dots);
            yield return new WaitForSecondsRealtime(0.4f);
        }
    }

    private void CreateUI()
    {
        // Canvas
        root = new GameObject("LoadingScreenCanvas");
        root.transform.SetParent(transform, false);
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // Always on top
        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        // Dark background
        GameObject bgObj = new GameObject("BG");
        bgObj.transform.SetParent(root.transform, false);
        Image bg = bgObj.AddComponent<Image>();
        bg.color = new Color(0.01f, 0.02f, 0.04f, 0.96f);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Logo
        GameObject logoObj = new GameObject("Logo");
        logoObj.transform.SetParent(root.transform, false);
        RectTransform logoRect = logoObj.AddComponent<RectTransform>();
        logoRect.anchorMin = new Vector2(0.5f, 0.65f);
        logoRect.anchorMax = new Vector2(0.5f, 0.65f);
        logoRect.pivot = new Vector2(0.5f, 0.5f);
        logoRect.sizeDelta = new Vector2(600f, 280f);
        Image logoImage = logoObj.AddComponent<Image>();
        Sprite logoSprite = Resources.Load<Sprite>("TapRushLogo");
        if (logoSprite != null)
        {
            logoImage.sprite = logoSprite;
            logoImage.preserveAspect = true;
        }

        // Loading text
        GameObject textObj = new GameObject("LoadingText");
        textObj.transform.SetParent(root.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.32f);
        textRect.anchorMax = new Vector2(0.5f, 0.32f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(600f, 80f);
        loadingText = textObj.AddComponent<TextMeshProUGUI>();
        loadingText.text = "Loading...";
        loadingText.fontSize = 36;
        loadingText.alignment = TextAlignmentOptions.Center;
        loadingText.color = new Color(0.7f, 0.8f, 0.85f, 0.9f);
        loadingText.fontStyle = FontStyles.Bold;
        loadingText.outlineWidth = 0.15f;
        loadingText.outlineColor = new Color(0.1f, 0.2f, 0.15f, 0.5f);
    }
}
