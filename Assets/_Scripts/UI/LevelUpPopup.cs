using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LevelUpPopup : Popup
{
    [SerializeField] private List<HoverLift> levelUpPanels; // UI panels
    [SerializeField] private List<UpgradeBase> upgrades;    // pool các upgrade

    [SerializeField] private Animator animator;

    void Start()
    {
        EventBus.On(GameEvent.SelectUpgrade, (obj) => { OnSelectUpgrade((HoverLift)obj); });
    }

    void OnEnable()
    {
        animator.enabled = true;
        RollPanels();
    }

    public override void Hide()
    {
        base.Hide();
        Time.timeScale = 1f;
    }

    public override void Show()
    {
        base.Show();
        Time.timeScale = 0f;
    }

    private void OnSelectUpgrade(HoverLift selectedPanel)
    {
        animator.enabled = false;

        foreach (var panel in levelUpPanels)
        {
            if (panel == selectedPanel)
            {
                panel.GetComponent<RectTransform>().DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
            }
            else
            {
                panel.GetComponent<RectTransform>().DOScale(new Vector3(0f, 1f, 1f), 0.2f).SetEase(Ease.InBack).SetUpdate(true);
            }
        }
        DOVirtual.DelayedCall(2f, () =>
        {
            upgrades.Remove(selectedPanel.Upgrade);
            Hide();
        });
    }

    private void RollPanels()
    {
        if (upgrades.Count < 3)
        {
            Debug.LogWarning("Không đủ upgrade để roll!");
            return;
        }

        // Chọn ngẫu nhiên 3 upgrade khác nhau
        List<UpgradeBase> chosenUpgrades = new();
        for (int i = 0; i < 3; i++)
        {
            int index = UnityEngine.Random.Range(0, upgrades.Count);
            chosenUpgrades.Add(upgrades[index]);
        }

        // Gán cho các panel
        for (int i = 0; i < levelUpPanels.Count; i++)
        {
            if (i < chosenUpgrades.Count)
            {
                levelUpPanels[i].CanSelect = true;
                levelUpPanels[i].Setup(chosenUpgrades[i]);
            }
            else
            {
                levelUpPanels[i].gameObject.SetActive(false); // ẩn nếu không có
            }
        }
    }
}