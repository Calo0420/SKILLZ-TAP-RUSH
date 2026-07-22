using UnityEngine;

public class GameSceneBootstrap : MonoBehaviour
{
    void Start()
    {
        GameManager.Instance?.StartGame();
        TimerManager.Instance?.StartTimer();
        TargetSpawner.Instance?.StartSpawning();
    }
}
