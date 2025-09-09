// XPOrb.cs
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class XPOrb : MonoBehaviour
{
    [Header("XP")]
    public int xpValue = 1;

    [Header("Attract")]
    public float attractRange = 6f;
    public float speed = 8f;

    private Transform player;

    void Start()
    {
        // Find the player once at startup
        var pc = FindFirstObjectByType<TopDownPlayerController>();
        if (pc != null) player = pc.transform;

        // Ensure collider is a trigger
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    void Update()
    {
        if (!player) return;

        float d = Vector3.Distance(transform.position, player.position);
        if (d < attractRange)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                player.position,
                speed * Time.deltaTime
            );
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!player) return;
        if (other.transform == player)
        {
            PlayerXP.Add(xpValue);
            Destroy(gameObject);
        }
    }
}
