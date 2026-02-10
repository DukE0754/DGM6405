using UnityEngine;

/// <summary>
///     Handles shooting/ranged combat.
///     Manages projectile spawning, weapon visibility, and shooting animations.
/// </summary>
public class ShootSystem : PausableBehaviour, IShootListener
{
	[Header("References")]
	[Tooltip("ProjectileShooter component for spawning projectiles. Required.")]
	[SerializeField] private ProjectileShooter _projectileShooter;

	[Tooltip("AimSystem for getting aim point. Optional, but recommended for accurate aiming.")]
	[SerializeField] private AimSystem _aimSystem;

	[Tooltip("CharacterContext component. If null, will try to find on same GameObject.")]
	[SerializeField] private CharacterContext _context;

	[Header("Debug Gizmos")]
	[Tooltip("Show shoot system gizmos in scene view when selected.")]
	[SerializeField] private bool _showGizmos = true;

	// Internal state

	// Public properties
	public bool IsShooting { get; private set; }
	
	private void Awake()
	{
		InitializeComponents();
	}

	private void InitializeComponents()
	{
		// Validate context
		if (_context == null) _context = GetComponent<CharacterContext>();

		if (_context == null)
		{
			Debug.LogError($"[{name}] ShootSystem: CharacterContext is required!", this);
			enabled = false;
			return;
		}

		// Validate projectile shooter
		if (_projectileShooter == null) _projectileShooter = GetComponent<ProjectileShooter>();

		if (_projectileShooter == null)
		{
			Debug.LogError($"[{name}] ShootSystem: ProjectileShooter is required!", this);
			enabled = false;
			return;
		}

		// Get aim system if not assigned
		if (_aimSystem == null) _aimSystem = GetComponent<AimSystem>();
	}

	private void OnDrawGizmosSelected()
	{
		if (!_showGizmos)
			return;

		// Aim point visualization
		if (_aimSystem != null && _aimSystem.IsAiming)
		{
			var aimPoint = _aimSystem.GetAimPoint();
			var firePoint = _projectileShooter != null && _projectileShooter.transform != null ?
				_projectileShooter.transform.position :
				transform.position;

			// Aim line
			Gizmos.color = Color.red;
			Gizmos.DrawLine(firePoint, aimPoint);

			// Aim point sphere
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(aimPoint, 0.15f);
		}

		// Fire point position
		if (_projectileShooter != null && _projectileShooter.transform != null)
		{
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireSphere(_projectileShooter.transform.position, 0.1f);
		}
	}

	private void OnValidate()
	{
		// Auto-hookup in editor
		if (_context == null) _context = GetComponent<CharacterContext>();
		if (_projectileShooter == null) _projectileShooter = GetComponent<ProjectileShooter>();
		if (_aimSystem == null) _aimSystem = GetComponent<AimSystem>();
	}

	/// <summary>
	///     Attempts to shoot based on input command.
	/// </summary>
	/// <param name="isShooting">Whether shoot input is active.</param>
	void IShootListener.OnShoot(bool shootInput)
	{
		TryShoot(shootInput);
	}

	private void TryShoot(bool isShooting)
	{
		// Check game state
		if (GameMgr.Instance != null && !GameMgr.Instance.IsGameRunning)
		{
			// Cancel shooting if game is not running
			if (IsShooting) IsShooting = false;

			return;
		}

		// Update shooting state
		var wasShooting = IsShooting;
		IsShooting = isShooting;

		// If just started shooting, fire projectile
		if (IsShooting && !wasShooting) FireProjectile();
	}

	/// <summary>
	///     Fires a projectile using ProjectileShooter.
	/// </summary>
	private void FireProjectile()
	{
		// Validate projectile shooter
		if (_projectileShooter == null)
		{
			Debug.LogError($"[{name}] ShootSystem: ProjectileShooter reference is null! Cannot shoot.", this);
			return;
		}

		// Get aim point from AimSystem if available, otherwise shoot forward
		if (_aimSystem != null)
		{
			var aimPoint = _aimSystem.GetAimPoint();
			_projectileShooter.ShootAt(aimPoint);
		}
		else
		{
			// Fallback to forward shooting if no aim system
			_projectileShooter.ShootForward();
		}
	}

	protected override void OnPaused()
	{
		// Cancel shooting when paused
		if (IsShooting) IsShooting = false;
	}
}
