using UnityEngine;
using UnityEngine.UI; 
using DG.Tweening;    
using System.Collections;

public class PlayerFXController : MonoBehaviour
{
    [Header("Heal Effect Settings")]
    [Tooltip("Kéo Prefab chứa Image dấu '+' của bạn vào đây.")]
    [SerializeField] private Image healImagePrefab;

    [Tooltip("Kéo Canvas chính của bạn vào đây để hiệu ứng hiển thị đúng.")]
    [SerializeField] private RectTransform uiCanvasRect; 

    [Header("Animation Config")]
    [SerializeField] private int numberOfSigns = 5;
    [SerializeField] private float effectDuration = 1.5f;
    [SerializeField] private float floatHeight = 150f;
    [SerializeField] private float spawnRadius = 100f;
    [SerializeField] private float spawnInterval = 0.1f;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        EventBus.On(GameEvent.Heal, OnHeal);
    }

    private void OnDisable()
    {
        EventBus.Off(GameEvent.Heal, OnHeal);
    }

    private void OnHeal(object data)
    {
        StartCoroutine(HealEffectRoutine());
    }

    private IEnumerator HealEffectRoutine()
    {
        if (healImagePrefab == null || uiCanvasRect == null || mainCamera == null)
        {
            Debug.LogWarning("Chưa gán Prefab, Canvas hoặc không tìm thấy Main Camera!");
            yield break; 
        }

        for (int i = 0; i < numberOfSigns; i++)
        {
            SpawnHealSign();
            yield return new WaitForSecondsRealtime(spawnInterval);
        }
    }

    private void SpawnHealSign()
    {
        Image signInstance = Instantiate(healImagePrefab);
        
        Vector2 screenPoint = mainCamera.WorldToScreenPoint(transform.position);
        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;

        Canvas canvas = uiCanvasRect.GetComponent<Canvas>();
        Camera uiCamera = null;
        if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = mainCamera; 
        }

        Vector2 anchoredPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            uiCanvasRect,
            screenPoint + randomOffset, 
            uiCamera,
            out anchoredPosition
        );
        
        signInstance.transform.SetParent(uiCanvasRect, false);
        
        signInstance.rectTransform.anchoredPosition = anchoredPosition;

        signInstance.canvasRenderer.SetAlpha(200f);

        Sequence healSequence = DOTween.Sequence();
        
        healSequence.Join(signInstance.rectTransform.DOAnchorPosY(signInstance.rectTransform.anchoredPosition.y + floatHeight, effectDuration).SetEase(Ease.OutQuad));
        healSequence.Join(signInstance.DOFade(1f, effectDuration / 2).SetLoops(2, LoopType.Yoyo));
        healSequence.SetUpdate(true); 

        healSequence.OnComplete(() =>
        {
            Destroy(signInstance.gameObject);
        });
    }
}