using UnityEngine;

public class StunEffect : DurationEffect
{
    private BaseEnemy _enemy;

    public StunEffect(float duration) : base(duration) { }

    public override void OnAttached(IAffectable target)
    {
        base.OnAttached(target);
        if ((target as MonoBehaviour).TryGetComponent<BaseEnemy>(out _enemy))
        {
            _enemy.SetStunned(true);
        }
    }

    public override void OnDetached(IAffectable target)
    {
        base.OnDetached(target);
        if (_enemy != null)
        {
            _enemy.SetStunned(false);
        }
    }
}