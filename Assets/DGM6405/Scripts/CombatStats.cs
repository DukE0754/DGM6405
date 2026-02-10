using DGM6405.Events;
using UnityEngine;

public class CombatStats : MonoBehaviour
{
	[Header("Events")]
	[SerializeField] private LocalEventBus _bus;

	[Header("Health")]
	[SerializeField] private float maxHP = 100f;

	// Runtime
	public float CurrentHP { get; private set; }
	public float MaxHP => maxHP;
	public bool IsDead { get; private set; }

	private void Awake()
	{
		CurrentHP = maxHP;
		_bus?.Raise<IHealthListener>(l => l.OnHealthChanged(CurrentHP, maxHP));
	}

	private void Update()
	{
		if (IsDead) return;
	}

	/// <summary>
	///     Apply damage to this entity. Returns true if damage was applied (not ignored).
	/// </summary>
	public bool TakeDamage(float incomingDamage, GameObject source = null)
	{
		if (IsDead) return false;
		if (incomingDamage <= 0f) return false;

		var dmg = incomingDamage;

		// 1) flat defense
		dmg = Mathf.Max(0f, dmg);

		if (dmg <= 0f) return false;

		CurrentHP = Mathf.Clamp(CurrentHP - dmg, 0f, maxHP);
		_bus?.Raise<IHealthListener>(l => l.OnHealthChanged(CurrentHP, maxHP));

		if (CurrentHP <= 0f) Die(source);

		return true;
	}

	public void Heal(float amount)
	{
		if (IsDead) return;
		if (amount <= 0f) return;

		CurrentHP = Mathf.Clamp(CurrentHP + amount, 0f, maxHP);
		_bus?.Raise<IHealthListener>(l => l.OnHealthChanged(CurrentHP, maxHP));
	}

	public void SetMaxHP(float newMaxHP, bool fillToMax = false)
	{
		maxHP = Mathf.Max(1f, newMaxHP);

		if (fillToMax) CurrentHP = maxHP;
		else CurrentHP = Mathf.Clamp(CurrentHP, 0f, maxHP);

		_bus?.Raise<IHealthListener>(l => l.OnHealthChanged(CurrentHP, maxHP));
	}

	private void Die(GameObject killer)
	{
		if (IsDead) return;
		IsDead = true;

		// Optional: disable collider / movement / AI here
		// GetComponent<Collider>()?.enabled = false;

		_bus?.Raise<IHealthListener>(l => l.OnDied());
	}
}
