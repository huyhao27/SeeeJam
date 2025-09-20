using System.Collections.Generic;
using UnityEngine;

public class SkillUpgradeManager : Singleton<SkillUpgradeManager>
{
    private readonly Dictionary<BaseSkill, int> _skillLevels = new Dictionary<BaseSkill, int>();

    /// <summary>
    /// Lấy cấp độ hiện tại của một kỹ năng. Mặc định là 0 nếu chưa được nâng cấp.
    /// </summary>
    public int GetSkillLevel(BaseSkill skill)
    {
        _skillLevels.TryGetValue(skill, out int level);
        return level;
    }

    /// <summary>
    /// Nâng cấp một kỹ năng lên 1 cấp.
    /// </summary>
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

    /// <summary>
    /// Reset toàn bộ cấp độ kỹ năng.
    /// </summary>
    public void ResetAllSkillLevels()
    {
        _skillLevels.Clear();
        Debug.Log("Đã reset cấp độ của tất cả kỹ năng.");
    }

    // --- THÊM MỚI: CHỨC NĂNG DEBUG ---
    // Đoạn code này sẽ chỉ chạy trong Unity Editor
#if UNITY_EDITOR
    private void Update()
    {
        // Nhấn phím '1' để nâng cấp Skill 1
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            UpgradeSkillByIndex(0);
        }

        // Nhấn phím '2' để nâng cấp Skill 2
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            UpgradeSkillByIndex(1);
        }

        // Nhấn phím '3' để nâng cấp Skill 3
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
    // --- KẾT THÚC PHẦN THÊM MỚI ---
}