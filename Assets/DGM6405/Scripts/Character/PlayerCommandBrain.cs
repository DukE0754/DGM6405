using DGM6405.Events;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
///     Command brain for player character.
///     Reads input from PlayerInputHandler and dispatches commands to modular systems.
/// </summary>
public class PlayerCommandBrain : PausableBehaviour, ILevelListener
{
	[Header("Events")]
	[SerializeField] private LocalEventBus _bus;

	[Header("Input")]
	[Tooltip("PlayerInputHandler component. Required.")]
	[SerializeField] private PlayerInputHandler _inputHandler;

#if ENABLE_INPUT_SYSTEM
	[Tooltip("PlayerInput component for detecting control scheme. Optional.")]
	[SerializeField] private PlayerInput _playerInput;
#endif

	// Cached control scheme check
	private bool _isCurrentDeviceMouse;

	private bool _isInitialised;

	private void Awake()
	{
		InitializeComponents();
	}

	private void Start()
	{
		// Fallback for test scenes where GameLoopManager might not exist or already fired event
		// Wait a frame to ensure GameLoopManager has a chance to run its Start
		Invoke(nameof(CheckInitialisationFallback), 0.1f);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		GlobalEventBus.Instance.Register<ILevelListener>(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		GlobalEventBus.Instance.Unregister<ILevelListener>(this);
	}

	private void OnValidate()
	{
		// Auto-hookup in editor
		if (_inputHandler == null) _inputHandler = GetComponent<PlayerInputHandler>();
		if (_bus == null) _bus = GetComponent<LocalEventBus>();
#if ENABLE_INPUT_SYSTEM
		if (_playerInput == null) _playerInput = GetComponent<PlayerInput>();
#endif
	}

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
			_isInitialised = true;
			GameMgr.Instance.StartGame();
		}
	}

	private void InitializeComponents()
	{
		// Validate input handler
		if (_inputHandler == null)
		{
			Debug.LogError(
				$"[{name}] PlayerCommandBrain: PlayerInputHandler is required! " +
				"Assign reference in inspector.",
				this
			);
			enabled = false;
			return;
		}

		// Validate bus
		if (_bus == null)
		{
			Debug.LogError(
				$"[{name}] PlayerCommandBrain: LocalEventBus is required! " +
				"Assign reference in inspector.",
				this
			);
			enabled = false;
		}
	}

	private void ApplyLevelRestrictions()
	{
		// With events, we might need a different way to handle restrictions.
		// For now, we'll keep it simple and just enable/disable the brain's processing.
		if (LevelMgr.Instance.TryGetCurrentLevelInfo(out var levelInfo))
			Debug.Log($"[{name}] PlayerCommandBrain: Level restrictions applied (via LevelInfo).", this);
		else
			EnableAllSystems();
	}

	private void EnableAllSystems()
	{
		// This might be handled by the systems themselves now.
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
		if (_inputHandler == null || _bus == null)
			return;

		// Process movement
		_bus.Raise<IMovementListener>(l => l.OnMove(_inputHandler.move, _inputHandler.sprint));

		// Process jump and gravity
		_bus.Raise<IJumpListener>(l => l.OnJump(_inputHandler.jump));
		_inputHandler.jump = false;
	}

	/// <summary>
	///     Processes camera rotation commands from input.
	/// </summary>
	private void ProcessCameraCommands()
	{
		// Validate input handler and camera system
		if (_inputHandler == null || _bus == null)
			return;

		// Process camera rotation
		// Note: CameraRotationSystem should implement a listener or we handle it differently.
		// For now, let's assume it implements IAimListener or similar, but looking is slightly different.
		// I'll add ILookListener.
		_bus.Raise<ILookListener>(l => l.OnLook(_inputHandler.look, _isCurrentDeviceMouse));
	}

	/// <summary>
	///     Processes combat commands (block, shoot, melee) from input.
	/// </summary>
	private void ProcessCombatCommands()
	{
		// Validate input handler
		if (_inputHandler == null || _bus == null)
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
		_bus.Raise<IMeleeListener>(l => l.OnMelee(isMeleeInput));
		if (isMeleeInput)
			_bus.Raise<IWeaponSlotListener>(l => l.OnWeaponSlotChanged(WeaponHandSlots.WeaponSlotType.Melee));

		// Process shoot
		_bus.Raise<IShootListener>(l => l.OnShoot(isShootInput));
		if (isShootInput)
			_bus.Raise<IWeaponSlotListener>(l => l.OnWeaponSlotChanged(WeaponHandSlots.WeaponSlotType.Ranged));

		// Process block
		_bus.Raise<IBlockListener>(l => l.OnBlock(isBlockInput));
		if (isBlockInput)
			_bus.Raise<IWeaponSlotListener>(l => l.OnWeaponSlotChanged(WeaponHandSlots.WeaponSlotType.Shield));

		// If no combat action is active, clear weapon slots
		if (!isMeleeInput && !isShootInput && !isBlockInput)
			_bus.Raise<IWeaponSlotListener>(l => l.OnWeaponSlotChanged(WeaponHandSlots.WeaponSlotType.None));
	}

	protected override void OnPaused()
	{
		// Clear all inputs when paused
		if (_inputHandler != null) _inputHandler.ClearInputs();

		// Notify systems to stop active actions via events
		if (_bus != null)
		{
			_bus.Raise<IMovementListener>(l => l.OnMove(Vector2.zero, false));
			_bus.Raise<IBlockListener>(l => l.OnBlock(false));
			_bus.Raise<IShootListener>(l => l.OnShoot(false));
			_bus.Raise<IMeleeListener>(l => l.OnMelee(false));
		}
	}
}
