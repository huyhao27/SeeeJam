using UnityEngine;
using System.Collections;

public class BossBomb : MonoBehaviour, IPoolable
{
    [SerializeField] private float selfDestructTime = 5f;
    [SerializeField] private float triggerRadius = 0.6f; // nếu có collider thì không cần
    [SerializeField] private LayerMask playerLayer;

    private float damage;
    private float timer;
    private bool exploded;

    private GameObject _originalPrefab;

    public void Setup(float dmg)
    {
        damage = dmg;
    }

    public void OnPoolSpawn()
    {
        timer = selfDestructTime;
        exploded = false;
        gameObject.SetActive(true);
    }

    public void OnPoolDespawn()
    {
        // reset state
    }

    public void SetOriginalPrefab(GameObject prefab)
    {
        _originalPrefab = prefab;
    }

    public GameObject GetOriginalPrefab()
    {
        return _originalPrefab;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Explode();
            PoolManager.Instance.Despawn(this);
        }
        else
        {
            // simple proximity trigger (nếu không dùng collider trigger riêng)
            Collider2D hit = Physics2D.OverlapCircle(transform.position, triggerRadius, playerLayer);
            if (hit != null)
            {
                Explode();
                PoolManager.Instance.Despawn(this);
            }
        }
    }

    private void Explode()
    {
        if (exploded) return;
        exploded = true;
        // Gây damage & có thể emit event (tạm dùng PlayerDamaged)
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, triggerRadius, playerLayer);
        foreach (var h in hits)
        {
            if (h.CompareTag("Player"))
            {
                EventBus.Emit(GameEvent.PlayerDamaged, (float)damage);
            }
        }
        // TODO: VFX / âm thanh
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, triggerRadius);
    }
#endif
}
