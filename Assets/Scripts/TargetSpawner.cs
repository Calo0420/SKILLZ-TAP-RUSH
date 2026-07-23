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
    private Target currentTarget;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartSpawning()
    {
        spawning = true;
        spawnTimer = spawnInterval;
        currentTarget = null;
    }

    public void StopSpawning()
    {
        spawning = false;
    }

    void Update()
    {
        if (!spawning || !GameManager.Instance.IsGameActive) return;

        if (singleActiveTarget && currentTarget != null)
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

        GameObject spawned = Instantiate(targetPrefab, pos, Quaternion.identity);
        currentTarget = spawned.GetComponent<Target>();

        // Fallback in case the script sits on a child of the spawned prefab.
        if (currentTarget == null)
        {
            currentTarget = spawned.GetComponentInChildren<Target>();
        }
    }
}
