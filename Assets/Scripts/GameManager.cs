using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private const string MainMenuSceneName = "MainMenu";
    private const string EndScreenSceneName = "EndScreen";

    public static GameManager Instance { get; private set; }

    [Header("Scoring")]
    [SerializeField] private int baseTapPoints = 100;
    [SerializeField] private int maxSpeedBonus = 50;
    [SerializeField] private int wrongTapPenalty = 75;
    [SerializeField] private bool debugComboMilestones = false;
    [SerializeField] private bool debugComboResets = false;

    public float ScoreMultiplier { get; private set; } = 1f;
    private Coroutine powerUpRoutine;
    private bool powerUpActive;

    
public int Score { get; private set; }
    public int ComboCount { get; private set; }
    public bool IsGameActive { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Skillz Unity docs: implement OnApplicationPause defensively to manage game state
    /// when the app is backgrounded (phone call, notification, home button, app switch).
    /// See: https://docs.skillz.com/docs/unity-script-execution/
    ///
    /// We freeze gameplay cleanly on pause and resume it on return — we do NOT call
    /// SkillzCrossPlatform.AbortMatch() here. Per Skillz's own best-practices doc
    /// (https://docs.skillz.com/docs/aborts), the SDK already auto-detects and reports
    /// backgrounded/terminated/timeout matches server-side with correct categorization.
    /// Calling AbortMatch() ourselves for routine backgrounding would be an unnecessary
    /// "Intentional" abort and inflate the game's true abort-rate stability metric.
    /// </summary>
    void OnApplicationPause(bool pauseStatus)
    {
        if (!IsGameActive) return;

        if (pauseStatus)
        {
            Debug.Log("[TapRush] App backgrounded mid-match — freezing gameplay state.");
            Time.timeScale = 0f;
        }
        else
        {
            Debug.Log("[TapRush] App resumed mid-match — resuming gameplay.");
            Time.timeScale = 1f;
        }
    }

public void StartGame()
    {
        Score = 0;
        ComboCount = 0;
        IsGameActive = true;
        ScoreMultiplier = 1f;
        powerUpActive = false;
        powerUpRoutine = null;

        AudioManager.Instance?.SetChaosMusicState(false);

        UIManager.Instance?.UpdateScore(Score);
        UIManager.Instance?.UpdateCombo(ComboCount);
    }

public void RegisterHit(float speedScore01, float sizeBonus01 = 0f)
    {
        if (!IsGameActive) return;

        ComboCount++;
        Debug.Log($"[TapRush] RegisterHit: ComboCount={ComboCount}");

        float comboMultiplier = 1f;
        if (ComboCount >= 10) comboMultiplier = 1.5f;
        else if (ComboCount >= 5) comboMultiplier = 1.2f;

        int speedBonus = Mathf.RoundToInt(Mathf.Clamp01(speedScore01) * maxSpeedBonus);
        int sizeBonus  = Mathf.RoundToInt(Mathf.Clamp01(sizeBonus01)  * maxSpeedBonus);
        int points = Mathf.RoundToInt((baseTapPoints + speedBonus + sizeBonus) * comboMultiplier * ScoreMultiplier);

        Score += points;
        UIManager.Instance?.UpdateScore(Score);
        UIManager.Instance?.UpdateCombo(ComboCount);
        AudioManager.Instance?.PlayCorrectTap();
        HapticManager.Instance?.PlayLight();

        if (ComboCount > 0 && ComboCount % 5 == 0)
        {
            if (debugComboMilestones)
                Debug.Log("[TapRush] Combo milestone reached at streak x" + ComboCount);

            AudioManager.Instance?.PlayComboMilestone();
            UIManager.Instance?.ShowComboMilestone(ComboCount);
            TargetSpawner.Instance?.OnComboMilestone(ComboCount);
            HapticManager.Instance?.PlayHeavy();
        }
    }

public void RegisterBonusHit(float speedScore01)
    {
        if (!IsGameActive) return;

        ComboCount++;
        int points = Mathf.RoundToInt(((baseTapPoints * 3f) + Mathf.Clamp01(speedScore01) * maxSpeedBonus) * ScoreMultiplier);
        Score += points;
        UIManager.Instance?.UpdateScore(Score);
        UIManager.Instance?.UpdateCombo(ComboCount);
        AudioManager.Instance?.PlayBonusHit();
        HapticManager.Instance?.PlayMedium();

        if (ComboCount > 0 && ComboCount % 5 == 0)
        {
            AudioManager.Instance?.PlayComboMilestone();
            UIManager.Instance?.ShowComboMilestone(ComboCount);
            TargetSpawner.Instance?.OnComboMilestone(ComboCount);
            HapticManager.Instance?.PlayHeavy();
        }

        Debug.Log($"[TapRush] BONUS HIT! +{points} pts");
    }

public void ActivatePowerUp(float duration)
    {
        if (powerUpRoutine != null)
            StopCoroutine(powerUpRoutine);
        powerUpActive = true;
        HapticManager.Instance?.PlayHeavy();
        powerUpRoutine = StartCoroutine(PowerUpRoutine(duration));
    }

    private void CancelPowerUp()
    {
        Debug.Log($"[TapRush] CancelPowerUp called. powerUpActive={powerUpActive}, ScoreMultiplier={ScoreMultiplier}");
        if (powerUpActive)
        {
            if (powerUpRoutine != null)
            {
                StopCoroutine(powerUpRoutine);
                powerUpRoutine = null;
            }
            powerUpActive = false;
            ScoreMultiplier = 1f;
            UIManager.Instance?.HidePowerUpDisplay();
            Debug.Log("[TapRush] Power-up CANCELLED!");
        }
    }

    private System.Collections.IEnumerator PowerUpRoutine(float duration)
    {
        ScoreMultiplier = 2f;
        Debug.Log($"[TapRush] Power-up activated! 2x for {duration}s");
        yield return new WaitForSeconds(duration);
        ScoreMultiplier = 1f;
        powerUpActive = false;
        powerUpRoutine = null;
        Debug.Log("[TapRush] Power-up ended.");
    }



    public void RegisterMiss()
    {
        if (!IsGameActive)
        {
            return;
        }

        // In multi-target mode, missed targets do NOT break combo or cancel power-up.
        // Only wrong taps (red balls / empty space) should penalize the player.
        // Just log it for analytics.
        Debug.Log("[TapRush] Target expired (no penalty in multi-target mode).");
    }

    public void RegisterWrongTap()
    {
        if (!IsGameActive)
        {
            return;
        }

        Score -= wrongTapPenalty;
        if (Score < 0)
        {
            Score = 0;
        }

        CancelPowerUp();

        if (debugComboResets && ComboCount > 1)
        {
            Debug.Log("[TapRush] Combo reset due to wrong tap at combo x" + ComboCount);
        }

        ResetCombo("wrong tap");
        UIManager.Instance?.UpdateScore(Score);
        UIManager.Instance?.PlayWrongTapCameraShake();
        AudioManager.Instance?.PlayWrongTap();
        HapticManager.Instance?.PlayWarning();
        ScreenFX.Instance?.OnWrongTap();
    }

public void EndGame()
    {
        Time.timeScale = 1f;
        IsGameActive = false;
        TargetSpawner.Instance?.StopSpawning();
        AudioManager.Instance?.SetChaosMusicState(false);

        GameSessionData.LastScore = Score;

        // Report score to Skillz if in a tournament match
        SkillzMatchController.Instance?.ReportScore(Score);

#if UNITY_WEBGL
        // Skip our own end screen entirely on web. Every web playthrough is a real Skillz
        // match (there's no reachable Practice mode on web — see MainMenu.cs), and this
        // screen's "PLAY AGAIN"/"MAIN MENU" buttons reload scenes directly, bypassing
        // Skillz's match flow entirely and leaving their backend/wrapper with no valid
        // match context — that's what was causing the stuck state Oscar hit. Skillz's own
        // results screen (triggered by ReportScore -> DisplayTournamentResultsWithScore
        // above) already shows the score and handles what comes next.
        return;
#endif

        if (UIManager.Instance != null && UIManager.Instance.ShowEndScreen(Score))
        {
            return;
        }

        // Load end screen when present; otherwise return to main menu to avoid flow breaks.
        if (CanLoadScene(EndScreenSceneName))
        {
            SceneManager.LoadScene(EndScreenSceneName);
            return;
        }

        Debug.LogWarning("[TapRush] EndScreen scene is not in Build Settings. Returning to MainMenu.");
        SceneManager.LoadScene(MainMenuSceneName);
    }

    private static bool CanLoadScene(string sceneName)
    {
        return Application.CanStreamedLevelBeLoaded(sceneName);
    }

    private void ResetCombo(string reason)
    {
        if (ComboCount == 0)
        {
            return;
        }

        Debug.Log($"[TapRush] Combo reset ({reason}): ComboCount {ComboCount} -> 0");

        ComboCount = 0;
        UIManager.Instance?.UpdateCombo(ComboCount);
        ScreenFX.Instance?.OnComboReset();

        // Reset spawn speed-ups earned from combo milestones
        TargetSpawner.Instance?.ResetComboSpeed();
    }
}
