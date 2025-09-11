using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class XPCollector : MonoBehaviour
{
    [Header("Pickup")]
    [Tooltip("Radius of the collector trigger (set on the attached SphereCollider).")]
    public float radius = 3.5f;

    [Header("XP Target")]
    public PlayerExperience playerXP;  // if null, auto-finds on this GO

    SphereCollider _trigger;

    void Reset()
    {
        _trigger = GetComponent<SphereCollider>();
        _trigger.isTrigger = true;
        _trigger.radius = radius;
    }

    void Awake()
    {
        _trigger = GetComponent<SphereCollider>();
        _trigger.isTrigger = true;
        _trigger.radius = radius;

        if (!playerXP)
            playerXP = GetComponent<PlayerExperience>();
    }

    // Orbs call this directly or via trigger enter
    public void CollectOrb(XPOrb orb)
    {
        if (!orb) return;
        int val = Mathf.Max(0, orb.value);
        if (playerXP) playerXP.AddXP(val);
        Destroy(orb.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        var orb = other.GetComponent<XPOrb>();
        if (!orb) return;

        // If the orb didn't already know our target, set it just in case
        orb.SetTarget(transform);

        // Instant collect on enter
        CollectOrb(orb);
    }
}
