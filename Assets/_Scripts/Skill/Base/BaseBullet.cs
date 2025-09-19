using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class BaseBullet : MonoBehaviour, IPoolable
{
    [Header("Base Bullet Stats")]
    [SerializeField] protected float speed = 15f;
    [SerializeField] protected float lifetime = 3f;
    [SerializeField] protected LayerMask hitLayers; 

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
        if ((hitLayers.value & (1 << other.gameObject.layer)) > 0)
        {
            OnHit(other.gameObject);
            PoolManager.Instance.Despawn(this);
        }
    }

    protected abstract void OnHit(GameObject target);
}