using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

[RequireComponent(typeof(CircleCollider2D))]
public class AoeWave : MonoBehaviour, IPoolable
{
    [SerializeField] private SpriteRenderer waveSprite;
    [SerializeField] private int damage = 15; // Ví dụ: Thêm sát thương cho AOE

    private CircleCollider2D circleCollider;
    private List<Collider2D> targetsHit = new List<Collider2D>();

    #region IPoolable Implementation
    private GameObject _originalPrefab;

    public void OnPoolSpawn()
    {
        targetsHit.Clear();
        transform.localScale = Vector3.zero;
        waveSprite.color = new Color(1, 1, 1, 1);
        gameObject.SetActive(true);
    }

    public void OnPoolDespawn() { }

    public void SetOriginalPrefab(GameObject prefab)
    {
        _originalPrefab = prefab;
    }

    public GameObject GetOriginalPrefab()
    {
        return _originalPrefab;
    }
    #endregion

    private void Awake()
    {
        circleCollider = GetComponent<CircleCollider2D>();
    }

    public void Expand(float finalRadius, float duration)
    {
        // Hiệu ứng phóng to và mờ dần
        transform.DOScale(finalRadius * 2, duration).SetEase(Ease.OutQuad);
        waveSprite.DOFade(0, duration).SetEase(Ease.InQuad)
            .OnComplete(() => {
                // Khi hiệu ứng kết thúc, trả về pool
                PoolManager.Instance.Despawn(this);
            });
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Đảm bảo mỗi mục tiêu chỉ bị đánh một lần
        if (targetsHit.Contains(other)) return;

        // Chỉ tác dụng lên Enemy
        if (other.TryGetComponent<BaseEnemy>(out var enemy))
        {
            targetsHit.Add(other);
            enemy.TakeDamage(damage); // Gây sát thương
        }
    }
}