using UnityEngine;

public class Skill2Bullet : BaseBullet
{
    // [Header("Frost Specific")]
    // [SerializeField] private float slowDuration = 3f;
    // [SerializeField] private float slowAmount = 0.5f; 

    protected override void OnHit(GameObject target)
    {
        
    }
    
    protected override void ReturnToPool()
    {
        rb.velocity = Vector2.zero;
        ObjectPool<Skill2Bullet>.Instance.Return(this); 
    }
}