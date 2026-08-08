using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Lightweight object pool for target spawning. Eliminates GC spikes during chaos mode.
/// Usage: TargetPool.Instance.Get(prefab, position) / TargetPool.Instance.Return(obj)
/// </summary>
public class TargetPool : MonoBehaviour
{
    public static TargetPool Instance { get; private set; }

    private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();

    [SerializeField] private int preWarmCount = 20;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Pre-warm the pool with inactive instances of a prefab.
    /// </summary>
    public void PreWarm(GameObject prefab, int count)
    {
        string id = prefab.name;
        if (!pools.ContainsKey(id))
        {
            pools[id] = new Queue<GameObject>();
        }

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
            obj.SetActive(false);
            pools[id].Enqueue(obj);
        }
    }

    /// <summary>
    /// Get an object from the pool (or instantiate if empty).
    /// </summary>
    public GameObject Get(GameObject prefab, Vector3 position)
    {
        string id = prefab.name;

        if (!pools.ContainsKey(id))
        {
            pools[id] = new Queue<GameObject>();
        }

        GameObject obj;
        if (pools[id].Count > 0)
        {
            obj = pools[id].Dequeue();
            obj.transform.position = position;
            obj.transform.rotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(prefab, position, Quaternion.identity);
        }

        // Tag for return
        PoolTag tag = obj.GetComponent<PoolTag>();
        if (tag == null) tag = obj.AddComponent<PoolTag>();
        tag.PoolKey = id;

        return obj;
    }

    /// <summary>
    /// Return an object to the pool instead of destroying it.
    /// </summary>
    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(transform, false);

        PoolTag tag = obj.GetComponent<PoolTag>();
        if (tag != null && pools.ContainsKey(tag.PoolKey))
        {
            pools[tag.PoolKey].Enqueue(obj);
        }
        else
        {
            Destroy(obj);
        }
    }
}

/// <summary>
/// Attached to pooled objects so they can find their way back to the correct pool.
/// </summary>
public class PoolTag : MonoBehaviour
{
    public string PoolKey;
}
