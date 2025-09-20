using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Upgrade/StatUpgrade")]
public class UpgradeStat : UpgradeBase
{
    public StatType statType;
    public float value;

    public override void Apply(PlayerStats stats)
    {
        switch (statType)
        {
            case StatType.MoveSpeed:
                stats.MoveSpeed += value;
                break;
            case StatType.MaxHpAndHeal:
                Debug.Log("ngon");
                stats.MaxHp += value;
                EventBus.Emit(GameEvent.MaxHpChanged, stats.MaxHp);
                EventBus.Emit(GameEvent.Heal, value);
                break;
            case StatType.CollectRadius:
                stats.CollectRadius += value;
                break;
            case StatType.NoteCount:
                stats.NoteCount += (int)value;
                break;
                // thêm case khác... 
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
