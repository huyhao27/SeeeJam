using UnityEngine;

// Đạn của enemy: khi trúng Player sẽ in Debug log
public class EnemyBullet : BaseBullet
{
    [SerializeField] private float damage = 5f; // có thể dùng sau nếu bạn muốn xử lý sát thương qua event

    public float Damage => damage;
    protected override void OnHit(GameObject target)
    {
        if (target.CompareTag("Player"))
        {
            // Debug.Log($"[EnemyBullet] Hit Player: -{damage} HP (chỉ log, chưa trừ máu)");
            // EventBus.Emit(GameEvent.PlayerDamaged, new object[] { damage, this.gameObject, target });
            PoolManager.Instance.Despawn(this.gameObject);
        }
    }
}
