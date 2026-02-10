using FIMSpace;
using UnityEngine;

/// <summary>
///     Connects character movement and grounding state to the LeaningAnimator component.
///     Follows the decoupled system architecture of the project.
/// </summary>
public class LeaningAnimatorSystem : PausableBehaviour, IMovementSpeedListener, IGroundListener, IMovementListener
{
	[Header("References")]
	[Tooltip("CharacterContext component. If null, will try to find on same GameObject.")]
	[SerializeField] private CharacterContext _context;

	[Tooltip("LeaningAnimator component to manage. If null, will try to find on same GameObject or children.")]
	[SerializeField] private LeaningAnimator _leaningAnimator;

	[Header("Settings")]
	[Tooltip("If true, this system will automatically configure LeaningAnimator for manual control on Start.")]
	[SerializeField] private bool _autoConfigure = true;

	private float _originalAllEffectsBlend = 1f;
	private bool _isMovingInputActive;

	private void Awake()
	{
		// Get context if not assigned
		if (_context == null) _context = GetComponent<CharacterContext>();

		// Get leaning animator if not assigned
		if (_leaningAnimator == null)
		{
			_leaningAnimator = GetComponentInChildren<LeaningAnimator>();
		}

		if (_leaningAnimator == null)
		{
			Debug.LogWarning(
				$"[{name}] LeaningAnimatorSystem: LeaningAnimator not found. " +
				"Assign LeaningAnimator reference in inspector or add LeaningAnimator component.",
				this
			);
			enabled = false;
			return;
		}

		_originalAllEffectsBlend = _leaningAnimator.Parameters.AllEffectsBlend;
	}

	private void Start()
	{
		if (_autoConfigure && _leaningAnimator != null)
		{
			// Recommended settings from Readme: disable auto-detection to allow manual control through this system
			_leaningAnimator.Parameters.AccelerationDetection =
				LeaningProcessor.EMotionDetection.CustomDetection_AutoDetectionOFF;
			_leaningAnimator.Parameters.TryAutoDetectGround = false;
		}
	}

	/// <summary>
	///     Captures movement input intention for better responsiveness.
	/// </summary>
	void IMovementListener.OnMove(Vector2 moveInput, bool isSprinting)
	{
		_isMovingInputActive = moveInput.sqrMagnitude > 0.001f;

		if (_leaningAnimator != null)
		{
			// Update acceleration state based on input intention. 
			// If false, and speed is high, LeaningAnimator handles braking lean.
			_leaningAnimator.SetIsAccelerating = _isMovingInputActive;
		}
	}

	/// <summary>
	///     Updates LeaningAnimator with movement speed and synchronization of reference speeds.
	/// </summary>
	void IMovementSpeedListener.OnSpeedChanged(float speed, float animationBlend, float walkSpeed, float sprintSpeed,
		float velocityX, float velocityZ)
	{
		if (_leaningAnimator == null) return;

		// Deliver current speed to LeaningAnimator
		_leaningAnimator.User_DeliverAccelerationSpeed(speed);

		// Sync reference speeds for procedural animation scaling
		_leaningAnimator.Parameters.ObjSpeedWhenBraking = walkSpeed;
		_leaningAnimator.Parameters.ObjSpeedWhenRunning = sprintSpeed;
	}

	/// <summary>
	///     Updates LeaningAnimator grounding state.
	/// </summary>
	void IGroundListener.OnGroundedChanged(bool grounded)
	{
		if (_leaningAnimator == null) return;
		_leaningAnimator.SetIsGrounded = grounded;
	}

	/// <summary>
	///     Informs LeaningAnimator that the character is falling.
	/// </summary>
	void IGroundListener.OnFall()
	{
		if (_leaningAnimator == null) return;
		_leaningAnimator.SetIsGrounded = false;
	}

	protected override void OnPaused()
	{
		if (_leaningAnimator != null) _leaningAnimator.Parameters.AllEffectsBlend = 0f;
	}

	protected override void OnResumed()
	{
		if (_leaningAnimator != null) _leaningAnimator.Parameters.AllEffectsBlend = _originalAllEffectsBlend;
	}

	protected override void PausableUpdate()
	{
		// LeaningAnimator handles its own internal updates in Update, FixedUpdate, and LateUpdate
	}
}
