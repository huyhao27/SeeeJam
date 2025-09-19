using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : Singleton<PlayerStats>
{
    [SerializeField] private PlayerData _data;
    public PlayerData Data => _data;

    // runtime values
    public float MoveSpeed;
    public float MaxHp;
    public int NoteCount;
    public float CollectRadius;
    public float DashCoolDown;

    public List<BaseSkill> Skills;

    protected override void Awake()
    {
        // base.Awake();
        ResetStat();
    }

    public void ResetStat()
    {
        MoveSpeed = Data.MoveSpeed;
        MaxHp = Data.MaxHp;
        NoteCount = Data.NoteCount;
        CollectRadius = Data.CollectRadius;
        DashCoolDown = Data.DashCoolDown;
        Skills = new List<BaseSkill>(Data.Skills);
    }

}




