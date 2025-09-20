using UnityEngine;

[CreateAssetMenu(fileName = "NewShockwaveSkill", menuName = "Skills/Shockwave Skill")]
public class ShockwaveSkill : BaseSkill
{
    [Header("Visuals")]
    [Tooltip("Prefab của hiệu ứng sóng xung kích (chỉ dùng làm hình ảnh).")]
    [SerializeField] private AoeWave wavePrefab;
    [Tooltip("Thời gian để sóng lan ra tối đa.")]
    [SerializeField] private float expandDuration = 0.3f;

    [Header("Base Settings")]
    [Tooltip("Bán kính mặc định của vùng ảnh hưởng.")]
    [SerializeField] private float baseRadius = 4f;
    [Tooltip("Layer của các đối tượng bị ảnh hưởng (thường là Enemy).")]
    [SerializeField] private LayerMask targetLayers;

    [Header("Buff Settings")]
    [Tooltip("Thời gian làm choáng cơ bản.")]
    [SerializeField] private float buff_baseStunDuration = 1.0f;
    [Tooltip("Thời gian làm choáng sau khi nâng cấp Buff 1.")]
    [SerializeField] private float buff1_upgradedStunDuration = 1.5f;
    [Tooltip("Lực đẩy lùi của Buff 2.")]
    [SerializeField] private float buff2_knockbackForce = 10f;
    [Tooltip("Bán kính sau khi nâng cấp Buff 3.")]
    [SerializeField] private float buff3_upgradedRadius = 6f;

    public override void Activate(MonoBehaviour caster, Transform firePoint)
    {
        int level = SkillUpgradeManager.Instance.GetSkillLevel(this);

        float currentRadius = (level >= 3) ? buff3_upgradedRadius : baseRadius;

        Collider2D[] hits = Physics2D.OverlapCircleAll(caster.transform.position, currentRadius, targetLayers);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IAffectable>(out var affectableTarget))
            {
                if (level >= 2)
                {
                    Vector2 direction = (hit.transform.position - caster.transform.position).normalized;
                    if (direction == Vector2.zero)
                    {
                        direction = Random.insideUnitCircle.normalized;
                    }
                    affectableTarget.AddEffect(new KnockbackEffect(direction, buff2_knockbackForce));
                }
                else
                {
                    float stunDuration = (level >= 1) ? buff1_upgradedStunDuration : buff_baseStunDuration;
                    affectableTarget.AddEffect(new StunEffect(stunDuration));
                }
            }
        }
        
        if (wavePrefab != null)
        {
            var waveInstance = PoolManager.Instance.Spawn(wavePrefab, caster.transform.position, Quaternion.identity);
            if (waveInstance != null)
            {
                waveInstance.Expand(currentRadius, expandDuration);
            }
        }
    }
}