using UnityEngine;

public class ArcBullet : BaseBullet
{
    [Header("Arc Settings")]
    [Tooltip("Sát thương gây ra bởi vụ nổ (dành cho Buff 3).")]
    [SerializeField] private int explosionDamage = 99999;

    [Header("Growth Settings")]
    [Tooltip("Kích thước tối đa mà vòng cung sẽ đạt được vào cuối đời (gấp bao nhiêu lần kích thước ban đầu). Giá trị 1 = không thay đổi.")]
    [SerializeField] private float endScaleMultiplier = 2.0f;

    [Tooltip("Đường cong tăng trưởng. Trục X (0->1) là % vòng đời, Trục Y (0->1) là % mức độ tăng trưởng từ kích thước ban đầu đến kích thước tối đa.")]
    [SerializeField] private AnimationCurve growthCurve = AnimationCurve.Linear(0, 0, 1, 1);

    private bool _explodesOnDeath = false;
    private float _explosionRadius = 0f;
    private Vector3 _initialScale;
    private Vector3 _currentInitialScale; 

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
        _currentInitialScale = _initialScale; 
        
        this.CanPierce = true;
    }

    protected override void Update()
    {
        base.Update();

        if (lifetime > 0)
        {
            float lifeProgress = 1f - (_lifetimeTimer / lifetime);
            float curveValue = growthCurve.Evaluate(lifeProgress);
            float currentMultiplier = Mathf.Lerp(1f, endScaleMultiplier, curveValue);

            transform.localScale = _currentInitialScale * currentMultiplier;
        }

        if (_lifetimeTimer <= 0 && _explodesOnDeath)
        {
            Explode();
        }
    }

    public void SetInitialScale(float multiplier)
    {
        _currentInitialScale = _initialScale * multiplier;
        transform.localScale = _currentInitialScale;
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
            if (target.TryGetComponent<IPoolable>(out var poolableProjectile))
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
#if UNITY_EDITOR
        if (_explodesOnDeath && _explosionRadius > 0f)
        {
            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, _explosionRadius);
        }
#endif
    }
}