using UnityEngine;

/// <summary>
/// Ambient floating neon particles in the background.
/// Intensity increases with combo. Creates atmosphere.
/// </summary>
public class AmbientParticles : MonoBehaviour
{
    private ParticleSystem ps;
    private ParticleSystem.EmissionModule emission;
    private float baseRate = 8f;
    private float maxRate = 35f;

    void Start()
    {
        CreateParticleSystem();
    }

    void Update()
    {
        // Scale particle intensity with combo
        float comboNorm = 0f;
        if (GameManager.Instance != null)
            comboNorm = Mathf.Clamp01(GameManager.Instance.ComboCount / 20f);

        emission.rateOverTime = Mathf.Lerp(baseRate, maxRate, comboNorm);
    }

    private void CreateParticleSystem()
    {
        GameObject psObj = new GameObject("AmbientDust");
        psObj.transform.SetParent(transform, false);
        psObj.transform.localPosition = Vector3.zero;

        ps = psObj.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.playOnAwake = true;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(3f, 6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.6f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.08f);
        main.maxParticles = 200;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = -0.02f; // Slight upward drift

        // Color — random neon hues
        Gradient startGrad = new Gradient();
        startGrad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.2f, 1f, 0.5f), 0f),
                new GradientColorKey(new Color(0.3f, 0.5f, 1f), 0.33f),
                new GradientColorKey(new Color(1f, 0.3f, 0.8f), 0.66f),
                new GradientColorKey(new Color(0.2f, 1f, 1f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.6f, 0f),
                new GradientAlphaKey(0.6f, 1f)
            }
        );
        main.startColor = new ParticleSystem.MinMaxGradient(startGrad);

        emission = ps.emission;
        emission.rateOverTime = baseRate;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(18f, 10f, 0.1f);

        var colorOverLife = ps.colorOverLifetime;
        colorOverLife.enabled = true;
        Gradient fadeGrad = new Gradient();
        fadeGrad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.7f, 0.2f),
                new GradientAlphaKey(0.7f, 0.8f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLife.color = fadeGrad;

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.4f;
        noise.frequency = 0.5f;
        noise.scrollSpeed = 0.3f;

        // Renderer
        var psr = psObj.GetComponent<ParticleSystemRenderer>();
        psr.renderMode = ParticleSystemRenderMode.Billboard;
        Shader pShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (pShader == null) pShader = Shader.Find("Particles/Standard Unlit");
        if (pShader != null)
        {
            Material pMat = new Material(pShader);
            pMat.SetFloat("_Surface", 1);
            psr.material = pMat;
        }
        psr.sortingOrder = -1; // Behind gameplay
    }
}
