using UnityEngine;
using Unity.Cinemachine; // <- CM3
using UnityEngine.InputSystem;

public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner I;

    [Header("Required")]
    public GameObject playerPrefab;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;
    public bool pickRandomSpawn = true;

    [Header("Character Loadout")]
    public CharacterData defaultCharacter;

    [Header("Camera Hookup (CM3)")]
    public CinemachineCamera vcam; // CM3 camera

    [Header("GameLoop")]
    public bool startRunAfterSpawn = true;

    [HideInInspector] public GameObject currentPlayer;

    void Awake()
    {
        if (I == null) I = this; else { Destroy(gameObject); return; }
    }

    void Start()
    {
        if (currentPlayer == null) Spawn(defaultCharacter);
    }

    public GameObject Spawn(CharacterData characterOverride = null)
    {
        if (currentPlayer) Destroy(currentPlayer);

        Transform t = transform;
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int idx = pickRandomSpawn ? Random.Range(0, spawnPoints.Length) : 0;
            if (spawnPoints[idx]) t = spawnPoints[idx];
        }

        currentPlayer = Instantiate(playerPrefab, t.position, t.rotation);

        // Apply character data (stats, abilities, passive)
        var setup = currentPlayer.GetComponent<PlayerSetup>() ?? currentPlayer.AddComponent<PlayerSetup>();
        var cd = characterOverride != null ? characterOverride : defaultCharacter;
        if (cd) setup.ApplyData(cd);

        // Hook CM3 camera follow
        if (vcam) vcam.Follow = currentPlayer.transform;

        // PlayerInput camera ref (useful for look-based actions)
        var pi = currentPlayer.GetComponent<PlayerInput>();
        if (pi && Camera.main) pi.camera = Camera.main;

        if (startRunAfterSpawn && GameLoop.I) GameLoop.I.StartRun();
        return currentPlayer;
    }
}
