using UnityEngine;

/// <summary>
///     A basic implementation of IDamageReceiver that
///     forwards damage to a Health component.
///     This allows the collider to be on a different
///     GameObject than the Health component.
/// </summary>
public class SimpleDamageReceiver : MonoBehaviour, IDamageReceiver
{
	[SerializeField] private Health _health;
	[SerializeField] private float _damageMultiplier = 1f;

	private void Awake()
	{
		// Fallback to local if not assigned
		if (_health == null) _health = GetComponentInParent<Health>();
	}

	private void OnEnable()
	{
		var context = GetComponentInParent<CharacterContext>();
		var bus = context != null ? context.EventBus : GetComponentInParent<LocalEventBus>();

		var colliders = GetComponentsInChildren<Collider>();
		foreach (var col in colliders)
		{
			ColliderMgr.Instance.Register(col, this);
			if (bus != null) ColliderMgr.Instance.Register(col, bus);
		}
	}

	private void OnDisable()
	{
		var colliders = GetComponentsInChildren<Collider>();
		foreach (var col in colliders)
		{
			if (col) ColliderMgr.Instance?.Unregister(col);
		}
	}

	public void ApplyDamage(DamageInfo info)
	{
		if (_health == null) return;

		// Apply multiplier (e.g., for weak points)
		info.Amount = Mathf.RoundToInt(info.Amount * _damageMultiplier);

		_health.ApplyDamage(info);
	}
}
