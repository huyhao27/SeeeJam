using UnityEngine;

public class NormalBullet : BaseBullet
{
    [SerializeField] private int damage = 99999;

    protected override void OnHit(GameObject target)
    {
        if (target.TryGetComponent<BaseEnemy>(out var enemy))
        {
            enemy.TakeDamage(damage);
        }
    }
}