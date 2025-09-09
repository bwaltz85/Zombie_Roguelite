using UnityEngine;

public class AutoFireWeapon : MonoBehaviour
{
    [Header("Weapon")]
    public float fireRate = 2f;        // shots per second
    public float damage = 5f;
    public float range = 15f;

    [Header("Targeting")]
    public LayerMask targetLayers;     // set to Enemy in Inspector
    public bool useLayerFilter = true; // turn OFF to ignore layer mask for debugging
    public bool includeTriggers = false;

    [Header("FX (optional)")]
    public Animator animator;
    public string fireTrigger = "Fire";
    public ParticleSystem muzzleFlash;

    [Header("Debug")]
    public bool verboseLogs = true;

    private readonly Collider[] _hits = new Collider[128];
    private float _timer;

    void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            if (TryFireOnce())
                _timer = 1f / fireRate;
            else
                _timer = Mathf.Min(_timer + 0.05f, 0f);
        }
    }

    bool TryFireOnce()
    {
        int count;
        if (useLayerFilter)
        {
            count = Physics.OverlapSphereNonAlloc(
                transform.position, range, _hits, targetLayers,
                includeTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore
            );
        }
        else
        {
            count = Physics.OverlapSphereNonAlloc(
                transform.position, range, _hits, ~0,
                includeTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore
            );
        }

        if (verboseLogs && count == 0)
        {
            Debug.Log($"[AutoFire] No colliders in range. range={range}, usingLayerFilter={useLayerFilter}");
        }

        IDamageable bestDmg = null;
        Transform bestT = null;
        float bestSqr = float.PositiveInfinity;
        Vector3 p = transform.position;

        for (int i = 0; i < count; i++)
        {
            var c = _hits[i]; if (!c) continue;

            if (!c.TryGetComponent<IDamageable>(out var dmg))
            {
                // common case: script is on root or attached rigidbody
                if (c.attachedRigidbody)
                    dmg = c.attachedRigidbody.GetComponent<IDamageable>();
                if (dmg == null)
                    dmg = c.GetComponentInParent<IDamageable>();
            }
            if (dmg == null) continue;

            var t = dmg.GetTransform();
            if (!t) continue;

            float sqr = (t.position - p).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                bestDmg = dmg;
                bestT = t;
            }
        }

        if (bestDmg == null)
        {
            if (verboseLogs)
                Debug.Log("[AutoFire] Found colliders but none had IDamageable (check enemy health script & placement).");
            return false;
        }

        bestDmg.TakeDamage(damage, transform.position);

        if (animator && !string.IsNullOrEmpty(fireTrigger))
            animator.SetTrigger(fireTrigger);
        if (muzzleFlash) muzzleFlash.Play();

        // Debug line so you can see shots in Scene view
        Debug.DrawLine(transform.position, bestT.position, Color.red, 0.15f);

        return true;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.2f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, range);
    }
#endif
}
