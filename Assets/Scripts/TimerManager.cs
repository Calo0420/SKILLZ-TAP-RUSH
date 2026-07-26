using UnityEngine;
using UnityEngine.Events;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance { get; private set; }

    [SerializeField] private float matchDuration = 60f;
    public UnityEvent OnTimerEnd;

    private float timeRemaining;
    private bool isRunning;

    [Header("Chaos Finale")]
    [SerializeField] private float chaosStartTimeRemaining = 15f;
    [SerializeField] private float chaosSlowMoDuration = 0.5f;
    [SerializeField] private float chaosSlowMoScale = 0.2f;

    private bool chaosTriggered;


    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

public void StartTimer()
    {
        Time.timeScale = 1f;
        timeRemaining = matchDuration;
        isRunning = true;
        chaosTriggered = false;
    }

void Update()
    {
        if (!isRunning) return;

        timeRemaining -= Time.deltaTime;
        UIManager.Instance?.UpdateTimer(Mathf.Max(timeRemaining, 0f));

        if (!chaosTriggered && timeRemaining <= chaosStartTimeRemaining)
        {
            chaosTriggered = true;
            StartCoroutine(TriggerChaosFinale());
        }

        if (timeRemaining <= 0f)
        {
            Time.timeScale = 1f;
            isRunning = false;
            OnTimerEnd?.Invoke();
            GameManager.Instance?.EndGame();
        }
    }

    public float GetTimeRemaining() => Mathf.Max(timeRemaining, 0f);


private System.Collections.IEnumerator TriggerChaosFinale()
    {
        UIManager.Instance?.ShowChaosAnnouncement();
        AudioManager.Instance?.PlayChaosStartCue();

        Time.timeScale = chaosSlowMoScale;
        yield return new WaitForSecondsRealtime(chaosSlowMoDuration);
        Time.timeScale = 1f;

        TargetSpawner.Instance?.EnterChaosMode();
        AudioManager.Instance?.SetChaosMusicState(true);
    }
}
