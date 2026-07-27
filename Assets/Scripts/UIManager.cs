using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private float comboMilestoneDisplaySeconds = 0.65f;
    [SerializeField] private float comboShakeDuration = 0.16f;
    [SerializeField] private float comboShakeMagnitude = 0.08f;

    private GameObject runtimeEndScreenRoot;
    private TextMeshProUGUI runtimeFinalScoreText;
    private Coroutine comboMilestoneRoutine;
    private TextMeshProUGUI gaugeProgressText;
    private TextMeshProUGUI powerUpText;
    private Coroutine powerUpDisplayRoutine;
    private TextMeshProUGUI chaosAnnouncementText;
    private UnityEngine.UI.Image chaosFlashImage;
    private Coroutine chaosAnnouncementRoutine;


    private Coroutine cameraShakeRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    public void UpdateTimer(float time)
    {
        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(time).ToString();
        }
    }

    public void UpdateCombo(int combo)
    {
        if (comboText == null)
        {
            return;
        }

        if (combo <= 1)
        {
            comboText.text = string.Empty;
        }
        else
        {
            comboText.text = "Combo x" + combo;
        }
    }

    public void ShowComboMilestone(int comboCount)
    {
        if (comboText == null)
        {
            return;
        }

        if (comboMilestoneRoutine != null)
        {
            StopCoroutine(comboMilestoneRoutine);
        }

        comboMilestoneRoutine = StartCoroutine(ShowComboMilestoneRoutine(comboCount));
    }

    public bool ShowEndScreen(int finalScore)
    {
        EnsureRuntimeEndScreen();
        if (runtimeEndScreenRoot == null || runtimeFinalScoreText == null)
        {
            return false;
        }

        runtimeFinalScoreText.text = "Final Score: " + finalScore;
        runtimeEndScreenRoot.SetActive(true);
        return true;
    }

public void ShowGaugeProgress(int current, int required)
    {
        EnsureGaugeProgressText();
        if (gaugeProgressText != null)
        {
            gaugeProgressText.text = $"CHARGE! {current}/{required}";
            gaugeProgressText.color = Color.Lerp(new Color(0.2f, 0.8f, 1f), Color.white, (float)current / required);
        }
    }

    public void HideGaugeProgress()
    {
        if (gaugeProgressText != null)
            gaugeProgressText.text = string.Empty;
    }

    public void ShowPowerUpActive(float duration)
    {
        HideGaugeProgress();
        EnsurePowerUpText();
        if (powerUpDisplayRoutine != null) StopCoroutine(powerUpDisplayRoutine);
        powerUpDisplayRoutine = StartCoroutine(PowerUpDisplayRoutine(duration));
    }

    public void HidePowerUpDisplay()
    {
        if (powerUpDisplayRoutine != null)
        {
            StopCoroutine(powerUpDisplayRoutine);
            powerUpDisplayRoutine = null;
        }
        if (powerUpText != null) powerUpText.text = string.Empty;
    }

public void ShowChaosAnnouncement()
    {
        if (chaosAnnouncementRoutine != null)
        {
            StopCoroutine(chaosAnnouncementRoutine);
        }

        EnsureChaosAnnouncementVisuals();
        chaosAnnouncementRoutine = StartCoroutine(ShowChaosAnnouncementRoutine());
    }

    private IEnumerator ShowChaosAnnouncementRoutine()
    {
        if (chaosFlashImage != null)
        {
            chaosFlashImage.gameObject.SetActive(true);
            Color flashColor = new Color(1f, 1f, 1f, 0.8f);
            chaosFlashImage.color = flashColor;
        }

        if (chaosAnnouncementText != null)
        {
            chaosAnnouncementText.text = "OH GOD, HERE WE GO!";
            chaosAnnouncementText.gameObject.SetActive(true);
            chaosAnnouncementText.color = new Color(1f, 0.2f, 0.2f, 1f);
            chaosAnnouncementText.outlineColor = Color.black;
            chaosAnnouncementText.outlineWidth = 0.3f;
            chaosAnnouncementText.fontSize = 92f;
        }

        float flashTime = 0.2f;
        float elapsed = 0f;
        while (elapsed < flashTime)
        {
            elapsed += Time.unscaledDeltaTime;
            if (chaosFlashImage != null)
            {
                float a = Mathf.Lerp(0.8f, 0f, elapsed / flashTime);
                Color c = chaosFlashImage.color;
                c.a = a;
                chaosFlashImage.color = c;
            }
            yield return null;
        }

        if (chaosFlashImage != null)
        {
            chaosFlashImage.gameObject.SetActive(false);
        }

        float textTime = 1.25f;
        elapsed = 0f;
        while (elapsed < textTime)
        {
            elapsed += Time.unscaledDeltaTime;
            if (chaosAnnouncementText != null)
            {
                float pulse = 1f + Mathf.Sin(elapsed * 18f) * 0.08f;
                chaosAnnouncementText.transform.localScale = new Vector3(pulse, pulse, 1f);
            }
            yield return null;
        }

        if (chaosAnnouncementText != null)
        {
            chaosAnnouncementText.text = string.Empty;
            chaosAnnouncementText.transform.localScale = Vector3.one;
        }

        chaosAnnouncementRoutine = null;
    }

    private void EnsureChaosAnnouncementVisuals()
    {
        Canvas canvas = FindOrCreateHUDCanvas();

        if (chaosFlashImage == null)
        {
            GameObject flashObject = new GameObject("ChaosFlash");
            flashObject.transform.SetParent(canvas.transform, false);
            chaosFlashImage = flashObject.AddComponent<UnityEngine.UI.Image>();
            RectTransform flashRect = flashObject.GetComponent<RectTransform>();
            flashRect.anchorMin = Vector2.zero;
            flashRect.anchorMax = Vector2.one;
            flashRect.offsetMin = Vector2.zero;
            flashRect.offsetMax = Vector2.zero;
            chaosFlashImage.color = new Color(1f, 1f, 1f, 0f);
            chaosFlashImage.gameObject.SetActive(false);
        }

        if (chaosAnnouncementText == null)
        {
            chaosAnnouncementText = CreateLabel(canvas.transform, "ChaosAnnouncementText", 92, new Vector2(0.5f, 0.55f));
            chaosAnnouncementText.text = string.Empty;
            chaosAnnouncementText.fontStyle = FontStyles.Bold;
            chaosAnnouncementText.alignment = TextAlignmentOptions.Center;
        }
    }


    private System.Collections.IEnumerator PowerUpDisplayRoutine(float duration)
    {
        float remaining = duration;
        while (remaining > 0f)
        {
            remaining -= Time.deltaTime;
            if (powerUpText != null)
            {
                powerUpText.text = $"2x POWER! {remaining:F1}s";
                powerUpText.color = Color.Lerp(Color.red, Color.yellow, remaining / duration);
            }
            yield return null;
        }
        if (powerUpText != null) powerUpText.text = string.Empty;
        powerUpDisplayRoutine = null;
    }

    private void EnsureGaugeProgressText()
    {
        if (gaugeProgressText != null) return;
        gaugeProgressText = CreateLabel(transform, "GaugeProgressText", 42, new Vector2(0.5f, 0.15f));
        gaugeProgressText.text = string.Empty;
        gaugeProgressText.transform.SetParent(FindOrCreateHUDCanvas().transform, false);
    }

    private void EnsurePowerUpText()
    {
        if (powerUpText != null) return;
        powerUpText = CreateLabel(transform, "PowerUpText", 46, new Vector2(0.5f, 0.22f));
        powerUpText.text = string.Empty;
        powerUpText.transform.SetParent(FindOrCreateHUDCanvas().transform, false);
    }

    private Canvas hudCanvas;
    private Canvas FindOrCreateHUDCanvas()
    {
        if (hudCanvas != null) return hudCanvas;
        GameObject go = new GameObject("HUDCanvas");
        hudCanvas = go.AddComponent<Canvas>();
        hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        hudCanvas.sortingOrder = 10;
        go.AddComponent<UnityEngine.UI.CanvasScaler>();
        go.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        return hudCanvas;
    }


    private void EnsureRuntimeEndScreen()
    {
        if (runtimeEndScreenRoot != null)
        {
            return;
        }

        GameObject canvasRoot = new GameObject("RuntimeEndScreenCanvas");
        Canvas canvas = canvasRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasRoot.AddComponent<CanvasScaler>();
        canvasRoot.AddComponent<GraphicRaycaster>();

        runtimeEndScreenRoot = new GameObject("EndScreenPanel");
        runtimeEndScreenRoot.transform.SetParent(canvasRoot.transform, false);
        Image panelImage = runtimeEndScreenRoot.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.82f);

        RectTransform panelRect = runtimeEndScreenRoot.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        runtimeFinalScoreText = CreateLabel(runtimeEndScreenRoot.transform, "FinalScoreText", 54, new Vector2(0.5f, 0.66f));
        runtimeFinalScoreText.text = "Final Score: 0";

        Button playAgainButton = CreateButton(runtimeEndScreenRoot.transform, "PlayAgainButton", "Play Again", new Vector2(0.5f, 0.46f));
        playAgainButton.onClick.AddListener(() => SceneManager.LoadScene("GameScene"));

        Button menuButton = CreateButton(runtimeEndScreenRoot.transform, "MainMenuButton", "Main Menu", new Vector2(0.5f, 0.32f));
        menuButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));

        runtimeEndScreenRoot.SetActive(false);
    }

    private TextMeshProUGUI CreateLabel(Transform parent, string objectName, int fontSize, Vector2 anchor)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = anchor;
        textRect.anchorMax = anchor;
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(900f, 130f);

        TextMeshProUGUI tmp = textObject.AddComponent<TextMeshProUGUI>();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        return tmp;
    }

    private IEnumerator ShowComboMilestoneRoutine(int comboCount)
    {
        comboText.color = new Color(1f, 0.92f, 0.35f, 1f);
        comboText.text = "COMBO x" + comboCount + "!";

        if (cameraShakeRoutine != null)
        {
            StopCoroutine(cameraShakeRoutine);
        }

        cameraShakeRoutine = StartCoroutine(PlayComboCameraShake());
        yield return new WaitForSeconds(comboMilestoneDisplaySeconds);

        comboText.color = Color.white;
        UpdateCombo(GameManager.Instance != null ? GameManager.Instance.ComboCount : 0);
        comboMilestoneRoutine = null;
    }

    private IEnumerator PlayComboCameraShake()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            cameraShakeRoutine = null;
            yield break;
        }

        Transform camTransform = cam.transform;
        Vector3 originalPosition = camTransform.position;
        float elapsed = 0f;

        while (elapsed < comboShakeDuration)
        {
            elapsed += Time.deltaTime;
            float fade = 1f - Mathf.Clamp01(elapsed / comboShakeDuration);
            float x = Random.Range(-1f, 1f) * comboShakeMagnitude * fade;
            float y = Random.Range(-1f, 1f) * comboShakeMagnitude * fade;
            camTransform.position = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);
            yield return null;
        }

        camTransform.position = originalPosition;
        cameraShakeRoutine = null;
    }

    private Button CreateButton(Transform parent, string objectName, string label, Vector2 anchor)
    {
        GameObject buttonObject = new GameObject(objectName);
        buttonObject.transform.SetParent(parent, false);

        RectTransform buttonRect = buttonObject.AddComponent<RectTransform>();
        buttonRect.anchorMin = anchor;
        buttonRect.anchorMax = anchor;
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(340f, 95f);

        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(0.12f, 0.25f, 0.45f, 1f);

        Button button = buttonObject.AddComponent<Button>();

        TextMeshProUGUI labelText = CreateLabel(buttonObject.transform, "Label", 36, new Vector2(0.5f, 0.5f));
        labelText.text = label;
        labelText.raycastTarget = false;

        return button;
    }
}
