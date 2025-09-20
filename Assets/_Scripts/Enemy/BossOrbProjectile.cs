using UnityEngine;

public class BossOrbProjectile : BaseBullet
{
    private float explosionRadius;
    private float damage;
    private bool exploded;

    public void Setup(float damage, float radius)
    {
        this.damage = damage;
        this.explosionRadius = radius;
        exploded = false;
    }

    protected override void OnHit(GameObject target)
    {
        Explode();
    }

    protected override void Update()
    {
        base.Update();
        // Nếu lifetime hết, base sẽ despawn -> ta muốn nổ trước khi despawn
        if (!exploded && rb.velocity.sqrMagnitude <= 0.01f)
        {
            // không bắt buộc, giữ nguyên
        }
    }

    private void Explode()
    {
        if (exploded) return;
        exploded = true;
        // Overlap Player
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var h in hits)
        {
            if (h.CompareTag("Player"))
            {
                EventBus.Emit(GameEvent.PlayerDamaged, new object[]{ (int)damage, this.gameObject, h.gameObject });
            }
        }
        // TODO: VFX explosion
    }

    public override void OnPoolDespawn()
    {
        base.OnPoolDespawn();
        exploded = false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (explosionRadius > 0)
        {
            UnityEditor.Handles.color = Color.magenta;
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, explosionRadius);
        }
    }
#endif
}
