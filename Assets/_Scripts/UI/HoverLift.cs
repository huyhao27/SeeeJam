using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using System;

public class HoverLift : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{

    private RectTransform rectTransform;
    private Vector2 originalPos;

    [SerializeField] private float liftAmount = 20f;
    [SerializeField] private float duration = 0.2f;

    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private Image icon;

    public UpgradeBase Upgrade;
    public bool CanSelect = false;
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPos = rectTransform.anchoredPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        rectTransform.DOAnchorPos(originalPos + new Vector2(0, liftAmount), duration)
                     .SetEase(Ease.OutCubic)
                     .SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        rectTransform.DOAnchorPos(originalPos, duration)
                     .SetEase(Ease.OutCubic)
                     .SetUpdate(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!CanSelect) return;
        EventBus.Emit(GameEvent.SelectUpgrade, this);
        this.Upgrade.Apply(PlayerStats.Instance);
    }

    public void Setup(UpgradeBase upgrade)
    {
        this.Upgrade = upgrade;
        description.text = upgrade.description;
        icon.sprite = upgrade.icon;
    }
}