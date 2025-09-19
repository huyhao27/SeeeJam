using UnityEngine;

public class RangedEnemy : BaseEnemy
{
    [Header("Ranged Settings")] 
    [SerializeField] private BaseBullet bulletPrefab;          // Prefab đạn (đã có trong PoolManager)
    [SerializeField] private Transform firePoint;              // Điểm bắn (gắn trên prefab enemy)
    [SerializeField] private float projectileSpread = 0f;      // Độ lệch góc ngẫu nhiên (độ)

    [Header("Attack Override")]
    [SerializeField] private float rangedAttackCooldown = 1.5f; // Thay thế contact attack cooldown
    [SerializeField] private int damageOverride = -1;           // Nếu >=0 sẽ set vào bullet nếu bullet có trường damage public

    protected override void DoAttack(Transform target)
    {
        if (target == null || bulletPrefab == null || firePoint == null)
        {
            base.DoAttack(target);
            return;
        }

        // Cooldown
        if (attackTimer > 0f) return;

        // Xoay hướng tới target
        Vector2 dir = (target.position - firePoint.position).normalized;

        // Spread
        if (projectileSpread > 0f)
        {
            float half = projectileSpread * 0.5f;
            float rand = Random.Range(-half, half);
            dir = Quaternion.Euler(0,0, rand) * dir;
        }

        // Spawn bullet qua PoolManager
        var bullet = PoolManager.Instance.Spawn(bulletPrefab, firePoint.position, Quaternion.LookRotation(Vector3.forward, dir));
        if (bullet != null)
        {
            bullet.Launch(dir);

            if (damageOverride >= 0)
            {
                var type = bullet.GetType();
                var dmgField = type.GetField("damage", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                if (dmgField != null && dmgField.FieldType == typeof(int))
                {
                    dmgField.SetValue(bullet, damageOverride);
                }
            }
        }

        attackTimer = rangedAttackCooldown; 
    }
}
