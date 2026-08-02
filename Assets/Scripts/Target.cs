using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField] private float lifetime = 2f;

    private static int lastConsumedTapFrame = -1;

    private bool tapped;
    private bool isBonus;
    private Vector3 driftVelocity;
    private Vector3 originalScale;
    private float shrinkRateMultiplier = 1f;


    private bool isDecoy;

    private float spawnTime;

    public static void MarkTapConsumedThisFrame()
    {
        lastConsumedTapFrame = Time.frameCount;
    }

    
public static bool WasTapConsumedThisFrame()
    {
        return Time.frameCount == lastConsumedTapFrame;
    }

public void SetAsBonus()
    {
        isBonus = true;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = new Color(1f, 0.85f, 0f, 1f);
        transform.localScale *= 0.6f;
        lifetime *= 0.6f;
    }

    /// <summary>
    /// Sets this target's size directly. The randomness now lives in TargetSpawner via the
    /// seeded/deterministic RNG (Phase 4.5), so the exact scale rolled is reproducible given
    /// the same match seed — this method just applies whatever value it's handed.
    /// </summary>
    public void SetSize(float scale)
    {
        transform.localScale *= scale;
    }

    public void SetDrift(Vector2 velocity)
    {
        driftVelocity = new Vector3(velocity.x, velocity.y, 0f);
    }

public void SetShrinkRateMultiplier(float multiplier)
    {
        shrinkRateMultiplier = Mathf.Max(0.1f, multiplier);
    }


    
public void SetAsDecoy()
    {
        isDecoy = true;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = new Color(0.95f, 0.15f, 0.15f, 1f);
        transform.localScale *= 0.8f;
    }


void Start()
    {
        spawnTime = Time.time;
        Invoke(nameof(Miss), lifetime);

        // Add neon glow + particle burst visuals
        NeonTargetFX nfx = gameObject.AddComponent<NeonTargetFX>();
        if (isDecoy) nfx.SetType(NeonTargetFX.FXType.Decoy);
        else if (isBonus) nfx.SetType(NeonTargetFX.FXType.Bonus);
        else nfx.SetType(NeonTargetFX.FXType.Normal);

        if (!isDecoy && !isBonus)
        {
            StartCoroutine(ShrinkOverLifetime());
        }
    }

    void OnMouseDown()
    {
        TryTap();
    }

public bool TryTap()
    {
        if (tapped || GameManager.Instance == null || !GameManager.Instance.IsGameActive)
        {
            return false;
        }

        tapped = true;
        lastConsumedTapFrame = Time.frameCount;
        CancelInvoke(nameof(Miss));

        if (isDecoy)
        {
            GameManager.Instance?.RegisterWrongTap();
        }
        else if (isBonus)
        {
            float elapsed = Time.time - spawnTime;
            float speedScore01 = 1f - Mathf.Clamp01(elapsed / lifetime);
            GameManager.Instance?.RegisterBonusHit(speedScore01);
        }
        else
        {
            float elapsed = Time.time - spawnTime;
            float sizeBonus01 = originalScale == Vector3.zero ? 0f :
                Mathf.Clamp01(1f - (transform.localScale.x / originalScale.x));
            float speedScore01 = 1f - Mathf.Clamp01(elapsed / lifetime);
            GameManager.Instance?.RegisterHit(speedScore01, sizeBonus01);
        }

        // Play particle burst before destroying
        NeonTargetFX nfx = GetComponent<NeonTargetFX>();
        if (nfx != null) nfx.PlayBurst();

        Destroy(gameObject);
        return true;
    }

private void Miss()
    {
        if (tapped) return;

        // Missing a decoy is intentional — no penalty
        if (!isDecoy)
        {
            GameManager.Instance?.RegisterMiss();
        }

        Destroy(gameObject);
    }

private System.Collections.IEnumerator ShrinkOverLifetime()
    {
        originalScale = transform.localScale;
        float elapsed = 0f;
        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01((elapsed / lifetime) * shrinkRateMultiplier);
            // Ease-in shrink: starts slow, accelerates toward end
            // Floor at 45% so targets remain fairly tappable at end of life
            float scale = Mathf.Lerp(1f, 0.45f, t * t);
            transform.localScale = originalScale * scale;
            yield return null;
        }
    }

    private void Update()
    {
        if (driftVelocity != Vector3.zero)
        {
            transform.position += driftVelocity * Time.deltaTime;
        }
    }

}
