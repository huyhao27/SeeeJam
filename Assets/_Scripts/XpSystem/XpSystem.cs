using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class XpSystem : MonoBehaviour
{
    [SerializeField] private Image xp;           // Thanh máu (Image type = Filled)

    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private float currentXp;
    [SerializeField] private float fillSpeed = 2f;
    [SerializeField] private float maxXp = 50f;

    [SerializeField] private float penaltyXp = 1.1f;

    private int currentLevel = 1;

    void Start()
    {
        UpdateBar();
        levelText.text = currentLevel + "";
        EventBus.On(GameEvent.GetXp, (xpAmount) => { OnGetXp((float)xpAmount); });
    }

    public void SetFill(float percent)
    {
        percent = Mathf.Clamp01(percent);

        // Tween từ giá trị hiện tại tới percent trong 0.3 giây
        xp.DOFillAmount(percent, 0.3f)
          .SetEase(Ease.OutCubic);
    }

    private void UpdateBar()
    {
        StopAllCoroutines();
        StartCoroutine(AnimateXpBar());
    }

    private IEnumerator AnimateXpBar()
    {
        while (currentXp >= maxXp)
        {
            float overflow = currentXp - maxXp;

            // thời gian tween tới full = quãng đường (1 - xp.fillAmount) / tốc độ
            float duration = (1f - xp.fillAmount) / fillSpeed;

            // Tween tới full
            yield return xp.DOFillAmount(1f, duration)
                           .SetEase(Ease.Linear) // giữ tốc độ đều
                           .WaitForCompletion();

            // Level up
            OnLevelUp();

            currentXp = overflow;
            xp.fillAmount = 0;
        }

        // tween phần cuối cùng
        float percent = currentXp / maxXp;
        float finalDuration = (percent - xp.fillAmount) / fillSpeed;

        yield return xp.DOFillAmount(percent, finalDuration)
                       .SetEase(Ease.Linear)
                       .WaitForCompletion();
    }

    private void OnLevelUp()
    {
        levelText.text = ++currentLevel + "";
        maxXp *= penaltyXp; // tăng maxXp
    }
    private void OnGetXp(float xpAmount)
    {
        currentXp += xpAmount;
        UpdateBar();
    }
}
