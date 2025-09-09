using UnityEngine;

[CreateAssetMenu(menuName = "Game/Character", fileName = "NewCharacter")]
public class CharacterData : ScriptableObject
{
    public string characterName = "Unnamed";
    public Sprite portrait;

    [Header("Base Stats")]
    public float maxHP = 100f;
    public float moveSpeed = 7f;

    [Header("Starting Loadout")]
    public GameObject startingWeaponPrefab;   // optional child prefab if you need one

    [Header("Active Abilities (Q / E / F)")]
    public GameObject active1Prefab;  // e.g., Dash
    public GameObject active2Prefab;  // e.g., Grenade
    public GameObject active3Prefab;  // e.g., Shield

    [Header("Ultimate (R)")]
    public GameObject ultimateAbilityPrefab;  // e.g., Shockwave

    [Header("Passive")]
    public PassiveBase passive;
    [TextArea] public string passiveDescription;
}
