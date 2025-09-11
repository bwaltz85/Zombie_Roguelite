using System.Collections;
using UnityEngine;

public class DashAbility : MonoBehaviour
{
    [Header("Input")]
    public KeyCode dashKey = KeyCode.LeftShift;

    [Header("Dash Tuning")]
    [Tooltip("How far to travel during a dash (in meters).")]
    public float dashDistance = 6f;

    [Tooltip("How long the dash takes (seconds).")]
    public float dashDuration = 0.12f;

    [Tooltip("Minimum time between dashes (seconds).")]
    public float cooldown = 0.6f;

    [Tooltip("Small upward nudge to avoid ground clipping when using CharacterController.")]
    public float groundLift = 0.05f;

    [Header("I-Frames (optional)")]
    [Tooltip("Seconds of invulnerability starting at dash begin. Set 0 for none.")]
    public float invulnSeconds = 0.15f;

    [Header("Effects (optional)")]
    public GameObject dashVFXPrefab;

    [Header("Debug")]
    public bool logVerbose = false;

    CharacterController _cc;
    bool _coolingDown;
    bool _dashing;
    float _cooldownUntil;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (GameLoop.I != null && GameLoop.I.State != GameState.Playing) return;

        if (Input.GetKeyDown(dashKey))
            TryDash();
    }

    void TryDash()
    {
        if (_dashing) return;

        if (_coolingDown && Time.time < _cooldownUntil) return;

        // Determine dash direction: prefer current move input if you have one,
        // otherwise use forward.
        Vector3 dir = GetDesiredDirection();
        if (dir.sqrMagnitude < 0.0001f)
            dir = transform.forward;

        StartCoroutine(DashRoutine(dir.normalized));
    }

    Vector3 GetDesiredDirection()
    {
        // If you have your own player controller that exposes a move vector, grab it here.
        // For generic use, read WASD (or stick to forward).
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 camFwd = Vector3.forward;
        Vector3 camRight = Vector3.right;
        // If you have a camera-aligned movement system, substitute here.
        Vector3 dir = (camRight * h + camFwd * v);
        dir.y = 0f;
        return dir;
    }

    IEnumerator DashRoutine(Vector3 direction)
    {
        _dashing = true;
        _coolingDown = true;
        _cooldownUntil = Time.time + cooldown;

        if (dashVFXPrefab)
            Instantiate(dashVFXPrefab, transform.position, Quaternion.LookRotation(direction, Vector3.up));

        

        // Move over time
        float dist = Mathf.Max(0f, dashDistance);
        float dur = Mathf.Max(0.01f, dashDuration);

        Vector3 start = transform.position;
        Vector3 end = start + direction * dist;

        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / dur);
            Vector3 target = Vector3.Lerp(start, end, k);

            if (_cc)
            {
                Vector3 delta = target - transform.position;
                // slight lift to avoid ground snapping interference
                delta.y += groundLift;
                _cc.Move(delta);
            }
            else
            {
                transform.position = target;
            }

            yield return null;
        }

        if (logVerbose) Debug.Log("[DashAbility] Dash complete.");

        // finish
        _dashing = false;
        // cooldown handled via timestamp; no extra wait needed
    }
}
