using UnityEngine;

public abstract class BaseSkill : ScriptableObject
{
    [Header("General Settings")]
    [SerializeField] private string skillName = "New Skill";
    [SerializeField] private float cooldown = 1f;

    public string SkillName => skillName;
    public float Cooldown => cooldown;
    public abstract void Activate(MonoBehaviour caster, Transform firePoint);
}