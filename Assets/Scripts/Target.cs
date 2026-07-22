using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private int pointValue = 100;

    private bool tapped;

    void Start()
    {
        Invoke(nameof(Miss), lifetime);
    }

    void OnMouseDown()
    {
        if (tapped || !GameManager.Instance.IsGameActive) return;
        Tap();
    }

    private void Tap()
    {
        tapped = true;
        CancelInvoke(nameof(Miss));
        GameManager.Instance?.AddScore(pointValue);
        Destroy(gameObject);
    }

    private void Miss()
    {
        if (tapped) return;
        Destroy(gameObject);
    }
}
