using UnityEngine;

public class DashAbility : AbilityBase
{
    public float dashSpeed = 18f;
    public float dashTime = 0.12f;

    private float t;
    private Vector3 dir;
    private CharacterController cc;

    void Awake() { cc = GetComponentInParent<CharacterController>(); }

    void Update()
    {
        if (t > 0f && cc != null)
        {
            cc.Move(dir * dashSpeed * Time.deltaTime);
            t -= Time.deltaTime;
        }
    }

    protected override bool OnCast()
    {
        var host = transform.root;
        if (!host) return false;
        dir = host.forward.sqrMagnitude > 0.001f ? host.forward.normalized : Vector3.forward;
        t = dashTime;
        return true;
    }
}
