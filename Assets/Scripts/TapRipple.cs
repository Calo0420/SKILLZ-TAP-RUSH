using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns a radial shockwave ring at the tap position.
/// Uses a lightweight object pool to eliminate garbage collection.
/// </summary>
public class TapRipple : MonoBehaviour
{
    private static readonly Queue<TapRipple> RipplePool = new Queue<TapRipple>();
    private static Transform poolParent;

    private float lifetime = 0.4f;
    private float elapsed;
    private float maxRadius = 1.8f;
    private Color rippleColor;
    private SpriteRenderer sr;
    private bool active;

    private static Sprite rippleSprite;

    public static void Spawn(Vector3 worldPos, Color color, float scale = 1f)
    {
        TapRipple ripple = GetFromPool();
        ripple.Play(worldPos, color, scale);
    }

    private static TapRipple GetFromPool()
    {
        while (RipplePool.Count > 0)
        {
            TapRipple item = RipplePool.Dequeue();
            if (item != null)
            {
                item.gameObject.SetActive(true);
                return item;
            }
        }

        if (poolParent == null)
        {
            GameObject root = new GameObject("TapRipplePool");
            poolParent = root.transform;
            DontDestroyOnLoad(root);
        }

        GameObject go = new GameObject("PooledTapRipple");
        go.transform.SetParent(poolParent);
        TapRipple newRipple = go.AddComponent<TapRipple>();
        newRipple.InitRenderer();
        return newRipple;
    }

    private void InitRenderer()
    {
        EnsureRippleSprite();
        sr = gameObject.GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = rippleSprite;
        sr.sortingOrder = 10;
    }

    public void Play(Vector3 worldPos, Color color, float scale)
    {
        InitRenderer();
        transform.position = worldPos;
        transform.localScale = Vector3.zero;
        rippleColor = color;
        maxRadius = 1.8f * scale;
        elapsed = 0f;
        active = true;

        Color c = rippleColor;
        c.a = 0.7f;
        sr.color = c;
    }

    void Update()
    {
        if (!active) return;

        elapsed += Time.deltaTime;
        float t = elapsed / lifetime;

        if (t >= 1f)
        {
            active = false;
            gameObject.SetActive(false);
            RipplePool.Enqueue(this);
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
