using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        // check enemy
        if (collision.CompareTag("Enemy"))
        {
            // Dự phòng: prefab có thể gắn collider trên child khác với BaseEnemy
            BaseEnemy comp;
            if (!collision.TryGetComponent<BaseEnemy>(out comp))
            {
                comp = collision.GetComponentInParent<BaseEnemy>();
            }
            if (comp != null)
            {
                float dmg = (float)comp.ContactDamage;
                EventBus.Emit(GameEvent.PlayerDamaged, dmg);
            }
            else
            {
                Debug.LogWarning($"[PlayerCollision] Collider tag=Enemy nhưng không tìm thấy BaseEnemy trên {collision.name}. Kiểm tra prefab hoặc tag.");
            }
        }

        // check đạn enemy
        if (collision.CompareTag("EnemyBullet"))
        {
            EnemyBullet bullet;
            if (!collision.TryGetComponent<EnemyBullet>(out bullet))
            {
                bullet = collision.GetComponentInParent<EnemyBullet>();
            }
            if (bullet != null)
            {
                float dmg = bullet.Damage;
                EventBus.Emit(GameEvent.PlayerDamaged, dmg);
            }
            else
            {
                Debug.LogWarning($"[PlayerCollision] Collider tag=EnemyBullet nhưng không tìm thấy EnemyBullet component trên {collision.name}.");
            }
        }
    }
}
