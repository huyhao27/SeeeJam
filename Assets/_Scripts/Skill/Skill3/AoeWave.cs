using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class AoeWave : MonoBehaviour, IPoolable
{
    [SerializeField] private SpriteRenderer waveSprite;

    #region IPoolable Implementation
    private GameObject _originalPrefab;

    public void OnPoolSpawn()
    {
        transform.localScale = Vector3.zero;
        if (waveSprite != null)
        {
            waveSprite.color = new Color(1, 1, 1, 1);
        }
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

    public void Expand(float finalRadius, float duration)
    {
        // Hiệu ứng phóng to và mờ dần
        transform.DOScale(finalRadius * 2, duration).SetEase(Ease.OutQuad);
        
        if (waveSprite != null)
        {
            waveSprite.DOFade(0, duration).SetEase(Ease.InQuad)
                .OnComplete(() => {
                    PoolManager.Instance.Despawn(this);
                });
        }
        else
        {
            DOVirtual.DelayedCall(duration, () => PoolManager.Instance.Despawn(this));
        }
    }

}