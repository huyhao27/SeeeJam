
public interface IAffectable
{
    void AddEffect(IEffect effect);
}

public interface IEffect
{
    void Tick(IAffectable target);
    
    void OnAttached(IAffectable target);
    
    void OnDetached(IAffectable target);
    
    bool IsFinished { get; }
}