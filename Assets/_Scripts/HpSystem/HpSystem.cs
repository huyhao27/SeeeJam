using UnityEngine;
using UnityEngine.UI;

public class HpSystem : MonoBehaviour
{
    [SerializeField] private Image hp;           // Thanh máu (Image type = Filled)
    [SerializeField] private Image hpIndicator;  // Viền/khung nếu cần

    [SerializeField] private float currentHp;
    private float maxHp = 50f;

    void Start()
    {
        currentHp = maxHp;
        UpdateBar();
    }

    public void SetFill(float percent)
    {
        percent = Mathf.Clamp01(percent);
        hp.fillAmount = percent;   // Đây chính là "filled" của UI Image
    }

    public void TakeDamage(float damage)
    {
        currentHp = Mathf.Max(0, currentHp - damage);
        SetFill(currentHp / maxHp);
    }

    public void Heal(float amount)
    {
        currentHp = Mathf.Min(maxHp, currentHp + amount);
        SetFill(currentHp / maxHp);
    }

    private void UpdateBar()
    {
        Debug.Log(currentHp / maxHp);
        
        SetFill(currentHp / maxHp);
    }
}