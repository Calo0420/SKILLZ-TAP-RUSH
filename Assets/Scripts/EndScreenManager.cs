using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class EndScreenManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI finalScoreText;

    void Start()
    {
        if (finalScoreText != null)
        {
            finalScoreText.text = "Final Score: " + GameSessionData.LastScore;
        }
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
