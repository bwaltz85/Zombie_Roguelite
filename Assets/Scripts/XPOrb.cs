using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class XPOrb : MonoBehaviour
{
    [Header("Value")]
    public int value = 1;

    [Header("Homing")]
    [Tooltip("If true, auto-locate the player's XPCollector at spawn.")]
    public bool autoFindCollector = true;
    [Tooltip("Start homing immediately.")]
    public bool homeImmediately = true;
    [Tooltip("Initial homing speed.")]
    public float startSpeed = 8f;
    [Tooltip("Maximum homing speed.")]
    public float maxSpeed = 18f;
    [Tooltip("Acceleration while homing.")]
    public float accel = 50f;
    [Tooltip("Distance at which the orb instantly collects even without trigger overlap.")]
    public float pickupDistance = 0.5f;

    [Header("Lifetime")]
    public float maxLifetime = 20f;

    Rigidbody _rb;
    SphereCollider _col;
    Transform _target;
    float _speed;
    float _spawnTime;

    void Awake()
    {
        _col = GetComponent<SphereCollider>();
        _col.isTrigger = true;                 // trigger-only, no physical collisions
        _col.radius = Mathf.Max(0.1f, _col.radius);

        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;                // kinematic so two triggers can fire
        _rb.useGravity = false;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void OnEnable()
    {
        _spawnTime = Time.time;
        _speed = Mathf.Max(0f, startSpeed);

        if (autoFindCollector)
        {
            var collector = Object.FindFirstObjectByType<XPCollector>();
            if (collector) _target = collector.transform;
        }
    }

    public void SetTarget(Transform t)
    {
        _target = t;
    }

    void Update()
    {
        if (_target && homeImmediately)
        {
            _speed = Mathf.Min(maxSpeed, _speed + accel * Time.deltaTime);

            Vector3 to = _target.position - transform.position;
            float dist = to.magnitude;

            if (dist <= pickupDistance)
            {
                TryCollectInto(_target);
                return;
            }

            if (dist > 0.001f)
            {
                Vector3 step = to.normalized * _speed * Time.deltaTime;
                // clamp so we don't overshoot wildly
                if (step.sqrMagnitude > to.sqrMagnitude) step = to;
                transform.position += step;
            }
        }

        if (Time.time - _spawnTime > maxLifetime)
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        // If we hit a collector, collect immediately.
        var collector = other.GetComponent<XPCollector>();
        if (collector)
        {
            collector.CollectOrb(this);
            return;
        }
    }

    void TryCollectInto(Transform maybeCollector)
    {
        if (!maybeCollector) return;
        var collector = maybeCollector.GetComponent<XPCollector>();
        if (collector) collector.CollectOrb(this);
    }
}
