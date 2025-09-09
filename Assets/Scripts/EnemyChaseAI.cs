using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyChaseAI : MonoBehaviour
{
    [Header("Targeting")]
    public float retargetInterval = 0.25f;
    public float repathEvery = 0.25f;
    public float maxChaseDistance = 100f;
    public string playerTagFallback = "Player";

    private NavMeshAgent agent;
    private Transform target;
    private float retargetTimer, repathTimer;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError($"{name}: Missing NavMeshAgent component.");
            enabled = false; // hard fail: script can’t run
            return;
        }

        // sane defaults
        agent.isStopped = false;
        agent.updatePosition = true;
        agent.updateRotation = true;
        if (agent.speed <= 0f) agent.speed = 3.5f;
        if (agent.acceleration <= 0f) agent.acceleration = 12f;
        if (agent.angularSpeed <= 0f) agent.angularSpeed = 720f;
        if (agent.stoppingDistance < 0.1f) agent.stoppingDistance = 1.0f;
    }

    void OnEnable()
    {
        // Snap to mesh if spawned slightly off
        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                agent.Warp(hit.position);
            else
                Debug.LogWarning($"{name}: No NavMesh under spawn position.");
        }

        retargetTimer = Random.Range(0f, retargetInterval);
        repathTimer = Random.Range(0f, repathEvery);
    }

    void Update()
    {
        if (agent == null) return;           // safety
        if (!agent.isOnNavMesh) return;      // wait until placed on mesh

        // Periodically choose nearest player
        retargetTimer -= Time.deltaTime;
        if (retargetTimer <= 0f)
        {
            retargetTimer = retargetInterval;
            target = FindNearestPlayer();
            // If no target, just wait for next tick instead of NRE
            if (target == null) return;
        }

        // Keep path fresh toward moving target
        if (target == null) return; // target could have despawned this frame

        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f)
        {
            repathTimer = repathEvery;
            bool ok = agent.SetDestination(target.position);
            // Optional debug:
            // Debug.Log($"{name} -> SetDestination ok:{ok} to {target.name}");
        }
    }

    Transform FindNearestPlayer()
    {
        // Prefer registry
        if (PlayerRegistry.Players != null && PlayerRegistry.Players.Count > 0)
            return NearestFromList(PlayerRegistry.Players);

        // Fallback by tag (works even if registry wasn’t added)
        var tagged = GameObject.FindGameObjectsWithTag(playerTagFallback);
        if (tagged.Length == 0) return null;

        var tmp = new List<Transform>(tagged.Length);
        foreach (var go in tagged) if (go) tmp.Add(go.transform);
        return NearestFromList(tmp);
    }

    Transform NearestFromList(IList<Transform> list)
    {
        Transform best = null;
        float bestSqr = float.PositiveInfinity;
        Vector3 p = transform.position;
        float maxSqr = maxChaseDistance * maxChaseDistance;

        for (int i = 0; i < list.Count; i++)
        {
            var t = list[i];
            if (!t) continue;
            float sqr = (t.position - p).sqrMagnitude;
            if (sqr < bestSqr && sqr <= maxSqr)
            {
                bestSqr = sqr;
                best = t;
            }
        }
        return best;
    }
}
