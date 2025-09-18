using UnityEngine;
using DG.Tweening; 
using System.Collections.Generic;

[RequireComponent(typeof(CircleCollider2D))]
public class AoeWave : MonoBehaviour
{
    [SerializeField] private SpriteRenderer waveSprite; 
    private CircleCollider2D circleCollider;
    
    private List<Collider2D> targetsHit = new List<Collider2D>();

    private void Awake()
    {
        circleCollider = GetComponent<CircleCollider2D>();
    }
    
    private void OnEnable()
    {
        targetsHit.Clear();
    }
    
    public void Expand(float finalRadius, float duration)
    {
        transform.localScale = Vector3.zero;
        waveSprite.color = new Color(1, 1, 1, 1); 
        
        transform.DOScale(Vector3.one * finalRadius * 2, duration).SetEase(Ease.OutQuad);
        
        // mờ dần
        waveSprite.DOFade(0, duration).SetEase(Ease.InQuad)
            .OnComplete(() => {
                ReturnToPool();
            });
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (targetsHit.Contains(other)) return;

        targetsHit.Add(other);
        
        // TODO: Xử lý logic gây sát thương/hiệu ứng cho "other"
        
        // Ví dụ: Áp dụng hiệu ứng Stun
        // if (other.TryGetComponent<IAffectable>(out var affectableTarget))
        // {
        //     affectableTarget.AddEffect(new StunEffect(0.5f), Element.None);
        // }
    }

    private void ReturnToPool()
    {
        ObjectPool<AoeWave>.Instance.Return(this);
    }
}