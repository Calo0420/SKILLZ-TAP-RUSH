using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Bridges Skillz SDK with Tap Rush gameplay.
/// Handles match lifecycle: seed extraction, score submission.
/// Lives on a DontDestroyOnLoad object — created in MainMenu or IntroScene.
///
/// Abort handling: we deliberately do NOT call SkillzCrossPlatform.AbortMatch() for
/// crashes/backgrounding/force-quit. Per Skillz's own best-practices doc
/// (https://docs.skillz.com/docs/aborts), the SDK auto-detects and categorizes those
/// as server-side aborts (Backgrounded/Terminated/Timeout/Crash) without any client
/// call needed. AbortMatch() is reserved for a future in-game "forfeit" button, which
/// per the same doc should submit a real (likely 0) score rather than an abort, since
/// an abort should never be a way to dodge a bad score. See GameManager.OnApplicationPause
/// for the mid-match backgrounding freeze/resume handling.
/// </summary>
public class SkillzMatchController : MonoBehaviour
{
    public static SkillzMatchController Instance { get; private set; }

    private bool isSkillzMatch;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // WebSDK-only: SkillzManager doesn't expose this as an Inspector UnityEvent like
        // onMatchWillBegin/onSkillzWillExit — it's a static C# event, subscribed in code.
        SkillzSDK.SkillzEvents.OnReceivedMemoryWarning += OnReceivedMemoryWarning;
    }

    void OnDestroy()
    {
        SkillzSDK.SkillzEvents.OnReceivedMemoryWarning -= OnReceivedMemoryWarning;
    }

    /// <summary>
    /// Called by MainMenu "Compete" button.
    /// </summary>
    public void LaunchSkillzTournament()
    {
        Debug.Log("[TapRush] Launching Skillz tournament UI...");
        LoadingScreen.Show("Connecting to Skillz...");
        SkillzCrossPlatform.LaunchSkillz();
    }

    /// <summary>
    /// Called by MainMenu "Practice" button — plays without Skillz.
    /// </summary>
    public void LaunchPractice()
    {
        isSkillzMatch = false;
        GameSessionData.ResetSeedForNewMatch();
        LoadingScreen.Show("Loading game...");
        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// Called from SkillzManager.OnMatchWillBegin event (wired in Inspector).
    /// Extracts the Skillz seed and stores it before GameScene loads.
    /// </summary>
    public void OnSkillzMatchWillBegin(SkillzSDK.Match matchInfo)
    {
        LoadingScreen.Show("Starting match...");
        isSkillzMatch = true;
        GameSessionData.ResetSeedForNewMatch();

        // Use Skillz Random to generate a deterministic seed both players share
        float skillzRandom = SkillzCrossPlatform.Random.Value();
        int seed = Mathf.RoundToInt(skillzRandom * int.MaxValue);
        GameSessionData.MatchSeed = seed;

        Debug.Log($"[TapRush] Skillz match starting. Seed={seed} (from Skillz Random={skillzRandom})");

        // SkillzManager will load GameScene via its gameScene field — we don't need to load it here
    }

    /// <summary>
    /// Called from SkillzManager.OnSkillzWillExit event.
    /// </summary>
    public void OnSkillzWillExit()
    {
        isSkillzMatch = false;
        Debug.Log("[TapRush] Skillz session ended, returning to menu.");
    }

    /// <summary>
    /// Subscribed in Awake() to SkillzSDK.SkillzEvents.OnReceivedMemoryWarning (WebSDK
    /// only). Fires when the browser signals it's approaching a memory limit.
    /// Deliberately lightweight: frees caches only, never touches gameplay state.
    /// </summary>
    public void OnReceivedMemoryWarning()
    {
        Debug.LogWarning("[TapRush] Browser memory warning received — releasing non-critical caches.");
        Resources.UnloadUnusedAssets();
    }

    /// <summary>
    /// Call this when the game ends to submit score to Skillz.
    /// </summary>
    public void ReportScore(int score)
    {
        if (!isSkillzMatch)
        {
            Debug.Log($"[TapRush] Practice mode — score {score} not submitted to Skillz.");
            return;
        }

        Debug.Log($"[TapRush] Submitting score {score} to Skillz...");
        LoadingScreen.Show("Submitting score...");
        SkillzCrossPlatform.SubmitScore(score, OnScoreSubmitSuccess, OnScoreSubmitFailure);
    }

    /// <summary>
    /// After score is submitted, show results in Skillz UI.
    /// </summary>
    public void ShowResults(int score)
    {
        if (!isSkillzMatch) return;

        Debug.Log($"[TapRush] Displaying tournament results for score {score}.");
        SkillzCrossPlatform.DisplayTournamentResultsWithScore(score);
    }

    public bool IsSkillzMatch()
    {
        return isSkillzMatch;
    }

    private void OnScoreSubmitSuccess()
    {
        Debug.Log("[TapRush] Score submitted successfully to Skillz!");
        LoadingScreen.Hide();
        ShowResults(GameSessionData.LastScore);
    }

    private void OnScoreSubmitFailure(string error)
    {
        Debug.LogError($"[TapRush] Score submission failed: {error}");
        LoadingScreen.Hide();
        // Still try to show results
        ShowResults(GameSessionData.LastScore);
    }
}
