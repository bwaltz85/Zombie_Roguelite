using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Game/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Info")]
    public string characterName = "New Character";
    public Sprite portrait;

    [Header("Player Prefab (optional)")]
    [Tooltip("If you want to spawn a specific player prefab per character from data.")]
    public GameObject playerPrefab;

    [Header("Abilities (1-4)")]
    [Tooltip("Triggered by Alpha1")]
    public GameObject ability1Prefab;

    [Tooltip("Triggered by Alpha2")]
    public GameObject ability2Prefab;

    [Tooltip("Triggered by Alpha3")]
    public GameObject ability3Prefab;

    [Tooltip("Triggered by Alpha4 (ultimate)")]
    public GameObject ultimatePrefab;

    [Header("Passive")]
    public GameObject passivePrefab;
}
