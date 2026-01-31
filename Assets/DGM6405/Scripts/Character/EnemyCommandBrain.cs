using UnityEngine;

/// <summary>
///     Command brain for enemy/AI characters.
///     Reuses the same systems as PlayerCommandBrain but driven by AI logic instead of player input.
/// </summary>
public class EnemyCommandBrain : PausableBehaviour, ICharacterBrain
{
	[Header("AI Settings")]
	[Tooltip("Whether this enemy brain is currently active.")]
	[SerializeField] private bool _isActive = true;

	[Header("Systems")]
	[Tooltip("CharacterMovementSystem for movement commands. Required.")]
	[SerializeField] private CharacterMovementSystem _movementSystem;

	[Tooltip("JumpGravitySystem for jump commands. Optional.")]
	[SerializeField] private JumpGravitySystem _jumpGravitySystem;

	[Tooltip("CameraRotationSystem for look rotation. Optional.")]
	[SerializeField] private CameraRotationSystem _cameraRotationSystem;

	[Tooltip("BlockSystem for blocking commands. Optional.")]
	[SerializeField] private BlockSystem _blockSystem;

	[Tooltip("ShootSystem for shooting commands. Optional.")]
	[SerializeField] private ShootSystem _shootSystem;

	[Tooltip("MeleeSystem for melee attack commands. Optional.")]
	[SerializeField] private MeleeSystem _meleeSystem;

	[Tooltip("AimSystem for aim point updates. Optional.")]
	[SerializeField] private AimSystem _aimSystem;

	[Header("AI Target")]
	[Tooltip("Target transform to move towards or attack. Can be set by AI behavior.")]
	[SerializeField] private Transform _targetTransform;

	private bool _aiBlock;
	private bool _aiJump;
	private Vector2 _aiLookInput;
	private bool _aiMelee;

	// AI state (can be extended with behavior tree, state machine, etc.)
	private Vector2 _aiMoveInput;
	private bool _aiShoot;
	private bool _aiSprint;

	private void Awake()
	{
		// Get systems if not assigned (same pattern as PlayerCommandBrain)
		if (_movementSystem == null) _movementSystem = GetComponent<CharacterMovementSystem>();

		if (_jumpGravitySystem == null) _jumpGravitySystem = GetComponent<JumpGravitySystem>();

		if (_cameraRotationSystem == null) _cameraRotationSystem = GetComponent<CameraRotationSystem>();

		if (_blockSystem == null) _blockSystem = GetComponent<BlockSystem>();

		if (_shootSystem == null) _shootSystem = GetComponent<ShootSystem>();

		if (_meleeSystem == null) _meleeSystem = GetComponent<MeleeSystem>();

		if (_aimSystem == null) _aimSystem = GetComponent<AimSystem>();

		// Validate at least movement system exists
		if (_movementSystem == null)
			Debug.LogWarning(
				$"[{name}] EnemyCommandBrain: CharacterMovementSystem not found. " +
				"Enemy will not be able to move. Add CharacterMovementSystem component.",
				this
			);
	}

	// ICharacterBrain implementation
	public bool IsActive => _isActive && enabled;

	protected override void PausableUpdate()
	{
		// Check game state
		if (GameMgr.Instance == null)
		{
			Debug.LogWarning($"[{name}] EnemyCommandBrain: GameMgr.Instance is null. Skipping update.", this);
			return;
		}

		if (!GameMgr.Instance.IsGameRunning)
			return;

		if (!_isActive)
			return;

		// Update AI decision making (stub - can be replaced with behavior tree, state machine, etc.)
		UpdateAIDecisions();

		// Process movement and jump
		ProcessMovementCommands();

		// Process combat commands
		ProcessCombatCommands();
	}

	protected override void PausableLateUpdate()
	{
		// Check game state
		if (GameMgr.Instance == null || !GameMgr.Instance.IsGameRunning)
			return;

		if (!_isActive)
			return;

		// Process camera/look rotation
		ProcessLookCommands();
	}

	/// <summary>
	///     Updates AI decision making (stub implementation).
	///     Replace this with actual AI logic (NavMesh, behavior tree, state machine, etc.).
	/// </summary>
	private void UpdateAIDecisions()
	{
		// Stub implementation - replace with actual AI logic
		// Example: Simple move towards target
		if (_targetTransform != null)
		{
			var toTarget = _targetTransform.position - transform.position;
			toTarget.y = 0f; // Ignore vertical component for movement
			_aiMoveInput = new Vector2(toTarget.normalized.x, toTarget.normalized.z);
			_aiSprint = toTarget.magnitude > 5f; // Sprint if far away

			// Update aim system if available
			if (_aimSystem != null) _aimSystem.SetAimTarget(_targetTransform.position);
		}
		else
		{
			_aiMoveInput = Vector2.zero;
			_aiSprint = false;
		}

		// Stub: No combat actions for now
		_aiJump = false;
		_aiBlock = false;
		_aiShoot = false;
		_aiMelee = false;
	}

	/// <summary>
	///     Processes movement and jump commands from AI decisions.
	/// </summary>
	private void ProcessMovementCommands()
	{
		// Process movement
		if (_movementSystem != null) _movementSystem.ApplyMovement(_aiMoveInput, _aiSprint);

		// Process jump and gravity
		if (_jumpGravitySystem != null) _jumpGravitySystem.TickVertical(_aiJump);
	}

	/// <summary>
	///     Processes look rotation commands from AI decisions.
	/// </summary>
	private void ProcessLookCommands()
	{
		// Process camera/look rotation if available
		if (_cameraRotationSystem != null && _aiLookInput != Vector2.zero)
			_cameraRotationSystem.ApplyLook(_aiLookInput, false);
	}

	/// <summary>
	///     Processes combat commands from AI decisions.
	/// </summary>
	private void ProcessCombatCommands()
	{
		// Process block
		if (_blockSystem != null) _blockSystem.SetBlocking(_aiBlock);

		// Process shoot
		if (_shootSystem != null) _shootSystem.TryShoot(_aiShoot);

		// Process melee
		if (_meleeSystem != null) _meleeSystem.TryMelee(_aiMelee);
	}

	/// <summary>
	///     Sets the target transform for AI to move towards or attack.
	/// </summary>
	/// <param name="target">Target transform. Null to clear target.</param>
	public void SetTarget(Transform target)
	{
		_targetTransform = target;
	}

	/// <summary>
	///     Sets AI movement input directly (for external AI systems).
	/// </summary>
	/// <param name="moveInput">Movement input direction.</param>
	/// <param name="sprint">Whether to sprint.</param>
	public void SetMovementInput(Vector2 moveInput, bool sprint)
	{
		_aiMoveInput = moveInput;
		_aiSprint = sprint;
	}

	/// <summary>
	///     Sets AI combat actions directly (for external AI systems).
	/// </summary>
	/// <param name="block">Whether to block.</param>
	/// <param name="shoot">Whether to shoot.</param>
	/// <param name="melee">Whether to melee attack.</param>
	public void SetCombatActions(bool block, bool shoot, bool melee)
	{
		_aiBlock = block;
		_aiShoot = shoot;
		_aiMelee = melee;
	}

	protected override void OnPaused()
	{
		// Clear all AI commands when paused
		_aiMoveInput = Vector2.zero;
		_aiSprint = false;
		_aiJump = false;
		_aiBlock = false;
		_aiShoot = false;
		_aiMelee = false;

		// Notify systems to stop active actions
		if (_movementSystem != null) _movementSystem.ApplyMovement(Vector2.zero, false);

		if (_blockSystem != null) _blockSystem.SetBlocking(false);

		if (_shootSystem != null) _shootSystem.TryShoot(false);

		if (_meleeSystem != null) _meleeSystem.TryMelee(false);
	}
}
