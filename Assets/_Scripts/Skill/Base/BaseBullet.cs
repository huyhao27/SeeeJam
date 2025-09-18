using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public abstract class BaseBullet : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] protected float speed = 15f;
    [SerializeField] protected float lifetime = 3f;

    protected Rigidbody2D rb;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void OnEnable()
    {
        Invoke(nameof(ReturnToPool), lifetime);
    }

    protected virtual void OnDisable()
    {
        CancelInvoke();
    }
    
    public virtual void Launch(Vector2 direction)
    {
        rb.velocity = direction.normalized * speed;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        OnHit(other.gameObject);
        ReturnToPool();
    }
    
    protected abstract void OnHit(GameObject target);

    protected abstract void ReturnToPool();
}