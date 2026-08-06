using UnityEngine;
using System.Collections;

/// <summary>
/// Punchy score text animation: scale up then back + slight Y bounce.
/// Attach to the score TextMeshPro object. UIManager triggers it.
/// </summary>
public class ScorePunchFX : MonoBehaviour
{
    private Vector3 baseScale;
    private Coroutine punchRoutine;
    private float punchAmount = 1.25f;
    private float punchDuration = 0.15f;

    void Awake()
    {
        baseScale = transform.localScale;
    }

    public void Punch(float intensity = 1f)
    {
        if (punchRoutine != null) StopCoroutine(punchRoutine);
        punchRoutine = StartCoroutine(DoPunch(intensity));
    }

    private IEnumerator DoPunch(float intensity)
    {
        float scale = Mathf.Lerp(1f, punchAmount, Mathf.Clamp01(intensity));
        float elapsed = 0f;

        while (elapsed < punchDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / punchDuration;
            // Overshoot then settle
            float s;
            if (t < 0.4f)
                s = Mathf.Lerp(1f, scale, t / 0.4f);
            else
                s = Mathf.Lerp(scale, 1f, (t - 0.4f) / 0.6f);

            transform.localScale = baseScale * s;
            yield return null;
        }

        transform.localScale = baseScale;
        punchRoutine = null;
    }
}
