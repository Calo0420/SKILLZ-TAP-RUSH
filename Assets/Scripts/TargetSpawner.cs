using UnityEngine;

public class TargetSpawner : MonoBehaviour
{
    public static TargetSpawner Instance { get; private set; }

    [SerializeField] private GameObject targetPrefab;
    [SerializeField] private float spawnInterval = 1.0f;
    [SerializeField] private bool singleActiveTarget = true;

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
    [SerializeField] private float chaosRedSpawnChance = 0.92f;
    [SerializeField] private int chaosMinRedPerWave = 4;
    [SerializeField] private int chaosMaxRedPerWave = 7;
    [SerializeField] private float chaosWhiteSpawnChance = 0.32f;


    private bool chaosModeActive;
    private float baseBonusSpawnChance;



    [SerializeField] private float speedUpPerMilestone = 0.12f;



    // Spawn bounds (match your camera/canvas size — adjust as needed)
    [SerializeField] private float xMin = -4f, xMax = 4f;
    [SerializeField] private float yMin = -3f, yMax = 3f;

    private float spawnTimer;
    private int milestoneCount;

    private bool spawning;
    private Target currentTarget;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

public void StartSpawning()
    {
        spawning = true;
        spawnInterval = 0.75f;
        spawnTimer = spawnInterval;
        currentTarget = null;
        milestoneCount = 0;
        gaugeTimer = gaugeSpawnInterval;

        chaosModeActive = false;
        baseBonusSpawnChance = bonusSpawnChance;
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
        GaugeTarget[] gauges = FindObjectsByType<GaugeTarget>(FindObjectsSortMode.None);
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
        if (!spawning || !GameManager.Instance.IsGameActive) return;

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
            int redWaves = Random.Range(chaosMinRedPerWave, chaosMaxRedPerWave + 1);
            for (int i = 0; i < redWaves; i++)
            {
                if (Random.value < chaosRedSpawnChance)
                {
                    SpawnOne(true, false);
                }
            }

            if (Random.value < chaosWhiteSpawnChance)
            {
                SpawnOne(false, false);
            }

            if (Random.value < bonusSpawnChance)
            {
                SpawnOne(false, true);
            }

            return;
        }

        SpawnOne(false, false);

        if (Random.value < bonusSpawnChance)
            SpawnOne(false, true);

        int decoyCount = Random.value < decoySpawnChance ? 1 : 0;
        if (milestoneCount >= 4 && Random.value < 0.3f) decoyCount++;
        for (int i = 0; i < decoyCount; i++)
            SpawnOne(true, false);
    }

private void SpawnOne(bool asDecoy, bool asBonus)
    {
        Vector3 pos = new Vector3(
            Random.Range(xMin, xMax),
            Random.Range(yMin, yMax),
            0f
        );

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
            target.SetRandomSize(targetMinScale, targetMaxScale);
            target.SetAsDecoy();
        }
        else if (asBonus)
        {
            target.SetAsBonus();
        }
        else
        {
            target.SetRandomSize(targetMinScale, targetMaxScale);
            float driftMax = driftSpeedMax + milestoneCount * 0.1f;
            Vector2 drift = Random.insideUnitCircle.normalized * Random.Range(driftSpeedMin, driftMax);
            target.SetDrift(drift);
            currentTarget = target;
        }
    }

private void SpawnGauge()
    {
        Vector3 pos = new Vector3(
            Random.Range(xMin, xMax),
            Random.Range(yMin, yMax),
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
