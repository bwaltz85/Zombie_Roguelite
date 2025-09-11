using UnityEngine;

/// <summary>
/// Applies CharacterData to the player at spawn/scene load.
/// Ensures AbilityManager exists and attach points are rebound to the instance.
/// </summary>
public class PlayerSetup : MonoBehaviour
{
    [Header("References")]
    public AbilityManager abilityManager;

    [Header("Attach Points (optional; instance children will be used/created)")]
    public Transform abilityAttach; // forwarded into AbilityManager if set
    public Transform passiveAttach; // forwarded into AbilityManager if set

    void Reset()
    {
        abilityManager = GetComponent<AbilityManager>();
    }

    void Awake()
    {
        if (!abilityManager) abilityManager = GetComponent<AbilityManager>();
        if (!abilityManager) abilityManager = gameObject.AddComponent<AbilityManager>();

        // Forward any explicitly set attach points (they will be rebound in AbilityManager.Awake)
        if (abilityAttach) abilityManager.abilityAttach = abilityAttach;
        if (passiveAttach) abilityManager.passiveAttach = passiveAttach;
    }

    public void ApplyData(CharacterData data)
    {
        if (!data)
        {
            Debug.LogWarning("[PlayerSetup] ApplyData called with null CharacterData.");
            return;
        }

        if (!abilityManager)
        {
            abilityManager = GetComponent<AbilityManager>();
            if (!abilityManager) abilityManager = gameObject.AddComponent<AbilityManager>();
        }

        abilityManager.SetAbilitiesFromPrefabs(
            data.ability1Prefab,
            data.ability2Prefab,
            data.ability3Prefab,
            data.ultimatePrefab
        );

        abilityManager.SetPassiveFromPrefab(data.passivePrefab);

        Debug.Log(
            "[PlayerSetup] Applied CharacterData '" + data.name + "' to '" + gameObject.name + "'.\n" +
            "- ability1: " + (abilityManager.ability1 ? abilityManager.ability1.GetType().Name : "NULL") + "\n" +
            "- ability2: " + (abilityManager.ability2 ? abilityManager.ability2.GetType().Name : "NULL") + "\n" +
            "- ability3: " + (abilityManager.ability3 ? abilityManager.ability3.GetType().Name : "NULL") + "\n" +
            "- ultimate: " + (abilityManager.ultimate ? abilityManager.ultimate.GetType().Name : "NULL") + "\n" +
            "- passiveInstance: " + (abilityManager.passiveInstance ? abilityManager.passiveInstance.name : "NULL")
        );
    }
}
