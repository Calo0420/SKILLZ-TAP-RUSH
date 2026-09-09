using UnityEngine;

/// <summary>
/// Adds a spawn animation to targets: scale punch + slight rotation wobble.
/// Attach via code (Target.Start adds it) or place on prefab.
/// </summary>
public class TargetSpawnAnim : MonoBehaviour
{
    private float duration = 0.25f;
    private float overshoot = 1.3f;
    private float elapsed;
    private Vector3 targetScale;
    private bool animating;

    public void Play(Vector3 scale)
    {
        targetScale = scale;
        transform.localScale = Vector3.zero;
        transform.rotation = Quaternion.identity;
        elapsed = 0f;
        animating = true;
    }

    void Start()
    {
        if (!animating)
        {
            Play(transform.localScale);
        }
    }

    void Update()
    {
        if (!animating) return;

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);

        // Elastic ease-out
        float p = 0.3f;
        float s = p / 4f;
        float elastic = Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - s) * (2f * Mathf.PI) / p) + 1f;

        // Overshoot blend
        float scale = Mathf.LerpUnclamped(0f, 1f, elastic);
        if (t < 0.5f) scale *= overshoot;
        else scale = Mathf.Lerp(overshoot, 1f, (t - 0.5f) / 0.5f);

        transform.localScale = targetScale * Mathf.Max(scale, 0f);

        // Slight rotation wobble
        float wobble = Mathf.Sin(t * Mathf.PI * 3f) * (1f - t) * 8f;
        transform.rotation = Quaternion.Euler(0f, 0f, wobble);

        if (t >= 1f)
        {
            transform.localScale = targetScale;
            transform.rotation = Quaternion.identity;
            animating = false;
        }
    }
}
