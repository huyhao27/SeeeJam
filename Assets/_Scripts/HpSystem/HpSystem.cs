using UnityEngine;
using UnityEngine.UI;

public class HpSystem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image hp;      
    [SerializeField] private Image hpIndicator;  

    [Header("Stats")]
    [SerializeField] private float maxHp = 50f;
    [SerializeField] private float currentHp;

    [Header("Flags")]
    [Tooltip("Đánh dấu đây là HP của Player để tự động nhận damage từ EventBus.")]
    [SerializeField] private bool isPlayerHp = true; 

    private void Awake()
    {
        currentHp = Mathf.Clamp(currentHp <= 0 ? maxHp : currentHp, 0, maxHp);
        UpdateBar();
    }

    private void OnEnable()
    {
        if (isPlayerHp)
        {
            EventBus.On(GameEvent.PlayerDamaged, OnPlayerDamagedEvent);
        }
    }

    private void OnDisable()
    {
        if (isPlayerHp)
        {
            EventBus.Off(GameEvent.PlayerDamaged, OnPlayerDamagedEvent);
        }
    }

    // Handler cho event PlayerDamaged
    private void OnPlayerDamagedEvent(object payload)
    {
        // Expect: object[] { int damage, GameObject attacker, GameObject target }
        if (payload is not object[] arr || arr.Length < 3) return;

        int damage;
        try
        {
            damage = (int)arr[0];
        }
        catch
        {
            return; // Payload không hợp lệ
        }

        var targetObj = arr[2] as GameObject;
        if (targetObj == null) return;

        if (targetObj == this.gameObject)
        {
            TakeDamage(damage);
        }
    }

    public void SetFill(float percent)
    {
        percent = Mathf.Clamp01(percent);
        if (hp != null)
            hp.fillAmount = percent;  
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
    }

    // Public API phụ nếu cần chỉnh HP runtime từ nơi khác
    public void SetMaxHp(float newMax, bool fillToMax = true)
    {
        if (newMax <= 0f) return;
        maxHp = newMax;
        if (fillToMax) currentHp = maxHp;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);
        UpdateBar();
    }

    public float CurrentHp => currentHp;
    public float MaxHp => maxHp;
}