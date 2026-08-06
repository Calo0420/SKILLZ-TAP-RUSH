using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class EndScreenManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private float countUpDuration = 1.8f;

    void Start()
    {
        if (finalScoreText != null)
        {
            StartCoroutine(CountUpScore());
        }
    }

    private IEnumerator CountUpScore()
    {
        int targetScore = GameSessionData.LastScore;
        float elapsed = 0f;
        int lastDisplayed = -1;

        while (elapsed < countUpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / countUpDuration;
            // Ease-out for dramatic slowdown at the end
            float eased = 1f - (1f - t) * (1f - t) * (1f - t);
            int displayed = Mathf.RoundToInt(Mathf.Lerp(0f, targetScore, eased));

            if (displayed != lastDisplayed)
            {
                finalScoreText.text = "Final Score: " + displayed;
                lastDisplayed = displayed;

                // Subtle scale pulse as numbers tick
                float pulse = 1f + Mathf.Sin(t * Mathf.PI * 8f) * 0.03f * (1f - t);
                finalScoreText.transform.localScale = Vector3.one * pulse;
            }
            yield return null;
        }

        finalScoreText.text = "Final Score: " + targetScore;
        finalScoreText.transform.localScale = Vector3.one;

        // Final punch
        yield return PunchScale(finalScoreText.transform, 1.15f, 0.2f);
    }

    private IEnumerator PunchScale(Transform target, float punchSize, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float s = t < 0.3f ? Mathf.Lerp(1f, punchSize, t / 0.3f) : Mathf.Lerp(punchSize, 1f, (t - 0.3f) / 0.7f);
            target.localScale = Vector3.one * s;
            yield return null;
        }
        target.localScale = Vector3.one;
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
