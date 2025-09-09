using UnityEngine;

public abstract class PassiveBase : ScriptableObject
{
    // Called once when the character spawns / is applied
    public abstract void ApplyOnSpawn(GameObject player);

    // Optional hook for later (kill triggers, etc.)
    public virtual void OnEnemyKilled(GameObject player, GameObject enemy) { }
}
