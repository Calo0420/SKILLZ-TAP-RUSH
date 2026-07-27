using UnityEngine;

public class GameSceneBootstrap : MonoBehaviour
{
    void Start()
    {
        SetupVisualEnvironment();
        EnsureWrongTapDetector();

        // Small delay to ensure all Awake/Start calls finish on GameManager, TimerManager, etc.
        Invoke(nameof(BeginGame), 0.1f);
    }

    private void BeginGame()
    {
        GameManager.Instance?.StartGame();
        TimerManager.Instance?.StartTimer();
        TargetSpawner.Instance?.StartSpawning();
    }

    private void SetupVisualEnvironment()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.06f, 1f);
        }
    }

    private void EnsureWrongTapDetector()
    {
        WrongTapDetector existingDetector = FindAnyObjectByType<WrongTapDetector>();
        if (existingDetector != null) return;

        GameObject detectorRoot = new GameObject("WrongTapDetector");
        detectorRoot.AddComponent<WrongTapDetector>();
        Debug.Log("[TapRush] Auto-wired missing WrongTapDetector in GameScene.");
    }
}
