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

        if (ComboCount > 0 && ComboCount % 5 == 0)
        {
            if (debugComboMilestones)
                Debug.Log("[TapRush] Combo milestone reached at streak x" + ComboCount);

            AudioManager.Instance?.PlayComboMilestone();
            UIManager.Instance?.ShowComboMilestone(ComboCount);
            TargetSpawner.Instance?.OnComboMilestone(ComboCount);
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

        if (ComboCount > 0 && ComboCount % 5 == 0)
        {
            AudioManager.Instance?.PlayComboMilestone();
            UIManager.Instance?.ShowComboMilestone(ComboCount);
            TargetSpawner.Instance?.OnComboMilestone(ComboCount);
        }

        Debug.Log($"[TapRush] BONUS HIT! +{points} pts");
    }

public void ActivatePowerUp(float duration)
    {
        if (powerUpRoutine != null)
            StopCoroutine(powerUpRoutine);
        powerUpActive = true;
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
        ScreenFX.Instance?.OnWrongTap();
    }

public void EndGame()
    {
        Time.timeScale = 1f;
        IsGameActive = false;
        TargetSpawner.Instance?.StopSpawning();
        AudioManager.Instance?.SetChaosMusicState(false);

        GameSessionData.LastScore = Score;

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
