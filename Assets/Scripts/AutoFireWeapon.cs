using UnityEngine;
using UnityEngine.Serialization;

public class AutoFireWeapon : MonoBehaviour
{
    [Header("Scan")]
    public float range = 12f;

    [Tooltip("Layers considered 'enemies'. Set this to your Enemy layer(s).")]
    public LayerMask enemyLayers;

    [Tooltip("Only target objects (or their parents) that carry this tag. Leave blank to ignore tag filtering.")]
    public string targetTag = "Enemy";

    public bool includeTriggers = true;

    [Header("Firing")]
    public float fireRate = 4f;

    [FormerlySerializedAs("damagePerShot")]
    public float damage = 10f;

    public Transform muzzle;
    public bool requireLineOfSight = false;
    public LayerMask losBlockers;

    [Header("Debug / Safety")]
    [Tooltip("Temporarily ignore the enemyLayers filter and scan ALL layers (debug only).")]
    public bool debugIgnoreLayerMask = false;

    [Tooltip("If enemyLayers is ZERO at runtime, we can scan ALL layers but we still enforce tag + self-exclusion to avoid friendly fire.")]
    public bool fallbackScanAllIfMaskZero = true;

    public bool logVerbose = true;
    public bool drawGizmos = true;

    float _fireTimer;
    Vector3 _lastShotFrom, _lastShotTo;
    bool _hadHitThisFrame;

    Transform _root; // cached root to exclude self

    void Awake()
    {
        _root = transform.root;
    }

    void Reset()
    {
        muzzle = transform;
        // Try to auto-assign "Enemy" layer if it exists
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer >= 0) enemyLayers = 1 << enemyLayer;
    }

    void Update()
    {
        if (GameLoop.I == null || GameLoop.I.State != GameState.Playing) return;

        _fireTimer -= Time.deltaTime;
        if (_fireTimer <= 0f)
        {
            TryFireOnce();
            _fireTimer = 1f / Mathf.Max(0.01f, fireRate);
        }
    }

    void TryFireOnce()
    {
        var origin = muzzle ? muzzle.position : transform.position;
        _hadHitThisFrame = false;
        _lastShotFrom = origin;
        _lastShotTo = origin;

        // Build mask safely
        int mask;
        if (debugIgnoreLayerMask) mask = ~0;
        else
        {
            mask = enemyLayers.value;
            if (mask == 0 && fallbackScanAllIfMaskZero)
            {
                mask = ~0;
                if (logVerbose)
                    Debug.LogWarning("[AutoFire] enemyLayers mask is ZERO; using ALL layers for now. Set enemyLayers to your Enemy layer to remove this warning.");
            }
        }

        // Scan
        var colliders = Physics.OverlapSphere(
            origin,
            range,
            mask,
            includeTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore
        );

        if (colliders == null || colliders.Length == 0)
        {
            if (logVerbose)
                Debug.Log($"[AutoFire] No colliders in range. range={range}, usingLayerFilter={!debugIgnoreLayerMask}, mask=0x{mask:X}");
            return;
        }

        IDamageable best = null;
        Transform bestTf = null;
        float bestDistSqr = float.PositiveInfinity;

        foreach (var col in colliders)
        {
            if (!col) continue;

            // Exclude any collider that belongs to our own root (prevents shooting self)
            if (IsInSameRoot(col.transform, _root)) continue;

            // Optional tag gate: accept if the collider OR any parent carries the targetTag
            if (!string.IsNullOrEmpty(targetTag))
            {
                if (!HasTagInParents(col.transform, targetTag))
                    continue;
            }

            var dmg = col.GetComponentInParent<IDamageable>();
            if (dmg == null) continue;

            var targetTf = dmg.GetTransform();
            if (targetTf == null) targetTf = col.transform;

            if (requireLineOfSight && !HasLineOfSight(origin, targetTf.position))
                continue;

            float d2 = (targetTf.position - origin).sqrMagnitude;
            if (d2 < bestDistSqr)
            {
                best = dmg;
                bestTf = targetTf;
                bestDistSqr = d2;
            }
        }

        if (best == null)
        {
            if (logVerbose)
                Debug.Log("[AutoFire] Found colliders but none passed filters (IDamageable / tag / self-exclusion / LOS).");
            return;
        }

        best.TakeDamage(damage, origin);
        _hadHitThisFrame = true;
        _lastShotTo = bestTf ? bestTf.position : origin;

        if (logVerbose)
        {
            string targetName = bestTf ? bestTf.name : "target";
            Debug.Log($"[AutoFire] DEALT {damage} dmg to {targetName} @ {Mathf.Sqrt(bestDistSqr):0.0}m");
        }
    }

    // ---- helpers ----

    static bool IsInSameRoot(Transform t, Transform root)
    {
        if (t == null || root == null) return false;
        var p = t;
        while (p != null)
        {
            if (p == root) return true;
            p = p.parent;
        }
        return false;
    }

    static bool HasTagInParents(Transform t, string tag)
    {
        if (string.IsNullOrEmpty(tag)) return true;
        var p = t;
        while (p != null)
        {
            if (p.CompareTag(tag)) return true;
            p = p.parent;
        }
        return false;
    }

    bool HasLineOfSight(Vector3 from, Vector3 to)
    {
        if (losBlockers.value == 0) return true;

        Vector3 dir = (to - from);
        float dist = dir.magnitude;
        if (dist <= 0.0001f) return true;
        dir /= dist;

        bool blocked = Physics.Raycast(from, dir, dist, losBlockers, QueryTriggerInteraction.Ignore);
        if (blocked && logVerbose) Debug.Log("[AutoFire] LOS blocked.");
        return !blocked;
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        Gizmos.DrawWireSphere(muzzle ? muzzle.position : transform.position, range);
        if (_hadHitThisFrame) Gizmos.DrawLine(_lastShotFrom, _lastShotTo);
    }
}
