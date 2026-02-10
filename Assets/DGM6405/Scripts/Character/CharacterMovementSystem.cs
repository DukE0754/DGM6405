using UnityEngine;

/// <summary>
///     Handles horizontal movement and rotation for character.
///     Uses CharacterController for movement and rotates character to face movement direction.
/// </summary>
public class CharacterMovementSystem : PausableBehaviour, IMovementListener
{
	private const float THRESHOLD = 0.01f;

	[Header("Movement Settings")]
	[Tooltip("Move speed of the character in m/s")]
	[SerializeField] private float _moveSpeed = 2.0f;

	[Tooltip("Sprint speed of the character in m/s")]
	[SerializeField] private float _sprintSpeed = 5.335f;

	[Tooltip("How fast the character turns to face movement direction")]
	[Range(0.0f, 0.3f)]
	[SerializeField] private float _rotationSmoothTime = 0.12f;

	[Tooltip("Acceleration and deceleration rate")]
	[SerializeField] private float _speedChangeRate = 10.0f;

	[Header("References")]
	[Tooltip("CharacterContext component. If null, will try to find on same GameObject.")]
	[SerializeField] private CharacterContext _context;

	[Tooltip("CharacterController component. If null, will use from CharacterContext.")]
	[SerializeField] private CharacterController _controller;

	[Tooltip("Main camera transform. If null, will try to find Camera.main.")]
	[SerializeField] private Transform _mainCamera;

	[Tooltip("JumpGravitySystem for getting vertical velocity. Required for proper movement.")]
	[SerializeField] private JumpGravitySystem _jumpGravitySystem;

	[Header("Debug Gizmos")]
	[Tooltip("Show movement gizmos in scene view when selected.")]
	[SerializeField] private bool _showGizmos = true;

	private float _rotationVelocity;

	// Internal movement state

	// Public properties
	public float Speed { get; private set; }

	public float AnimationBlend { get; private set; }

	public float TargetRotation { get; private set; }

	private void Awake()
	{
		// Get context if not assigned
		if (_context == null) _context = GetComponent<CharacterContext>();

		// Get controller from context or direct reference
		if (_controller == null)
		{
			if (_context != null)
				_controller = _context.Controller;
			else
				_controller = GetComponent<CharacterController>();
		}

		// Validate controller
		if (_controller == null)
		{
			Debug.LogError(
				$"[{name}] CharacterMovementSystem: CharacterController is required! " +
				"Either add CharacterController component or assign CharacterContext with controller reference.",
				this
			);
			enabled = false;
			return;
		}

		// Find main camera if not assigned
		if (_mainCamera == null)
		{
			var mainCam = Camera.main;
			if (mainCam != null)
				_mainCamera = mainCam.transform;
			else
				Debug.LogWarning(
					$"[{name}] CharacterMovementSystem: Main camera not found. " +
					"Movement rotation relative to camera will not work correctly. " +
					"Assign camera reference in inspector or ensure scene has a camera tagged 'MainCamera'.",
					this
				);
		}

		if (_jumpGravitySystem == null)
		{
			_jumpGravitySystem = GetComponent<JumpGravitySystem>();
			if (_jumpGravitySystem == null)
				Debug.LogWarning(
					$"[{name}] CharacterMovementSystem: JumpGravitySystem not found. " +
					"Vertical movement will not work correctly. Add JumpGravitySystem component.",
					this
				);
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (!_showGizmos)
			return;

		// Velocity visualization
		if (_controller != null)
		{
			var velocity = _controller.velocity;
			velocity.y = 0f; // Horizontal only
			var speedMagnitude = velocity.magnitude;

			// Color based on movement direction
			if (speedMagnitude > 0.1f)
			{
				var dot = Vector3.Dot(transform.forward, velocity.normalized);
				if (dot > 0.5f)
					Gizmos.color = Color.green; // Moving forward
				else if (dot < -0.5f)
					Gizmos.color = Color.yellow; // Moving backward
				else
					Gizmos.color = Color.cyan; // Moving sideways
			}
			else
			{
				Gizmos.color = Color.red; // Stationary
			}

			// Draw velocity line
			var startPos = transform.position;
			var endPos = startPos + velocity.normalized * Mathf.Min(speedMagnitude, 5f);
			Gizmos.DrawLine(startPos, endPos);
			Gizmos.DrawWireSphere(endPos, 0.1f);
		}

		// Target rotation indicator
		if (TargetRotation != 0f)
		{
			Gizmos.color = Color.magenta;
			var forward = Quaternion.Euler(0f, TargetRotation, 0f) * Vector3.forward;
			Gizmos.DrawRay(transform.position, forward * 2f);
		}

		// Current forward direction
		Gizmos.color = Color.blue;
		Gizmos.DrawRay(transform.position, transform.forward * 1.5f);
	}

	private void OnValidate()
	{
		// Clamp values to valid ranges
		_moveSpeed = Mathf.Max(0f, _moveSpeed);
		_sprintSpeed = Mathf.Max(_moveSpeed, _sprintSpeed); // Sprint must be >= move speed
		_speedChangeRate = Mathf.Max(0f, _speedChangeRate);
	}

	/// <summary>
	///     Applies movement based on input direction and sprint state.
	/// </summary>
	/// <param name="moveInput">Normalized input direction (x, y).</param>
	/// <param name="sprint">Whether sprint is active.</param>
	void IMovementListener.OnMove(Vector2 moveInput, bool sprint)
	{
		ApplyMovement(moveInput, sprint);
	}

	protected override void PausableUpdate()
	{
		// Check game state
		if (GameMgr.Instance == null)
		{
			Debug.LogWarning($"[{name}] CharacterMovementSystem: GameMgr.Instance is null. Skipping update.", this);
			return;
		}

		if (!GameMgr.Instance.IsGameRunning)
			return;

		// Movement is applied via ApplyMovement() called by command brain
		// This update loop can be used for continuous movement if needed
	}

	private void ApplyMovement(Vector2 moveInput, bool sprint)
	{
		// Validate controller
		if (_controller == null)
		{
			Debug.LogError(
				$"[{name}] CharacterMovementSystem: CharacterController reference is null! Assign in inspector.", this);
			return;
		}

		// Set target speed based on move speed, sprint speed and if sprint is pressed
		var inputMagnitude = Mathf.Clamp01(moveInput.magnitude);
		var targetSpeed = (sprint ? _sprintSpeed : _moveSpeed) * inputMagnitude;

		// A reference to the players current horizontal velocity
		var currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

		var speedOffset = 0.1f;

		// Accelerate or decelerate to target speed
		if (currentHorizontalSpeed < targetSpeed - speedOffset ||
			currentHorizontalSpeed > targetSpeed + speedOffset)
		{
			// Creates curved result rather than a linear one giving a more organic speed change
			// Note T in Lerp is clamped, so we don't need to clamp our speed
			Speed = Mathf.Lerp(
				currentHorizontalSpeed, targetSpeed,
				Time.deltaTime * _speedChangeRate);

			// Round speed to 3 decimal places
			Speed = Mathf.Round(Speed * 1000f) / 1000f;
		}
		else
		{
			Speed = targetSpeed;
		}

		AnimationBlend = Mathf.Lerp(AnimationBlend, targetSpeed, Time.deltaTime * _speedChangeRate);
		// Removed threshold snap to allow smooth transition to 0 in animator blend tree

		// Normalize input direction
		var inputDirection = new Vector3(moveInput.x, 0.0f, moveInput.y).normalized;

		// Calculate target rotation relative to camera
		if (moveInput != Vector2.zero && _mainCamera != null)
			TargetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
							_mainCamera.transform.eulerAngles.y;

		var targetDirection = Quaternion.Euler(0.0f, TargetRotation, 0.0f) * Vector3.forward;

		// Get vertical velocity from jump/gravity system
		var verticalVelocity = 0f;
		if (_jumpGravitySystem != null) verticalVelocity = _jumpGravitySystem.VerticalVelocity;

		// Move the player
		var movement = targetDirection.normalized * (Speed * Time.deltaTime) +
						new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime;
		_controller.Move(movement);

		// Calculate local velocity for animations (VelocityX and VelocityZ)
		// We want the velocity relative to the character's orientation
		var localVelocity = transform.InverseTransformDirection(_controller.velocity);

		// Update systems via events
		if (_context != null && _context.EventBus != null)
			_context.EventBus.Raise<IMovementSpeedListener>(l =>
				l.OnSpeedChanged(Speed, AnimationBlend, _moveSpeed, _sprintSpeed, localVelocity.x, localVelocity.z));
	}

	protected override void OnPaused()
	{
		// Clear movement state when paused
		Speed = 0f;
		AnimationBlend = 0f;

		// Notify systems via events
		if (_context != null && _context.EventBus != null)
			_context.EventBus.Raise<IMovementSpeedListener>(l => l.OnSpeedChanged(
				0f, 0f, _moveSpeed, _sprintSpeed, 0f, 0f));
	}
}
