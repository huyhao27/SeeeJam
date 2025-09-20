using UnityEngine;

[CreateAssetMenu(fileName = "NewSkillUpgrade", menuName = "Upgrades/Skill Upgrade")]
public class SkillUpgradeData : UpgradeBase
{
    [Header("Skill Settings")]
    [Tooltip("Kéo ScriptableObject của kỹ năng (ProjectileSkill, ArcSkill,...) vào đây.")]
    public BaseSkill targetSkill;

    public override void Apply()
    {
        if (targetSkill == null)
        {
            Debug.LogError($"Chưa gán Target Skill cho upgrade: {this.name}");
            return;
        }
        SkillUpgradeManager.Instance.UpgradeSkill(targetSkill);
    }
}