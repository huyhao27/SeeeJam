using System.Collections.Generic;
using UnityEngine;

public class SkillUpgradeManager : Singleton<SkillUpgradeManager>
{
    private readonly Dictionary<BaseSkill, int> _skillLevels = new Dictionary<BaseSkill, int>();

 
    public int GetSkillLevel(BaseSkill skill)
    {
        _skillLevels.TryGetValue(skill, out int level);
        return level;
    }


    public void UpgradeSkill(BaseSkill skill)
    {
        if (skill == null) return;

        if (_skillLevels.ContainsKey(skill))
        {
            _skillLevels[skill]++;
        }
        else
        {
            _skillLevels[skill] = 1;
        }

        Debug.Log($"<color=cyan>ĐÃ NÂNG CẤP:</color> Kỹ năng '{skill.SkillName}' lên <color=yellow>Cấp độ {GetSkillLevel(skill)}</color>!");
    }

    public void ResetAllSkillLevels()
    {
        _skillLevels.Clear();
        Debug.Log("Đã reset cấp độ của tất cả kỹ năng.");
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            UpgradeSkillByIndex(0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            UpgradeSkillByIndex(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            UpgradeSkillByIndex(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ResetAllSkillLevels();
        }
    }

    private void UpgradeSkillByIndex(int index)
    {
        if (PlayerStats.Instance != null && PlayerStats.Instance.Skills.Count > index)
        {
            BaseSkill skillToUpgrade = PlayerStats.Instance.Skills[index];
            UpgradeSkill(skillToUpgrade);
        }
        else
        {
            Debug.LogWarning($"Không tìm thấy kỹ năng ở vị trí {index + 1} để debug.");
        }
    }
#endif
}