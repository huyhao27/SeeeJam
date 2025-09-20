using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; 

public class HpSystem : MonoBehaviour
{
    [SerializeField] private Image hp;           
    [SerializeField] private Image hpIndicator;  
    
    private void OnEnable()
    {
        EventBus.On(GameEvent.PlayerHealthUpdated, OnHealthUpdated);
    }

    private void OnDisable()
    {
        EventBus.Off(GameEvent.PlayerHealthUpdated, OnHealthUpdated);
    }

    private void OnHealthUpdated(object data)
    {
        if (data is object[] args && args.Length == 2 && args[0] is float currentHp && args[1] is float maxHp)
        {
            UpdateBar(currentHp, maxHp);
        }
    }

    private void UpdateBar(float currentHp, float maxHp)
    {
        if (maxHp <= 0) return;

        float percent = currentHp / maxHp;
        percent = Mathf.Clamp01(percent);

        hp.DOFillAmount(percent, 0.2f).SetEase(Ease.OutCubic);
        
        if (hpIndicator != null)
        {
            hpIndicator.DOFillAmount(percent, 0.5f).SetEase(Ease.OutCubic).SetDelay(0.3f);
        }
    }
}