using System.Collections.Generic;
using UnityEngine;

public interface IPoolable
{
    void OnSpawned();    // Havuzdan alındığında
    void OnDespawned();  // Havuzda dinlenmeye alındığında
}

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    [System.Serializable]
    public class PoolConfig
    {
        public GameObject prefab;
        [Min(0)] public int initialSize = 10;
        public bool expandable = true;
    }

    [Header("Başlangıçta Hazırlanacak Havuzlar")]
    [SerializeField] private List<PoolConfig> initialPools = new();

    // prefab -> kuyruk
    private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new();
    // prefab -> container (hierarchy düzeni için)
    private readonly Dictionary<GameObject, Transform> _containers = new();
    // instance -> prefab (geri iade ederken hangi havuza gideceğini bilmek için)
    private readonly Dictionary<GameObject, GameObject> _instanceToPrefab = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // (İstersen) DontDestroyOnLoad(gameObject);

        // Inspector’da tanımlanan havuzları hazırla
        foreach (var cfg in initialPools)
        {
            if (cfg.prefab == null) continue;
            EnsurePool(cfg.prefab, cfg.initialSize, cfg.expandable);
        }
    }

    /// <summary> Belirtilen prefab için havuz varsa döner, yoksa oluşturur. </summary>
    public void EnsurePool(GameObject prefab, int initialSize = 0, bool expandable = true)
    {
        if (prefab == null) return;
        if (_pools.ContainsKey(prefab)) return;

        _pools[prefab] = new Queue<GameObject>();
        _containers[prefab] = CreateContainer(prefab.name);
        // expandable bilgisini saklamak için Container objesine bir işaret koyuyoruz
        var flag = _containers[prefab].gameObject.AddComponent<_PoolExpandFlag>();
        flag.expandable = expandable;

        if (initialSize > 0)
            Preload(prefab, initialSize);
    }

    /// <summary> Havuzu önceden doldur. </summary>
    public void Preload(GameObject prefab, int count)
    {
        EnsurePool(prefab);
        for (int i = 0; i < count; i++)
        {
            var go = CreateNewInstance(prefab);
            Despawn(go);
        }
    }

    /// <summary> Objeyi spawn eder. </summary>
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (prefab == null)
        {
            Debug.LogWarning("[Pool] Prefab null!");
            return null;
        }

        EnsurePool(prefab);

        var queue = _pools[prefab];
        GameObject go = null;

        if (queue.Count > 0)
        {
            go = queue.Dequeue();
        }
        else
        {
            var expandable = _containers[prefab].GetComponent<_PoolExpandFlag>().expandable;
            if (expandable)
                go = CreateNewInstance(prefab);
            else
                Debug.LogWarning($"[Pool] '{prefab.name}' havuzu boş ve genişletilemez!");
        }

        if (go == null) return null;

        go.transform.SetParent(parent, false);
        go.transform.SetPositionAndRotation(position, rotation);
        go.SetActive(true);

        // IPoolable callback
        if (go.TryGetComponent<IPoolable>(out var poolable))
            poolable.OnSpawned();

        return go;
    }

    /// <summary> Generic versiyon – direkt komponent döndürür. </summary>
    public T Spawn<T>(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
    {
        var go = Spawn(prefab, position, rotation, parent);
        return go ? go.GetComponent<T>() : null;
    }

    /// <summary> Objeyi havuza iade eder. </summary>
    public void Despawn(GameObject instance)
    {
        if (instance == null) return;

        if (!_instanceToPrefab.TryGetValue(instance, out var prefab) || !_pools.ContainsKey(prefab))
        {
            // Havuz dışı bir obje olabilir – güvenli kapatma
            instance.SetActive(false);
            return;
        }

        // IPoolable callback
        if (instance.TryGetComponent<IPoolable>(out var poolable))
            poolable.OnDespawned();

        instance.SetActive(false);
        instance.transform.SetParent(_containers[prefab], false);
        _pools[prefab].Enqueue(instance);
    }

    /// <summary> Belirli bir prefabın tüm aktif instancelarını iade et. </summary>
    public void DespawnAll(GameObject prefab)
    {
        if (!_containers.ContainsKey(prefab)) return;

        // Container altındaki aktif objeleri bul
        var root = _containers[prefab];
        // Hierarchy’de aktif değilse bile sahnede aktif olanları çekmek için:
        var all = root.GetComponentsInChildren<Transform>(true);
        foreach (var t in all)
        {
            if (t == root) continue;
            var go = t.gameObject;
            if (go.activeSelf)
                Despawn(go);
        }
    }

    private GameObject CreateNewInstance(GameObject prefab)
    {
        var go = Instantiate(prefab);
        _instanceToPrefab[go] = prefab;
        // Default olarak container altında dursun (Despawn edilene kadar SetActive(false) yapmayacağız)
        go.transform.SetParent(_containers[prefab], false);
        return go;
    }

    private Transform CreateContainer(string prefabName)
    {
        var go = new GameObject($"[Pool] {prefabName}");
        go.transform.SetParent(transform, false);
        return go.transform;
    }

    // Container üzerinde expand bilgisini taşımak için minik bir component
    private class _PoolExpandFlag : MonoBehaviour
    {
        public bool expandable = true;
    }
}
