using UnityEngine;

[CreateAssetMenu(fileName = "NewProjectileSkill", menuName = "Skills/Projectile Skill")]
public class ProjectileSkill : BaseSkill
{
    [Header("Projectile Settings")]
    [Tooltip("Kéo prefab của loại đạn bất kỳ (NormalBullet, Skill2Bullet,...) vào đây")]
    [SerializeField] private BaseBullet bulletPrefab;

    public override void Activate(GameObject caster, Transform firePoint)
    {
        if (firePoint == null)
        {
            Debug.LogError($"'FirePoint' chưa được gán cho PlayerSkillManager!");
            return;
        }

        var bullet = PoolManager.Instance.Spawn(bulletPrefab, firePoint.position, firePoint.rotation);

        if (bullet != null)
        {
            bullet.Launch(firePoint.right); 
            Debug.Log($"Player activated skill: {this.SkillName}!");
        }
    }
}