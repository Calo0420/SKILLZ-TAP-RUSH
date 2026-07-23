using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scoring")]
    [SerializeField] private int baseTapPoints = 100;
    [SerializeField] private int maxSpeedBonus = 50;
    [SerializeField] private int wrongTapPenalty = 75;
    [SerializeField] private bool debugComboMilestones = false;
    [SerializeField] private bool debugComboResets = false;
    [SerializeField] private bool verboseDebugLogs = false;

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

        UIManager.Instance?.UpdateScore(Score);
        UIManager.Instance?.UpdateCombo(ComboCount);
    }

    public void RegisterHit(float speedScore01)
    {
        if (!IsGameActive)
        {
            return;
        }

        ComboCount++;
        if (verboseDebugLogs)
        {
            Debug.Log($"[TapRush] RegisterHit: ComboCount={ComboCount}");
        }

        float comboMultiplier = 1f;
        if (ComboCount >= 10)
        {
            comboMultiplier = 1.5f;
        }
        else if (ComboCount >= 5)
        {
            comboMultiplier = 1.2f;
        }

        int speedBonus = Mathf.RoundToInt(Mathf.Clamp01(speedScore01) * maxSpeedBonus);
        int points = Mathf.RoundToInt((baseTapPoints + speedBonus) * comboMultiplier);

        Score += points;
        UIManager.Instance?.UpdateScore(Score);
        UIManager.Instance?.UpdateCombo(ComboCount);

        AudioManager.Instance?.PlayCorrectTap();
        if (ComboCount > 0 && ComboCount % 5 == 0)
        {
            if (debugComboMilestones)
            {
                Debug.Log("[TapRush] Combo milestone reached at streak x" + ComboCount);
            }

            AudioManager.Instance?.PlayComboMilestone();
            UIManager.Instance?.ShowComboMilestone(ComboCount);
        }
    }

    public void RegisterMiss()
    {
        if (!IsGameActive)
        {
            return;
        }

        if (debugComboResets && ComboCount > 1)
        {
            Debug.Log("[TapRush] Combo reset due to missed target at combo x" + ComboCount);
        }

        ResetCombo("missed target");
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

        if (debugComboResets && ComboCount > 1)
        {
            Debug.Log("[TapRush] Combo reset due to wrong tap at combo x" + ComboCount);
        }

        ResetCombo("wrong tap");
        UIManager.Instance?.UpdateScore(Score);
        AudioManager.Instance?.PlayWrongTap();
    }

    public void EndGame()
    {
        IsGameActive = false;
        TargetSpawner.Instance?.StopSpawning();

        GameSessionData.LastScore = Score;

        if (UIManager.Instance == null || !UIManager.Instance.ShowEndScreen(Score))
        {
            Debug.LogWarning("[TapRush] Runtime end screen could not be displayed (UIManager missing or failed).");
        }
    }

    private void ResetCombo(string reason)
    {
        if (ComboCount == 0)
        {
            return;
        }

        if (verboseDebugLogs)
        {
            Debug.Log($"[TapRush] Combo reset ({reason}): ComboCount {ComboCount} -> 0");
        }

        ComboCount = 0;
        UIManager.Instance?.UpdateCombo(ComboCount);
    }
}
