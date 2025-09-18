using UnityEngine;

public class NormalBullet : BaseBullet
{
    protected override void OnHit(GameObject target)
    {
        Debug.Log($"Normal Bullet hit {target.name}");
        
        // TODO: Có thể thêm logic gây sát thương cơ bản ở đây
        // if (target.TryGetComponent<EnemyHealth>(out var enemyHealth))
        // {
        //     enemyHealth.TakeDamage(10);
        // }
    }

    protected override void ReturnToPool()
    {
        rb.velocity = Vector2.zero;
        ObjectPool<NormalBullet>.Instance.Return(this); 
    }
}