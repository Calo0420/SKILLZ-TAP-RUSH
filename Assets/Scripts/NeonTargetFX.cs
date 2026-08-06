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
    private static Sprite coreSprite;

    public void SetType(FXType type)
    {
        fxType = type;
        Initialize();
    }

    private void Initialize()
    {
        EnsureSprites();

        // Set the main sprite to a filled circle with bright core color
        mainSR = GetComponent<SpriteRenderer>();
        if (mainSR != null)
        {
            mainSR.sprite = coreSprite;
            mainSR.color = GetCoreColor(fxType);
            mainSR.sortingOrder = 2;
        }

        // Create glow child
        glowColor = GetGlowColor(fxType);
        GameObject glowObj = new GameObject("Glow");
        glowObj.transform.SetParent(transform, false);
        glowObj.transform.localPosition = Vector3.zero;
        glowObj.transform.localScale = Vector3.one * 2.2f;
        glowSR = glowObj.AddComponent<SpriteRenderer>();
        glowSR.sprite = glowSprite;
        glowSR.color = glowColor;
        glowSR.sortingOrder = 1;

        // Create particle system for tap burst
        CreateBurstParticles();
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
        if (burstPS != null)
        {
            burstPS.transform.SetParent(null);
            burstPS.Play();
            Destroy(burstPS.gameObject, 1.5f);
            burstPS = null;
        }
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

    // --- Procedural sprite generation (cached, created once) ---

    private static void EnsureSprites()
    {
        if (coreSprite != null && glowSprite != null) return;
        coreSprite = GenerateCircleSprite(64, true);
        glowSprite = GenerateCircleSprite(128, false);
    }

    /// <summary>
    /// Generates a filled or soft-edged circle sprite procedurally.
    /// </summary>
    private static Sprite GenerateCircleSprite(int size, bool filled)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float center = size * 0.5f;
        float radius = center - 1f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float norm = dist / radius;

                if (filled)
                {
                    // Solid circle with slight soft edge
                    if (norm > 1f)
                        tex.SetPixel(x, y, Color.clear);
                    else if (norm > 0.85f)
                    {
                        float edge = 1f - ((norm - 0.85f) / 0.15f);
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, edge));
                    }
                    else
                    {
                        // Bright center, slightly dimmer edges for depth
                        float bright = 1f - norm * 0.3f;
                        tex.SetPixel(x, y, new Color(bright, bright, bright, 1f));
                    }
                }
                else
                {
                    // Soft glow falloff
                    if (norm > 1f)
                        tex.SetPixel(x, y, Color.clear);
                    else
                    {
                        float alpha = Mathf.Pow(1f - norm, 2.5f);
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                }
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
