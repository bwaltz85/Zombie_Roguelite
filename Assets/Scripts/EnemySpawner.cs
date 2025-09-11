using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy")]
    public GameObject enemyPrefab;

    [Header("Timing")]
    public float spawnInterval = 2f;
    public int enemiesPerWave = 5;

    [Header("Spawn Points (optional)")]
    public Transform[] spawnPoints;

    [Header("Around Player Fallback (optional)")]
    public Transform player;
    public float spawnRadius = 20f;
    public float minDistanceFromPlayer = 8f;

    [Header("NavMesh Safety")]
    public bool snapToNavMesh = true;
    public float navMeshSnapMaxDist = 8f;
    public bool requireReachableFromPlayer = true;

    [Header("Layers (optional)")]
    [Tooltip("If set, newly spawned enemies will be forced onto this layer (and all children).")]
    public string forceEnemyLayerName = "Enemy";
    public bool forceEnemyLayer = true;

    [Header("Diagnostics")]
    public bool logVerbose = true;

    float _timer;

    void OnValidate()
    {
        if (spawnInterval < 0.1f) spawnInterval = 0.1f;
        if (enemiesPerWave < 0) enemiesPerWave = 0;
        if (spawnRadius < 0f) spawnRadius = 0f;
        if (minDistanceFromPlayer < 0f) minDistanceFromPlayer = 0f;
        if (navMeshSnapMaxDist < 0.1f) navMeshSnapMaxDist = 0.1f;

        if (spawnPoints != null)
        {
            int valid = 0;
            for (int i = 0; i < spawnPoints.Length; i++) if (spawnPoints[i] != null) valid++;
            if (valid != spawnPoints.Length)
            {
                var tmp = new Transform[valid]; int j = 0;
                for (int i = 0; i < spawnPoints.Length; i++) if (spawnPoints[i] != null) tmp[j++] = spawnPoints[i];
                spawnPoints = tmp;
            }
        }
    }

    IEnumerator Start()
    {
        if (logVerbose) Debug.Log("[EnemySpawner] Start() begin");

        yield return new WaitUntil(() => GameLoop.I != null);
        if (logVerbose) Debug.Log($"[EnemySpawner] GameLoop found. State={GameLoop.I.State}");

        yield return new WaitUntil(() => GameLoop.I.State == GameState.Playing);
        if (logVerbose) Debug.Log("[EnemySpawner] Game state is Playing");

        if (!player)
        {
            var pgo = GameObject.FindGameObjectWithTag("Player");
            if (pgo) player = pgo.transform;
            if (logVerbose) Debug.Log($"[EnemySpawner] Player reference {(player ? "set" : "NOT found")}");
        }

        if (!enemyPrefab)
        {
            Debug.LogError("[EnemySpawner] enemyPrefab not assigned. No spawns will occur.");
            yield break;
        }

        if ((spawnPoints == null || spawnPoints.Length == 0) && !player)
        {
            Debug.LogWarning("[EnemySpawner] No spawn points and no player set. Cannot determine spawn positions.");
            yield break;
        }

        _timer = 0f; // spawn immediately
        if (logVerbose) Debug.Log("[EnemySpawner] Spawn loop starting.");
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (GameLoop.I != null && GameLoop.I.State == GameState.Playing)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                if (logVerbose) Debug.Log("[EnemySpawner] Spawning wave…");
                SpawnWave();
                _timer = Mathf.Max(0.1f, spawnInterval);
            }
            yield return null;
        }
        if (logVerbose) Debug.Log("[EnemySpawner] Spawn loop stopped (state not Playing).");
    }

    public void SpawnWave()
    {
        if (!enemyPrefab) { Debug.LogError("[EnemySpawner] enemyPrefab not assigned."); return; }

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            for (int i = 0; i < enemiesPerWave; i++)
                SpawnAtPoint(spawnPoints[Random.Range(0, spawnPoints.Length)]);
        }
        else if (player != null)
        {
            for (int i = 0; i < enemiesPerWave; i++)
                SpawnNearPlayer();
        }
        else
        {
            Debug.LogWarning("[EnemySpawner] No spawn points and no player fallback assigned.");
        }
    }

    public void SpawnWaveScaled(int count, float healthMultiplier)
    {
        if (!enemyPrefab) { Debug.LogError("[EnemySpawner] enemyPrefab not assigned."); return; }

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            for (int i = 0; i < count; i++)
                SpawnAtPoint(spawnPoints[Random.Range(0, spawnPoints.Length)], healthMultiplier);
        }
        else if (player != null)
        {
            for (int i = 0; i < count; i++)
                SpawnNearPlayer(healthMultiplier);
        }
        else
        {
            Debug.LogWarning("[EnemySpawner] No spawn configuration set for scaled wave.");
        }
    }

    void SpawnAtPoint(Transform point, float healthMultiplier = 1f)
    {
        if (!point) return;

        Vector3 pos = point.position;
        Quaternion rot = point.rotation;

        if (snapToNavMesh && !TrySnapToNavMesh(pos, out pos))
        {
            if (logVerbose) Debug.LogWarning("[EnemySpawner] Spawn point off NavMesh; skipping.");
            return;
        }

        var enemy = SpawnEnemy(pos, rot);
        ApplyHealthMult(enemy, healthMultiplier);
    }

    void SpawnNearPlayer(float healthMultiplier = 1f)
    {
        if (!player) return;

        if (!TryGetSpawnPositionNearPlayer(player.position, minDistanceFromPlayer, spawnRadius, out var pos))
        {
            if (!TryPickRandomPointOnNavMesh(out pos))
            {
                if (logVerbose) Debug.LogWarning("[EnemySpawner] Could not find a valid spawn position near player or on NavMesh.");
                return;
            }
        }

        var enemy = SpawnEnemy(pos, Quaternion.identity);
        ApplyHealthMult(enemy, healthMultiplier);
    }

    GameObject SpawnEnemy(Vector3 pos, Quaternion rot)
    {
        var go = Instantiate(enemyPrefab, pos, rot);

        // Optionally force layer on all spawned enemies so the weapon mask matches.
        if (forceEnemyLayer && !string.IsNullOrEmpty(forceEnemyLayerName))
        {
            int layer = LayerMask.NameToLayer(forceEnemyLayerName);
            if (layer >= 0)
                SetLayerRecursively(go, layer);
        }

        var agent = go.GetComponent<NavMeshAgent>();
        if (agent && !agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(pos, out var hit, navMeshSnapMaxDist, NavMesh.AllAreas))
                agent.Warp(hit.position);
            else
                Debug.LogWarning("[EnemySpawner] Spawned enemy off NavMesh and couldn’t warp onto it.");
        }
        return go;
    }

    void ApplyHealthMult(GameObject enemy, float mult)
    {
        if (!enemy || mult <= 1.001f) return;
        var eh = enemy.GetComponent<EnemyHealth>();
        if (eh) eh.ApplyHealthMultiplier(mult);
    }

    // ----- Helpers -----

    bool TryGetSpawnPositionNearPlayer(Vector3 center, float minR, float maxR, out Vector3 pos)
    {
        for (int i = 0; i < 12; i++)
        {
            float ang = Random.Range(0f, Mathf.PI * 2f);
            float r = Random.Range(Mathf.Max(1f, minR), Mathf.Max(minR + 1f, maxR));
            Vector3 candidate = center + new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * r;

            if (TrySnapToNavMesh(candidate, out var snapped))
            {
                if (!requireReachableFromPlayer || HasPath(snapped, center))
                {
                    pos = snapped;
                    return true;
                }
            }
        }
        pos = default;
        return false;
    }

    bool TryPickRandomPointOnNavMesh(out Vector3 pos)
    {
        var tri = NavMesh.CalculateTriangulation();
        if (tri.indices == null || tri.indices.Length < 3) { pos = default; return false; }

        int t = Random.Range(0, tri.indices.Length / 3) * 3;
        Vector3 a = tri.vertices[tri.indices[t]];
        Vector3 b = tri.vertices[tri.indices[t + 1]];
        Vector3 c = tri.vertices[tri.indices[t + 2]];

        float r1 = Mathf.Sqrt(Random.value);
        float r2 = Random.value;
        Vector3 p = (1 - r1) * a + r1 * (1 - r2) * b + r1 * r2 * c;

        return TrySnapToNavMesh(p, out pos);
    }

    bool TrySnapToNavMesh(Vector3 candidate, out Vector3 snapped)
    {
        if (!snapToNavMesh)
        {
            snapped = candidate;
            return true;
        }

        if (NavMesh.SamplePosition(candidate, out var hit, navMeshSnapMaxDist, NavMesh.AllAreas))
        {
            snapped = hit.position;
            return true;
        }

        snapped = default;
        return false;
    }

    bool HasPath(Vector3 from, Vector3 to)
    {
        var path = new NavMeshPath();
        if (!NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path)) return false;
        return path.status == NavMeshPathStatus.PathComplete;
    }

    static void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform t in go.transform)
            SetLayerRecursively(t.gameObject, layer);
    }
}
