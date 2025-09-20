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

    private float maxHp
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

    [Header("Flags")]
    [Tooltip("Đánh dấu đây là HP của Player để tự động nhận damage từ EventBus.")]
    [SerializeField] private bool isPlayerHp = true;

    private void Awake()
    {
        currentHp = Mathf.Clamp(currentHp <= 0 ? maxHp : currentHp, 0, maxHp);
        UpdateBar();
        hpText.text = currentHp + "/" + maxHp;
        EventBus.On(GameEvent.Heal, (amount) => { Heal((float)amount); });
        EventBus.On(GameEvent.MaxHpChanged, (amount) =>
        {
            Debug.Log("Max hp update!");
            UpdateBar();
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
        if (damage <= 0f || currentHp <= 0f) return;
        currentHp = Mathf.Max(0, currentHp - damage);
        SetFill(currentHp / maxHp);
        Debug.Log($"[HpSystem] TakeDamage {damage} -> currentHp={currentHp}");

        if (currentHp <= 0)
        {
            OnHpDepleted();
        }
    }

    public void Heal(float amount)
    {
        if (amount <= 0f || currentHp <= 0f) return;
        currentHp = Mathf.Min(maxHp, currentHp + amount);
        SetFill(currentHp / maxHp);
    }

    private void OnHpDepleted()
    {
        // Có thể emit event GameOver
        Debug.Log("Player HP depleted");
    }

    private void UpdateBar()
    {
        SetFill(maxHp > 0 ? currentHp / maxHp : 0f);
        hpText.text = currentHp + "/" + maxHp;
    }

    // Public API phụ nếu cần chỉnh HP runtime từ nơi khác
    public void SetMaxHp(float newMax)
    {
        if (newMax <= 0f) return;
        maxHp = newMax;
        UpdateBar();
    }

    public float CurrentHp => currentHp;
    public float MaxHp => maxHp;
}