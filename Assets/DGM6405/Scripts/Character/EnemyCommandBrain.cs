using DGM6405.Events;
using UnityEngine;

/// <summary>
///     Command brain for enemy/AI characters.
///     Reuses the same systems as PlayerCommandBrain but driven by AI logic instead of player input.
/// </summary>
public class EnemyCommandBrain : PausableBehaviour, ICharacterBrain
{
	[Header("AI Settings")]
	[Tooltip("CharacterContext component. If null, will try to find on same GameObject.")]
	[SerializeField] private CharacterContext _context;

	[Tooltip("Whether this enemy brain is currently active.")]
	[SerializeField] private bool _isActive = true;

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
		InitializeComponents();
	}

	private void OnValidate()
	{
		// Auto-hookup in editor
		if (_context == null) _context = GetComponent<CharacterContext>();
	}

	// ICharacterBrain implementation
	public bool IsActive => _isActive && enabled;

	private void InitializeComponents()
	{
		// Get context if not assigned
		if (_context == null) _context = GetComponent<CharacterContext>();

		// Validate context
		if (_context == null)
		{
			Debug.LogError(
				$"[{name}] EnemyCommandBrain: CharacterContext is required!",
				this
			);
			enabled = false;
		}
	}

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

			// Update aim system if available via event
			_context?.EventBus?.Raise<IAimTargetListener>(l => l.OnSetAimTarget(_targetTransform.position));
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
		if (_context?.EventBus == null) return;

		// Process movement
		_context.EventBus.Raise<IMovementListener>(l => l.OnMove(_aiMoveInput, _aiSprint));

		// Process jump and gravity
		_context.EventBus.Raise<IJumpListener>(l => l.OnJump(_aiJump));
	}

	/// <summary>
	///     Processes look rotation commands from AI decisions.
	/// </summary>
	private void ProcessLookCommands()
	{
		if (_context?.EventBus == null) return;

		// AI character rotation (facing)
		if (_aiLookInput != Vector2.zero)
		{
			var worldDirection = new Vector3(_aiLookInput.x, 0f, _aiLookInput.y).normalized;
			_context.EventBus.Raise<IRotationListener>(l => l.OnRotate(worldDirection));
		}

		// Process camera/look rotation if available (for rare cases where enemy has a camera)
		if (_aiLookInput != Vector2.zero)
			_context.EventBus.Raise<ILookListener>(l => l.OnLook(_aiLookInput, false));
	}

	/// <summary>
	///     Processes combat commands from AI decisions.
	/// </summary>
	private void ProcessCombatCommands()
	{
		if (_context?.EventBus == null) return;

		// Process block
		_context.EventBus.Raise<IBlockListener>(l => l.OnBlock(_aiBlock));

		// Process shoot
		_context.EventBus.Raise<IShootListener>(l => l.OnShoot(_aiShoot));

		// Process melee
		_context.EventBus.Raise<IMeleeListener>(l => l.OnMelee(_aiMelee));
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

		// Notify systems to stop active actions via events
		if (_context?.EventBus != null)
		{
			_context.EventBus.Raise<IMovementListener>(l => l.OnMove(Vector2.zero, false));
			_context.EventBus.Raise<IBlockListener>(l => l.OnBlock(false));
			_context.EventBus.Raise<IShootListener>(l => l.OnShoot(false));
			_context.EventBus.Raise<IMeleeListener>(l => l.OnMelee(false));
		}
	}
}
