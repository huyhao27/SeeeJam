using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class HpSystem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image hp;
    [SerializeField] private Image hpIndicator;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private float fillSpeed = 2f;
    [SerializeField] private Image nhaydo;

    private void OnEnable()
    {
        EventBus.On(GameEvent.PlayerHealthUpdated, OnHealthUpdate);
        EventBus.On(GameEvent.PlayerDamaged, OnPlayerDamaged);
    }

    private void OnDisable()
    {
        EventBus.Off(GameEvent.PlayerHealthUpdated, OnHealthUpdate);
        EventBus.Off(GameEvent.PlayerDamaged, OnPlayerDamaged);
    }

    private void OnHealthUpdate(object data)
    {
        if (data is object[] args && args.Length == 2 && args[0] is float currentHp && args[1] is float maxHp)
        {
            UpdateBar(currentHp, maxHp);
        }
    }
    
    private void OnPlayerDamaged(object data)
    {
        nhaydo.DOKill();
        Sequence seq = DOTween.Sequence();
        seq.Append(nhaydo.DOFade(0.8f, 0.2f))
            .Append(nhaydo.DOFade(0f, 0.2f));
    }

    private void UpdateBar(float currentHp, float maxHp)
    {
        float percent = maxHp > 0 ? currentHp / maxHp : 0f;
        
        hp.DOKill();
        hpIndicator.DOKill();

        hp.DOFillAmount(percent, 1f / fillSpeed).SetUpdate(true); 
        hpIndicator.DOFillAmount(percent, 1.5f / fillSpeed).SetDelay(0.2f).SetUpdate(true);

        hpText.text = $"{Mathf.CeilToInt(currentHp)} / {Mathf.CeilToInt(maxHp)}";
        
        if (currentHp <= 0) 
        {
            GameManager.Instance.ChangeState(GameState.GameOver);
        }
    }

    private void OnDestroy()
    {
        hp.DOKill();
        hpIndicator.DOKill();
        nhaydo.DOKill();
    }
}