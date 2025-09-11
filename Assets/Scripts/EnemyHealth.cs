using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public float maxHP = 30f;
    public float currentHP = 0f;

    [Header("Death Flow")]
    [Tooltip("Animator with a 'Die' trigger (optional).")]
    public Animator animator;
    [Tooltip("If true, we wait for an Animation Event to call OnDeathDropOrbs(); otherwise we use dropDelaySeconds.")]
    public bool spawnOnAnimationEvent = false;
    [Tooltip("If not using animation event, how long to wait after death before dropping orbs/VFX.")]
    public float dropDelaySeconds = 0.15f;

    [Tooltip("Extra time before the GameObject is destroyed, after drops/VFX.")]
    public float destroyDelay = 0.75f;

    [Tooltip("Disable common components immediately at death (AI/Colliders/Input).")]
    public bool disableComponentsOnDeath = true;

    [Header("VFX")]
    public GameObject deathVFX;

    [Header("XP Drop")]
    [Tooltip("Orb prefab with XPOrb (trigger + kinematic Rigidbody).")]
    public GameObject xpOrbPrefab;
    [Tooltip("Total XP value to drop on death.")]
    public int totalXPValue = 3;
    [Tooltip("How many orbs to spawn; totalXPValue is split across them.")]
    public int orbCount = 3;
    [Tooltip("Random planar spread radius for drops.")]
    public float dropSpreadRadius = 0.6f;

    [Header("Debug")]
    public bool logDamage = false;

    public bool IsAlive => !_dead && currentHP > 0f;

    // internal state
    bool _dead = false;
    bool _orbsDropped = false;

    void Reset()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void OnEnable()
    {
        var col = GetComponent<Collider>();
        if (col) col.enabled = true;

        if (currentHP <= 0f || currentHP > maxHP)
            currentHP = maxHP;

        _dead = false;
        _orbsDropped = false;
    }

    // ----- IDamageable -----
    public void TakeDamage(float amount, Vector3 sourcePosition)
    {
        if (!IsAlive) return;

        float dmg = Mathf.Max(0f, amount);
        if (dmg <= 0f) return;

        currentHP -= dmg;

        if (logDamage)
            Debug.Log($"[EnemyHealth] {name} took {dmg} dmg, HP={Mathf.Max(0f, currentHP)}/{maxHP}");

        if (currentHP <= 0f)
        {
            currentHP = 0f;
            StartDeathSequence();
        }
    }

    public Transform GetTransform() => this.transform;

    // Optional external scaling
    public void ApplyHealthMultiplier(float mult)
    {
        if (mult <= 0f) return;
        maxHP *= mult;
        if (currentHP <= 0f) currentHP = maxHP;
        else currentHP *= mult;
    }

    // ----- Death handling -----
    void StartDeathSequence()
    {
        if (_dead) return;
        _dead = true;

        if (logDamage) Debug.Log($"[EnemyHealth] {name} DIED (starting death sequence).");

        // Disable immediate “living” behavior
        if (disableComponentsOnDeath)
        {
            var rb = GetComponent<Rigidbody>(); if (rb) rb.isKinematic = true;

            // Disable all non-render scripts except this one
            var behaviours = GetComponents<MonoBehaviour>();
            foreach (var m in behaviours)
            {
                if (m == this) continue;
                m.enabled = false;
            }

            // Keep the collider enabled until after drops if you want it visible; 
            // otherwise disable now to prevent interactions:
            var col = GetComponent<Collider>(); if (col) col.enabled = false;
        }

        // Play death animation if available
        if (animator)
        {
            // Assumes you have a "Die" trigger; change as needed.
            if (animator.runtimeAnimatorController != null)
                animator.SetTrigger("Die");
        }

        // Kick off coroutine to time drops and cleanup
        StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        // Wait until it's time to drop orbs/VFX
        if (spawnOnAnimationEvent)
        {
            // We will wait until an animation event calls OnDeathDropOrbs().
            // Add a safety timeout so we don't wait forever if the event is missing.
            float timeout = Mathf.Max(0.25f, dropDelaySeconds) + 2.0f; // small safety buffer
            float start = Time.time;
            while (!_orbsDropped && (Time.time - start) < timeout)
                yield return null;

            if (!_orbsDropped)
            {
                // Fallback if event didn't fire
                if (logDamage) Debug.LogWarning($"[EnemyHealth] {name} no animation event received; dropping orbs via fallback.");
                OnDeathDropOrbs();
            }
        }
        else
        {
            // Fixed delay approach
            if (dropDelaySeconds > 0f)
                yield return new WaitForSeconds(dropDelaySeconds);

            OnDeathDropOrbs();
        }

        // After drops/VFX, wait before destroying
        if (destroyDelay > 0f)
            yield return new WaitForSeconds(destroyDelay);

        Destroy(gameObject);
    }

    /// <summary>
    /// Call this from an Animation Event at the right frame of your death animation,
    /// OR it will be called automatically by the DeathRoutine based on dropDelaySeconds.
    /// </summary>
    public void OnDeathDropOrbs()
    {
        if (_orbsDropped) return; // only once
        _orbsDropped = true;

        // Spawn death VFX
        if (deathVFX)
        {
            var vfx = Instantiate(deathVFX, transform.position, Quaternion.identity);
            Destroy(vfx, 5f);
        }

        // Spawn XP orbs
        if (xpOrbPrefab && totalXPValue > 0 && orbCount > 0)
            SpawnXPOrbs();
    }

    void SpawnXPOrbs()
    {
        int count = Mathf.Max(1, orbCount);
        int baseValue = Mathf.Max(1, totalXPValue / count);
        int remainder = Mathf.Max(0, totalXPValue - baseValue * count);

        // Cache player collector once (optional)
        XPCollector collector = Object.FindFirstObjectByType<XPCollector>();

        for (int i = 0; i < count; i++)
        {
            int value = baseValue + (i < remainder ? 1 : 0);

            Vector3 planar = Random.insideUnitSphere; planar.y = 0f;
            Vector3 offset = planar.normalized * Random.Range(0f, dropSpreadRadius);
            Vector3 pos = transform.position + offset + Vector3.up * 0.25f;

            var go = Instantiate(xpOrbPrefab, pos, Quaternion.identity);
            var orb = go.GetComponent<XPOrb>();
            if (orb)
            {
                orb.value = value;
                if (collector) orb.SetTarget(collector.transform); // start homing immediately
            }
        }
    }
}
