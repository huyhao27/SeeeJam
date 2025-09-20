using System.Collections.Generic;
using UnityEngine;
using System.Linq; 

public class LevelUpPopup : Popup
{
    [SerializeField] private List<HoverLift> levelUpPanels;
    [SerializeField] private List<UpgradeBase> statUpgrades; 
    [SerializeField] private Animator animator;

    void OnEnable()
    {
        animator.enabled = true;
        Time.timeScale = 0f;
        RollPanels();
    }

    public override void Hide()
    {
        base.Hide();
        Time.timeScale = 1f;
    }
    
    public void SelectUpgrade(HoverLift selectedPanel)
    {
        selectedPanel.Upgrade.Apply();

        Hide();
    }

    private void RollPanels()
    {
        List<UpgradeBase> possibleUpgrades = new List<UpgradeBase>();
        possibleUpgrades.AddRange(statUpgrades);

        var playerSkills = PlayerStats.Instance.Skills;
        foreach (var skill in playerSkills)
        {
            if (skill == null || skill.upgradeChain == null) continue;
            int currentLevel = SkillUpgradeManager.Instance.GetSkillLevel(skill);
            if (currentLevel < skill.upgradeChain.Count)
            {
                possibleUpgrades.Add(skill.upgradeChain[currentLevel]);
            }
        }

        var chosenUpgrades = possibleUpgrades.OrderBy(x => System.Guid.NewGuid()).Take(levelUpPanels.Count).ToList();

        for (int i = 0; i < levelUpPanels.Count; i++)
        {
            var panel = levelUpPanels[i];
            if (i < chosenUpgrades.Count)
            {
                panel.gameObject.SetActive(true);
                
                panel.CanSelect = true; 
                
                panel.Setup(chosenUpgrades[i]);
            }
            else
            {
                panel.gameObject.SetActive(false); 
            }
        }
    }
}