using UnityEngine;
using UnityEngine.UI;
using System.Collections;
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
    public float MaxHp
    {
        get => PlayerStats.Instance ? PlayerStats.Instance.MaxHp : 50f;
        set
        {
            if (PlayerStats.Instance != null)
            {
                SetMaxHp(value);
                PlayerStats.Instance.MaxHp = value;
            }
        }
    }

    [SerializeField] private float currentHp;

    void Start()
    {
        currentHp = MaxHp;
        UpdateBar();
        hpText.text = currentHp + "/" + MaxHp;
        EventBus.On(GameEvent.Heal, (amount) => { Heal((float)amount); });
        EventBus.On(GameEvent.MaxHpChanged, (amount) =>
        {
            UpdateBar();
        });

        EventBus.On(GameEvent.PlayerDamaged, (amount) =>
        {
            TakeDamage((float)amount);
        });
    }



    public void SetFill(float percent)
    {
        percent = Mathf.Clamp01(percent);

        if (hp != null)
        {
            // Kill tween cũ
            hp.DOKill();
            hpIndicator.DOKill();

            // Tween thanh máu chính ngay lập tức
            hp.DOFillAmount(percent, 1f / fillSpeed)
              .SetUpdate(true); // Unscaled time

            // Thanh indicator tụt chậm hơn (delay)
            hpIndicator.DOFillAmount(percent, 1.5f / fillSpeed) // lâu hơn một chút
                       .SetDelay(0.2f)                          // delay nhỏ (0.2s)
                       .SetUpdate(true);
        }
    }
    public void TakeDamage(float damage)
    {
        currentHp = Mathf.Max(0, currentHp - damage);

        nhaydo.DOKill();
        float currentAlpha = nhaydo.color.a;

        Sequence seq = DOTween.Sequence();
        seq.Append(nhaydo.DOFade(0.8f, 0.2f).From(currentAlpha)) // từ currentAlpha -> 1
           .Append(nhaydo.DOFade(0f, 0.2f));                         // 1 -> 0

        UpdateBar();
    }

    public void Heal(float amount)
    {
        currentHp = Mathf.Min(MaxHp, currentHp + amount);
        UpdateBar();
    }

    private void UpdateBar()
    {
        SetFill(MaxHp > 0 ? currentHp / MaxHp : 0f);
        hpText.text = currentHp + "/" + MaxHp;

        if (currentHp <= 0) { GameManager.Instance.ChangeState(GameState.GameOver); }
    }

    // Public API phụ nếu cần chỉnh HP runtime từ nơi khác
    public void SetMaxHp(float newMax)
    {
        if (newMax <= 0f) return;
        MaxHp = newMax;
        UpdateBar();
    }

    void OnDestroy()
    {
        nhaydo.DOKill();
        hp.DOKill();
        hpIndicator.DOKill();

        StopAllCoroutines();
    }

}