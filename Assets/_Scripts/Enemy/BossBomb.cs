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
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sfxSpawn;
    [SerializeField] private AudioClip sfxExplode;
    [Header("Destructible Settings")]
    [Tooltip("Bomb có thể bị đạn player phá không.")]
    [SerializeField] private bool destructibleByPlayerBullet = true;
    [Tooltip("Layer mask dùng để nhận diện đạn player khi OnTrigger/Collision.")]
    [SerializeField] private LayerMask playerBulletLayer;
    [Tooltip("Phá bomb sẽ nổ (gây damage) hay chỉ biến mất.")]
    [SerializeField] private bool explodeOnBulletDestroy = true;
    [Tooltip("Nếu true: dùng collider trigger để bắt đạn thay vì OverlapCircle thủ công.")]
    [SerializeField] private bool useTriggerColliderForBullet = true;
    [Tooltip("Nếu phá bằng đạn muốn phát âm thanh riêng (nếu null dùng sfxExplode nếu explodeOnBulletDestroy= true).")]
    [SerializeField] private AudioClip sfxDestroySilent;

    private Collider2D _collider;

    public void Setup(float dmg)
    {
        damage = dmg;
    }

    public void OnPoolSpawn()
    {
        timer = selfDestructTime;
        exploded = false;
        gameObject.SetActive(true);
        if (_collider == null) _collider = GetComponent<Collider2D>();
        if (_collider != null && useTriggerColliderForBullet)
        {
            _collider.isTrigger = true; // để OnTriggerEnter2D nhận đạn
        }
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false; audioSource.loop = false;
        }
        if (sfxSpawn != null) audioSource.PlayOneShot(sfxSpawn);
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
            if (!useTriggerColliderForBullet)
            {
                // simple proximity trigger (nếu không dùng collider trigger riêng)
                Collider2D hit = Physics2D.OverlapCircle(transform.position, triggerRadius, playerLayer);
                if (hit != null)
                {
                    Explode();
                    if (sfxExplode != null) audioSource.PlayOneShot(sfxExplode);
                    PoolManager.Instance.Despawn(this);
                }
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
        if (sfxExplode != null && audioSource != null) audioSource.PlayOneShot(sfxExplode);
    }

    private void DestroyByPlayerBullet(bool forceExplode)
    {
        if (exploded) return;
        if (forceExplode || explodeOnBulletDestroy)
        {
            Explode();
        }
        else
        {
            // silent destroy (không gây damage)
            exploded = true; // đánh dấu để không nổ lại
            if (sfxDestroySilent != null && audioSource != null) audioSource.PlayOneShot(sfxDestroySilent);
        }
        PoolManager.Instance.Despawn(this);
    }

    private bool IsPlayerBulletLayer(int layer)
    {
        return ((1 << layer) & playerBulletLayer) != 0;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!destructibleByPlayerBullet) return;
        if (!useTriggerColliderForBullet) return; // đang dùng overlap thủ công và trigger chỉ cho proximity -> bỏ qua
        if (other == null) return;

        if (IsPlayerBulletLayer(other.gameObject.layer))
        {
            DestroyByPlayerBullet(false);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!destructibleByPlayerBullet) return;
        if (useTriggerColliderForBullet) return; // khi đã bật trigger mode không xử lý collision
        if (collision == null) return;
        if (IsPlayerBulletLayer(collision.gameObject.layer))
        {
            DestroyByPlayerBullet(false);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, triggerRadius);
    }
#endif
}
