using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
///     Command brain for player character.
///     Reads input from PlayerInputHandler and dispatches commands to modular systems.
/// </summary>
public class PlayerCommandBrain : PausableBehaviour
{
	[Header("Input")]
	[Tooltip("PlayerInputHandler component. Required.")]
	[SerializeField] private PlayerInputHandler _inputHandler;

#if ENABLE_INPUT_SYSTEM
	[Tooltip("PlayerInput component for detecting control scheme. Optional.")]
	[SerializeField] private PlayerInput _playerInput;
#endif

	[Header("Systems")]
	[Tooltip("CharacterMovementSystem for movement commands. Required.")]
	[SerializeField] private CharacterMovementSystem _movementSystem;

	[Tooltip("JumpGravitySystem for jump commands. Required.")]
	[SerializeField] private JumpGravitySystem _jumpGravitySystem;

	[Tooltip("CameraRotationSystem for camera rotation. Required.")]
	[SerializeField] private CameraRotationSystem _cameraRotationSystem;

	[Tooltip("BlockSystem for blocking commands. Optional.")]
	[SerializeField] private BlockSystem _blockSystem;

	[Tooltip("ShootSystem for shooting commands. Optional.")]
	[SerializeField] private ShootSystem _shootSystem;

	[Tooltip("MeleeSystem for melee attack commands. Optional.")]
	[SerializeField] private MeleeSystem _meleeSystem;

	[Tooltip("AimSystem for aim point updates. Optional.")]
	[SerializeField] private AimSystem _aimSystem;

	[Tooltip("WeaponHandSlots for clearing weapon slots when no combat system is active. Optional.")]
	[SerializeField] private WeaponHandSlots _weaponHandSlots;

	// Cached control scheme check
	private bool _isCurrentDeviceMouse;

	private void Awake()
	{
		InitializeComponents();
	}

	private void OnEnable()
	{
		GameLoopManager.OnLevelReady += OnLevelReady;
	}

	private void OnDisable()
	{
		GameLoopManager.OnLevelReady -= OnLevelReady;
	}

	private void Start()
	{
		// Fallback for test scenes where GameLoopManager might not exist or already fired event
		// Wait a frame to ensure GameLoopManager has a chance to run its Start
		Invoke(nameof(CheckInitialisationFallback), 0.1f);
	}

	private bool _isInitialised = false;

	private void OnLevelReady()
	{
		if (_isInitialised) return;
		ApplyLevelRestrictions();
		_isInitialised = true;
	}

	private void CheckInitialisationFallback()
	{
		if (!_isInitialised)
		{
			Debug.Log($"[{name}] PlayerCommandBrain: Initialising via fallback (Test Scene mode).", this);
			// Default to all enabled in test scenes
			EnableAllSystems();
			_isInitialised = true;
			GameMgr.Instance.StartGame();
		}
	}

	private void InitializeComponents()
	{
		// Get input handler if not assigned
		if (_inputHandler == null) _inputHandler = GetComponent<PlayerInputHandler>();

		// Validate input handler
		if (_inputHandler == null)
		{
			Debug.LogError(
				$"[{name}] PlayerCommandBrain: PlayerInputHandler is required! " +
				"Add PlayerInputHandler component or assign reference in inspector.",
				this
			);
			enabled = false;
			return;
		}

#if ENABLE_INPUT_SYSTEM
		// Get player input if not assigned
		if (_playerInput == null) _playerInput = GetComponent<PlayerInput>();
#endif

		// Get movement system if not assigned
		if (_movementSystem == null) _movementSystem = GetComponent<CharacterMovementSystem>();

		// Validate movement system
		if (_movementSystem == null)
		{
			Debug.LogError(
				$"[{name}] PlayerCommandBrain: CharacterMovementSystem is required! " +
				"Add CharacterMovementSystem component or assign reference in inspector.",
				this
			);
			enabled = false;
			return;
		}

		// Get jump gravity system if not assigned
		if (_jumpGravitySystem == null) _jumpGravitySystem = GetComponent<JumpGravitySystem>();

		// Validate jump gravity system
		if (_jumpGravitySystem == null)
		{
			Debug.LogError(
				$"[{name}] PlayerCommandBrain: JumpGravitySystem is required! " +
				"Add JumpGravitySystem component or assign reference in inspector.",
				this
			);
			enabled = false;
			return;
		}

		// Get camera rotation system if not assigned
		if (_cameraRotationSystem == null) _cameraRotationSystem = GetComponent<CameraRotationSystem>();

		// Validate camera rotation system
		if (_cameraRotationSystem == null)
		{
			Debug.LogError(
				$"[{name}] PlayerCommandBrain: CameraRotationSystem is required! " +
				"Add CameraRotationSystem component or assign reference in inspector.",
				this
			);
			enabled = false;
			return;
		}

		// Get combat systems if not assigned (optional)
		if (_blockSystem == null) _blockSystem = GetComponent<BlockSystem>();

		if (_shootSystem == null) _shootSystem = GetComponent<ShootSystem>();

		if (_meleeSystem == null) _meleeSystem = GetComponent<MeleeSystem>();

		if (_aimSystem == null) _aimSystem = GetComponent<AimSystem>();

		if (_weaponHandSlots == null) _weaponHandSlots = GetComponent<WeaponHandSlots>();
	}

	private void ApplyLevelRestrictions()
	{
		if (LevelMgr.Instance.TryGetCurrentLevelInfo(out var levelInfo))
		{
			if (_blockSystem != null) _blockSystem.enabled = levelInfo.AllowBlock;
			if (_shootSystem != null) _shootSystem.enabled = levelInfo.AllowShoot;
			if (_meleeSystem != null) _meleeSystem.enabled = levelInfo.AllowMelee;
			
			// Aim system should be enabled only if shooting is allowed
			if (_aimSystem != null) _aimSystem.enabled = levelInfo.AllowShoot;
			
			Debug.Log($"[{name}] PlayerCommandBrain: Level restrictions applied. " +
			          $"Block:{levelInfo.AllowBlock}, Shoot:{levelInfo.AllowShoot}, Melee:{levelInfo.AllowMelee}, Aim:{levelInfo.AllowShoot}", this);
		}
		else
		{
			EnableAllSystems();
		}
	}

	private void EnableAllSystems()
	{
		if (_blockSystem != null) _blockSystem.enabled = true;
		if (_shootSystem != null) _shootSystem.enabled = true;
		if (_meleeSystem != null) _meleeSystem.enabled = true;
		if (_aimSystem != null) _aimSystem.enabled = true;
	}

	private void OnValidate()
	{
		// Warn if required components not assigned
		if (_inputHandler == null)
			Debug.LogWarning(
				$"[{name}] PlayerCommandBrain: PlayerInputHandler reference not assigned in inspector.", this);

		if (_movementSystem == null)
			Debug.LogWarning(
				$"[{name}] PlayerCommandBrain: CharacterMovementSystem reference not assigned in inspector.", this);

		if (_jumpGravitySystem == null)
			Debug.LogWarning(
				$"[{name}] PlayerCommandBrain: JumpGravitySystem reference not assigned in inspector.", this);

		if (_cameraRotationSystem == null)
			Debug.LogWarning(
				$"[{name}] PlayerCommandBrain: CameraRotationSystem reference not assigned in inspector.", this);
	}

	protected override void PausableUpdate()
	{
		// Check game state before processing input
		if (GameMgr.Instance == null)
		{
			Debug.LogWarning($"[{name}] PlayerCommandBrain: GameMgr.Instance is null. Skipping update.", this);
			return;
		}

		if (!GameMgr.Instance.IsGameRunning)
			return;

		// Update control scheme check
		UpdateControlScheme();

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

		// Process camera rotation in LateUpdate
		ProcessCameraCommands();
	}

	/// <summary>
	///     Updates the current control scheme detection.
	/// </summary>
	private void UpdateControlScheme()
	{
#if ENABLE_INPUT_SYSTEM
		if (_playerInput != null)
			_isCurrentDeviceMouse = _playerInput.currentControlScheme == "KeyboardMouse";
		else
			_isCurrentDeviceMouse = false;
#else
        _isCurrentDeviceMouse = false;
#endif
	}

	/// <summary>
	///     Processes movement and jump commands from input.
	/// </summary>
	private void ProcessMovementCommands()
	{
		// Validate input handler
		if (_inputHandler == null)
			return;

		// Process movement
		if (_movementSystem != null) _movementSystem.ApplyMovement(_inputHandler.move, _inputHandler.sprint);

		// Process jump and gravity
		if (_jumpGravitySystem != null)
		{
			_jumpGravitySystem.TickVertical(_inputHandler.jump);
			_inputHandler.jump = false;
		}
	}

	/// <summary>
	///     Processes camera rotation commands from input.
	/// </summary>
	private void ProcessCameraCommands()
	{
		// Validate input handler and camera system
		if (_inputHandler == null || _cameraRotationSystem == null)
			return;

		// Process camera rotation
		_cameraRotationSystem.ApplyLook(_inputHandler.look, _isCurrentDeviceMouse);
	}

	/// <summary>
	///     Processes combat commands (block, shoot, melee) from input.
	/// </summary>
	private void ProcessCombatCommands()
	{
		// Validate input handler
		if (_inputHandler == null)
			return;

		// Enforce exclusivity: Melee > Shoot > Block
		var isMeleeInput = _inputHandler.melee;
		var isShootInput = _inputHandler.shoot;
		var isBlockInput = _inputHandler.block;

		if (isMeleeInput)
		{
			isShootInput = false;
			isBlockInput = false;
		}
		else if (isShootInput)
		{
			isBlockInput = false;
		}

		// Process melee
		if (_meleeSystem != null && _meleeSystem.enabled) 
			_meleeSystem.TryMelee(isMeleeInput);
		else
			isMeleeInput = false;

		// Process shoot
		if (_shootSystem != null && _shootSystem.enabled) 
			_shootSystem.TryShoot(isShootInput);
		else
			isShootInput = false;

		// Process block
		if (_blockSystem != null && _blockSystem.enabled) 
			_blockSystem.SetBlocking(isBlockInput);
		else
			isBlockInput = false;

		// If no combat action is active, clear weapon slots
		if (!isMeleeInput && !isShootInput && !isBlockInput)
		{
			if (_weaponHandSlots != null)
				_weaponHandSlots.SetActiveSlot(WeaponHandSlots.WeaponSlotType.None);
		}

		// Note: Dodge can be added here when implemented
		// if (_dodgeSystem != null)
		// {
		//     _dodgeSystem.TryDodge(_inputHandler.dodge);
		// }
	}

	protected override void OnPaused()
	{
		// Clear all inputs when paused
		if (_inputHandler != null) _inputHandler.ClearInputs();

		// Notify systems to stop active actions
		if (_movementSystem != null) _movementSystem.ApplyMovement(Vector2.zero, false);

		if (_blockSystem != null) _blockSystem.SetBlocking(false);

		if (_shootSystem != null) _shootSystem.TryShoot(false);

		if (_meleeSystem != null) _meleeSystem.TryMelee(false);
	}
}
