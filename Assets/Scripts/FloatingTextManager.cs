using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance { get; private set; }

    [SerializeField] private float floatDuration = 0.8f;
    [SerializeField] private float floatDistance = 2f;
    [SerializeField] private float fontSize = 32f;

    private Canvas worldCanvas;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureWorldCanvas();
    }

    public void ShowFloatingScore(Vector3 worldPosition, int points, bool isBonus = false, bool isPenalty = false)
    {
        EnsureWorldCanvas();
        if (worldCanvas == null) return;

        string text = points > 0 ? $"+{points}" : points.ToString();
        if (isBonus) text += " BONUS";

        GameObject floatingObj = new GameObject("FloatingScore");
        floatingObj.transform.SetParent(worldCanvas.transform, false);
        
        RectTransform rectTransform = floatingObj.AddComponent<RectTransform>();
        
        // Convert world position to canvas position
        Vector2 canvasPos = WorldToCanvasPosition(worldPosition);
        rectTransform.anchoredPosition = canvasPos;

        TextMeshProUGUI textMesh = floatingObj.AddComponent<TextMeshProUGUI>();
        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.outlineWidth = 0.2f;
        
        if (isPenalty)
        {
            textMesh.color = new Color(1f, 0.2f, 0.2f, 1f);
            textMesh.outlineColor = new Color(0.3f, 0f, 0f, 1f);
            textMesh.fontStyle = FontStyles.Bold;
        }
        else if (isBonus)
        {
            textMesh.color = new Color(1f, 0.85f, 0f, 1f);
            textMesh.outlineColor = new Color(0.5f, 0.4f, 0f, 1f);
            textMesh.fontStyle = FontStyles.Bold;
        }
        else
        {
            textMesh.color = new Color(0.2f, 1f, 0.2f, 1f);
            textMesh.outlineColor = Color.black;
        }

        RectTransform textRect = textMesh.rectTransform;
        textRect.sizeDelta = new Vector2(220f, 70f);

        StartCoroutine(FloatAndFadeRoutine(floatingObj, rectTransform, textMesh));
    }

    private IEnumerator FloatAndFadeRoutine(GameObject floatingObj, RectTransform rectTransform, TextMeshProUGUI textMesh)
    {
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 endPos = startPos + Vector2.up * floatDistance;
        
        float elapsed = 0f;
        while (elapsed < floatDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / floatDuration;

            // Ease-out position (starts fast, slows down)
            float easeT = 1f - (1f - t) * (1f - t);
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, easeT);

            // Fade out
            Color color = textMesh.color;
            color.a = Mathf.Lerp(1f, 0f, t);
            textMesh.color = color;

            yield return null;
        }

        Destroy(floatingObj);
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
