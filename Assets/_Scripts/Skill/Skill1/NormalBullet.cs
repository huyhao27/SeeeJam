using UnityEngine;

public class NormalBullet : BaseBullet
{
    [SerializeField] private int damage = 10;
    protected override void OnHit(GameObject target)
    {
        Debug.Log($"Normal Bullet hit {target.name}");
        if (target.TryGetComponent<BaseEnemy>(out var enemy))
        {
            enemy.TakeDamage(damage);
        }
    }

    protected override void ReturnToPool()
    {
        rb.velocity = Vector2.zero;
        ObjectPool<NormalBullet>.Instance.Return(this); 
    }
}