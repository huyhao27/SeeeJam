using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class BaseBullet : MonoBehaviour, IPoolable
{
    [Header("Base Bullet Stats")]
    [SerializeField] protected float speed = 15f;
    [SerializeField] protected float lifetime = 3f;
    [SerializeField] protected LayerMask hitLayers; 

    [Header("Debug")]
    [SerializeField] private bool debugCollisions = false; // Bật để in log các va chạm & kết quả kiểm tra layer

    protected Rigidbody2D rb;
    private float _lifetimeTimer;
    
    #region IPoolable Implementation
    private GameObject _originalPrefab;

    public virtual void OnPoolSpawn()
    {
        _lifetimeTimer = lifetime;
        gameObject.SetActive(true);
    }

    public virtual void OnPoolDespawn()
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0;
    }

    public void SetOriginalPrefab(GameObject prefab)
    {
        _originalPrefab = prefab;
    }

    public GameObject GetOriginalPrefab()
    {
        return _originalPrefab;
    }
    #endregion

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Update()
    {
        _lifetimeTimer -= Time.deltaTime;
        if (_lifetimeTimer <= 0)
        {
            PoolManager.Instance.Despawn(this);
        }
    }

    public virtual void Launch(Vector2 direction)
    {
        rb.velocity = direction.normalized * speed;
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        int otherLayer = other.gameObject.layer;
        bool pass = (hitLayers.value & (1 << otherLayer)) != 0;

        if (debugCollisions)
        {
            Debug.Log($"[BaseBullet Debug] {name} trigger -> {other.name} (layer={LayerMask.LayerToName(otherLayer)}) passMask={pass}");
        }

        if (pass)
        {
            OnHit(other.gameObject);
            PoolManager.Instance.Despawn(this);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Print Hit Layers")] 
    private void PrintHitLayers()
    {
        var layers = GetLayerNamesFromMask(hitLayers);
        Debug.Log($"[BaseBullet] {name} hitLayers = {string.Join(", ", layers)}");
    }

    private static System.Collections.Generic.List<string> GetLayerNamesFromMask(LayerMask mask)
    {
        var result = new System.Collections.Generic.List<string>();
        for (int i = 0; i < 32; i++)
        {
            if ((mask.value & (1 << i)) != 0)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName)) result.Add(layerName);
                else result.Add($"Layer{i}");
            }
        }
        return result;
    }
#endif

    protected abstract void OnHit(GameObject target);
}