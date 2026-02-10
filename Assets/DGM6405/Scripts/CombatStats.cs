using UnityEngine;

public class CombatStats : MonoBehaviour
{
	[Header("Events")]
	[SerializeField] private LocalEventBus _bus;

	[Header("Health")]
	[SerializeField] private float maxHP = 100f;

	// Runtime
	public float CurrentHp { get; private set; }
	public float MaxHp => maxHP;
	public bool IsDead { get; private set; }

	private void Awake()
	{
		CurrentHp = maxHP;
		_bus?.Raise<IHealthListener>(l => l.OnHealthChanged(CurrentHp, maxHP));
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

		CurrentHp = Mathf.Clamp(CurrentHp - dmg, 0f, maxHP);
		_bus?.Raise<IHealthListener>(l => l.OnHealthChanged(CurrentHp, maxHP));

		if (CurrentHp <= 0f) Die(source);

		return true;
	}

	public void Heal(float amount)
	{
		if (IsDead) return;
		if (amount <= 0f) return;

		CurrentHp = Mathf.Clamp(CurrentHp + amount, 0f, maxHP);
		_bus?.Raise<IHealthListener>(l => l.OnHealthChanged(CurrentHp, maxHP));
	}

	public void SetMaxHp(float newMaxHp, bool fillToMax = false)
	{
		maxHP = Mathf.Max(1f, newMaxHp);

		if (fillToMax) CurrentHp = maxHP;
		else CurrentHp = Mathf.Clamp(CurrentHp, 0f, maxHP);

		_bus?.Raise<IHealthListener>(l => l.OnHealthChanged(CurrentHp, maxHP));
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
