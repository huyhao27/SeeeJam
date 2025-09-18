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
        AoeWave waveInstance = ObjectPool<AoeWave>.Instance.Get();
        if (waveInstance == null) return;
        
        waveInstance.transform.position = caster.transform.position;
        
        waveInstance.Expand(finalRadius, expandDuration);
        
        Debug.Log($"Player activated AOE skill: {this.SkillName}!");
    }
}