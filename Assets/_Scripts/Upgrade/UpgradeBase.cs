using UnityEngine;

public abstract class UpgradeBase : ScriptableObject
{
    public string upgradeName;
    public string description;
    public Sprite icon;

    public abstract void Apply(PlayerStats stats);
}