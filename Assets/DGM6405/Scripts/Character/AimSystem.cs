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

	[Header("Auto-Lock Settings")]
	[Tooltip("DetectionSystem for auto-locking targets. Optional for players.")]
	[SerializeField] private DetectionSystem _detectionSystem;

	

	[Tooltip("A physical transform that represents the aim point. If assigned, its position will be updated.")]
	[SerializeField] private Transform _visualAimTarget;

	[Header("Debug Gizmos")]
	[Tooltip("Show aim system gizmos in scene view when selected.")]
	[SerializeField] private bool _showGizmos = true;
	
	[Header("Debug")]
	[SerializeField] private string _currentStateDebug;


	private Transform _mainCamera;
	private Transform _targetTransform;

	// Internal state
	private Vector3 _smoothAimPoint;
	private Vector3 _smoothAimVelocity;
	private Transform _lockedTarget;

	// Public properties
	public bool IsAiming { get; private set; }
	public bool IsTargeting { get; private set; }

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

		if (_detectionSystem == null) _detectionSystem = GetComponent<DetectionSystem>();
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
			_animator.SetLookAtWeight(_headIKWeight, 0f, 1f, 0f, 0.5f);
			_animator.SetLookAtPosition(_smoothAimPoint);
		}

		// Apply hand/weapon IK
		if (_weaponHandBone != null && _handIKWeight > 0f)
		{
			// Basic hand orientation towards target
			// Note: For more complex weapons, we'd use a two-bone IK or Fabrik
			// but for now we'll use the Animator's IK to set a goal position
			// or just let the ProjectileWeapon handle its own muzzle rotation.
			// To truly "point" the arm, we can use SetIKPosition:
			_animator.SetIKPositionWeight(AvatarIKGoal.RightHand, _handIKWeight);
			_animator.SetIKPosition(AvatarIKGoal.RightHand, _smoothAimPoint);
			
			// Optional: Rotate hand to face target
			var handToTarget = (_smoothAimPoint - _weaponHandBone.position).normalized;
			if (handToTarget != Vector3.zero)
			{
				_animator.SetIKRotationWeight(AvatarIKGoal.RightHand, _handIKWeight);
				_animator.SetIKRotation(AvatarIKGoal.RightHand, Quaternion.LookRotation(handToTarget));
			}
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
			Gizmos.color = _lockedTarget != null ? Color.red : Color.magenta;
			Gizmos.DrawLine(startPos, _smoothAimPoint);

			// Aim point sphere
			Gizmos.color = _lockedTarget != null ? Color.red : Color.green;
			Gizmos.DrawWireSphere(_smoothAimPoint, 0.2f);

			if (_lockedTarget != null)
			{
				var targetCenter = GetTargetCenter(_lockedTarget);
				Gizmos.DrawWireCube(targetCenter, Vector3.one * 0.5f);
				
				// Line to actual target center (not smoothed)
				Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
				Gizmos.DrawLine(startPos, targetCenter);
			}

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
		UpdateDebugState();

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
		
		IsTargeting = _targetTransform != null || _lockedTarget != null;

		// Smooth aim point
		if (_aimSmoothing > 0f)
			_smoothAimPoint = Vector3.SmoothDamp(
				_smoothAimPoint, CurrentAimPoint, ref _smoothAimVelocity, _aimSmoothing);
		else
			_smoothAimPoint = CurrentAimPoint;

		// Update visual aim target if assigned
		if (_visualAimTarget != null)
		{
			_visualAimTarget.position = _smoothAimPoint;
		}

		// Raise event
		if (IsAiming && _context != null && _context.EventBus != null)
		{
			_context.EventBus.Raise<IAimListener>(l => l.OnAimUpdate(_smoothAimPoint, IsTargeting));
			_context.EventBus.Raise<IAimTargetListener>(l => l.OnSetAimTarget(_smoothAimPoint));
		}
	}

	private void UpdateDebugState()
	{
		if (GameMgr.Instance == null || !GameMgr.Instance.IsGameRunning)
		{
			_currentStateDebug = "Game Not Running";
			return;
		}

		if (!IsAiming)
		{
			_currentStateDebug = "Not Aiming";
			return;
		}

		if (IsTargeting)
		{
			string targetName = _lockedTarget != null ? _lockedTarget.name : (_targetTransform != null ? _targetTransform.name : "Unknown");
			_currentStateDebug = $"Targeting: {targetName}";
		}
		else
		{
			_currentStateDebug = "Aiming (Fallback)";
		}
	}

	/// <summary>
	///     Updates the current aim point based on camera or target transform.
	/// </summary>
	private void UpdateAimPoint()
	{
		Vector3 aimDirection;
		Vector3 origin = transform.position;
		if (_headBone != null) origin = _headBone.position;
		else if (_mainCamera != null) origin = _mainCamera.position;

		// Try auto-locking if we have a detection system
		if (_detectionSystem != null)
		{
			_lockedTarget = _detectionSystem.GetBestTarget();
		}

		// Calculate target world position
		Vector3 targetPos = Vector3.zero;
		bool hasTarget = false;

		if (_targetTransform != null)
		{
			targetPos = GetTargetCenter(_targetTransform);
			hasTarget = true;
		}
		else if (_lockedTarget != null)
		{
			targetPos = GetTargetCenter(_lockedTarget);
			hasTarget = true;
		}

		// AI or Player with auto-lock target
		if (hasTarget)
		{
			// When we have a specific target, we want to overlap with it eventually.
			// We only clamp distance for fallback/camera-based aiming where we shoot into "infinity".
			CurrentAimPoint = targetPos;
			IsAiming = true;
		}
		// Player: use camera forward
		else if (_mainCamera != null)
		{
			aimDirection = _mainCamera.forward;
			CurrentAimPoint = _mainCamera.position + aimDirection * _maxAimDistance;
			IsAiming = true;
		}
		// Fallback: use character forward
		else
		{
			aimDirection = transform.forward;
			CurrentAimPoint = origin + aimDirection * _maxAimDistance;
			IsAiming = false;
		}
	}

	/// <summary>
	///     Gets the center of a target, prioritizing colliders if available.
	/// </summary>
	private Vector3 GetTargetCenter(Transform target)
	{
		if (target == null) return Vector3.zero;

		// Try to find a collider via ColliderMgr
		var colliders = target.GetComponentsInChildren<Collider>();
		foreach (var col in colliders)
		{
			if (ColliderMgr.Instance != null && ColliderMgr.Instance.TryGetDamageReceiver(col, out _))
			{
				return col.bounds.center;
			}
		}

		// Fallback 1: Use a child called "Center" if it exists
		var center = target.Find("Center");
		if (center != null) return center.position;

		// Fallback 2: Offset from base transform (assuming humanoid)
		return target.position + Vector3.up * 1f;
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
