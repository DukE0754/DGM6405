using UnityEngine;

/// <summary>
///     Brain for the Bud Turret.
///     Handles 3 targeting modes: Fixed Axis, Direct Aim, and Arc Fire.
///     Emits aim and shoot commands via LocalEventBus.
/// </summary>
public class BudBrain : MonoBehaviour, IHealthListener
{
	public enum FireMode
	{
		FixedAxis,
		DirectAim,
		ArcFire
	}

	[Header("Components")]
	[SerializeField] private Health _health;

	[SerializeField] private ProjectileWeapon _weapon;
	[SerializeField] private DetectionSystem _detection;
	[SerializeField] private RotateToTarget _rotator;
	[SerializeField] private EnemyAnimatorDriver _animator;
	[SerializeField] private LocalEventBus _bus;

	[Header("Settings")]
	[SerializeField] private FireMode _mode = FireMode.DirectAim;

	[SerializeField] private Vector3 _fixedAxis = Vector3.forward;

	private ITargetProvider _targetProvider;

	private void Awake()
	{
		_targetProvider = GetComponent<ITargetProvider>();
		if (_health == null) _health = GetComponent<Health>();
		if (_animator == null) _animator = GetComponent<EnemyAnimatorDriver>();
		if (_bus == null) _bus = GetComponent<LocalEventBus>();
	}

	private void Update()
	{
		if (_health != null && _health.IsDead) return;

		switch (_mode)
		{
			case FireMode.FixedAxis:
				HandleFixedAxis();
				break;
			case FireMode.DirectAim:
				HandleDirectAim();
				break;
			case FireMode.ArcFire:
				HandleArcFire();
				break;
		}
	}

	// IHealthListener
	public void OnHealthChanged(float current, float max)
	{
	}

	public void OnDied()
	{
		// Stop firing, maybe play an effect
		enabled = false;
	}

	private void HandleFixedAxis()
	{
		// Compute a far target along fixed axis from this transform
		var range = _detection != null ? _detection.DetectionRange : 50f;
		var targetPos = transform.position + transform.TransformDirection(_fixedAxis).normalized * range;
		_bus?.Raise<IAimTargetListener>(l => l.OnSetAimTarget(targetPos));

		// Ensure direct fire
		if (_weapon != null) _weapon.SetUseArc(false);
		_bus?.Raise<IShootListener>(l => l.OnShoot(true));
	}

	private void HandleDirectAim()
	{
		if (_targetProvider == null || !_targetProvider.HasTarget) return;

		var target = _targetProvider.GetTarget();
		var targetPos = _targetProvider.GetTargetPosition();
		if (_detection.IsTargetInDetectionRange(target) && _detection.HasLineOfSight(target))
		{
			_bus?.Raise<IAimTargetListener>(l => l.OnSetAimTarget(targetPos));
			if (_weapon != null) _weapon.SetUseArc(false);
			_bus?.Raise<IShootListener>(l => l.OnShoot(true));
		}
	}

	private void HandleArcFire()
	{
		if (_targetProvider == null || !_targetProvider.HasTarget) return;

		var target = _targetProvider.GetTarget();
		var targetPos = _targetProvider.GetTargetPosition();
		if (_detection.IsTargetInDetectionRange(target) && _detection.HasLineOfSight(target))
		{
			_bus?.Raise<IAimTargetListener>(l => l.OnSetAimTarget(targetPos));
			if (_weapon != null) _weapon.SetUseArc(true);
			_bus?.Raise<IShootListener>(l => l.OnShoot(true));
		}
	}
}
