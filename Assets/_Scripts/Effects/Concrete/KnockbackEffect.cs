using UnityEngine;

public class KnockbackEffect : IEffect
{
    public bool IsFinished { get; private set; } = false;

    private Vector2 _direction;
    private float _force;

    public KnockbackEffect(Vector2 direction, float force)
    {
        _direction = direction;
        _force = force;
    }
    
    public void OnAttached(IAffectable target)
    {
        if ((target as MonoBehaviour).TryGetComponent<BaseEnemy>(out var enemy))
        {
            enemy.ApplyKnockback(_direction, _force);
        }
        IsFinished = true;
    }

    public void Tick(IAffectable target) { }
    public void OnDetached(IAffectable target) { }
}