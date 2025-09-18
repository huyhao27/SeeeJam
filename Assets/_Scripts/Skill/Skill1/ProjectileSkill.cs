using UnityEngine;

[CreateAssetMenu(fileName = "NewProjectileSkill", menuName = "Skills/Projectile Skill")]
public class ProjectileSkill : BaseSkill
{
    [Header("Projectile Settings")]

    [SerializeField] private BaseBullet bulletPrefab;

    public override void Activate(GameObject caster)
    {
        Transform firePoint = caster.transform.Find("FirePoint");
        if (firePoint == null)
        {
            Debug.LogError($"'{caster.name}' does not have a child object named 'FirePoint'!");
            return;
        }
        
        if (bulletPrefab is NormalBullet)
        {
            NormalBullet bullet = ObjectPool<NormalBullet>.Instance.Get();
            bullet.transform.position = firePoint.position;
            bullet.Launch(caster.transform.up);
        }
        else if (bulletPrefab is Skill2Bullet)
        {
            Skill2Bullet bullet = ObjectPool<Skill2Bullet>.Instance.Get();
            bullet.transform.position = firePoint.position;
            bullet.Launch(caster.transform.up);
        }

        Debug.Log($"Player activated skill: {this.SkillName}!");
    }
}