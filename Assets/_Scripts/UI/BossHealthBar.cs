using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private GameObject root;

    private void Awake()
    {
        if (root == null) root = this.gameObject;
        root.SetActive(false);
    }

    private void OnEnable()
    {
        EventBus.On(GameEvent.BossSpawned, OnBossSpawned);
        EventBus.On(GameEvent.BossHpChanged, OnBossHpChanged);
        EventBus.On(GameEvent.BossDied, OnBossDied);
    }

    private void OnDisable()
    {
        EventBus.Off(GameEvent.BossSpawned, OnBossSpawned);
        EventBus.Off(GameEvent.BossHpChanged, OnBossHpChanged);
        EventBus.Off(GameEvent.BossDied, OnBossDied);
    }

    private void OnBossSpawned(object obj)
    {
        root.SetActive(true);
    }

    private void OnBossHpChanged(object payload)
    {
        if (payload is object[] arr && arr.Length >= 2)
        {
            int current = (int)arr[0];
            int max = (int)arr[1];
            float pct = max > 0 ? (float)current / max : 0f;
            if (fillImage != null) fillImage.fillAmount = pct;
            if (hpText != null) hpText.text = current + "/" + max;
        }
    }

    private void OnBossDied(object obj)
    {
        root.SetActive(false);
    }
}
