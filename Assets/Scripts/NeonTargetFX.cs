using UnityEngine;
using System.Collections;

/// <summary>
/// Pure-2D neon visual effects for targets: glow ring, color-coded type,
/// pulse animation, and particle burst on tap. Works entirely with SpriteRenderers.
/// </summary>
public class NeonTargetFX : MonoBehaviour
{
    public enum FXType { Normal, Decoy, Bonus, Gauge }

    private static readonly Color NormalCore  = new Color(0.3f, 1f, 0.5f, 1f);
    private static readonly Color NormalGlow  = new Color(0.1f, 0.9f, 0.3f, 0.5f);
    private static readonly Color DecoyCore   = new Color(1f, 0.2f, 0.2f, 1f);
    private static readonly Color DecoyGlow   = new Color(1f, 0.05f, 0.05f, 0.5f);
    private static readonly Color BonusCore   = new Color(1f, 0.9f, 0.2f, 1f);
    private static readonly Color BonusGlow   = new Color(1f, 0.75f, 0f, 0.55f);
    private static readonly Color GaugeCore   = new Color(0.3f, 0.9f, 1f, 1f);
    private static readonly Color GaugeGlow   = new Color(0f, 0.7f, 1f, 0.5f);

    private SpriteRenderer mainSR;
    private SpriteRenderer glowSR;
    private FXType fxType;
    private Color glowColor;
    private ParticleSystem burstPS;

    private static Sprite glowSprite;
    private static Sprite normalMetallicSprite;
    private static Sprite decoyMetallicSprite;
    private static Sprite bonusMetallicSprite;
    private static Sprite gaugeMetallicSprite;

    public void SetType(FXType type)
    {
        fxType = type;
        Initialize();
    }

    private void Initialize()
    {
        EnsureSprites();

        // Set the main sprite to a 3D-shaded metallic orb
        if (mainSR == null) mainSR = GetComponent<SpriteRenderer>();
        if (mainSR != null)
        {
            mainSR.sprite = GetMetallicSprite(fxType);
            mainSR.color = Color.white;
            mainSR.sortingOrder = 2;
        }

        // Create or update glow child
        glowColor = GetGlowColor(fxType);
        if (glowSR == null)
        {
            Transform existingGlow = transform.Find("Glow");
            if (existingGlow != null)
            {
                glowSR = existingGlow.GetComponent<SpriteRenderer>();
            }
            else
            {
                GameObject glowObj = new GameObject("Glow");
                glowObj.transform.SetParent(transform, false);
                glowObj.transform.localPosition = Vector3.zero;
                glowObj.transform.localScale = Vector3.one * 2.2f;
                glowSR = glowObj.AddComponent<SpriteRenderer>();
                glowSR.sprite = glowSprite;
                glowSR.sortingOrder = 1;
            }
        }

        if (glowSR != null)
        {
            glowSR.color = glowColor;
        }
    }

    private float phaseOffset;

    private void Awake()
    {
        phaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        if (glowSR == null) return;
        float pulse = 0.35f + Mathf.Sin(Time.time * 5f + phaseOffset) * 0.2f;
        Color c = glowColor;
        c.a = pulse;
        glowSR.color = c;

        float s = 2.2f + Mathf.Sin(Time.time * 3f + phaseOffset) * 0.15f;
        glowSR.transform.localScale = Vector3.one * s;
    }

    public void PlayBurst()
    {
        TapBurstPool.PlayBurst(transform.position, GetCoreColor(fxType), transform.localScale.x);
    }

    // --- Cached soft round particle texture ---
    private static Texture2D softParticleTexture;

    private static Texture2D GetSoftParticleTexture()
    {
        if (softParticleTexture != null) return softParticleTexture;
        int size = 64;
        softParticleTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        softParticleTexture.filterMode = FilterMode.Bilinear;
        float center = size * 0.5f;
        float radius = center - 1f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float norm = dist / radius;
                float alpha = norm > 1f ? 0f : Mathf.Pow(1f - norm, 2f);
                softParticleTexture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        softParticleTexture.Apply();
        return softParticleTexture;
    }

    private void CreateBurstParticles()
    {
        GameObject psObj = new GameObject("TapBurst");
        psObj.transform.SetParent(transform, false);
        psObj.transform.localPosition = Vector3.zero;

        burstPS = psObj.AddComponent<ParticleSystem>();

        var main = burstPS.main;
        main.playOnAwake = false;
        main.loop = false;
        main.startLifetime = 0.45f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 9f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.2f);
        main.startColor = GetCoreColor(fxType);
        main.maxParticles = 24;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0.3f;

        var emission = burstPS.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 16, 24)
        });

        var shape = burstPS.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.2f;

        var colorOverLife = burstPS.colorOverLifetime;
        colorOverLife.enabled = true;
        Gradient grad = new Gradient();
        Color core = GetCoreColor(fxType);
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(core, 0f),
                new GradientColorKey(Color.white, 0.3f),
                new GradientColorKey(core, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLife.color = grad;

        var sizeOverLife = burstPS.sizeOverLifetime;
        sizeOverLife.enabled = true;
        sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f,
            AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

        // Soft round particle material
        var psr = psObj.GetComponent<ParticleSystemRenderer>();
        psr.renderMode = ParticleSystemRenderMode.Billboard;
        Shader pShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (pShader == null) pShader = Shader.Find("Particles/Standard Unlit");
        if (pShader != null)
        {
            Material pMat = new Material(pShader);
            pMat.SetFloat("_Surface", 1); // transparent
            pMat.mainTexture = GetSoftParticleTexture();
            psr.material = pMat;
        }
    }

    private Color GetCoreColor(FXType type)
    {
        switch (type)
        {
            case FXType.Decoy:  return DecoyCore;
            case FXType.Bonus:  return BonusCore;
            case FXType.Gauge:  return GaugeCore;
            default:            return NormalCore;
        }
    }

    private Color GetGlowColor(FXType type)
    {
        switch (type)
        {
            case FXType.Decoy:  return DecoyGlow;
            case FXType.Bonus:  return BonusGlow;
            case FXType.Gauge:  return GaugeGlow;
            default:            return NormalGlow;
        }
    }

    // --- Procedural 3D metallic sprite generation (cached, created once) ---

    private static Sprite GetMetallicSprite(FXType type)
    {
        EnsureSprites();
        switch (type)
        {
            case FXType.Decoy: return decoyMetallicSprite;
            case FXType.Bonus: return bonusMetallicSprite;
            case FXType.Gauge: return gaugeMetallicSprite;
            default:           return normalMetallicSprite;
        }
    }

    private static void EnsureSprites()
    {
        if (normalMetallicSprite != null && glowSprite != null) return;

        const int orbResolution = 256;
        normalMetallicSprite = GenerateMetallicOrbSprite(orbResolution, FXType.Normal);
        decoyMetallicSprite  = GenerateMetallicOrbSprite(orbResolution, FXType.Decoy);
        bonusMetallicSprite  = GenerateMetallicOrbSprite(orbResolution, FXType.Bonus);
        gaugeMetallicSprite  = GenerateMetallicOrbSprite(orbResolution, FXType.Gauge);

        glowSprite = GenerateGlowSprite(128);
    }

    /// <summary>
    /// Generates a high-definition 3D-shaded metallic sphere with specular gleam,
    /// metallic Fresnel rim, brushed micro-texture, and smooth anti-aliased edge.
    /// </summary>
    private static Sprite GenerateMetallicOrbSprite(int size, FXType type)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float center = size * 0.5f;
        float radius = center - 2f;

        // Color palettes tuned for lustrous metallic sheen
        Color ambientCol, diffuseCol, specCol, rimCol, bounceCol;
        switch (type)
        {
            case FXType.Decoy:
                // Ruby / Crimson Chromium
                ambientCol = new Color(0.28f, 0.03f, 0.06f);
                diffuseCol = new Color(0.95f, 0.12f, 0.18f);
                specCol    = new Color(1.0f, 0.90f, 0.92f);
                rimCol     = new Color(1.0f, 0.35f, 0.20f);
                bounceCol  = new Color(0.35f, 0.02f, 0.06f);
                break;

            case FXType.Bonus:
                // Polished 24K Gold Ingot
                ambientCol = new Color(0.42f, 0.25f, 0.04f);
                diffuseCol = new Color(1.0f, 0.84f, 0.12f);
                specCol    = new Color(1.0f, 0.98f, 0.88f);
                rimCol     = new Color(1.0f, 0.92f, 0.40f);
                bounceCol  = new Color(0.40f, 0.20f, 0.02f);
                break;

            case FXType.Gauge:
                // Electric Cobalt / Plasma Cyan
                ambientCol = new Color(0.04f, 0.20f, 0.38f);
                diffuseCol = new Color(0.15f, 0.80f, 1.0f);
                specCol    = new Color(0.92f, 0.98f, 1.0f);
                rimCol     = new Color(0.35f, 0.92f, 1.0f);
                bounceCol  = new Color(0.06f, 0.22f, 0.45f);
                break;

            default:
                // Emerald Titanium / Cyber Green Chrome
                ambientCol = new Color(0.06f, 0.26f, 0.12f);
                diffuseCol = new Color(0.20f, 0.88f, 0.38f);
                specCol    = new Color(0.88f, 1.0f, 0.92f);
                rimCol     = new Color(0.40f, 0.95f, 0.65f);
                bounceCol  = new Color(0.05f, 0.35f, 0.22f);
                break;
        }

        Vector3 lightDir = new Vector3(-0.45f, 0.45f, 0.77f).normalized;
        Vector3 halfVector = (lightDir + Vector3.forward).normalized;
        Vector3 bounceDir = new Vector3(0.40f, -0.40f, 0.35f).normalized;

        for (int y = 0; y < size; y++)
        {
            float v = (y - center) / radius;
            for (int x = 0; x < size; x++)
            {
                float u = (x - center) / radius;
                float distSq = u * u + v * v;

                if (distSq > 1f)
                {
                    tex.SetPixel(x, y, Color.clear);
                    continue;
                }

                float dist = Mathf.Sqrt(distSq);
                float nz = Mathf.Sqrt(Mathf.Max(0f, 1f - distSq));
                Vector3 normal = new Vector3(u, v, nz);

                // Diffuse lighting with wrap for smooth metallic curvature
                float NdotL = Mathf.Clamp01(Vector3.Dot(normal, lightDir));
                float diffuse = Mathf.Pow(NdotL, 0.85f);

                // Sharp metallic specular reflection (Blinn-Phong)
                float NdotH = Mathf.Clamp01(Vector3.Dot(normal, halfVector));
                float spec = Mathf.Pow(NdotH, 24f) * 0.75f + Mathf.Pow(NdotH, 60f) * 0.65f;

                // Secondary ground bounce light (bottom-right reflection)
                float bounce = Mathf.Clamp01(Vector3.Dot(normal, bounceDir)) * 0.32f;

                // Chrome Fresnel rim reflection
                float fresnel = Mathf.Pow(1f - nz, 2.6f);

                // Machined brushed metal concentric micro-sheen
                float brushed = 1f + 0.04f * Mathf.Sin(dist * 60f);

                // Beveled metal edge ring
                float bevelRing = (dist >= 0.80f && dist <= 0.94f)
                    ? Mathf.Sin(((dist - 0.80f) / 0.14f) * Mathf.PI) * 0.22f
                    : 0f;

                // Blend final shaded metal surface
                Color pixel = Color.Lerp(ambientCol, diffuseCol, diffuse) * brushed;
                pixel += bounceCol * bounce;
                pixel = Color.Lerp(pixel, rimCol, fresnel * 0.65f + bevelRing);
                pixel += specCol * spec;

                // Smooth anti-aliased perimeter edge
                float alpha = dist > 0.95f ? Mathf.Clamp01((1f - dist) / 0.05f) : 1f;
                pixel.a = alpha;

                tex.SetPixel(x, y, pixel);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>
    /// Soft circular glow aura for backlayer.
    /// </summary>
    private static Sprite GenerateGlowSprite(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float center = size * 0.5f;
        float radius = center - 1f;

        for (int y = 0; y < size; y++)
        {
            float v = (y - center) / radius;
            for (int x = 0; x < size; x++)
            {
                float u = (x - center) / radius;
                float distSq = u * u + v * v;
                if (distSq > 1f)
                {
                    tex.SetPixel(x, y, Color.clear);
                }
                else
                {
                    float dist = Mathf.Sqrt(distSq);
                    float alpha = Mathf.Pow(1f - dist, 2.4f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
