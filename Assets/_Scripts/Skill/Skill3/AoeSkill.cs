using UnityEngine;

[CreateAssetMenu(fileName = "NewAoeSkill", menuName = "Skills/AOE Skill")]
public class AoeSkill : BaseSkill
{
    [Header("AOE Settings")]
    [SerializeField] private AoeWave wavePrefab;
    [SerializeField] private float finalRadius = 5f;
    [SerializeField] private float expandDuration = 0.5f;

    public override void Activate(GameObject caster)
    {
        var waveInstance = PoolManager.Instance.Spawn(wavePrefab, caster.transform.position, Quaternion.identity);

        if (waveInstance != null)
        {
            waveInstance.Expand(finalRadius, expandDuration);
            Debug.Log($"Player activated AOE skill: {this.SkillName}!");
        }
    }
}