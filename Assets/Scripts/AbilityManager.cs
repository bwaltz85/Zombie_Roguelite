using System.Reflection;
using UnityEngine;

/// <summary>
/// Spawns ability prefabs and tracks the first component that exposes a trigger method
/// (TryUse/Use/Activate/Cast/TryCast). Passive is tracked as a spawned GameObject.
/// This version NEVER attempts to SetParent() on prefab-asset transforms.
/// </summary>
public class AbilityManager : MonoBehaviour
{
    [Header("Equipped Abilities (resolved components)")]
    public Component ability1;
    public Component ability2;
    public Component ability3;
    public Component ultimate;

    [Header("Passive (spawned instance)")]
    public GameObject passiveInstance;

    [Header("Attach Points (instance-only)")]
    [Tooltip("If this points to a prefab asset, it will be ignored and replaced by a child on this instance.")]
    public Transform abilityAttach;

    [Tooltip("If this points to a prefab asset, it will be ignored and replaced by a child on this instance.")]
    public Transform passiveAttach;

    [Tooltip("Names used if we need to create/find instance children.")]
    public string abilityAttachName = "AbilityAttach";
    public string passiveAttachName = "PassiveAttach";

    public static readonly string[] TriggerMethodOrder = { "TryUse", "Use", "Activate", "Cast", "TryCast" };

    void Awake()
    {
        // Make sure fields point to THIS instance (or create them)
        abilityAttach = EnsureInstanceAttach(abilityAttach, abilityAttachName);
        passiveAttach = EnsureInstanceAttach(passiveAttach, passiveAttachName);
    }

    // ---------- Public API ----------

    public void SetAbilitiesFromPrefabs(GameObject a1Prefab, GameObject a2Prefab, GameObject a3Prefab, GameObject ultPrefab)
    {
        ability1 = DestroyAbilityIfExists(ability1);
        ability2 = DestroyAbilityIfExists(ability2);
        ability3 = DestroyAbilityIfExists(ability3);
        ultimate = DestroyAbilityIfExists(ultimate);

        if (a1Prefab) ability1 = SpawnAbility(a1Prefab, "Ability1");
        if (a2Prefab) ability2 = SpawnAbility(a2Prefab, "Ability2");
        if (a3Prefab) ability3 = SpawnAbility(a3Prefab, "Ability3");
        if (ultPrefab) ultimate = SpawnAbility(ultPrefab, "Ultimate");
    }

    public void SetPassiveFromPrefab(GameObject passivePrefab)
    {
        if (passiveInstance)
        {
            Destroy(passiveInstance);
            passiveInstance = null;
        }

        if (!passivePrefab) return;

        passiveAttach = EnsureInstanceAttach(passiveAttach, passiveAttachName);
        var parent = passiveAttach ? passiveAttach : transform;

        passiveInstance = Instantiate(passivePrefab, parent);
        passiveInstance.name = passivePrefab.name;
        Debug.Log($"[AbilityManager] Spawned Passive instance '{passiveInstance.name}' under '{parent.name}'.");
    }

    // ---------- Helpers ----------

    /// <summary>
    /// Returns an attach Transform that is guaranteed to be under THIS instance.
    /// If the provided transform is null or belongs to a prefab asset (persistent), a child with the given name is found/created.
    /// </summary>
    Transform EnsureInstanceAttach(Transform provided, string defaultName)
    {
        // If provided exists AND belongs to this scene instance, keep it.
        if (provided && provided.gameObject.scene.IsValid() && provided.IsChildOf(transform))
            return provided;

        // Try to find an existing child with that name under this instance.
        var found = FindDeepChild(transform, defaultName);
        if (!found)
        {
            var go = new GameObject(defaultName);
            go.transform.SetParent(transform, false);
            found = go.transform;
        }

        if (provided && (!provided.gameObject.scene.IsValid() || !provided.IsChildOf(transform)))
        {
            Debug.LogWarning($"[AbilityManager] Ignoring attach '{provided.name}' because it is not an instance child (likely a Prefab Asset). Using '{found.name}' on the player instance instead.");
        }

        return found;
    }

    static Transform FindDeepChild(Transform root, string name)
    {
        foreach (Transform child in root)
        {
            if (child.name == name) return child;
            var deep = FindDeepChild(child, name);
            if (deep) return deep;
        }
        return null;
    }

    Component SpawnAbility(GameObject prefab, string label)
    {
        abilityAttach = EnsureInstanceAttach(abilityAttach, abilityAttachName);
        var parent = abilityAttach ? abilityAttach : transform;

        var go = Instantiate(prefab, parent);
        go.name = prefab.name;

        var comp = ResolveTriggerComponent(go);
        if (!comp)
        {
            Debug.LogWarning($"[AbilityManager] {label} prefab '{prefab.name}' has no component with a trigger method ({string.Join("/", TriggerMethodOrder)}).");
        }
        else
        {
            Debug.Log($"[AbilityManager] Spawned {label} '{go.name}' and resolved trigger on component '{comp.GetType().Name}'.");
        }
        return comp;
    }

    Component DestroyAbilityIfExists(Component comp)
    {
        if (comp) Destroy(comp.gameObject);
        return null;
    }

    Component ResolveTriggerComponent(GameObject go)
    {
        // Prefer root, then children
        var monos = go.GetComponents<MonoBehaviour>();
        var found = FindWithTrigger(monos);
        if (found) return found;

        monos = go.GetComponentsInChildren<MonoBehaviour>(true);
        return FindWithTrigger(monos);
    }

    Component FindWithTrigger(MonoBehaviour[] list)
    {
        foreach (var mb in list)
        {
            if (!mb) continue;
            var t = mb.GetType();
            foreach (var name in TriggerMethodOrder)
            {
                var m = t.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, System.Type.EmptyTypes, null);
                if (m != null) return mb;
            }
        }
        return null;
    }
}
