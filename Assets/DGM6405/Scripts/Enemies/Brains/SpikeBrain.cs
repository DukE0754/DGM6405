using UnityEngine;

/// <summary>
///     Brain for the Spike enemy.
///     Focuses on patrol movement and contact damage.
/// </summary>
public class SpikeBrain : MonoBehaviour, IHealthListener
{
	[SerializeField] private Health _health;
	[SerializeField] private PatrolMotor _patrolMotor;
	[SerializeField] private ContactDamage _contactDamage;
	[SerializeField] private EnemyAnimatorDriver _animatorDriver;
	[SerializeField] private LocalEventBus _bus;

	private IMover _mover;

	private void Awake()
	{
		if (_health == null) _health = GetComponent<Health>();
		if (_patrolMotor == null) _patrolMotor = GetComponent<PatrolMotor>();
		if (_contactDamage == null) _contactDamage = GetComponent<ContactDamage>();
		if (_animatorDriver == null) _animatorDriver = GetComponent<EnemyAnimatorDriver>();
		if (_bus == null) _bus = GetComponent<LocalEventBus>();
		_mover = GetComponent<IMover>();
	}

	private void Update()
	{
		if (_health != null && _health.IsDead) return;

		// Raise movement speed for animator listeners
		if (_mover != null)
		{
			var speed = _mover.Velocity.magnitude;
			_bus?.Raise<IMovementSpeedListener>(l => l.OnSpeedChanged(speed, 0f, 0f, 0f, 0f, 0f));
		}
	}

	// IHealthListener
	public void OnHealthChanged(float current, float max)
	{
		// Not used here
	}

	public void OnDied()
	{
		if (_patrolMotor != null) _patrolMotor.enabled = false;
		if (_contactDamage != null) _contactDamage.enabled = false;
		if (_mover != null)
		{
			_mover.Stop();
			_mover.SetEnabled(false);
		}
	}
}
