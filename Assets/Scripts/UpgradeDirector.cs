// UpgradeDirector.cs
using UnityEngine;
using System.Linq;

public class UpgradeDirector : MonoBehaviour
{
    [Header("Catalog")]
    public UpgradeBase[] allUpgrades;

    [Header("Choices")]
    public int choices = 3;
    public KeyCode[] choiceKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3 };

    private UpgradeBase[] currentOffer = new UpgradeBase[0];
    private bool showing;

    /// <summary>Pause the game and present upgrade choices.</summary>
    public void OfferChoices()
    {
        if (allUpgrades == null || allUpgrades.Length == 0)
        {
            Debug.LogWarning("[UpgradeDirector] No upgrades assigned.");
            return;
        }

        // Pick random unique upgrades
        currentOffer = allUpgrades
            .OrderBy(_ => Random.value)
            .Take(Mathf.Clamp(choices, 1, allUpgrades.Length))
            .ToArray();

        showing = true;
        Time.timeScale = 0f;

        // Debug text prompt (replace with UI later)
        string list = string.Join("  ",
            currentOffer.Select((u, i) => $"{i + 1}) {u.title}")
        );
        Debug.Log($"[UpgradeDirector] Choose: {list}");
    }

    void Update()
    {
        if (!showing) return;

        for (int i = 0; i < currentOffer.Length && i < choiceKeys.Length; i++)
        {
            if (Input.GetKeyDown(choiceKeys[i]))
            {
                ApplyChoice(i);
                break;
            }
        }
    }

    private void ApplyChoice(int index)
    {
        var pc = FindFirstObjectByType<TopDownPlayerController>();
        if (!pc)
        {
            Debug.LogWarning("[UpgradeDirector] No player found to apply upgrade.");
            CancelOffer();
            return;
        }

        var upgrade = currentOffer[index];
        if (upgrade != null)
        {
            upgrade.Apply(pc.gameObject);
            Debug.Log($"[UpgradeDirector] Applied: {upgrade.title}");
        }
        else
        {
            Debug.LogWarning("[UpgradeDirector] Selected upgrade was null.");
        }

        CancelOffer();
    }

    private void CancelOffer()
    {
        showing = false;
        Time.timeScale = 1f;
        currentOffer = System.Array.Empty<UpgradeBase>();
    }
}
