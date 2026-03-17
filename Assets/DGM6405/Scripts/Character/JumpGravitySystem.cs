// Assets/Scripts/Character/Movement/JumpGravitySystem.cs

using UnityEngine;

/// <summary>
///     Vertical movement for CharacterController.
///     Compatible with PlayerCommandBrain:
///     - Brain raises IJumpListener.OnJump(bool) every frame and clears inputHandler.jump immediately after.
///     - Therefore OnJump(true) is treated as a one-frame "pressed" pulse.
///     Features:
///     - Coyote time as "time since last grounded" (robust reset).
///     - Jump buffer (press a bit early -> jump on landing).
///     - Gravity always applied (no coyote float); optional gravity ramp never starts at 0.
///     - Ground snap / ledge safety (flat platforms).
///     - Restores OnGroundedChanged for landing transitions.
///     - OnFall fires once per airborne episode (after fall timeout).
/// </summary>
public class JumpGravitySystem : PausableBehaviour, IJumpListener, IWaterVolumeListener
{
	private const float StickToGroundVelocity = -2f;
	private const float JumpingUpThreshold = 0.1f;
	private const float TerminalVelocity = 53.0f;

	[Header("Jump Settings")]
	[SerializeField] private float _jumpHeight = 1.2f;

	[Tooltip("Negative value.")]
	[SerializeField] private float _gravity = -15.0f;

	[Tooltip("Cooldown after a jump before another jump can start. Set 0 for immediate.")]
	[SerializeField] private float _jumpTimeout;

	[Tooltip("Delay before raising OnFall (event timing only).")]
	[SerializeField] private float _fallTimeout = 0.10f;

	[Header("Forgiveness Settings")]
	[SerializeField] private float _coyoteTime = 0.12f;

	[SerializeField] private float _jumpBufferTime = 0.12f;

	[Header("Anti 'Late Midair Jump'")]
	[Tooltip("If falling faster than this (more negative), coyote is disabled for this airborne episode.")]
	[SerializeField] private float _coyoteMaxFallSpeed = -3.0f;

	[Header("Gravity Shaping (no zero-gravity coyote)")]
	[Tooltip("Seconds to ramp gravity after leaving ground. Set 0 for instant full gravity.")]
	[SerializeField] private float _gravityRampTime = 0.08f;

	[Tooltip("Gravity scale at the instant you leave ground (must be > 0 to avoid 'no gravity' feel).")]
	[SerializeField] [Range(0.05f, 1f)] private float _minAirGravityScale = 0.35f;

	[Header("Early Fall Clamp (optional)")]
	[SerializeField] private float _earlyFallTime = 0.10f;

	[SerializeField] private float _earlyFallMaxDownSpeed = 2.0f;

	[Header("Ground Snap / Ledge Safety (flat platforms)")]
	[SerializeField] private float _groundSnapDistance = 0.25f;

	[SerializeField] private float _groundSnapWindow = 0.12f;

	[SerializeField] [Range(0f, 1f)]
	private float _groundMinNormalY = 0.9f;

	[Header("Water Settings")]
	[SerializeField] private float _waterDrag = 10f;

	[SerializeField] private float _waterTerminalVelocity = -2f;

	[Header("Ground Detection")]
	[SerializeField] private float _groundedOffset = -0.14f;

	[SerializeField] private float _groundedRadius = 0.28f;
	[SerializeField] private LayerMask _groundLayers;

	[Header("References")]
	[SerializeField] private CharacterContext _context;

	private CharacterController _controller;
	private bool _coyoteDisabledThisAir;

	// Event gating
	private bool _fallEventFiredThisAir;
	private float _fallTimeoutRemaining;

	private bool _isInWater;
	private float _jumpBufferRemaining;

	// Timers/state
	private float _jumpCooldownRemaining;

	// Input pulse from brain (OnJump(true) occurs for one frame)
	private bool _jumpPressedThisFrame;

	// Robust coyote: time since last grounded + per-air-episode disable
	private float _lastGroundedTime;

	private float _timeSinceLeftGround;

	public bool IsGrounded { get; private set; }
	public float VerticalVelocity { get; private set; }

	private void Awake()
	{
		InitializeComponents();

		_jumpCooldownRemaining = 0f;
		_jumpBufferRemaining = 0f;
		_fallTimeoutRemaining = _fallTimeout;

		_timeSinceLeftGround = 0f;

		_lastGroundedTime = Time.time;
		_coyoteDisabledThisAir = false;

		_fallEventFiredThisAir = false;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		_context?.EventBus?.Register<IWaterVolumeListener>(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_context?.EventBus?.Unregister<IWaterVolumeListener>(this);
	}

	private void OnValidate()
	{
		_jumpHeight = Mathf.Max(0f, _jumpHeight);
		_jumpTimeout = Mathf.Max(0f, _jumpTimeout);
		_fallTimeout = Mathf.Max(0f, _fallTimeout);

		_coyoteTime = Mathf.Max(0f, _coyoteTime);
		_jumpBufferTime = Mathf.Max(0f, _jumpBufferTime);

		_gravityRampTime = Mathf.Max(0f, _gravityRampTime);
		_minAirGravityScale = Mathf.Clamp(_minAirGravityScale, 0.05f, 1f);

		_earlyFallTime = Mathf.Max(0f, _earlyFallTime);
		_earlyFallMaxDownSpeed = Mathf.Max(0f, _earlyFallMaxDownSpeed);

		_groundSnapDistance = Mathf.Max(0f, _groundSnapDistance);
		_groundSnapWindow = Mathf.Max(0f, _groundSnapWindow);

		_groundedRadius = Mathf.Max(0f, _groundedRadius);
	}

	/// <summary>
	///     Brain semantics: OnJump(true) is a one-frame press pulse.
	/// </summary>
	void IJumpListener.OnJump(bool jumpRequested)
	{
		if (jumpRequested)
			_jumpPressedThisFrame = true;
	}

	void IWaterVolumeListener.OnEnteredWater(float surfaceHeight)
	{
		_isInWater = true;

		// Water cancels jump forgiveness.
		_jumpBufferRemaining = 0f;
		_coyoteDisabledThisAir = true;
	}

	void IWaterVolumeListener.OnExitedWater()
	{
		_isInWater = false;
		// Next time we touch ground, coyote resets naturally.
	}

	protected override void PausableUpdate()
	{
		if (GameMgr.Instance == null || !GameMgr.Instance.IsGameRunning)
			return;

		SimulateVertical();
		_jumpPressedThisFrame = false; // consume pulse
	}

	private void SimulateVertical()
	{
		if (_controller == null)
			return;

		var dt = Time.deltaTime;

		var wasGrounded = IsGrounded;
		GroundedCheckAndSnap(); // updates IsGrounded

		if (wasGrounded != IsGrounded)
			_context?.EventBus?.Raise<IGroundListener>(l => l.OnGroundedChanged(IsGrounded));

		if (IsGrounded)
		{
			_lastGroundedTime = Time.time;
			_timeSinceLeftGround = 0f;

			_fallTimeoutRemaining = _fallTimeout;
			_fallEventFiredThisAir = false;

			_coyoteDisabledThisAir = false; // re-arm for next ledge
		}
		else
		{
			_timeSinceLeftGround += dt;

			// Once we're clearly falling, treat as committed drop for THIS air episode.
			if (!_coyoteDisabledThisAir && VerticalVelocity <= _coyoteMaxFallSpeed)
				_coyoteDisabledThisAir = true;

			if (!_fallEventFiredThisAir)
			{
				_fallTimeoutRemaining -= dt;
				if (_fallTimeoutRemaining <= 0f)
				{
					_fallEventFiredThisAir = true;
					_context?.EventBus?.Raise<IGroundListener>(l => l.OnFall());
				}
			}
		}

		UpdateJumpTimers(dt);
		TryConsumeBufferedJump();
		ApplyVerticalPhysics(dt);
	}

	private void UpdateJumpTimers(float dt)
	{
		if (_jumpPressedThisFrame)
			_jumpBufferRemaining = _jumpBufferTime;
		else if (_jumpBufferRemaining > 0f)
			_jumpBufferRemaining -= dt;

		if (_jumpCooldownRemaining > 0f)
			_jumpCooldownRemaining -= dt;
	}

	private bool HasCoyoteTime()
	{
		if (_coyoteTime <= 0f)
			return false;

		if (_coyoteDisabledThisAir)
			return false;

		return Time.time - _lastGroundedTime <= _coyoteTime;
	}

	private void TryConsumeBufferedJump()
	{
		if (_jumpBufferRemaining <= 0f)
			return;

		if (_jumpCooldownRemaining > 0f)
			return;

		if (_isInWater)
			return;

		var canJump = IsGrounded || HasCoyoteTime();
		if (!canJump)
			return;

		PerformJump();
	}

	private void PerformJump()
	{
		VerticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);

		_jumpBufferRemaining = 0f;
		_jumpCooldownRemaining = _jumpTimeout;

		// After jumping, coyote should not apply until you touch ground again.
		_coyoteDisabledThisAir = true;

		_context?.EventBus?.Raise<IJumpListener>(l => l.OnJumpPerformed());
	}

	private void ApplyVerticalPhysics(float dt)
	{
		if (_isInWater)
		{
			ApplyWater(dt);
			return;
		}

		if (IsGrounded)
		{
			if (VerticalVelocity < 0f)
				VerticalVelocity = StickToGroundVelocity;
			return;
		}

		var gravityScale = ComputeAirGravityScale();
		VerticalVelocity += _gravity * gravityScale * dt;

		// Smooth early-fall limiter to avoid "invisible ledge" clunk.
		if (_timeSinceLeftGround < _earlyFallTime)
		{
			var t = Mathf.Clamp01(_timeSinceLeftGround / Mathf.Max(0.0001f, _earlyFallTime));

			// Fade out the limiter over the window: at t=0 strong, at t=1 none.
			// maxDown starts near -_earlyFallMaxDownSpeed and moves toward a large negative (effectively no cap).
			var startCap = -_earlyFallMaxDownSpeed;
			var endCap = -TerminalVelocity;

			// SmoothStep fade so it doesn't "release" suddenly.
			var smooth = t * t * (3f - 2f * t);
			var cap = Mathf.Lerp(startCap, endCap, smooth);

			// If we're falling faster than the current cap, ease toward it instead of snapping.
			if (VerticalVelocity < cap)
			{
				// How quickly we enforce the cap (bigger = tighter, smaller = smoother).
				const float capTightness = 40f;
				VerticalVelocity = Mathf.MoveTowards(VerticalVelocity, cap, capTightness * Time.deltaTime);
			}
		}

		if (VerticalVelocity < -TerminalVelocity)
			VerticalVelocity = -TerminalVelocity;
	}

	private float ComputeAirGravityScale()
	{
		if (_gravityRampTime <= 0f)
			return 1f;

		var t = Mathf.Clamp01(_timeSinceLeftGround / _gravityRampTime);
		var smooth = t * t * (3f - 2f * t); // SmoothStep
		return Mathf.Lerp(_minAirGravityScale, 1f, smooth);
	}

	private void ApplyWater(float dt)
	{
		if (VerticalVelocity < _waterTerminalVelocity)
		{
			VerticalVelocity = Mathf.MoveTowards(
				VerticalVelocity, _waterTerminalVelocity, _waterDrag * dt);
			return;
		}

		VerticalVelocity += _gravity * dt;
		if (VerticalVelocity < _waterTerminalVelocity)
			VerticalVelocity = _waterTerminalVelocity;
	}

	private void GroundedCheckAndSnap()
	{
		var spherePos = new Vector3(
			transform.position.x,
			transform.position.y - _groundedOffset,
			transform.position.z);

		var grounded = Physics.CheckSphere(
			spherePos, _groundedRadius, _groundLayers, QueryTriggerInteraction.Ignore);

		// Water overrides grounded for jump purposes.
		IsGrounded = grounded && !_isInWater;

		if (!IsGrounded && !_isInWater)
		{
			var allowSnap =
				_timeSinceLeftGround <= _groundSnapWindow &&
				VerticalVelocity <= JumpingUpThreshold;

			if (allowSnap && TrySnapToGround(spherePos))
				IsGrounded = true;
		}
	}

	private bool TrySnapToGround(Vector3 spherePos)
	{
		if (_groundSnapDistance <= 0f)
			return false;

		var origin = spherePos + Vector3.up * 0.05f;

		if (!Physics.SphereCast(
				origin,
				_groundedRadius,
				Vector3.down,
				out var hit,
				_groundSnapDistance + 0.05f,
				_groundLayers,
				QueryTriggerInteraction.Ignore))
			return false;

		return hit.normal.y >= _groundMinNormalY;
	}

	private void InitializeComponents()
	{
		if (_context == null)
			_context = GetComponent<CharacterContext>();

		_controller = _context != null ? _context.Controller : GetComponent<CharacterController>();

		if (_controller == null)
		{
			Debug.LogError($"[{name}] JumpGravitySystem: CharacterController is required.", this);
			enabled = false;
		}
	}

	protected override void OnPaused()
	{
		VerticalVelocity = 0f;
		_jumpPressedThisFrame = false;
		_fallEventFiredThisAir = false;
		_coyoteDisabledThisAir = false;
	}
}
