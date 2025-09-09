using UnityEngine;
using UnityEngine.InputSystem;

public class AbilityManager : MonoBehaviour
{
    [Header("Slots")]
    public AbilityBase active1;   // key 1
    public AbilityBase active2;   // key 2
    public AbilityBase active3;   // key 3
    public AbilityBase ultimate;  // key 4

    // Runtime input actions (no .inputactions asset needed)
    InputAction cast1, cast2, cast3, cast4;

    void Awake()
    {
        // Bind number keys 1–4
        cast1 = new InputAction("Cast1", InputActionType.Button, "<Keyboard>/1");
        cast2 = new InputAction("Cast2", InputActionType.Button, "<Keyboard>/2");
        cast3 = new InputAction("Cast3", InputActionType.Button, "<Keyboard>/3");
        cast4 = new InputAction("Cast4", InputActionType.Button, "<Keyboard>/4");

        // Subscribe once; enabling/disabling handled below
        cast1.performed += _ => { if (GameLoop.I && GameLoop.I.State == GameState.Playing) active1?.TryCast(); };
        cast2.performed += _ => { if (GameLoop.I && GameLoop.I.State == GameState.Playing) active2?.TryCast(); };
        cast3.performed += _ => { if (GameLoop.I && GameLoop.I.State == GameState.Playing) active3?.TryCast(); };
        cast4.performed += _ => { if (GameLoop.I && GameLoop.I.State == GameState.Playing) ultimate?.TryCast(); };
    }

    void OnEnable()
    {
        cast1?.Enable(); cast2?.Enable(); cast3?.Enable(); cast4?.Enable();
    }

    void OnDisable()
    {
        cast4?.Disable(); cast3?.Disable(); cast2?.Disable(); cast1?.Disable();
    }

    /// <summary>
    /// Clear slot refs so a fresh loadout can be assigned.
    /// </summary>
    public void ClearAbilitySlots()
    {
        active1 = active2 = active3 = ultimate = null;
    }

    /// <summary>
    /// Instantiates ability prefabs (if provided) under 'parent' (or this transform),
    /// and assigns their AbilityBase components into the four slots.
    /// </summary>
    public void SetAbilitiesFromPrefabs(GameObject a1, GameObject a2, GameObject a3, GameObject ult, Transform parent = null)
    {
        Transform p = parent ? parent : transform;

        if (a1)
        {
            var go = Instantiate(a1, p);
            active1 = go.GetComponentInChildren<AbilityBase>();
        }
        if (a2)
        {
            var go = Instantiate(a2, p);
            active2 = go.GetComponentInChildren<AbilityBase>();
        }
        if (a3)
        {
            var go = Instantiate(a3, p);
            active3 = go.GetComponentInChildren<AbilityBase>();
        }
        if (ult)
        {
            var go = Instantiate(ult, p);
            ultimate = go.GetComponentInChildren<AbilityBase>();
        }
    }

    // (Optional) If you ever want to auto-fill from children instead of prefabs:
    public void AssignSlotsFromChildren(Transform root = null)
    {
        var list = (root ? root : transform).GetComponentsInChildren<AbilityBase>(true);
        int idx = 0;
        foreach (var ab in list)
        {
            if (!ab) continue;
            if (idx == 0) active1 = ab;
            else if (idx == 1) active2 = ab;
            else if (idx == 2) active3 = ab;
            else if (idx == 3) { ultimate = ab; break; }
            idx++;
        }
    }
}
