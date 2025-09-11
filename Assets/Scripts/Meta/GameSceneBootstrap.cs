using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class GameSceneBootstrap : MonoBehaviour
{
    [Header("Player Spawning")]
    public GameObject defaultPlayerPrefab;
    public Transform fallbackSpawnPoint;
    public bool forcePlayerTag = true;

    void Start()
    {
        Time.timeScale = 1f;
        SelectedChoice.LogState("GameScene.Start (before bootstrap)");
        StartCoroutine(BootstrapRoutine());
    }

    IEnumerator BootstrapRoutine()
    {
        // Let any scene objects initialize first
        yield return null;

        Transform player = FindExistingPlayer();
        GameObject playerGO;

        if (player)
        {
            playerGO = player.gameObject;
            Debug.Log("[GameSceneBootstrap] Using existing player in scene.");
        }
        else
        {
            var prefab = SelectedChoice.PlayerPrefab != null ? SelectedChoice.PlayerPrefab : defaultPlayerPrefab;
            if (!prefab)
            {
                Debug.LogError("[GameSceneBootstrap] No SelectedChoice prefab and no defaultPlayerPrefab. Cannot create a player.");
                yield break;
            }
            playerGO = Instantiate(prefab, GetSpawnPos(), GetSpawnRot());
            Debug.Log($"[GameSceneBootstrap] Spawned player prefab '{prefab.name}'.");
        }

        if (forcePlayerTag && playerGO.tag != "Player")
            playerGO.tag = "Player";

        var agent = playerGO.GetComponent<NavMeshAgent>();
        if (agent && !agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(playerGO.transform.position, out var hit, 5f, NavMesh.AllAreas))
                agent.Warp(hit.position);
        }

        var setup = playerGO.GetComponent<PlayerSetup>();
        if (!setup) setup = playerGO.AddComponent<PlayerSetup>();

        if (!setup.abilityManager)
        {
            var am = playerGO.GetComponent<AbilityManager>();
            if (!am) am = playerGO.AddComponent<AbilityManager>();
            setup.abilityManager = am;
        }

        if (SelectedChoice.CharacterData != null)
        {
            Debug.Log("[GameSceneBootstrap] Applying CharacterData from SelectedChoice…");
            setup.ApplyData(SelectedChoice.CharacterData);
        }
        else
        {
            Debug.LogWarning("[GameSceneBootstrap] SelectedChoice.CharacterData is NULL; player will keep defaults.");
        }

        // Optional: start your loop
        if (GameLoop.I != null) GameLoop.I.StartRun();
    }

    Transform FindExistingPlayer()
    {
        var tagged = GameObject.FindGameObjectWithTag("Player");
        if (tagged) return tagged.transform;

        var setup = Object.FindFirstObjectByType<PlayerSetup>();
        return setup ? setup.transform : null;
    }

    Vector3 GetSpawnPos()
    {
        var t = fallbackSpawnPoint ? fallbackSpawnPoint : FindTaggedSpawn();
        return t ? t.position : Vector3.zero;
    }

    Quaternion GetSpawnRot()
    {
        var t = fallbackSpawnPoint ? fallbackSpawnPoint : FindTaggedSpawn();
        return t ? t.rotation : Quaternion.identity;
    }

    Transform FindTaggedSpawn()
    {
        var go = GameObject.FindGameObjectWithTag("PlayerSpawn");
        return go ? go.transform : null;
    }
}
