using UnityEngine;

/// Attach this to a single GameObject in GameScene.
/// It wires up and starts all systems when the scene loads.
public class GameSceneBootstrap : MonoBehaviour
{
    void Start()
    {
        GameManager.Instance?.StartGame();
        TimerManager.Instance?.StartTimer();
        TargetSpawner.Instance?.StartSpawning();
        UIManager.Instance?.UpdateScore(0);
    }
}
