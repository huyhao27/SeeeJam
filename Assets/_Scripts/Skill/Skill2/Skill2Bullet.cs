using UnityEngine;

public class Skill2Bullet : BaseBullet
{
    [SerializeField] private int damage = 8;

    protected override void OnHit(GameObject target)
    {
        if (target.TryGetComponent<BaseEnemy>(out var enemy))
        {
            enemy.TakeDamage(damage);
            // enemy.ApplySlow(slowAmount); // Ví dụ
        }
    }
}