// UpgradeBase.cs
using UnityEngine;

public abstract class UpgradeBase : ScriptableObject
{
    public string title;
    [TextArea] public string description;
    public int maxStacks = 5;
    public abstract void Apply(GameObject player);
}
