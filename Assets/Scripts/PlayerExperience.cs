using UnityEngine;

public class PlayerExperience : MonoBehaviour
{
    [Header("XP / Level")]
    public int level = 1;
    public int currentXP = 0;
    public int xpToNext = 10;

    [Header("Scaling")]
    [Tooltip("Multiplier applied to the XP needed each level (>= 1).")]
    public float xpGrowth = 1.25f;
    [Tooltip("Flat amount added to XP needed each level.")]
    public int xpFlatIncrement = 2;

    [Header("Debug")]
    public bool logLevelUps = true;

    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        currentXP += amount;

        while (currentXP >= xpToNext)
        {
            currentXP -= xpToNext;
            LevelUp();
        }
    }

    void LevelUp()
    {
        level++;
        if (logLevelUps)
            Debug.Log($"[PlayerExperience] LEVEL UP! -> L{level}");

        // Recompute xpToNext
        float next = xpToNext * Mathf.Max(1f, xpGrowth) + xpFlatIncrement;
        xpToNext = Mathf.Clamp(Mathf.RoundToInt(next), 1, int.MaxValue);

        // TODO: trigger upgrade UI, grant stat points, etc.
    }
}
