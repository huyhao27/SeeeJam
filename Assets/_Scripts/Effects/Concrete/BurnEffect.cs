public class BurnEffect : DurationEffect
{
    public BurnEffect(float duration, float damagePerSecond) : base(duration)
    {
        // TODO: Lưu lại giá trị damagePerSecond
    }

    public override void Tick(IAffectable target)
    {
        base.Tick(target); // Gọi Tick của lớp cha để đếm ngược thời gian
        
        // TODO: Gây sát thương mỗi giây cho target
        // Ví dụ: target.GetComponent<PlayerHealth>().TakeDamage(...);
    }
}

public class SlowEffect : DurationEffect
{
    public SlowEffect(float duration, float slowFactor) : base(duration)
    {
        // TODO: Lưu lại giá trị slowFactor (ví dụ: 0.5f cho 50% slow)
    }

    public override void OnAttached(IAffectable target)
    {
        base.OnAttached(target);
        // TODO: Giảm tốc độ di chuyển của target
        // Ví dụ: target.GetComponent<PlayerMovement>().moveSpeed *= slowFactor;
    }

    public override void OnDetached(IAffectable target)
    {
        base.OnDetached(target);
        // TODO: Hoàn lại tốc độ di chuyển ban đầu của target
        // Ví dụ: target.GetComponent<PlayerMovement>().moveSpeed /= slowFactor;
    }
}
