using UnityEngine;

public abstract class UpgradeBase : ScriptableObject
{
    public string upgradeName;
    [TextArea] public string description; 
    public Sprite icon;

    public abstract void Apply();
}