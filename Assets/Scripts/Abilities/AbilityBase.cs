using UnityEngine;

public abstract class AbilityBase : MonoBehaviour
{
    [Header("Cooldown")]
    public float cooldown = 5f;
    [Range(0.25f, 4f)] public float cooldownMultiplier = 1f;

    private float _nextReadyTime;

    protected virtual bool CanCast() => Time.time >= _nextReadyTime;

    public bool TryCast()
    {
        if (!CanCast()) return false;
        if (!OnCast()) return false; // ability can veto (e.g., no target)
        _nextReadyTime = Time.time + cooldown * Mathf.Max(0.01f, cooldownMultiplier);
        return true;
    }

    // Implement the actual effect in subclasses. Return true if it fired.
    protected abstract bool OnCast();

    // Optional helper if you later make a cooldown UI
    public float CooldownRemaining() => Mathf.Max(0f, _nextReadyTime - Time.time);
    public float CooldownPercent() => cooldown <= 0f ? 0f : Mathf.Clamp01(CooldownRemaining() / (cooldown * Mathf.Max(0.01f, cooldownMultiplier)));
}
