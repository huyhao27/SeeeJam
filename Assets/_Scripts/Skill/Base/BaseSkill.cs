using System.Collections.Generic; 
using UnityEngine;

public abstract class BaseSkill : ScriptableObject
{
    [Header("General Settings")]
    [SerializeField] private string skillName = "New Skill";
    [SerializeField] private float cooldown = 1f;
    
    [Header("Upgrade Path")]
    [Tooltip("Danh sách các nâng cấp theo đúng thứ tự từ Buff 1 -> 3")]
    public List<SkillUpgradeData> upgradeChain;

    public string SkillName => skillName;
    public float Cooldown => cooldown;
    public abstract void Activate(MonoBehaviour caster, Transform firePoint);
}