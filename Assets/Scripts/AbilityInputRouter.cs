// Assets/Scripts/AbilityInputRouter.cs
using System.Reflection;
using UnityEngine;

/// <summary>
/// Routes Alpha1–Alpha4 (top number row) to the components that AbilityManager
/// resolved for ability1/ability2/ability3/ultimate. It looks for a no-arg trigger
/// method on those components with one of these names: TryUse/Use/Activate/Cast/TryCast.
/// </summary>
public class AbilityInputRouter : MonoBehaviour
{
    public AbilityManager abilityManager;

    [Header("Keys (top number row)")]
    public KeyCode key1 = KeyCode.Alpha1;
    public KeyCode key2 = KeyCode.Alpha2;
    public KeyCode key3 = KeyCode.Alpha3;
    public KeyCode key4 = KeyCode.Alpha4;

    // Local list so we don't depend on AbilityManager internals
    static readonly string[] MethodOrder = { "TryUse", "Use", "Activate", "Cast", "TryCast" };

    void Reset()
    {
        abilityManager = GetComponent<AbilityManager>();
    }

    void Awake()
    {
        if (!abilityManager) abilityManager = GetComponent<AbilityManager>();
    }

    void Update()
    {
        if (!abilityManager) return;
        if (GameLoop.I != null && GameLoop.I.State != GameState.Playing) return;

        if (Input.GetKeyDown(key1)) Trigger(abilityManager.ability1);
        if (Input.GetKeyDown(key2)) Trigger(abilityManager.ability2);
        if (Input.GetKeyDown(key3)) Trigger(abilityManager.ability3);
        if (Input.GetKeyDown(key4)) Trigger(abilityManager.ultimate);
    }

    void Trigger(Component comp)
    {
        if (!comp) return;

        var type = comp.GetType();
        var m = FindTriggerMethod(type);
        if (m != null)
        {
            m.Invoke(comp, null);
            return;
        }

        // Extra-forgiving fallback: SendMessage to the GameObject
        foreach (var name in MethodOrder)
            comp.gameObject.SendMessage(name, SendMessageOptions.DontRequireReceiver);
    }

    static MethodInfo FindTriggerMethod(System.Type t)
    {
        foreach (var name in MethodOrder)
        {
            var m = t.GetMethod(
                name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                types: System.Type.EmptyTypes,
                modifiers: null
            );
            if (m != null) return m;
        }
        return null;
    }
}
