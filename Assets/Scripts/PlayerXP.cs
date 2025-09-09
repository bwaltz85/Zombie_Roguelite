// PlayerXP.cs
using UnityEngine;

public class PlayerXP : MonoBehaviour
{
    public static int Level { get; private set; }
    public static int XP { get; private set; }
    public static int NextXP { get; private set; } = 5;

    /// <summary>Add XP and handle level-ups.</summary>
    public static void Add(int amount)
    {
        XP += Mathf.Max(0, amount);

        bool leveled = false;
        while (XP >= NextXP)
        {
            XP -= NextXP;
            Level++;
            NextXP = Mathf.RoundToInt(NextXP * 1.6f);
            leveled = true;
        }

        if (leveled)
        {
            Debug.Log($"[XP] Level Up! New Level: {Level}, NextXP: {NextXP}");

            // Show upgrade choices (if director exists)
            var dir = Object.FindFirstObjectByType<UpgradeDirector>();
            if (dir) dir.OfferChoices();
        }
    }
}
