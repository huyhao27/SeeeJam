using UnityEngine;

public class ArcBullet : BaseBullet
{
    [Header("Arc Settings")]
    [Tooltip("Sát thương gây ra bởi vụ nổ (dành cho Buff 3).")]
    [SerializeField] private int explosionDamage = 99999; 

    private bool _explodesOnDeath = false;
    private float _explosionRadius = 0f;
    private Vector3 _initialScale; 

    protected override void Awake()
    {
        base.Awake();
        _initialScale = transform.localScale;
    }

    public override void OnPoolSpawn()
    {
        base.OnPoolSpawn();
        _explodesOnDeath = false;
        _explosionRadius = 0f;
        transform.localScale = _initialScale; 
        
        this.CanPierce = true;
    }
    
    protected override void Update()
    {
        base.Update(); 

        if (_lifetimeTimer <= 0 && _explodesOnDeath)
        {
            Explode();
        }
    }


    public void SetupExplosion(float radius, int damage)
    {
        _explodesOnDeath = true;
        _explosionRadius = radius;
        explosionDamage = damage;
    }

    protected override void OnHit(GameObject target)
    {
        if (target.layer == LayerMask.NameToLayer("EnemyProjectile"))
        {
            if(target.TryGetComponent<IPoolable>(out var poolableProjectile))
            {
                PoolManager.Instance.Despawn(poolableProjectile);
            }
            else
            {
                Destroy(target);
            }
            return; 
        }

    }

    private void Explode()
    {
        if (_explosionRadius <= 0) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _explosionRadius, hitLayers);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<BaseEnemy>(out var enemy))
            {
                enemy.TakeDamage(explosionDamage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_explodesOnDeath)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _explosionRadius);
        }
    }
}