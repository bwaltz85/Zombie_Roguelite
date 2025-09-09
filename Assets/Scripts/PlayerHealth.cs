using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public float maxHP = 100f;
    public float iFrameDuration = 0.3f;

    [Header("Events")]
    public UnityEvent onDamaged;
    public UnityEvent onDeath;

    [Header("Debug UI")]
    public bool showDebugBar = true;

    float _hp;
    float _iFrameT;
    bool _dead;

    void Awake() { _hp = maxHP; }

    void Update()
    {
        if (_iFrameT > 0f) _iFrameT -= Time.deltaTime;
    }

    public void TakeDamage(float amount, Vector3 sourcePosition)
    {
        if (_dead || _iFrameT > 0f) return;

        _hp -= amount;
        _iFrameT = iFrameDuration;
        onDamaged?.Invoke();

        if (_hp <= 0f)
        {
            _hp = 0f;
            if (_dead) return;
            _dead = true;

            onDeath?.Invoke();

            var cc = GetComponent<CharacterController>(); if (cc) cc.enabled = false;
            var pi = GetComponent<PlayerInput>(); if (pi) pi.enabled = false;

            if (GameLoop.I != null) GameLoop.I.GameOver();
            Debug.Log("[PlayerHealth] Player died.");
        }
    }

    public Transform GetTransform() => transform;

    void OnGUI()
    {
        if (!showDebugBar) return;
        const float w = 220f, h = 18f, pad = 2f;
        float x = 20f, y = 20f;
        GUI.Box(new Rect(x, y, w, h), GUIContent.none);
        float pct = Mathf.Clamp01(_hp / Mathf.Max(1f, maxHP));
        GUI.Box(new Rect(x + pad, y + pad, (w - pad * 2f) * pct, h - pad * 2f),
            $"HP {Mathf.CeilToInt(_hp)}/{Mathf.CeilToInt(maxHP)}");
    }

    public void Heal(float amount)
    {
        if (_dead) return;
        _hp = Mathf.Min(maxHP, _hp + Mathf.Abs(amount));
    }

    public void AddMaxHealth(float amount, bool healToFull = true)
    {
        maxHP += Mathf.Abs(amount);
        if (healToFull) _hp = maxHP;
    }
}
