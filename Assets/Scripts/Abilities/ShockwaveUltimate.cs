using UnityEngine;

public class ShockwaveUltimate : AbilityBase
{
    public float radius = 10f;
    public float damage = 50f;
    public LayerMask targetLayers;
    public bool includeTriggers = false;

    protected override bool OnCast()
    {
        var p = transform.position;

        var hits = Physics.OverlapSphere(
            p, radius, targetLayers,
            includeTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore
        );

        bool hitAny = false;

        foreach (var c in hits)
        {
            if (!c) continue;

            // Try to find an IDamageable on the collider, its rigidbody root, or a parent
            IDamageable dmg;
            if (!c.TryGetComponent<IDamageable>(out dmg))
            {
                if (c.attachedRigidbody)
                    dmg = c.attachedRigidbody.GetComponent<IDamageable>();

                if (dmg == null)
                    dmg = c.GetComponentInParent<IDamageable>();
            }

            if (dmg == null) continue; // <-- proper null check

            dmg.TakeDamage(damage, p);
            hitAny = true;
        }

        // (Optional) trigger VFX/SFX here

        return hitAny; // only start cooldown if we actually hit something
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
