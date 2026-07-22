using UnityEngine;
using UnityEngine.Events;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance { get; private set; }

    [SerializeField] private float matchDuration = 60f;
    public UnityEvent OnTimerEnd;

    private float timeRemaining;
    private bool isRunning;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartTimer()
    {
        timeRemaining = matchDuration;
        isRunning = true;
    }

    void Update()
    {
        if (!isRunning) return;
        timeRemaining -= Time.deltaTime;
        UIManager.Instance?.UpdateTimer(Mathf.Max(timeRemaining, 0f));
        if (timeRemaining <= 0f)
        {
            isRunning = false;
            OnTimerEnd?.Invoke();
            GameManager.Instance?.EndGame();
        }
    }

    public float GetTimeRemaining() => Mathf.Max(timeRemaining, 0f);
}
