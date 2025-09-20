using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerStat", menuName = "Player Stat")]
public class PlayerData : ScriptableObject
{
    public float MoveSpeed;
    public List<BaseSkill> Skills;

    public int NoteCount;

    public float MaxHp;

    public float CollectRadius;

    public float DashCoolDown;

    public float vitality;
}