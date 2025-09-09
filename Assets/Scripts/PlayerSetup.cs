using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
    [Tooltip("Optional default for quick tests; your character select will call ApplyData at runtime.")]
    public CharacterData data;

    [Header("Attach points (optional)")]
    public Transform weaponParent;   // where startingWeaponPrefab is placed (defaults to player root)
    public Transform abilityParent;  // where abilities are placed (defaults to player root)

    [Header("Cleanup Options")]
    public bool clearExistingAbilitiesOnApply = true;
    public bool clearExistingWeaponsOnApply = true;

    void Start()
    {
        // For direct Inspector testing: if data is set, apply it on scene start
        if (data != null) ApplyData(data);
    }

    /// <summary>
    /// Applies a CharacterData to this player: clears old loadout, sets stats, passive,
    /// and instantiates 3 actives + 1 ultimate abilities and optional starting weapon.
    /// Safe to call whenever you (re)select a character.
    /// </summary>
    public void ApplyData(CharacterData cd)
    {
        if (cd == null)
        {
            Debug.LogWarning("[PlayerSetup] ApplyData called with null CharacterData.");
            return;
        }

        data = cd;

        // 1) Clear previous abilities & weapon
        if (clearExistingAbilitiesOnApply) ClearAbilities();
        if (clearExistingWeaponsOnApply) ClearStartingWeapon();

        // 2) Base stats
        var hp = GetComponent<PlayerHealth>();
        if (hp)
        {
            // Adjust current max to target max; heal to full after change
            float delta = cd.maxHP - hp.maxHP;
            hp.AddMaxHealth(delta, healToFull: true);
        }

        var ctrl = GetComponent<TopDownPlayerController>();
        if (ctrl) ctrl.moveSpeed = cd.moveSpeed;

        // 3) Passive
        if (cd.passive) cd.passive.ApplyOnSpawn(gameObject);

        // 4) Abilities (3 actives + 1 ultimate)
        var mgr = GetComponent<AbilityManager>();
        if (!mgr) mgr = gameObject.AddComponent<AbilityManager>();
        mgr.ClearAbilitySlots();

        Transform abilRoot = abilityParent ? abilityParent : transform;
        mgr.SetAbilitiesFromPrefabs(cd.active1Prefab, cd.active2Prefab, cd.active3Prefab, cd.ultimateAbilityPrefab, abilRoot);

        // 5) Starting weapon (optional)
        if (cd.startingWeaponPrefab)
        {
            Transform wRoot = weaponParent ? weaponParent : transform;
            Instantiate(cd.startingWeaponPrefab, wRoot);
        }

        Debug.Log($"[PlayerSetup] Applied character: {cd.characterName}");
    }

    // ---- Helpers ----

    void ClearAbilities()
    {
        Transform root = abilityParent ? abilityParent : transform;
        var abilityInstances = root.GetComponentsInChildren<AbilityBase>(includeInactive: true);
        foreach (var ab in abilityInstances)
        {
            // Destroy the GameObject that holds the ability component (often a small child prefab)
            if (ab) Destroy(ab.gameObject);
        }
    }

    void ClearStartingWeapon()
    {
        if (!weaponParent) return;

        // Simple strategy: remove all children under the weaponParent.
        // If you later mix other things under this parent, add tags/layers to filter.
        for (int i = weaponParent.childCount - 1; i >= 0; i--)
        {
            var child = weaponParent.GetChild(i);
            Destroy(child.gameObject);
        }
    }
}
