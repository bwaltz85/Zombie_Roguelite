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
    public Transform[] spawnPoints;   // assign in Inspector

    [Header("Around Player Fallback (optional)")]
    public Transform player;          // assign Player root if you want near-player spawning
    public float spawnRadius = 20f;   // farthest distance from player
    public float minDistanceFromPlayer = 8f; // don’t spawn too close

    [Header("NavMesh Safety")]
    public bool snapToNavMesh = true;         // keep this ON
    public float navMeshSnapMaxDist = 8f;     // how far we’ll search to snap onto a mesh
    public bool requireReachableFromPlayer = true; // avoid isolated islands

    float _timer;

    void OnValidate()
    {
        if (spawnInterval < 0.1f) spawnInterval = 0.1f;
        if (enemiesPerWave < 0) enemiesPerWave = 0;
        if (spawnRadius < 0f) spawnRadius = 0f;
        if (minDistanceFromPlayer < 0f) minDistanceFromPlayer = 0f;
        if (navMeshSnapMaxDist < 0.1f) navMeshSnapMaxDist = 0.1f;

        // trim null spawn points
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

    void Update()
    {
        // Only spawn during gameplay
        if (GameLoop.I == null || GameLoop.I.State != GameState.Playing) return;

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            SpawnWave();
            _timer = Mathf.Max(0.1f, spawnInterval);
        }
    }

    // === Public APIs ===

    public void SpawnWave()
    {
        if (!enemyPrefab)
        {
            Debug.LogError("[EnemySpawner] enemyPrefab not assigned.");
            return;
        }

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

    // used by WaveDirector (scaling)
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

    // === Internals ===

    void SpawnAtPoint(Transform point, float healthMultiplier = 1f)
    {
        if (!point) return;

        Vector3 pos = point.position;
        Quaternion rot = point.rotation;

        if (snapToNavMesh && !TrySnapToNavMesh(pos, out pos))
        {
            Debug.LogWarning("[EnemySpawner] Spawn point off NavMesh; skipping.");
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
            // optional fallback: anywhere on navmesh
            if (!TryPickRandomPointOnNavMesh(out pos))
            {
                // give up silently to avoid spam
                return;
            }
        }

        var enemy = SpawnEnemy(pos, Quaternion.identity);
        ApplyHealthMult(enemy, healthMultiplier);
    }

    bool TryGetSpawnPositionNearPlayer(Vector3 center, float minR, float maxR, out Vector3 pos)
    {
        // try several angles/radii around the player, snapping each to NavMesh
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

        // random point in triangle
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

    GameObject SpawnEnemy(Vector3 pos, Quaternion rot)
    {
        var go = Instantiate(enemyPrefab, pos, rot);

        // ensure agent actually lands on the mesh
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
}
