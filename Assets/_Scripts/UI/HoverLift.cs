using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class HoverLift : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private RectTransform rectTransform;
    private Vector2 originalPos;

    [SerializeField] private float liftAmount = 20f;
    [SerializeField] private float duration = 0.2f;

    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private TextMeshProUGUI title; 
    [SerializeField] private Image icon;

    public UpgradeBase Upgrade { get; private set; } 
    public bool CanSelect = false;
    
    private LevelUpPopup _popupController;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPos = rectTransform.anchoredPosition;
        _popupController = GetComponentInParent<LevelUpPopup>();
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
        if (!CanSelect || _popupController == null) return;
        
        _popupController.SelectUpgrade(this);
    }

    public void Setup(UpgradeBase upgrade)
    {
        this.Upgrade = upgrade;
      //  title.text = upgrade.upgradeName; 
        description.text = upgrade.description;
        icon.sprite = upgrade.icon;
    }
}