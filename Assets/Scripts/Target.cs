using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField] private float lifetime = 2f;

    private bool tapped;
    private float spawnTime;

    void Start()
    {
        spawnTime = Time.time;
        Invoke(nameof(Miss), lifetime);
    }

    void OnMouseDown()
    {
        if (tapped || GameManager.Instance == null || !GameManager.Instance.IsGameActive)
        {
            return;
        }

        Tap();
    }

    private void Tap()
    {
        tapped = true;
        CancelInvoke(nameof(Miss));

        float elapsed = Time.time - spawnTime;
        float speedScore01 = 1f - Mathf.Clamp01(elapsed / lifetime);
        GameManager.Instance?.RegisterHit(speedScore01);

        Destroy(gameObject);
    }

    private void Miss()
    {
        if (tapped)
        {
            return;
        }

        GameManager.Instance?.RegisterMiss();
        Destroy(gameObject);
    }
}
