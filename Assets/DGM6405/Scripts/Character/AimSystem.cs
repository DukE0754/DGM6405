using UnityEngine;

/// <summary>
///     Handles aiming direction calculation and IK for head/weapon alignment.
///     Computes aim point from camera (player) or target transform (AI).
/// </summary>
public class AimSystem : PausableBehaviour, IAimTargetListener
{
	[Header("Aim Settings")]
	[Tooltip("Maximum aim distance. Targets beyond this distance will be clamped.")]
	[SerializeField] private float _maxAimDistance = 100f;

	[Tooltip("Smooth damping for aim point changes.")]
	[SerializeField] private float _aimSmoothing = 0.1f;

	[Header("IK Settings")]
	[Tooltip("Weight for head IK (0-1).")]
	[Range(0f, 1f)]
	[SerializeField] private float _headIKWeight = 1f;

	[Tooltip("Weight for hand IK (0-1).")]
	[Range(0f, 1f)]
	[SerializeField] private float _handIKWeight = 1f;

	[Header("References")]
	[Tooltip("CharacterContext component. If null, will try to find on same GameObject.")]
	[SerializeField] private CharacterContext _context;

	[Tooltip("Animator component. Required for IK. If null, will use from CharacterContext.")]
	[SerializeField] private Animator _animator;

	[Tooltip("Head bone transform for IK. If null, will try to find automatically.")]
	[SerializeField] private Transform _headBone;

	[Tooltip("Weapon hand bone transform for IK. If null, will try to find automatically.")]
	[SerializeField] private Transform _weaponHandBone;

	[Tooltip("Target transform for AI aiming. Leave null for player (uses camera).")]
	[SerializeField] private Transform _targetTransform;

	[Header("Debug Gizmos")]
	[Tooltip("Show aim system gizmos in scene view when selected.")]
	[SerializeField] private bool _showGizmos = true;


	private Transform _mainCamera;

	// Internal state
	private Vector3 _smoothAimPoint;
	private Vector3 _smoothAimVelocity;

	// Public properties
	public bool IsAiming { get; private set; }

	public Vector3 CurrentAimPoint { get; private set; }

	private void Awake()
	{
		// Get context if not assigned
		if (_context == null) _context = GetComponent<CharacterContext>();

		// Get animator from context or direct reference
		if (_animator == null)
		{
			if (_context != null)
				_animator = _context.Animator;
			else
				_animator = GetComponent<Animator>();
		}

		// Validate animator
		if (_animator == null)
			Debug.LogWarning(
				$"[{name}] AimSystem: Animator not found. IK will not work. " +
				"Add Animator component or assign CharacterContext with animator reference.",
				this
			);

		// Use event bus from context if available
		var bus = _context != null ? _context.EventBus : GetComponent<LocalEventBus>();
		if (bus == null)
			Debug.LogWarning(
				$"[{name}] AimSystem: LocalEventBus not found. Event-based aiming will not work.",
				this
			);

		// Find head bone if not assigned
		if (_headBone == null && _animator != null)
		{
			// Try to find head bone by name (common names)
			string[] headBoneNames = {"Head", "head", "HeadBone", "headBone", "neck", "Neck"};
			foreach (var boneName in headBoneNames)
			{
				var found = FindChildRecursive(transform, boneName);
				if (found != null)
				{
					_headBone = found;
					break;
				}
			}

			if (_headBone == null)
				Debug.LogWarning(
					$"[{name}] AimSystem: Head bone not found. Head IK will not work. " +
					"Assign head bone transform in inspector.",
					this
				);
		}

		// Find weapon hand bone if not assigned
		if (_weaponHandBone == null && _animator != null)
		{
			// Try to find weapon hand bone by name (common names)
			string[] handBoneNames = {"hand_r", "Hand_R", "RightHand", "rightHand", "weaponHand", "WeaponHand"};
			foreach (var boneName in handBoneNames)
			{
				var found = FindChildRecursive(transform, boneName);
				if (found != null)
				{
					_weaponHandBone = found;
					break;
				}
			}

			if (_weaponHandBone == null)
				Debug.LogWarning(
					$"[{name}] AimSystem: Weapon hand bone not found. Hand IK will not work. " +
					"Assign weapon hand bone transform in inspector.",
					this
				);
		}

		_mainCamera = _context?.CharacterCamera?.transform;
	}

	/// <summary>
	///     Unity's OnAnimatorIK callback for IK calculations.
	/// </summary>
	private void OnAnimatorIK(int layerIndex)
	{
		// Check if animator is valid
		if (_animator == null)
			return;

		// Check if aiming
		if (!IsAiming)
			return;

		// Apply head IK
		if (_headBone != null && _headIKWeight > 0f)
		{
			var lookDirection = (_smoothAimPoint - _headBone.position).normalized;
			var lookRotation = Quaternion.LookRotation(lookDirection);
			_animator.SetLookAtWeight(_headIKWeight, 0f, 1f, 0f, 0.5f);
			_animator.SetLookAtPosition(_smoothAimPoint);
		}

		// Apply hand IK (if needed, can be extended with custom IK solution)
		// For now, basic implementation - can be enhanced with FimpIK plugins
		if (_weaponHandBone != null && _handIKWeight > 0f)
		{
			// Basic hand IK - can be enhanced with full IK solution
			// This is a placeholder for more advanced IK implementation
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (!_showGizmos)
			return;

		// Aim target visualization
		if (IsAiming)
		{
			var startPos = transform.position;
			if (_headBone != null) startPos = _headBone.position;

			// Aim line
			Gizmos.color = Color.magenta;
			Gizmos.DrawLine(startPos, _smoothAimPoint);

			// Aim point sphere
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(_smoothAimPoint, 0.2f);

			// IK target positions
			if (_headBone != null)
			{
				Gizmos.color = Color.cyan;
				Gizmos.DrawWireSphere(_headBone.position, 0.15f);
			}

			if (_weaponHandBone != null)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawWireSphere(_weaponHandBone.position, 0.1f);
			}
		}

		// Aim range limit
		Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
		Gizmos.DrawWireSphere(transform.position, _maxAimDistance);
	}

	private void OnValidate()
	{
		// Clamp values to valid ranges
		_maxAimDistance = Mathf.Max(0f, _maxAimDistance);
		_aimSmoothing = Mathf.Max(0f, _aimSmoothing);
	}

	/// <summary>
	///     Sets the aim target for AI characters.
	/// </summary>
	/// <param name="worldPosition">World position to aim at.</param>
	public void OnSetAimTarget(Vector3 worldPosition)
	{
		SetAimTarget(worldPosition);
	}

	protected override void PausableLateUpdate()
	{
		// Check game state
		if (GameMgr.Instance == null)
		{
			Debug.LogWarning($"[{name}] AimSystem: GameMgr.Instance is null. Skipping update.", this);
			return;
		}

		if (!GameMgr.Instance.IsGameRunning)
		{
			IsAiming = false;
			return;
		}

		// Update aim point
		UpdateAimPoint();

		// Smooth aim point
		if (_aimSmoothing > 0f)
			_smoothAimPoint = Vector3.SmoothDamp(
				_smoothAimPoint, CurrentAimPoint, ref _smoothAimVelocity, _aimSmoothing);
		else
			_smoothAimPoint = CurrentAimPoint;

		// Raise event
		if (IsAiming && _context != null && _context.EventBus != null)
			_context.EventBus.Raise<IAimListener>(l => l.OnAimUpdate(_smoothAimPoint));
	}

	/// <summary>
	///     Updates the current aim point based on camera or target transform.
	/// </summary>
	private void UpdateAimPoint()
	{
		Vector3 aimDirection;

		// Player: use camera forward
		if (_targetTransform == null && _mainCamera != null)
		{
			aimDirection = _mainCamera.forward;
			CurrentAimPoint = _mainCamera.position + aimDirection * _maxAimDistance;
			IsAiming = true;
		}
		// AI: use target transform
		else if (_targetTransform != null)
		{
			var toTarget = _targetTransform.position - transform.position;
			var distance = Mathf.Min(toTarget.magnitude, _maxAimDistance);
			aimDirection = toTarget.normalized;
			CurrentAimPoint = transform.position + aimDirection * distance;
			IsAiming = true;
		}
		// Fallback: use character forward
		else
		{
			aimDirection = transform.forward;
			CurrentAimPoint = transform.position + aimDirection * _maxAimDistance;
			IsAiming = false;
		}
	}

	/// <summary>
	///     Gets the current computed aim point.
	/// </summary>
	/// <returns>World position of aim point.</returns>
	public Vector3 GetAimPoint()
	{
		return _smoothAimPoint;
	}

	private void SetAimTarget(Vector3 worldPosition)
	{
		CurrentAimPoint = worldPosition;
		IsAiming = true;
	}

	/// <summary>
	///     Recursively finds a child transform by name.
	/// </summary>
	private Transform FindChildRecursive(Transform parent, string name)
	{
		foreach (Transform child in parent)
		{
			if (child.name == name)
				return child;

			var found = FindChildRecursive(child, name);
			if (found != null)
				return found;
		}

		return null;
	}

	protected override void OnPaused()
	{
		// Reset aiming state when paused
		IsAiming = false;
	}
}
