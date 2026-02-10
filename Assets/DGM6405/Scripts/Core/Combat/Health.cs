
using UnityEngine;

/// <summary>
///     Manages health values and state for an entity.
///     Emits events via LocalEventBus for other systems (VFX, Audio, UI) to listen to.
/// </summary>
[RequireComponent(typeof(LocalEventBus))]
public class Health : MonoBehaviour
{
	[SerializeField] private int _maxHealth = 100;
	[SerializeField] private bool _isInvulnerable;
	[SerializeField] private LocalEventBus _bus;

	public int CurrentHealth { get; private set; }

	public int MaxHealth => _maxHealth;
	public bool IsDead { get; private set; }

	private void Awake()
	{
		if (_bus == null) _bus = GetComponent<LocalEventBus>();
		ResetHealth();
	}

	public void ResetHealth()
	{
		CurrentHealth = _maxHealth;
		IsDead = false;
		_bus?.Raise<IHealthListener>(l => l.OnHealthChanged(CurrentHealth, _maxHealth));
	}

	public void ApplyDamage(DamageInfo info)
	{
		if (IsDead || _isInvulnerable) return;

		CurrentHealth -= info.Amount;
		CurrentHealth = Mathf.Clamp(CurrentHealth, 0, _maxHealth);

		// Notify listeners about damage and updated health
		_bus?.Raise<IHealthListener>(l => l.OnHealthChanged(CurrentHealth, _maxHealth));

		if (CurrentHealth <= 0)
		{
			Die();
		}
	}

	public void Heal(int amount)
	{
		if (IsDead) return;

		CurrentHealth += amount;
		CurrentHealth = Mathf.Clamp(CurrentHealth, 0, _maxHealth);
		_bus?.Raise<IHealthListener>(l => l.OnHealthChanged(CurrentHealth, _maxHealth));
	}

	private void Die()
	{
		IsDead = true;
		_bus?.Raise<IHealthListener>(l => l.OnDied());
	}

	// For Training Dummy or special cases
	public void SetInvulnerable(bool invulnerable)
	{
		_isInvulnerable = invulnerable;
	}
}
