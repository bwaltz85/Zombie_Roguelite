using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public float maxHP = 20f;
    private float currentHP;

    [Header("XP Drop")]
    public GameObject xpOrbPrefab;
    public int xpDropCount = 1;

    void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(float amount, Vector3 sourcePosition)
    {
        currentHP -= amount;
        Debug.Log($"{name} took {amount} damage. HP left: {currentHP}");

        if (currentHP <= 0f)
        {
            Die();
        }
    }

    public Transform GetTransform() => transform;

    private void Die()
    {
        // Drop XP orbs
        if (xpOrbPrefab != null)
        {
            for (int i = 0; i < xpDropCount; i++)
            {
                Vector3 pos = transform.position + Random.insideUnitSphere * 0.5f;
                pos.y = transform.position.y;
                Instantiate(xpOrbPrefab, pos, Quaternion.identity);
            }
        }

        Destroy(gameObject);
    }

    public void ApplyHealthMultiplier(float m)
    {
        maxHP = Mathf.Ceil(maxHP * Mathf.Max(1f, m));
        currentHP = maxHP;
    }
}
