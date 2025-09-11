using UnityEngine;

public class ShockwaveAbility : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("Top-row number key by default (not numpad).")]
    public KeyCode castKey = KeyCode.Alpha1;

    [Header("Shockwave Stats")]
    public float radius = 5f;
    public float damage = 20f;
    public float cooldown = 3f;

    [Header("Targeting")]
    [Tooltip("Layers considered 'enemies'. If zero, we fall back to scan all layers (with a warning).")]
    public LayerMask enemyLayers;

    public bool includeTriggers = true;

    [Tooltip("Optional LOS check (raycast).")]
    public bool requireLineOfSight = false;

    [Tooltip("Blocks LOS, e.g., Default | Environment.")]
    public LayerMask losBlockers;

    [Header("Knockback (optional)")]
    public float knockbackForce = 6f;
    public ForceMode knockbackMode = ForceMode.Impulse;

    [Header("VFX/SFX (optional)")]
    public GameObject shockwaveVFXPrefab;

    [Header("Debug")]
    public bool logVerbose = true;
    public bool drawGizmos = true;

    float _cooldownUntil;

    void Reset()
    {
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer >= 0) enemyLayers = 1 << enemyLayer;
    }

    void Update()
    {
        if (GameLoop.I != null && GameLoop.I.State != GameState.Playing) return;

        if (Input.GetKeyDown(castKey))
            TryCast();
    }

    public void TryCast()
    {
        if (Time.time < _cooldownUntil) return;

        CastShockwave();
        _cooldownUntil = Time.time + Mathf.Max(0.01f, cooldown);
    }

    void CastShockwave()
    {
        Vector3 origin = transform.position;
        if (shockwaveVFXPrefab)
            Instantiate(shockwaveVFXPrefab, origin, Quaternion.identity);

        int mask = enemyLayers.value != 0 ? enemyLayers.value : ~0;
        if (enemyLayers.value == 0 && logVerbose)
            Debug.LogWarning("[ShockwaveAbility] enemyLayers mask is ZERO; scanning ALL layers (set the mask to your Enemy layer).");

        var cols = Physics.OverlapSphere(
            origin,
            radius,
            mask,
            includeTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore
        );

        if (cols == null || cols.Length == 0)
        {
            if (logVerbose) Debug.Log("[ShockwaveAbility] No targets found.");
            return;
        }

        int hitCount = 0;
        foreach (var col in cols)
        {
            if (!col) continue;

            var dmg = col.GetComponentInParent<IDamageable>();
            if (dmg == null) continue;

            Transform tf = dmg.GetTransform();
            Vector3 targetPos = tf ? tf.position : col.transform.position;

            if (requireLineOfSight && !HasLineOfSight(origin, targetPos))
                continue;

            // Deal damage
            dmg.TakeDamage(damage, origin);
            hitCount++;

            // Apply knockback if there is a rigidbody
            var rb = col.attachedRigidbody;
            if (rb && knockbackForce > 0f)
            {
                Vector3 dir = (rb.worldCenterOfMass - origin);
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.0001f)
                    rb.AddForce(dir.normalized * knockbackForce, knockbackMode);
            }
        }

        if (logVerbose) Debug.Log($"[ShockwaveAbility] Hit {hitCount} target(s).");
    }

    bool HasLineOfSight(Vector3 from, Vector3 to)
    {
        if (losBlockers.value == 0) return true;

        Vector3 dir = to - from;
        float dist = dir.magnitude;
        if (dist <= 0.0001f) return true;
        dir /= dist;

        bool blocked = Physics.Raycast(from, dir, dist, losBlockers, QueryTriggerInteraction.Ignore);
        if (blocked && logVerbose) Debug.Log("[ShockwaveAbility] LOS blocked.");
        return !blocked;
    }

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
