using UnityEngine;

public class Skill2Bullet : BaseBullet
{
    [SerializeField] private int damage = 8;
    // [Header("Frost Specific")]
    // [SerializeField] private float slowDuration = 3f;
    // [SerializeField] private float slowAmount = 0.5f; 

    protected override void OnHit(GameObject target)
    {
        if (target.TryGetComponent<BaseEnemy>(out var enemy))
        {
            enemy.TakeDamage(damage);
        }
    }
    
    protected override void ReturnToPool()
    {
        rb.velocity = Vector2.zero;
        ObjectPool<Skill2Bullet>.Instance.Return(this); 
    }
}