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
            BaseEnemy comp = collision.gameObject.GetComponent<BaseEnemy>();
            float dmg = (float)comp.ContactDamage;
            EventBus.Emit(GameEvent.PlayerDamaged, dmg);
        }

        // check đạn enemy
        if (collision.CompareTag("EnemyBullet"))
        {
            EnemyBullet comp = collision.gameObject.GetComponent<EnemyBullet>();
            float dmg = (float)comp.Damage;
            EventBus.Emit(GameEvent.PlayerDamaged, dmg);
        }
    }
}
