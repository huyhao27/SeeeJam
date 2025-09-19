using UnityEngine;

[CreateAssetMenu(fileName = "NewProjectileSkill", menuName = "Skills/Projectile Skill")]
public class ProjectileSkill : BaseSkill
{
    [Header("Projectile Settings")]
    [Tooltip("Kéo prefab của loại đạn bất kỳ (NormalBullet, Skill2Bullet,...) vào đây")]
    [SerializeField] private BaseBullet bulletPrefab;

    public override void Activate(GameObject caster)
    {
        Transform firePoint = caster.transform.Find("FirePoint");
        if (firePoint == null)
        {
            Debug.LogError($"'{caster.name}' không có 'FirePoint'!");
            return;
        }

        var bullet = PoolManager.Instance.Spawn(bulletPrefab, firePoint.position, caster.transform.rotation);

        if (bullet != null)
        {
            bullet.Launch(caster.transform.up);
            Debug.Log($"Player activated skill: {this.SkillName}!");
        }
    }
}