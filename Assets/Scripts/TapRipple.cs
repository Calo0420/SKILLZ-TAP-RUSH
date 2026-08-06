using UnityEngine;

/// <summary>
/// Spawns a radial shockwave ring at the tap position.
/// Self-destructs after the animation completes.
/// </summary>
public class TapRipple : MonoBehaviour
{
    private static Material rippleMaterial;
    private static Mesh quadMesh;

    private float lifetime = 0.4f;
    private float elapsed;
    private float maxRadius = 1.8f;
    private Color rippleColor;
    private SpriteRenderer sr;

    private static Sprite rippleSprite;

    public static void Spawn(Vector3 worldPos, Color color, float scale = 1f)
    {
        GameObject go = new GameObject("TapRipple");
        go.transform.position = worldPos;
        go.transform.localScale = Vector3.zero;

        TapRipple ripple = go.AddComponent<TapRipple>();
        ripple.rippleColor = color;
        ripple.maxRadius = 1.8f * scale;
    }

    void Start()
    {
        EnsureRippleSprite();

        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = rippleSprite;
        sr.color = rippleColor;
        sr.sortingOrder = 10;
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float t = elapsed / lifetime;

        if (t >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        // Expand with ease-out
        float eased = 1f - (1f - t) * (1f - t);
        float size = eased * maxRadius;
        transform.localScale = new Vector3(size, size, 1f);

        // Fade out
        Color c = rippleColor;
        c.a = (1f - t) * 0.7f;
        sr.color = c;
    }

    private static void EnsureRippleSprite()
    {
        if (rippleSprite != null) return;

        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float center = size * 0.5f;
        float outerRadius = center - 1f;
        float innerRadius = outerRadius * 0.75f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (dist > outerRadius || dist < innerRadius)
                {
                    tex.SetPixel(x, y, Color.clear);
                }
                else
                {
                    // Ring with soft edges
                    float ringCenter = (outerRadius + innerRadius) * 0.5f;
                    float ringWidth = (outerRadius - innerRadius) * 0.5f;
                    float alpha = 1f - Mathf.Abs(dist - ringCenter) / ringWidth;
                    alpha = Mathf.Pow(alpha, 1.5f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
        }

        tex.Apply();
        rippleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
