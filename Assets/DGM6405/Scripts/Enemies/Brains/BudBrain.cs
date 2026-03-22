using System;
using UnityEngine;

/// <summary>
///     Brain for the Bud Turret.
///     Handles 3 targeting modes: Fixed Axis, Direct Aim, and Arc Fire.
///     Emits aim and shoot commands via LocalEventBus.
/// </summary>
public class BudBrain : PausableBehaviour, IHealthListener
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

	protected override void PausableUpdate()
	{
		if (_health != null && _health.IsDead) return;

		// Check game state before processing input
		if (GameMgr.Instance == null)
		{
			Debug.LogWarning($"[{name}] PlayerCommandBrain: GameMgr.Instance is null. Skipping update.", this);
			return;
		}

		if (!GameMgr.Instance.IsGameRunning)
			return;
		
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
			default:
				throw new ArgumentOutOfRangeException();
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
		// Compute a far target along fixed axis from the muzzle (or transform if weapon/muzzle is missing)
		var range = _detection != null ? _detection.DetectionRange : 50f;
		var origin = (_weapon != null && _weapon.Muzzle != null) ? _weapon.Muzzle.position : transform.position;
		var targetPos = origin + transform.TransformDirection(_fixedAxis).normalized * range;
		_bus?.Raise<IAimTargetListener>(l => l.OnSetAimTarget(targetPos));

		// Ensure direct fire
		if (_weapon != null) _weapon.SetUseArc(false);
		_bus?.Raise<IShootListener>(l => l.OnShoot(true));
	}

	private void HandleDirectAim()
	{
		if (_targetProvider == null || !_targetProvider.HasTarget) return;

		var target = _targetProvider.GetTarget();
		var targetOffset = _targetProvider.GetTargetOffset();
		var targetPos = GetResolvedTargetPosition(target);
		if (_detection.IsTargetInDetectionRange(target, targetOffset) && _detection.HasLineOfSight(target, targetOffset))
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
		var targetOffset = _targetProvider.GetTargetOffset();
		var targetPos = GetResolvedTargetPosition(target);
		if (_detection.IsTargetInDetectionRange(target, targetOffset) && _detection.HasLineOfSight(target, targetOffset))
		{
			_bus?.Raise<IAimTargetListener>(l => l.OnSetAimTarget(targetPos));
			if (_weapon != null) _weapon.SetUseArc(true);
			_bus?.Raise<IShootListener>(l => l.OnShoot(true));
		}
	}

	private Vector3 GetResolvedTargetPosition(Transform target)
	{
		if (target == null) return _targetProvider.GetTargetPosition();
		return target.position + _targetProvider.GetTargetOffset();
	}
}
