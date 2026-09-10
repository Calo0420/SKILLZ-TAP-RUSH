using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField] private float lifetime = 2f;
    private float baseLifetime = 2f;

    private static int lastConsumedTapFrame = -1;

    private bool tapped;
    private bool isBonus;
    private Vector3 driftVelocity;
    private Vector3 originalScale;
    private float shrinkRateMultiplier = 1f;

    private bool isDecoy;
    private float spawnTime;

    public bool IsTapped => tapped;

    public static void MarkTapConsumedThisFrame()
    {
        lastConsumedTapFrame = Time.frameCount;
    }

    public static bool WasTapConsumedThisFrame()
    {
        return Time.frameCount == lastConsumedTapFrame;
    }

    private void Awake()
    {
        baseLifetime = lifetime;
    }

    public void ResetState()
    {
        tapped = false;
        isBonus = false;
        isDecoy = false;
        lifetime = baseLifetime;
        driftVelocity = Vector3.zero;
        shrinkRateMultiplier = 1f;
        originalScale = Vector3.one;
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;
        CancelInvoke();
        StopAllCoroutines();

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = new Color(0.3f, 1f, 0.5f, 1f);

        TargetSpawnAnim anim = GetComponent<TargetSpawnAnim>();
        if (anim != null) anim.Stop();
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


    public void ActivateTarget()
    {
        spawnTime = Time.time;
        CancelInvoke(nameof(Miss));
        Invoke(nameof(Miss), lifetime);

        originalScale = transform.localScale;

        // Add or reconfigure neon glow + particle visuals
        NeonTargetFX nfx = GetComponent<NeonTargetFX>();
        if (nfx == null) nfx = gameObject.AddComponent<NeonTargetFX>();
        if (isDecoy) nfx.SetType(NeonTargetFX.FXType.Decoy);
        else if (isBonus) nfx.SetType(NeonTargetFX.FXType.Bonus);
        else nfx.SetType(NeonTargetFX.FXType.Normal);

        // Spawn animation — elastic pop-in
        TargetSpawnAnim anim = GetComponent<TargetSpawnAnim>();
        if (anim == null) anim = gameObject.AddComponent<TargetSpawnAnim>();
        anim.Play(originalScale);

        if (!isDecoy && !isBonus)
        {
            StartCoroutine(ShrinkOverLifetime());
        }
    }

    void Start()
    {
        if (spawnTime <= 0f)
        {
            ActivateTarget();
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
        MarkTapConsumedThisFrame();
        CancelInvoke(nameof(Miss));

        if (isDecoy)
        {
            GameManager.Instance?.RegisterWrongTap();
            FloatingTextManager.Instance?.ShowFloatingScore(transform.position, -75, isBonus: false, isPenalty: true);
        }
        else if (isBonus)
        {
            float elapsed = Time.time - spawnTime;
            float speedScore01 = 1f - Mathf.Clamp01(elapsed / lifetime);
            int bonusPoints = CalculateBonusPoints(speedScore01);
            GameManager.Instance?.RegisterBonusHit(speedScore01);
            
            FloatingTextManager.Instance?.ShowFloatingScore(transform.position, bonusPoints, isBonus: true);
        }
        else
        {
            float elapsed = Time.time - spawnTime;
            float sizeBonus01 = (originalScale == Vector3.zero || originalScale.x <= 0f) ? 0f :
                Mathf.Clamp01(1f - (transform.localScale.x / originalScale.x));
            float speedScore01 = 1f - Mathf.Clamp01(elapsed / lifetime);
            int normalPoints = CalculateNormalPoints(speedScore01, sizeBonus01);
            GameManager.Instance?.RegisterHit(speedScore01, sizeBonus01);

            bool isPerfect = speedScore01 >= 0.85f;
            FloatingTextManager.Instance?.ShowFloatingScore(transform.position, normalPoints, isBonus: false, isPenalty: false, isPerfect: isPerfect);
        }

        // Play particle burst
        NeonTargetFX nfx = GetComponent<NeonTargetFX>();
        if (nfx != null) nfx.PlayBurst();

        // Tap ripple shockwave
        Color rippleColor = isDecoy ? new Color(1f, 0.2f, 0.2f) : isBonus ? new Color(1f, 0.9f, 0.2f) : new Color(0.3f, 1f, 0.6f);
        TapRipple.Spawn(transform.position, rippleColor, transform.localScale.x);

        // Screen effects
        if (!isDecoy) ScreenFX.Instance?.OnHit();

        Despawn();
        return true;
    }

    private void Despawn()
    {
        CancelInvoke(nameof(Miss));
        StopAllCoroutines();

        TargetSpawner.Instance?.NotifyTargetDespawned(this);

        if (TargetPool.Instance != null)
        {
            TargetPool.Instance.Return(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private int CalculateNormalPoints(float speedScore01, float sizeBonus01)
    {
        const int baseTapPoints = 100;
        const int maxSpeedBonus = 50;

        float comboMultiplier = 1f;
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.ComboCount >= 10) comboMultiplier = 1.5f;
            else if (GameManager.Instance.ComboCount >= 5) comboMultiplier = 1.2f;
        }

        int speedBonus = Mathf.RoundToInt(Mathf.Clamp01(speedScore01) * maxSpeedBonus);
        int sizeBonus = Mathf.RoundToInt(Mathf.Clamp01(sizeBonus01) * maxSpeedBonus);
        int points = Mathf.RoundToInt((baseTapPoints + speedBonus + sizeBonus) * comboMultiplier * GameManager.Instance.ScoreMultiplier);

        return points;
    }

    private int CalculateBonusPoints(float speedScore01)
    {
        const int baseTapPoints = 100;
        const int maxSpeedBonus = 50;

        int points = Mathf.RoundToInt(((baseTapPoints * 3f) + Mathf.Clamp01(speedScore01) * maxSpeedBonus) * GameManager.Instance.ScoreMultiplier);
        return points;
    }

private void Miss()
    {
        if (tapped) return;

        // Missing a decoy is intentional — no penalty
        if (!isDecoy)
        {
            GameManager.Instance?.RegisterMiss();
        }

        Despawn();
    }

private System.Collections.IEnumerator ShrinkOverLifetime()
    {
        const float popInDuration = 0.25f;
        yield return new WaitForSeconds(popInDuration);

        float shrinkDuration = Mathf.Max(0.1f, lifetime - popInDuration);
        float elapsed = 0f;
        while (elapsed < shrinkDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01((elapsed / shrinkDuration) * shrinkRateMultiplier);
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
