using UnityEngine;

[CreateAssetMenu(menuName = "Game/Upgrade/StatUpgrade")]
public class UpgradeStat : UpgradeBase
{
    public StatType statType;
    public float value;

    public override void Apply()
    {
        var stats = PlayerStats.Instance;
        if (stats == null) return;

        switch (statType)
        {
            case StatType.MoveSpeed:
                stats.MoveSpeed += value;
                break;
            case StatType.MaxHpAndHeal:
                EventBus.Emit(GameEvent.Heal, value); 
                stats.MaxHp += value;
                EventBus.Emit(GameEvent.MaxHpChanged, stats.MaxHp);
                break;
            case StatType.CollectRadius:
                stats.CollectRadius += value;
                break;
            case StatType.NoteCount:
                stats.NoteCount += (int)value;
                break;
        }
    }
}

public enum StatType
{
    MoveSpeed,
    MaxHpAndHeal,
    CollectRadius,
    NoteCount,
}