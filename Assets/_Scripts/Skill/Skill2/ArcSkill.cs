using UnityEngine;

[CreateAssetMenu(fileName = "NewArcSkill", menuName = "Skills/Arc Skill")]
public class ArcSkill : BaseSkill
{
    [Header("Arc Settings")]
    [Tooltip("Kéo prefab của ArcBullet vào đây.")]
    [SerializeField] private ArcBullet arcPrefab;

    [Header("Buff Settings")]
    [Tooltip("Nhân kích thước của vòng cung lên bao nhiêu lần khi có Buff 1. (VD: 1.5 = lớn hơn 50%)")]
    [SerializeField] private float buff1_scaleMultiplier = 1.5f;

    [Tooltip("Bán kính vụ nổ của Buff 3.")]
    [SerializeField] private float buff3_explosionRadius = 2.5f;
    [Tooltip("Sát thương của vụ nổ (dùng cho boss).")]
    [SerializeField] private int buff3_explosionDamage = 99999;

    public override void Activate(MonoBehaviour caster, Transform firePoint)
    {
        if (firePoint == null || arcPrefab == null)
        {
            Debug.LogError("Chưa gán FirePoint hoặc Arc Prefab!");
            return;
        }

        int level = SkillUpgradeManager.Instance.GetSkillLevel(this);

        bool widerArc = level >= 1;
        bool bidirectional = level >= 2;
        bool explodes = level >= 3;

        SpawnArc(firePoint.position, firePoint.rotation, widerArc, explodes);

        if (bidirectional)
        {
            Quaternion backwardRotation = firePoint.rotation * Quaternion.Euler(0, 0, 180);
            SpawnArc(firePoint.position, backwardRotation, widerArc, explodes);
        }
    }

    private void SpawnArc(Vector3 position, Quaternion rotation, bool isWider, bool willExplode)
    {
        var arc = PoolManager.Instance.Spawn(arcPrefab, position, rotation);
        if (arc == null) return;

        if (isWider)
        {
            arc.transform.localScale *= buff1_scaleMultiplier;
        }

        if (willExplode)
        {
            arc.SetupExplosion(buff3_explosionRadius, buff3_explosionDamage);
        }

        arc.Launch(rotation * Vector2.right);
    }
}