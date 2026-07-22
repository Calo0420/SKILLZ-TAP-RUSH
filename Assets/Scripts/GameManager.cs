using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int Score { get; private set; }
    public bool IsGameActive { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartGame()
    {
        Score = 0;
        IsGameActive = true;
    }

    public void AddScore(int points)
    {
        if (!IsGameActive) return;
        Score += points;
        UIManager.Instance?.UpdateScore(Score);
    }

    public void EndGame()
    {
        IsGameActive = false;
        SceneManager.LoadScene("EndScreen");
    }
}
