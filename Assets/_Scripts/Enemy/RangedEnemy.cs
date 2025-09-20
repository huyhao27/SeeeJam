using UnityEngine;

public class RangedEnemy : BaseEnemy
{
    [Header("Ranged Settings")] 
    [SerializeField] protected BaseBullet bulletPrefab;          // Prefab đạn (đã có trong PoolManager)
    [SerializeField] protected Transform firePoint;              // Điểm bắn (gắn trên prefab enemy)
    [SerializeField] protected float projectileSpread = 0f;      // Độ lệch góc ngẫu nhiên (độ)

    [Header("Attack Override")]
    [SerializeField] protected float rangedAttackCooldown = 1.5f; // Thay thế contact attack cooldown
    [SerializeField] protected int damageOverride = -1;           // Nếu >=0 sẽ set vào bullet nếu bullet có trường damage public

    protected override void DoAttack(Transform target)
    {
        if (target == null || bulletPrefab == null || firePoint == null)
        {
            base.DoAttack(target);
            return;
        }

        // Cooldown
        if (attackTimer > 0f) return;

        Vector2 baseDir = (target.position - firePoint.position).normalized;
        FireSingleBulletWithRandomSpread(baseDir);
        attackTimer = rangedAttackCooldown; 
    }

    /// <summary>
    /// Bắn 1 viên đạn theo baseDir (đã chuẩn hóa) + áp dụng random spread nếu có.
    /// Trả về hướng thực tế đã bắn (sau random) để subclass có thể dùng.
    /// </summary>
    protected virtual Vector2 FireSingleBulletWithRandomSpread(Vector2 baseDir)
    {
        if (bulletPrefab == null || firePoint == null) return Vector2.zero;

        Vector2 dir = baseDir;
        if (projectileSpread > 0f)
        {
            float half = projectileSpread * 0.5f;
            float rand = Random.Range(-half, half);
            dir = Quaternion.Euler(0,0, rand) * dir;
        }

        var bullet = PoolManager.Instance.Spawn(bulletPrefab, firePoint.position, Quaternion.LookRotation(Vector3.forward, dir));
        if (bullet != null)
        {
            bullet.Launch(dir);
            ApplyDamageOverrideIfAny(bullet);
        }
        return dir;
    }

    protected void ApplyDamageOverrideIfAny(BaseBullet bullet)
    {
        if (bullet == null || damageOverride < 0) return;
        var type = bullet.GetType();
        var dmgField = type.GetField("damage", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
        if (dmgField != null && dmgField.FieldType == typeof(int))
        {
            dmgField.SetValue(bullet, damageOverride);
        }
    }
}
