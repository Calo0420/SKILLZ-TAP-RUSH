using UnityEngine;

public class GameSceneBootstrap : MonoBehaviour
{
    void Start()
    {
        EnsureWrongTapDetector();

        GameManager.Instance?.StartGame();
        TimerManager.Instance?.StartTimer();
        TargetSpawner.Instance?.StartSpawning();
    }

    private void EnsureWrongTapDetector()
    {
        WrongTapDetector existingDetector = FindAnyObjectByType<WrongTapDetector>();
        if (existingDetector != null)
        {
            return;
        }

        GameObject detectorRoot = new GameObject("WrongTapDetector");
        detectorRoot.AddComponent<WrongTapDetector>();
        Debug.Log("[TapRush] Auto-wired missing WrongTapDetector in GameScene.");
    }
}
