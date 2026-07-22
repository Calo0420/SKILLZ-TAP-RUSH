using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scoring")]
    [SerializeField] private int baseTapPoints = 100;
    [SerializeField] private int maxSpeedBonus = 50;
    [SerializeField] private int wrongTapPenalty = 75;

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

    void Update()
    {
        if (!IsGameActive)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            CheckWrongTap();
        }
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
            AudioManager.Instance?.PlayComboMilestone();
        }
    }

    public void RegisterMiss()
    {
        if (!IsGameActive)
        {
            return;
        }

        ResetCombo();
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

        ResetCombo();
        UIManager.Instance?.UpdateScore(Score);
        AudioManager.Instance?.PlayWrongTap();
    }

    public void EndGame()
    {
        IsGameActive = false;
        TargetSpawner.Instance?.StopSpawning();

        GameSessionData.LastScore = Score;
        SceneManager.LoadScene("EndScreen");
    }

    private void ResetCombo()
    {
        if (ComboCount == 0)
        {
            return;
        }

        ComboCount = 0;
        UIManager.Instance?.UpdateCombo(ComboCount);
    }

    private void CheckWrongTap()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        Vector3 worldPoint = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 worldPoint2D = new Vector2(worldPoint.x, worldPoint.y);
        Collider2D hit2D = Physics2D.OverlapPoint(worldPoint2D);

        if (hit2D != null && hit2D.GetComponent<Target>() != null)
        {
            return;
        }

        RegisterWrongTap();
    }
}
