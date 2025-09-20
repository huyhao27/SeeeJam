using UnityEngine;

public class Skill2Bullet : BaseBullet
{
    [SerializeField] private int damage = 8;
    [SerializeField] private float knockbackForce = 3f;

    protected override void OnHit(GameObject target)
    {
        if (target.TryGetComponent<BaseEnemy>(out var enemy))
        {
            Vector2 dir = (enemy.transform.position - transform.position).normalized;
            enemy.TakeDamage(damage, dir, knockbackForce);
            // enemy.ApplySlow(slowAmount); // Ví dụ
        }
    }
}