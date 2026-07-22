using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI comboText;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    public void UpdateTimer(float time)
    {
        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(time).ToString();
        }
    }

    public void UpdateCombo(int combo)
    {
        if (comboText == null)
        {
            return;
        }

        if (combo <= 1)
        {
            comboText.text = string.Empty;
        }
        else
        {
            comboText.text = "Combo x" + combo;
        }
    }
}
