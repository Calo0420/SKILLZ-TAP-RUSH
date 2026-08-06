using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Bridges Skillz SDK with Tap Rush gameplay.
/// Handles match lifecycle: seed extraction, score submission, abort.
/// Lives on a DontDestroyOnLoad object — created in MainMenu or IntroScene.
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
    }

    /// <summary>
    /// Called by MainMenu "Compete" button.
    /// </summary>
    public void LaunchSkillzTournament()
    {
        Debug.Log("[TapRush] Launching Skillz tournament UI...");
        SkillzCrossPlatform.LaunchSkillz();
    }

    /// <summary>
    /// Called by MainMenu "Practice" button — plays without Skillz.
    /// </summary>
    public void LaunchPractice()
    {
        isSkillzMatch = false;
        GameSessionData.ResetSeedForNewMatch();
        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// Called from SkillzManager.OnMatchWillBegin event (wired in Inspector).
    /// Extracts the Skillz seed and stores it before GameScene loads.
    /// </summary>
    public void OnSkillzMatchWillBegin(SkillzSDK.Match matchInfo)
    {
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
        ShowResults(GameSessionData.LastScore);
    }

    private void OnScoreSubmitFailure(string error)
    {
        Debug.LogError($"[TapRush] Score submission failed: {error}");
        // Still try to show results
        ShowResults(GameSessionData.LastScore);
    }
}
