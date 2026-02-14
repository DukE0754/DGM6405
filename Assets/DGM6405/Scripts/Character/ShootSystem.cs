using UnityEngine;

/// <summary>
///     Handles shooting/ranged combat.
///     Manages shooting state and triggers animations.
///     Actual projectile firing is handled by weapons listening to animator events.
/// </summary>
public class ShootSystem : PausableBehaviour, IShootListener
{
	[Header("References")]
	[Tooltip("AimSystem for getting aim point. Optional, but recommended for accurate aiming.")]
	[SerializeField] private AimSystem _aimSystem;

	[Tooltip("CharacterContext component. If null, will try to find on same GameObject.")]
	[SerializeField] private CharacterContext _context;

	[Header("Debug Gizmos")]
	[Tooltip("Show shoot system gizmos in scene view when selected.")]
	[SerializeField] private bool _showGizmos = true;

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
			var firePoint = transform.position;

			// Aim line
			Gizmos.color = Color.red;
			Gizmos.DrawLine(firePoint, aimPoint);

			// Aim point sphere
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(aimPoint, 0.15f);
		}
	}

	private void OnValidate()
	{
		// Auto-hookup in editor
		if (_context == null) _context = GetComponent<CharacterContext>();
		if (_aimSystem == null) _aimSystem = GetComponent<AimSystem>();
	}

	/// <summary>
	///     Attempts to shoot based on input command.
	/// </summary>
	/// <param name="shootInput">Whether shoot input is active.</param>
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
		IsShooting = isShooting;
	}

	protected override void OnPaused()
	{
		// Cancel shooting when paused
		if (IsShooting) IsShooting = false;
	}
}
