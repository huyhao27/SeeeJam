using UnityEngine;

[CreateAssetMenu(fileName = "NewProjectileSkill", menuName = "Skills/Projectile Skill")]
public class ProjectileSkill : BaseSkill
{
    [Header("Projectile Settings")]
    [SerializeField] private Bullet bulletPrefab;

    public override void Activate(GameObject caster)
    {
        Transform firePoint = caster.transform.Find("FirePoint");
        if (firePoint == null)
        {
            Debug.LogError($"'{caster.name}' does not have a child object named 'FirePoint'!");
            return;
        }

        Bullet bullet = ObjectPool<Bullet>.Instance.Get();
        if (bullet == null) return;
        
        bullet.transform.position = firePoint.position;

        Vector2 fireDirection = caster.transform.up;

        bullet.Launch(fireDirection);
        Debug.Log($"Player activated skill: {this.SkillName}!");
    }
}