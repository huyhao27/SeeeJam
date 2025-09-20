using UnityEngine;

public class NormalBullet : BaseBullet
{
    [SerializeField] private int damage = 10;
    [SerializeField] private float knockbackForce = 4f; // lực đẩy vào enemy

    protected override void OnHit(GameObject target)
    {
        if (target.TryGetComponent<BaseEnemy>(out var enemy))
        {
            // Hướng = từ bullet -> enemy (có thể dùng vận tốc của bullet để chính xác)
            Vector2 dir = (enemy.transform.position - transform.position).normalized;
            enemy.TakeDamage(damage, dir, knockbackForce);
        }
    }
}