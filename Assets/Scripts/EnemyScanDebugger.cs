using System.Text;
using UnityEngine;

public class EnemyScanDebugger : MonoBehaviour
{
    public Transform originOverride;       // if null, uses this.transform
    public float range = 12f;
    public LayerMask mask = ~0;            // default: scan all
    public bool includeTriggers = true;
    public bool runEveryFrame = false;     // off = manual key only
    public KeyCode manualKey = KeyCode.P;  // press to print scan report

    void Update()
    {
        if (runEveryFrame || Input.GetKeyDown(manualKey))
            ScanAndReport();
    }

    void ScanAndReport()
    {
        var origin = originOverride ? originOverride.position : transform.position;

        var cols = Physics.OverlapSphere(
            origin,
            range,
            mask.value,
            includeTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore
        );

        var sb = new StringBuilder();
        sb.AppendLine($"[EnemyScanDebugger] Overlap {cols.Length} colliders within {range}m. Mask=0x{mask.value:X} includeTriggers={includeTriggers}");

        for (int i = 0; i < cols.Length; i++)
        {
            var c = cols[i];
            if (!c) continue;

            var dmg = c.GetComponentInParent<IDamageable>();
            var dmgComp = dmg as Component; // most implementations are MonoBehaviours
            var tf = dmgComp ? dmgComp.transform : c.transform;

            var path = GetHierarchyPath(tf);
            var layer = LayerMask.LayerToName(c.gameObject.layer);

            sb.AppendLine($"  [{i}] col={c.name}  layer={layer}  trigger={c.isTrigger}  path=/{path}  -> IDamageable={(dmg != null ? tf.name : "null")}");
        }

        Debug.Log(sb.ToString());
    }

    string GetHierarchyPath(Transform t)
    {
        var sb = new StringBuilder(t.name);
        var p = t.parent;
        while (p != null)
        {
            sb.Insert(0, p.name + "/");
            p = p.parent;
        }
        return sb.ToString();
    }

    void OnDrawGizmosSelected()
    {
        var origin = originOverride ? originOverride.position : transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origin, range);
    }
}
