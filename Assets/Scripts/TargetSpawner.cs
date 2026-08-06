using UnityEngine;

public class TargetSpawner : MonoBehaviour
{
    public static TargetSpawner Instance { get; private set; }

    [SerializeField] private GameObject targetPrefab;
    [SerializeField] private float spawnInterval = 0.30f;
    [SerializeField] private bool singleActiveTarget = false;

    [Header("Decoy Settings")]
    [SerializeField] private float decoySpawnChance = 0.3f;
    [SerializeField] private float minSpawnInterval = 0.35f;
    [SerializeField] private float targetMinScale = 0.5f;
    [SerializeField] private float targetMaxScale = 1.4f;
    [SerializeField] private float driftSpeedMin = 0.3f;
    [SerializeField] private float driftSpeedMax = 1.2f;
    [SerializeField] private float bonusSpawnChance = 0.08f;

    [Header("Gauge Power-Up")]
    [SerializeField] private float gaugeSpawnInterval = 22f;
    private float gaugeTimer;

    [Header("Chaos Mode")]
    [SerializeField] private float chaosSpawnRateMultiplier = 2f;
    [SerializeField] private float chaosGoldChanceMultiplier = 1.8f;
    [SerializeField] private float chaosShrinkMultiplier = 1.9f;
    [SerializeField] private float chaosMinSpawnInterval = 0.18f;
    [SerializeField] private float chaosRedSpawnChance = 0.55f;
    [SerializeField] private int chaosMinRedPerWave = 2;
    [SerializeField] private int chaosMaxRedPerWave = 4;
    [SerializeField] private float chaosWhiteSpawnChance = 0.75f;

    [Header("Phase 4.5 Replay Test (temporary debug tools)")]
    [Tooltip("Set to a non-zero value to force this exact match seed instead of a random one. Run the match once, note the logged spawn sequence, reset play, run again with the SAME value here — the logs should be byte-identical. Set back to 0 for normal play.")]
    [SerializeField] private int debugForceSeed = 0;
    [Tooltip("When on, logs every spawn's index/position/type to the Console so two runs can be diffed. Leave off for normal play, it's noisy.")]
    [SerializeField] private bool debugLogSpawnSequence = false;
    private int _debugSpawnIndex;


    private bool chaosModeActive;
    private float baseBonusSpawnChance;



    [SerializeField] private float speedUpPerMilestone = 0.12f;



    // Spawn bounds — computed from camera at startup, with margin so targets stay on screen
    private float xMin, xMax;
    private float yMin, yMax;
    [SerializeField] private float spawnMargin = 0.6f;

    private float spawnTimer;
    private int milestoneCount;

    private bool spawning;
    private Target currentTarget;

    // --- Deterministic/seeded RNG (Phase 4.5, hard gate before Skillz SDK integration) ---
    // Every piece of randomness that decides WHAT/WHERE/WHEN a target spawns MUST go through
    // this seeded generator, not UnityEngine.Random — that's what makes "same seed = identical
    // spawn sequence" true. Purely cosmetic randomness elsewhere (e.g. NeonTargetFX's glow
    // pulse phase offset) is fine left on UnityEngine.Random since it doesn't affect gameplay.
    private System.Random _rng;

    private float RngRange(float minInclusive, float maxInclusive)
    {
        return (float)(minInclusive + _rng.NextDouble() * (maxInclusive - minInclusive));
    }

    private int RngRange(int minInclusive, int maxExclusive)
    {
        return _rng.Next(minInclusive, maxExclusive);
    }

    private float RngValue()
    {
        return (float)_rng.NextDouble();
    }

    private Vector2 RngInsideUnitCircle()
    {
        double angle = _rng.NextDouble() * (System.Math.PI * 2.0);
        double radius = System.Math.Sqrt(_rng.NextDouble());
        return new Vector2((float)(System.Math.Cos(angle) * radius), (float)(System.Math.Sin(angle) * radius));
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

public void StartSpawning()
    {
        if (debugForceSeed != 0)
        {
            GameSessionData.ResetSeedForNewMatch();
            GameSessionData.MatchSeed = debugForceSeed;
        }
        _rng = new System.Random(GameSessionData.GetOrCreateMatchSeed());
        _debugSpawnIndex = 0;

        spawning = true;
        spawnInterval = 0.30f;
        spawnTimer = 0f;
        currentTarget = null;
        milestoneCount = 0;
        gaugeTimer = gaugeSpawnInterval;

        chaosModeActive = false;
        baseBonusSpawnChance = bonusSpawnChance;

        ComputeSpawnBounds();
        SpawnInitialBurst();
    }

    private void ComputeSpawnBounds()
    {
        Camera cam = Camera.main;
        if (cam != null && cam.orthographic)
        {
            float h = cam.orthographicSize - spawnMargin;
            float w = h * cam.aspect;
            xMin = -w;
            xMax = w;
            yMin = -h;
            yMax = h;
        }
        else
        {
            xMin = -8f; xMax = 8f;
            yMin = -4f; yMax = 4f;
        }
    }

    private void SpawnInitialBurst()
    {
        for (int i = 0; i < 10; i++)
            SpawnOne(false, false);
        for (int i = 0; i < 3; i++)
            SpawnOne(true, false);
    }

    public void StopSpawning()
    {
        spawning = false;
    }

public void OnComboMilestone(int comboCount)
    {
        milestoneCount++;
        float reduction = speedUpPerMilestone + (milestoneCount * 0.015f);
        spawnInterval = Mathf.Max(spawnInterval - reduction, minSpawnInterval);
        Debug.Log($"[TapRush] Speed up! Combo x{comboCount}, milestone #{milestoneCount} -> interval={spawnInterval:F2}s");
    }

    public void ResetComboSpeed()
    {
        if (milestoneCount > 0)
        {
            spawnInterval = 0.30f;
            milestoneCount = 0;
            Debug.Log("[TapRush] Spawn speed reset to base (combo broken)");
        }
    }

public void EnterChaosMode()
    {
        if (chaosModeActive)
        {
            return;
        }

        chaosModeActive = true;

        // Push chaos much harder in the final 15 seconds.
        spawnInterval = Mathf.Max(spawnInterval / (chaosSpawnRateMultiplier * 2f), 0.08f);

        // Gold targets become more common.
        bonusSpawnChance = Mathf.Clamp01(baseBonusSpawnChance * chaosGoldChanceMultiplier);

        // Disable prep mechanics during final survival section.
        GaugeTarget[] gauges = FindObjectsByType<GaugeTarget>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < gauges.Length; i++)
        {
            Destroy(gauges[i].gameObject);
        }

        // Force multi-target pressure by disabling single-active gating in chaos.
        singleActiveTarget = false;

        Debug.Log($"[TapRush] CHAOS MODE active. Interval={spawnInterval:F2}s, GoldChance={bonusSpawnChance:F2}");
    }



void Update()
    {
        if (!spawning || GameManager.Instance == null || !GameManager.Instance.IsGameActive) return;

        // Gauge power-up timer (disabled once chaos mode starts).
        if (!chaosModeActive)
        {
            gaugeTimer -= Time.deltaTime;
            if (gaugeTimer <= 0f)
            {
                SpawnGauge();
                gaugeTimer = gaugeSpawnInterval;
            }
        }

        if (!chaosModeActive && singleActiveTarget && currentTarget != null) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnTarget();
            spawnTimer = spawnInterval;
        }
    }

private void SpawnTarget()
    {
        if (chaosModeActive)
        {
            int redWaves = RngRange(chaosMinRedPerWave, chaosMaxRedPerWave + 1);
            for (int i = 0; i < redWaves; i++)
            {
                if (RngValue() < chaosRedSpawnChance)
                {
                    SpawnOne(true, false);
                }
            }

            if (RngValue() < chaosWhiteSpawnChance)
            {
                SpawnOne(false, false);
            }

            if (RngValue() < bonusSpawnChance)
            {
                SpawnOne(false, true);
            }

            return;
        }

        // Spawn 2-3 normal targets per tick to keep the screen active
        int normalCount = RngRange(2, 4);
        for (int i = 0; i < normalCount; i++)
            SpawnOne(false, false);

        if (RngValue() < bonusSpawnChance)
            SpawnOne(false, true);

        int decoyCount = RngValue() < decoySpawnChance ? 1 : 0;
        if (milestoneCount >= 4 && RngValue() < 0.3f) decoyCount++;
        for (int i = 0; i < decoyCount; i++)
            SpawnOne(true, false);
    }

private void SpawnOne(bool asDecoy, bool asBonus)
    {
        Vector3 pos = new Vector3(
            RngRange(xMin, xMax),
            RngRange(yMin, yMax),
            0f
        );

        float rolledSize = 0f;
        Vector2 rolledDrift = Vector2.zero;

        GameObject spawned = Instantiate(targetPrefab, pos, Quaternion.identity);
        Target target = spawned.GetComponent<Target>();
        if (target == null) target = spawned.GetComponentInChildren<Target>();

        if (target == null)
        {
            return;
        }

        target.SetShrinkRateMultiplier(chaosModeActive ? chaosShrinkMultiplier : 1f);

        if (asDecoy)
        {
            rolledSize = RngRange(targetMinScale, targetMaxScale);
            target.SetSize(rolledSize);
            target.SetAsDecoy();
        }
        else if (asBonus)
        {
            target.SetAsBonus();
        }
        else
        {
            rolledSize = RngRange(targetMinScale, targetMaxScale);
            target.SetSize(rolledSize);
            float driftMax = driftSpeedMax + milestoneCount * 0.1f;
            rolledDrift = RngInsideUnitCircle().normalized * RngRange(driftSpeedMin, driftMax);
            target.SetDrift(rolledDrift);
            currentTarget = target;
        }

        if (debugLogSpawnSequence)
        {
            _debugSpawnIndex++;
            string kind = asDecoy ? "decoy" : asBonus ? "bonus" : "normal";
            Debug.Log($"[TapRush][ReplayTest] #{_debugSpawnIndex} kind={kind} pos=({pos.x:F4},{pos.y:F4}) size={rolledSize:F4} drift=({rolledDrift.x:F4},{rolledDrift.y:F4})");
        }
    }

private void SpawnGauge()
    {
        Vector3 pos = new Vector3(
            RngRange(xMin, xMax),
            RngRange(yMin, yMax),
            0f
        );

        GameObject spawned = Instantiate(targetPrefab, pos, Quaternion.identity);

        // Disable normal target behaviour, add gauge behaviour
        Target t = spawned.GetComponent<Target>();
        if (t != null) Destroy(t);

        spawned.AddComponent<GaugeTarget>();
        spawned.transform.localScale *= 1.5f;
        Debug.Log("[TapRush] Gauge power-up spawned!");
    }

}
