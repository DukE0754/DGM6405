using UnityEngine;
using System;

public class CombatStats : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHP = 100f;

    // Runtime
    public float CurrentHP { get; private set; }
    public float MaxHP => maxHP;
    public bool IsDead { get; private set; }

    // Events (optional, for UI / effects)
    public event Action<float, float> OnHealthChanged; // (current, max)
    public event Action OnDied;

    private void Awake()
    {
        CurrentHP = maxHP;
        OnHealthChanged?.Invoke(CurrentHP, maxHP);
    }

    private void Update()
    {
        if (IsDead) return;
    }

    /// <summary>
    /// Apply damage to this entity. Returns true if damage was applied (not ignored).
    /// </summary>
    public bool TakeDamage(float incomingDamage, GameObject source = null)
    {
        if (IsDead) return false;
        if (incomingDamage <= 0f) return false;

        float dmg = incomingDamage;

        // 1) flat defense
        dmg = Mathf.Max(0f, dmg);

        if (dmg <= 0f) return false;

        CurrentHP = Mathf.Clamp(CurrentHP - dmg, 0f, maxHP);
        OnHealthChanged?.Invoke(CurrentHP, maxHP);

        if (CurrentHP <= 0f)
        {
            Die(source);
        }

        return true;
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        if (amount <= 0f) return;

        CurrentHP = Mathf.Clamp(CurrentHP + amount, 0f, maxHP);
        OnHealthChanged?.Invoke(CurrentHP, maxHP);
    }

    public void SetMaxHP(float newMaxHP, bool fillToMax = false)
    {
        maxHP = Mathf.Max(1f, newMaxHP);

        if (fillToMax) CurrentHP = maxHP;
        else CurrentHP = Mathf.Clamp(CurrentHP, 0f, maxHP);

        OnHealthChanged?.Invoke(CurrentHP, maxHP);
    }

    private void Die(GameObject killer)
    {
        if (IsDead) return;
        IsDead = true;

        // Optional: disable collider / movement / AI here
        // GetComponent<Collider>()?.enabled = false;

        OnDied?.Invoke();
    }
}
