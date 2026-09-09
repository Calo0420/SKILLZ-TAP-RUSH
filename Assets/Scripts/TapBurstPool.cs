using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zero-allocation pool for tap particle bursts.
/// Pre-warms and recycles ParticleSystem instances.
/// </summary>
public class TapBurstPool : MonoBehaviour
{
    public static TapBurstPool Instance { get; private set; }

    private readonly Queue<ParticleSystem> pool = new Queue<ParticleSystem>();
    private const int PreWarmCount = 16;
    private static Texture2D softParticleTexture;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < PreWarmCount; i++)
        {
            ParticleSystem ps = CreateNewBurstSystem();
            ps.gameObject.SetActive(false);
            pool.Enqueue(ps);
        }
    }

    public static void PlayBurst(Vector3 position, Color coreColor, float scale = 1f)
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("TapBurstPool");
            go.AddComponent<TapBurstPool>();
        }

        Instance.SpawnBurst(position, coreColor, scale);
    }

    private void SpawnBurst(Vector3 position, Color coreColor, float scale)
    {
        ParticleSystem ps = pool.Count > 0 ? pool.Dequeue() : CreateNewBurstSystem();

        ps.transform.position = position;
        ps.transform.localScale = Vector3.one * Mathf.Clamp(scale, 0.5f, 1.5f);

        var main = ps.main;
        main.startColor = coreColor;

        var colorOverLife = ps.colorOverLifetime;
        colorOverLife.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(coreColor, 0f),
                new GradientColorKey(Color.white, 0.3f),
                new GradientColorKey(coreColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLife.color = grad;

        ps.gameObject.SetActive(true);
        ps.Play();

        StartCoroutine(RecycleRoutine(ps, 0.55f));
    }

    private IEnumerator RecycleRoutine(ParticleSystem ps, float delay)
    {
        yield return new WaitForSeconds(delay);
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.gameObject.SetActive(false);
        pool.Enqueue(ps);
    }

    private ParticleSystem CreateNewBurstSystem()
    {
        GameObject psObj = new GameObject("PooledTapBurst");
        psObj.transform.SetParent(transform, false);

        ParticleSystem ps = psObj.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.playOnAwake = false;
        main.loop = false;
        main.startLifetime = 0.45f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 9f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.2f);
        main.maxParticles = 24;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0.3f;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 16, 24)
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.2f;

        var sizeOverLife = ps.sizeOverLifetime;
        sizeOverLife.enabled = true;
        sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f,
            AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

        var psr = psObj.GetComponent<ParticleSystemRenderer>();
        psr.renderMode = ParticleSystemRenderMode.Billboard;
        Shader pShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (pShader == null) pShader = Shader.Find("Particles/Standard Unlit");
        if (pShader != null)
        {
            Material pMat = new Material(pShader);
            pMat.SetFloat("_Surface", 1);
            pMat.mainTexture = GetSoftParticleTexture();
            psr.material = pMat;
        }

        return ps;
    }

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
}
