using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages floating combat/tap score text with object pooling and skill rating feedback.
/// </summary>
public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance { get; private set; }

    [SerializeField] private float floatDuration = 0.8f;
    [SerializeField] private float floatDistance = 2f;
    [SerializeField] private float fontSize = 32f;

    private Canvas worldCanvas;
    private readonly Queue<GameObject> textPool = new Queue<GameObject>();
    private const int PreWarmCount = 16;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureWorldCanvas();
        PreWarmPool();
    }

    private void PreWarmPool()
    {
        if (worldCanvas == null) return;
        for (int i = 0; i < PreWarmCount; i++)
        {
            GameObject obj = CreateNewFloatingObject();
            obj.SetActive(false);
            textPool.Enqueue(obj);
        }
    }

    private GameObject CreateNewFloatingObject()
    {
        GameObject floatingObj = new GameObject("FloatingScore");
        floatingObj.transform.SetParent(worldCanvas.transform, false);

        RectTransform rectTransform = floatingObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(260f, 70f);

        TextMeshProUGUI textMesh = floatingObj.AddComponent<TextMeshProUGUI>();
        textMesh.fontSize = fontSize;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontStyle = FontStyles.Bold;
        textMesh.outlineWidth = 0.2f;

        return floatingObj;
    }

    public void ShowFloatingScore(Vector3 worldPosition, int points, bool isBonus = false, bool isPenalty = false, bool isPerfect = false)
    {
        EnsureWorldCanvas();
        if (worldCanvas == null) return;

        string text;
        if (isPenalty)
        {
            text = points.ToString();
        }
        else if (isPerfect)
        {
            text = $"+{points} PERFECT!";
        }
        else if (isBonus)
        {
            text = $"+{points} BONUS";
        }
        else
        {
            text = points > 0 ? $"+{points}" : points.ToString();
        }

        GameObject floatingObj = textPool.Count > 0 ? textPool.Dequeue() : CreateNewFloatingObject();
        floatingObj.transform.localScale = Vector3.one;

        RectTransform rectTransform = floatingObj.GetComponent<RectTransform>();
        Vector2 canvasPos = WorldToCanvasPosition(worldPosition);
        rectTransform.anchoredPosition = canvasPos;

        TextMeshProUGUI textMesh = floatingObj.GetComponent<TextMeshProUGUI>();
        textMesh.text = text;

        if (isPenalty)
        {
            textMesh.color = new Color(1f, 0.25f, 0.2f, 1f);
            textMesh.outlineColor = new Color(0.4f, 0.05f, 0f, 0.9f);
        }
        else if (isPerfect)
        {
            textMesh.color = new Color(0.3f, 1f, 0.95f, 1f);
            textMesh.outlineColor = new Color(0f, 0.35f, 0.4f, 0.9f);
        }
        else if (isBonus)
        {
            textMesh.color = new Color(1f, 0.85f, 0.25f, 1f);
            textMesh.outlineColor = new Color(0.5f, 0.35f, 0f, 0.9f);
        }
        else
        {
            textMesh.color = new Color(0.5f, 1f, 0.7f, 1f);
            textMesh.outlineColor = new Color(0.1f, 0.3f, 0.15f, 0.8f);
        }

        floatingObj.SetActive(true);
        StartCoroutine(FloatAndFadeRoutine(floatingObj, rectTransform, textMesh, isPerfect));
    }

    private IEnumerator FloatAndFadeRoutine(GameObject floatingObj, RectTransform rectTransform, TextMeshProUGUI textMesh, bool isPerfect)
    {
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 endPos = startPos + Vector2.up * floatDistance;

        // Scale punch
        Vector3 baseScale = Vector3.one;
        float punchDuration = isPerfect ? 0.16f : 0.12f;
        float punchMax = isPerfect ? 1.6f : 1.35f;
        float punchElapsed = 0f;
        while (punchElapsed < punchDuration)
        {
            punchElapsed += Time.deltaTime;
            float pt = punchElapsed / punchDuration;
            float s = pt < 0.5f ? Mathf.Lerp(1f, punchMax, pt / 0.5f) : Mathf.Lerp(punchMax, 1f, (pt - 0.5f) / 0.5f);
            floatingObj.transform.localScale = baseScale * s;
            yield return null;
        }
        floatingObj.transform.localScale = baseScale;

        float elapsed = 0f;
        while (elapsed < floatDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / floatDuration;

            // Ease-out position
            float easeT = 1f - (1f - t) * (1f - t);
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, easeT);

            // Subtle horizontal drift
            float drift = Mathf.Sin(t * Mathf.PI * 2f) * 8f;
            rectTransform.anchoredPosition += new Vector2(drift, 0f);

            // Fade out
            Color color = textMesh.color;
            color.a = t > 0.6f ? Mathf.Lerp(1f, 0f, (t - 0.6f) / 0.4f) : 1f;
            textMesh.color = color;

            yield return null;
        }

        floatingObj.SetActive(false);
        textPool.Enqueue(floatingObj);
    }

    private Vector2 WorldToCanvasPosition(Vector3 worldPosition)
    {
        Camera cam = Camera.main;
        if (cam == null) return Vector2.zero;

        Vector3 screenPos = cam.WorldToScreenPoint(worldPosition);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            worldCanvas.GetComponent<RectTransform>(),
            screenPos,
            null,
            out Vector2 canvasPos
        );

        return canvasPos;
    }

    private void EnsureWorldCanvas()
    {
        if (worldCanvas != null) return;

        GameObject canvasObj = new GameObject("FloatingTextCanvas");
        canvasObj.transform.SetParent(transform);
        
        worldCanvas = canvasObj.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        worldCanvas.sortingOrder = 5;

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
    }
}
