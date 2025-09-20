using UnityEngine;

/// <summary>
/// Enemy bắn nhiều viên (mặc định 3) dạng chùm trải đều theo một tổng góc (fan spread).
/// Có thể cấu hình số viên, tổng góc spread, hoặc dùng layer random jitter như RangedEnemy gốc.
/// </summary>
public class SpreadRangedEnemy : RangedEnemy
{
    [Header("Spread Ranged Settings")]
    [Tooltip("Số viên đạn trong một loạt.")]
    [SerializeField] private int bulletCount = 3;
    [Tooltip("Tổng độ rộng (độ) của chùm. Ví dụ 30 nghĩa là viên ngoài cùng lệch +/-15 độ.")]
    [SerializeField] private float totalSpreadAngle = 30f;
    [Tooltip("Có thêm random nhỏ cho từng viên ngoài fan cố định.")]
    [SerializeField] private float perBulletRandomJitter = 0f;

    protected override void DoAttack(Transform target)
    {
        if (target == null || bulletCount <= 0)
        {
            base.DoAttack(target);
            return;
        }

        if (attackTimer > 0f) return; // cooldown
        if (firePoint == null || bulletPrefab == null)
        {
            base.DoAttack(target);
            return;
        }

        Vector2 baseDir = (target.position - firePoint.position).normalized;

        if (bulletCount == 1 || totalSpreadAngle <= 0f)
        {
            FireSingleBulletWithRandomSpread(baseDir); // dùng logic random spread gốc (projectileSpread)
        }
        else
        {
            // Fan spread: chia (bulletCount-1) khoảng giữa -half đến +half
            float half = totalSpreadAngle * 0.5f;
            for (int i = 0; i < bulletCount; i++)
            {
                float t = bulletCount == 1 ? 0f : (float)i / (bulletCount - 1); // 0..1
                float angle = Mathf.Lerp(-half, half, t);
                // jitter nhỏ riêng cho viên này (tuỳ chọn)
                if (perBulletRandomJitter > 0f)
                {
                    angle += Random.Range(-perBulletRandomJitter * 0.5f, perBulletRandomJitter * 0.5f);
                }
                Vector2 dir = Quaternion.Euler(0, 0, angle) * baseDir;
                // Bỏ qua random spread gốc để fan chính xác -> đặt tạm projectileSpread = 0 khi gọi
                float originalSpread = projectileSpread;
                projectileSpread = 0f;
                var firedDir = FireSingleBulletWithRandomSpread(dir.normalized);
                projectileSpread = originalSpread;
            }
        }

        attackTimer = rangedAttackCooldown; // cooldown custom của ranged
    }
}
