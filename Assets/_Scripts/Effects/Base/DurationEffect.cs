using UnityEngine;

public abstract class DurationEffect : IEffect
{
    public bool IsFinished { get; protected set; }
    protected float duration; 

    protected DurationEffect(float duration)
    {
        this.duration = duration;
    }

    public virtual void Tick(IAffectable target)
    {
        duration -= Time.deltaTime;
        if (duration <= 0)
        {
            IsFinished = true;
        }
    }

    public virtual void OnAttached(IAffectable target) { }
    public virtual void OnDetached(IAffectable target) { }
}