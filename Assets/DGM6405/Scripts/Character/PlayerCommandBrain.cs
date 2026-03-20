using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
///     Command brain for player character.
///     Reads input from PlayerInputHandler and dispatches commands to modular systems.
/// </summary>
public class PlayerCommandBrain : PausableBehaviour, ILevelListener, IHealthListener
{
	[Header("Input")]
	[Tooltip("CharacterContext component. If null, will try to find on same GameObject.")]
	[SerializeField] private CharacterContext _context;

	[Tooltip("PlayerInputHandler component. Required.")]
	[SerializeField] private PlayerInputHandler _inputHandler;

#if ENABLE_INPUT_SYSTEM
	[Tooltip("PlayerInput component for detecting control scheme. Optional.")]
	[SerializeField] private PlayerInput _playerInput;
#endif
	
	[Header("Debug")]
	[SerializeField] private string _currentStateDebug;

	private bool _isBlocking;

	// Cached control scheme check
	private bool _isCurrentDeviceMouse;

	private bool _isInitialised;
	private bool _isMelee;
	private bool _isShooting;
	private bool _isDead;

	private bool _allowShoot = true;
	private bool _allowBlock = true;
	private bool _allowMelee = true;

	private void Awake()
	{
		InitializeComponents();
	}

	private void Start()
	{
		// Fallback for test scenes where level might not exist or already fired event
		if (!LevelMgr.Instance.IsLevelLoaded)
			CheckInitialisationFallback();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		GlobalEventBus.Instance?.Register<ILevelListener>(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		GlobalEventBus.Instance?.Unregister<ILevelListener>(this);
	}

	private void OnValidate()
	{
		// Auto-hookup in editor
		if (_inputHandler == null) _inputHandler = GetComponent<PlayerInputHandler>();
		if (_context == null) _context = GetComponent<CharacterContext>();
#if ENABLE_INPUT_SYSTEM
		if (_playerInput == null) _playerInput = GetComponent<PlayerInput>();
#endif
	}

	public void OnLevelReady()
	{
		if (_isInitialised) return;

		if (LevelMgr.Instance.TryGetCurrentLevelInfo(out var info))
		{
			_allowBlock = info.AllowBlock;
			_allowShoot = info.AllowShoot;
			_allowMelee = info.AllowMelee;
		}

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

	private void UpdateDebugState()
	{
		if (_isDead)
		{
			_currentStateDebug = "Dead";
			return;
		}

		if (GameMgr.Instance == null || !GameMgr.Instance.IsGameRunning)
		{
			_currentStateDebug = "Game Not Running";
			return;
		}

		System.Collections.Generic.List<string> actions = new();
		if (_isShooting) actions.Add("Shooting");
		if (_isBlocking) actions.Add("Blocking");
		if (_isMelee) actions.Add("Melee");
		if (_inputHandler != null && _inputHandler.jump) actions.Add("Jumping");
		if (_inputHandler != null && _inputHandler.move != Vector2.zero) actions.Add("Moving");

		_currentStateDebug = actions.Count > 0 ? string.Join(", ", actions) : "Idle";
	}

	private void InitializeComponents()
	{
		// Get context if not assigned
		if (_context == null) _context = GetComponent<CharacterContext>();

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

		// Validate context
		if (_context == null)
		{
			Debug.LogError(
				$"[{name}] PlayerCommandBrain: CharacterContext is required!",
				this
			);
			enabled = false;
		}
	}

	protected override void PausableUpdate()
	{
		UpdateDebugState();

		// Check game state before processing input
		if (GameMgr.Instance == null)
		{
			Debug.LogWarning($"[{name}] PlayerCommandBrain: GameMgr.Instance is null. Skipping update.", this);
			return;
		}

		if (!GameMgr.Instance.IsGameRunning)
			return;

		if (_isDead)
		{
			ProcessDeathSkipInputs();
			return;
		}

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
		if (GameMgr.Instance == null || !GameMgr.Instance.IsGameRunning || _isDead)
			return;

		// Process camera rotation in LateUpdate
		ProcessCameraCommands();
	}

	/// <summary>
	///     Processes minimal inputs during death to support skipping the death sequence.
	/// </summary>
	private void ProcessDeathSkipInputs()
	{
		if (_inputHandler == null || _context?.EventBus == null)
			return;

		// Raise skip death event if any of the skip triggers are pressed
		if (_inputHandler.jump || _inputHandler.shoot || _inputHandler.sprint || _inputHandler.block)
		{
			_context.EventBus.Raise<ISkipDeathListener>(l => l.OnSkipDeathAnimation());

			// Consume jump input specifically if it was pressed to prevent it sticking when restarting (if applicable)
			if (_inputHandler.jump)
				_inputHandler.jump = false;
		}
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
		// Validate input handler and context
		if (_inputHandler == null || _context?.EventBus == null)
			return;

		// Process movement
		_context.EventBus.Raise<IMovementListener>(l => l.OnMove(_inputHandler.move, _inputHandler.sprint));

		// Process jump and gravity
		_context.EventBus.Raise<IJumpListener>(l => l.OnJump(_inputHandler.jump));
		_inputHandler.jump = false;
	}

	/// <summary>
	///     Processes camera rotation commands from input.
	/// </summary>
	private void ProcessCameraCommands()
	{
		// Validate input handler and context
		if (_inputHandler == null || _context?.EventBus == null)
			return;

		// Process camera rotation
		_context.EventBus.Raise<ILookListener>(l => l.OnLook(_inputHandler.look, _isCurrentDeviceMouse));
	}

	/// <summary>
	///     Processes combat commands (block, shoot, melee) from input.
	/// </summary>
	private void ProcessCombatCommands()
	{
		// Validate input handler and context
		if (_inputHandler == null || _context?.EventBus == null)
			return;

		// Enforce exclusivity: Melee > Shoot > Block
		var isMeleeInput = _inputHandler.melee && _allowMelee;
		var isShootInput = _inputHandler.shoot && _allowShoot;
		var isBlockInput = _inputHandler.block && _allowBlock;

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
		if (_isMelee != isMeleeInput)
		{
			_isMelee = isMeleeInput;
			_context.EventBus.Raise<IMeleeListener>(l => l.OnMelee(isMeleeInput));
		}

		if (isMeleeInput)
			_context.EventBus.Raise<IWeaponSlotListener>(l =>
				l.OnWeaponSlotChanged(WeaponHandSlots.WeaponSlotType.Melee));

		// Process shoot
		if (_isShooting != isShootInput)
		{
			_isShooting = isShootInput;
			_context.EventBus.Raise<IShootListener>(l => l.OnShoot(isShootInput));
			UpdateRotationMode();
		}

		if (isShootInput)
			_context.EventBus.Raise<IWeaponSlotListener>(l =>
				l.OnWeaponSlotChanged(WeaponHandSlots.WeaponSlotType.Ranged));

		// Process block
		if (_isBlocking != isBlockInput)
		{
			_isBlocking = isBlockInput;
			_context.EventBus.Raise<IBlockListener>(l => l.OnBlock(isBlockInput));
			UpdateRotationMode();
		}

		if (isBlockInput)
			_context.EventBus.Raise<IWeaponSlotListener>(l =>
				l.OnWeaponSlotChanged(WeaponHandSlots.WeaponSlotType.Shield));

		// If no combat action is active, clear weapon slots
		if (!isMeleeInput && !isShootInput && !isBlockInput)
			_context.EventBus.Raise<IWeaponSlotListener>(l =>
				l.OnWeaponSlotChanged(WeaponHandSlots.WeaponSlotType.None));
	}

	private void UpdateRotationMode()
	{
		if (_context?.EventBus == null || _isDead) return;

		// When blocking or shooting/aiming, face camera. Otherwise face movement.
		var faceCamera = _isBlocking || _isShooting;
		_context.EventBus.Raise<IRotationListener>(l => l.SetRotateToCamera(faceCamera));
		_context.EventBus.Raise<IRotationListener>(l => l.SetRotateToMovement(!faceCamera));
	}

	protected override void OnPaused()
	{
		// Clear all inputs when paused
		if (_inputHandler != null) _inputHandler.ClearInputs();

		// Notify systems to stop active actions via events
		if (_context?.EventBus != null)
		{
			_isShooting = false;
			_isBlocking = false;
			_isMelee = false;

			_context.EventBus.Raise<IMovementListener>(l => l.OnMove(Vector2.zero, false));
			_context.EventBus.Raise<IBlockListener>(l => l.OnBlock(false));
			_context.EventBus.Raise<IShootListener>(l => l.OnShoot(false));
			_context.EventBus.Raise<IMeleeListener>(l => l.OnMelee(false));

			UpdateRotationMode();
		}
	}

	void IHealthListener.OnHealthChanged(float current, float max)
	{
	}

	void IHealthListener.OnDamageTaken(int amount, Vector3 direction)
	{
	}

	void IHealthListener.OnDied()
	{
		if (_isDead) return;
		_isDead = true;

		// Stop all active actions
		if (_context?.EventBus != null)
		{
			_isShooting = false;
			_isBlocking = false;
			_isMelee = false;

			_context.EventBus.Raise<IMovementListener>(l => l.OnMove(Vector2.zero, false));
			_context.EventBus.Raise<IBlockListener>(l => l.OnBlock(false));
			_context.EventBus.Raise<IShootListener>(l => l.OnShoot(false));
			_context.EventBus.Raise<IMeleeListener>(l => l.OnMelee(false));
			_context.EventBus.Raise<IJumpListener>(l => l.OnJump(false));

			// Disable rotation
			_context.EventBus.Raise<IRotationListener>(l => l.SetRotateToCamera(false));
			_context.EventBus.Raise<IRotationListener>(l => l.SetRotateToMovement(false));
		}
		GlobalEventBus.Instance?.Raise<IHealthListener>(l => l.OnDied());
	}
}
