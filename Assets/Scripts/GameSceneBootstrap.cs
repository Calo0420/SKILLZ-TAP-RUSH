using UnityEngine;

public class GameSceneBootstrap : MonoBehaviour
{
    void Start()
    {
        SetupVisualEnvironment();
        EnsureWrongTapDetector();
        EnsureFloatingTextManager();
        EnsureReactiveBackground();
        EnsureComboStreakBar();

        // Small delay to ensure all Awake/Start calls finish on GameManager, TimerManager, etc.
        Invoke(nameof(BeginGame), 0.1f);
    }

    private void BeginGame()
    {
        // Phase 4.5: fresh deterministic seed per match (unless Skillz sets MatchSeed
        // externally right after this reset — see GameSessionData for the wiring point).
        GameSessionData.ResetSeedForNewMatch();

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

    private void EnsureFloatingTextManager()
    {
        FloatingTextManager existingManager = FindAnyObjectByType<FloatingTextManager>();
        if (existingManager != null) return;

        GameObject managerRoot = new GameObject("FloatingTextManager");
        managerRoot.AddComponent<FloatingTextManager>();
        Debug.Log("[TapRush] Auto-wired missing FloatingTextManager in GameScene.");
    }

    private void EnsureReactiveBackground()
    {
        ReactiveBackground existing = FindAnyObjectByType<ReactiveBackground>();
        if (existing != null) return;

        GameObject bgGO = new GameObject("ReactiveBackground");
        bgGO.AddComponent<ReactiveBackground>();
    }

    private void EnsureComboStreakBar()
    {
        ComboStreakBar existing = FindAnyObjectByType<ComboStreakBar>();
        if (existing != null) return;

        GameObject barGO = new GameObject("ComboStreakBar");
        barGO.AddComponent<ComboStreakBar>();
    }
}
