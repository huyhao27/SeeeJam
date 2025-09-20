using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NewProjectileSkill", menuName = "Skills/Projectile Skill")]
public class ProjectileSkill : BaseSkill
{
    [Header("Projectile Settings")]
    [Tooltip("Kéo prefab của loại đạn bất kỳ (NormalBullet, Skill2Bullet,...) vào đây")]
    [SerializeField] private BaseBullet bulletPrefab;

    [Header("Buff Settings")]
    [Tooltip("Góc lệch cho mỗi tia đạn khi có buff 2 (ví dụ: 15 độ)")]
    [SerializeField] private float multiShotAngle = 15f;
    [Tooltip("Thời gian chờ giữa 2 lượt bắn khi có buff 3 (ví dụ: 0.15 giây)")]
    [SerializeField] private float doubleTapDelay = 0.15f;

    // Thay đổi chữ ký của hàm Activate để nhận vào một MonoBehaviour
    // Điều này cần thiết để có thể gọi Coroutine (cho buff 3)
    public override void Activate(MonoBehaviour caster, Transform firePoint)
    {
        if (firePoint == null)
        {
            Debug.LogError($"'FirePoint' chưa được gán cho PlayerSkillManager!");
            return;
        }

        // Lấy cấp độ hiện tại của kỹ năng này
        int level = SkillUpgradeManager.Instance.GetSkillLevel(this);

        // Kiểm tra các buff dựa trên cấp độ
        bool pierce = level >= 1;
        bool multiShot = level >= 2;
        bool doubleTap = level >= 3;

        // Xử lý logic bắn
        if (doubleTap)
        {
            // Nếu có buff 3, gọi Coroutine để bắn 2 lượt
            caster.StartCoroutine(DoubleTapRoutine(firePoint, pierce, multiShot));
        }
        else
        {
            // Nếu không, bắn 1 lượt như bình thường
            FireProjectiles(firePoint, pierce, multiShot);
        }
    }

    private void FireProjectiles(Transform firePoint, bool canPierce, bool isMultiShot)
    {
        if (isMultiShot)
        {
            // Buff 2: Bắn 3 tia
            // Tia giữa
            SpawnBullet(firePoint.position, firePoint.rotation, canPierce);
            // Tia trái
            Quaternion leftRotation = firePoint.rotation * Quaternion.Euler(0, 0, multiShotAngle);
            SpawnBullet(firePoint.position, leftRotation, canPierce);
            // Tia phải
            Quaternion rightRotation = firePoint.rotation * Quaternion.Euler(0, 0, -multiShotAngle);
            SpawnBullet(firePoint.position, rightRotation, canPierce);
        }
        else
        {
            // Mặc định: Bắn 1 tia
            SpawnBullet(firePoint.position, firePoint.rotation, canPierce);
        }
    }

    private void SpawnBullet(Vector3 position, Quaternion rotation, bool canPierce)
    {
        // Sử dụng PoolManager để tạo đạn
        var bullet = PoolManager.Instance.Spawn(bulletPrefab, position, rotation);

        if (bullet != null)
        {
            // Thiết lập thuộc tính xuyên thấu (buff 1)
            bullet.CanPierce = canPierce;
            // Bắn đạn
            bullet.Launch(rotation * Vector2.right); // Dùng rotation * Vector2.right để có hướng chính xác
        }
    }

    private IEnumerator DoubleTapRoutine(Transform firePoint, bool canPierce, bool isMultiShot)
    {
        // Lượt bắn thứ nhất
        FireProjectiles(firePoint, canPierce, isMultiShot);
        
        // Chờ một khoảng thời gian ngắn
        yield return new WaitForSeconds(doubleTapDelay);

        // Lượt bắn thứ hai
        FireProjectiles(firePoint, canPierce, isMultiShot);
    }
}