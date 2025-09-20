using UnityEngine;

[CreateAssetMenu(fileName = "NewAoeSkill", menuName = "Skills/AOE Skill")]
public class AoeSkill : BaseSkill
{
    [Header("AOE Settings")]
    [SerializeField] private AoeWave wavePrefab;
    [SerializeField] private float finalRadius = 5f;
    [SerializeField] private float expandDuration = 0.5f;

    [Tooltip("Sát thương kỹ năng gây ra.")]
    [SerializeField] private int damage = 15;
    [Tooltip("Layer của các đối tượng bị ảnh hưởng (thường là Enemy).")]
    [SerializeField] private LayerMask targetLayers;

    public override void Activate(MonoBehaviour caster, Transform firePoint)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(caster.transform.position, finalRadius, targetLayers);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<BaseEnemy>(out var enemy))
            {
                enemy.TakeDamage(damage);
            }
        }

        var waveInstance = PoolManager.Instance.Spawn(wavePrefab, caster.transform.position, Quaternion.identity);

        if (waveInstance != null)
        {
            waveInstance.Expand(finalRadius, expandDuration);
        }
    }
}