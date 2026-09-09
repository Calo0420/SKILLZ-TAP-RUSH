using UnityEngine;

public class GameSceneBootstrap : MonoBehaviour
{
    void Start()
    {
        ConfigurePerformanceAndInput();
        SetupVisualEnvironment();
        EnsureHapticManager();
        EnsureWrongTapDetector();
        EnsureFloatingTextManager();
        EnsureReactiveBackground();
        EnsureComboStreakBar();

        // Small delay to ensure all Awake/Start calls finish on GameManager, TimerManager, etc.
        Invoke(nameof(BeginGame), 0.1f);
    }

    private void ConfigurePerformanceAndInput()
    {
        // Unlock mobile framerate: default 60 FPS (or 120 on ProMotion devices)
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        // Ensure responsive multi-finger tapping across the screen
        Input.multiTouchEnabled = true;

        // Prevent screen dimming mid-match
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    private void EnsureHapticManager()
    {
        if (HapticManager.Instance != null) return;
        GameObject hapticGO = new GameObject("HapticManager");
        hapticGO.AddComponent<HapticManager>();
    }

    private void BeginGame()
    {
        // Hide loading screen if visible
        LoadingScreen.Hide();

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
