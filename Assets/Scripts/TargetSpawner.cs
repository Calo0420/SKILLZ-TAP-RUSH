using UnityEngine;

public class TargetSpawner : MonoBehaviour
{
    public static TargetSpawner Instance { get; private set; }

    [SerializeField] private GameObject targetPrefab;
    [SerializeField] private float spawnInterval = 1.0f;
    [SerializeField] private bool singleActiveTarget = true;

    // Spawn bounds (match your camera/canvas size — adjust as needed)
    [SerializeField] private float xMin = -4f, xMax = 4f;
    [SerializeField] private float yMin = -3f, yMax = 3f;

    private float spawnTimer;
    private bool spawning;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartSpawning()
    {
        spawning = true;
        spawnTimer = spawnInterval;
    }

    public void StopSpawning()
    {
        spawning = false;
    }

    void Update()
    {
        if (!spawning || !GameManager.Instance.IsGameActive) return;

        if (singleActiveTarget && FindAnyObjectByType<Target>() != null)
        {
            return;
        }

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnTarget();
            spawnTimer = spawnInterval;
        }
    }

    private void SpawnTarget()
    {
        Vector3 pos = new Vector3(
            Random.Range(xMin, xMax),
            Random.Range(yMin, yMax),
            0f
        );
        Instantiate(targetPrefab, pos, Quaternion.identity);
    }
}
