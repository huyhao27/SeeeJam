using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class XpSystem : MonoBehaviour
{
    [SerializeField] private Image xp;           // Thanh máu (Image type = Filled)

    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private float currentXp;
    [SerializeField] private float maxXp = 50f;

    [SerializeField] private float penaltyXp = 1.05f;

    void Start()
    {
        currentXp = maxXp;
        UpdateBar();

        EventBus.On(GameEvent.GetXp, (xpAmount) => { OnGetXp((float)xpAmount); });
    }

    public void SetFill(float percent)
    {
        percent = Mathf.Clamp01(percent);
        xp.fillAmount = percent;
    }

    private void UpdateBar()
    {
        Debug.Log(currentXp / maxXp);

        SetFill(currentXp / maxXp);

        if (currentXp >= maxXp)
        {
            OnLevelUp();
        }
    }

    private void OnLevelUp()
    {
        maxXp *= penaltyXp;
        currentXp = 0;
        UpdateBar();
    }

    private void OnGetXp(float xpAmount)
    {
        currentXp = xpAmount;
        UpdateBar();
    }
}
