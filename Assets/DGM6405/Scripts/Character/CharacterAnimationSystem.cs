using UnityEngine;

/// <summary>
///     Centralized animation system that handles all animator parameter updates.
///     Separates animation logic from other systems for better maintainability.
/// </summary>
public class CharacterAnimationSystem : PausableBehaviour,
	IMovementSpeedListener, IGroundListener, IShootListener, IMeleeListener, IBlockListener, IJumpListener,
	IHealthListener,
	IBlockHitListener
{
	[Header("Animation Settings")]
	[Tooltip("Smoothing time for animation parameter changes")]
	[SerializeField] private float _animSmoothingTime = 0.1f;

	[Header("Animation Reference Speeds")]
	[Tooltip("Speed the character moves at when the walk animation is at full influence (motion speed 1)")]
	[SerializeField] private float _animationWalkSpeed = 2.0f;

	[Tooltip("Speed the character moves at when the run animation is at full influence (motion speed 1)")]
	[SerializeField] private float _animationRunSpeed = 5.335f;

	[Header("References")]
	[Tooltip("CharacterContext component. If null, will try to find on same GameObject.")]
	[SerializeField] private CharacterContext _context;

	[Tooltip("Animator component. If null, will use Animator from CharacterContext.")]
	[SerializeField] private Animator _animator;

	private int _animIDBlock;
	private int _animIDBlockedHit;
	private int _animIDDie;
	private int _animIDDodge;
	private int _animIDFreeFall;
	private int _animIDGrounded;
	private int _animIDHit;
	private int _animIDJump;
	private int _animIDMelee;
	private int _animIDMotionSpeed;
	private int _animIDShoot;

	// Animation parameter IDs (cached for performance)
	private int _animIDSpeed;
	private int _animIDVelocityX;
	private int _animIDVelocityZ;

	// Cached animator state
	private bool _hasAnimator;
	private bool _hasBlockedHitParam;

	private void Awake()
	{
		// Get context if not assigned
		if (_context == null)
		{
			_context = GetComponent<CharacterContext>();
			if (_context == null)
				Debug.LogWarning(
					$"[{name}] CharacterAnimationSystem: CharacterContext not found. " +
					"Assign CharacterContext reference in inspector or add CharacterContext component.",
					this
				);
		}

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
		{
			Debug.LogWarning(
				$"[{name}] CharacterAnimationSystem: Animator not found. Animation updates will be skipped.",
				this
			);
			_hasAnimator = false;
		}
		else
		{
			_hasAnimator = true;
			AssignAnimationIDs();
		}
	}

	private void OnValidate()
	{
		// Warn if animator not assigned
		if (_animator == null && _context == null)
			Debug.LogWarning(
				$"[{name}] CharacterAnimationSystem: Animator or CharacterContext reference not assigned in inspector.",
				this
			);

		// Clamp reference speeds to valid ranges
		_animationWalkSpeed = Mathf.Max(0.01f, _animationWalkSpeed);
		_animationRunSpeed = Mathf.Max(_animationWalkSpeed, _animationRunSpeed);
	}

	void IBlockHitListener.OnBlockHit(Vector3 hitPoint, Vector3 hitNormal, GameObject source)
	{
		SetBlockedHit();
	}

	/// <summary>
	///     Sets block animation state.
	/// </summary>
	/// <param name="blockInput"></param>
	void IBlockListener.OnBlock(bool blockInput)
	{
		SetBlock(blockInput);
	}

	/// <summary>
	///     Updates grounded state animation parameter.
	/// </summary>
	/// <param name="grounded">Whether the character is grounded.</param>
	public void OnGroundedChanged(bool grounded)
	{
		SetGrounded(grounded);
		if (grounded)
		{
			SetJumping(false);
			SetFreeFall(false);
		}
	}

	/// <summary>
	///     Sets free fall animation state.
	/// </summary>
	public void OnFall()
	{
		SetFreeFall(true);
	}

	void IHealthListener.OnHealthChanged(float current, float max)
	{
	}

	void IHealthListener.OnDamageTaken(int amount, Vector3 direction)
	{
		SetHit();
	}

	void IHealthListener.OnDied()
	{
		SetDie();
	}

	/// <summary>
	///     Sets jump animation state.
	/// </summary>
	public void OnJumpPerformed()
	{
		SetJumping(true);
	}

	/// <summary>
	///     Sets melee attack animation state.
	/// </summary>
	/// <param name="meleeInput"></param>
	void IMeleeListener.OnMelee(bool meleeInput)
	{
		SetMelee(meleeInput);
	}

	/// <summary>
	///     Updates movement animation parameters.
	/// </summary>
	/// <param name="animationBlend"></param>
	/// <param name="walkSpeed">Movement speed threshold for walking.</param>
	/// <param name="sprintSpeed">Movement speed threshold for sprinting.</param>
	/// <param name="speed"></param>
	/// <param name="velocityX"></param>
	/// <param name="velocityZ"></param>
	public void OnSpeedChanged(
		float speed, float animationBlend, float walkSpeed, float sprintSpeed, float velocityX,
		float velocityZ)
	{
		SetMovement(animationBlend, speed, walkSpeed, sprintSpeed, velocityX, velocityZ);
	}

	/// <summary>
	///     Sets shoot animation state.
	/// </summary>
	/// <param name="shootInput"></param>
	void IShootListener.OnShoot(bool shootInput)
	{
		SetShoot(shootInput);
	}

	/// <summary>
	///     Assigns animation parameter IDs using StringToHash for performance.
	/// </summary>
	private void AssignAnimationIDs()
	{
		_animIDSpeed = Animator.StringToHash("Speed");
		_animIDGrounded = Animator.StringToHash("Grounded");
		_animIDJump = Animator.StringToHash("Jump");
		_animIDFreeFall = Animator.StringToHash("FreeFall");
		_animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
		_animIDBlock = Animator.StringToHash("Block");
		_animIDMelee = Animator.StringToHash("Melee");
		_animIDShoot = Animator.StringToHash("Shoot");
		_animIDDodge = Animator.StringToHash("Dodge");
		_animIDHit = Animator.StringToHash("Hit");
		_animIDBlockedHit = Animator.StringToHash("BlockedHit");
		_animIDDie = Animator.StringToHash("Die");
		_animIDVelocityX = Animator.StringToHash("VelocityX");
		_animIDVelocityZ = Animator.StringToHash("VelocityZ");

		_hasBlockedHitParam = HasAnimatorParam("BlockedHit", AnimatorControllerParameterType.Trigger);
	}

	private void SetMovement(
		float speedBlend, float currentSpeed, float walkSpeed, float sprintSpeed, float velocityX,
		float velocityZ)
	{
		if (!_hasAnimator || _animator == null)
			return;

		// Calculate motion speed multiplier based on reference speeds
		var motionSpeed = 1f;
		const float threshold = 0.01f;

		if (currentSpeed > threshold)
		{
			// Interpolate expected speed based on current speedBlend
			// speedBlend is 0 (Idle) -> walkSpeed (Walk) -> sprintSpeed (Run)
			// Reference speeds are _animationWalkSpeed and _animationRunSpeed
			float expectedSpeed;
			if (speedBlend <= walkSpeed)
				// For the idle-to-walk transition, the Walk animation's playback speed 
				// should be proportional to the actual movement speed to avoid sliding.
				// We use the Walk reference speed as the baseline for this entire range.
				expectedSpeed = _animationWalkSpeed;
			else
				expectedSpeed = Mathf.Lerp(
					_animationWalkSpeed, _animationRunSpeed,
					(speedBlend - walkSpeed) / (sprintSpeed - walkSpeed));

			// Motion speed is actual speed / expected speed to align animation playback
			if (expectedSpeed > threshold) motionSpeed = currentSpeed / expectedSpeed;
		}
		else
		{
			// When nearly stopped, keep motion speed at 1 to allow the idle transition to play at normal speed
			motionSpeed = 1f;
		}

		_animator.SetFloat(_animIDSpeed, speedBlend, _animSmoothingTime, Time.deltaTime);
		_animator.SetFloat(_animIDMotionSpeed, motionSpeed, _animSmoothingTime, Time.deltaTime);
		_animator.SetFloat(_animIDVelocityX, velocityX, _animSmoothingTime, Time.deltaTime);
		_animator.SetFloat(_animIDVelocityZ, velocityZ, _animSmoothingTime, Time.deltaTime);
	}

	private void SetGrounded(bool grounded)
	{
		if (!_hasAnimator || _animator == null)
			return;

		_animator.SetBool(_animIDGrounded, grounded);
	}

	private void SetJumping(bool isJumping)
	{
		if (!_hasAnimator || _animator == null)
			return;

		_animator.SetBool(_animIDJump, isJumping);
	}

	private void SetFreeFall(bool isFreeFalling)
	{
		if (!_hasAnimator || _animator == null)
			return;

		_animator.SetBool(_animIDFreeFall, isFreeFalling);
	}

	private void SetBlock(bool isBlocking)
	{
		if (!_hasAnimator || _animator == null)
			return;

		_animator.SetBool(_animIDBlock, isBlocking);
	}

	private void SetMelee(bool isMeleeAttacking)
	{
		if (!_hasAnimator || _animator == null)
			return;

		_animator.SetBool(_animIDMelee, isMeleeAttacking);
	}

	private void SetShoot(bool isShooting)
	{
		if (!_hasAnimator || _animator == null)
			return;

		_animator.SetBool(_animIDShoot, isShooting);
	}

	/// <summary>
	///     Sets dodge animation state.
	/// </summary>
	/// <param name="isDodging">Whether the character is dodging.</param>
	public void SetDodge(bool isDodging)
	{
		if (!_hasAnimator || _animator == null)
			return;

		_animator.SetBool(_animIDDodge, isDodging);
	}

	private void SetHit()
	{
		if (!_hasAnimator || _animator == null)
			return;

		_animator.SetTrigger(_animIDHit);
	}

	private void SetBlockedHit()
	{
		if (!_hasAnimator || _animator == null)
			return;

		if (_hasBlockedHitParam)
			_animator.SetTrigger(_animIDBlockedHit);
		else
			SetHit();
	}

	private void SetDie()
	{
		if (!_hasAnimator || _animator == null)
			return;

		_animator.SetTrigger(_animIDDie);
	}

	private bool HasAnimatorParam(string paramName, AnimatorControllerParameterType paramType)
	{
		if (_animator == null) return false;

		var parameters = _animator.parameters;
		for (var i = 0; i < parameters.Length; i++)
			if (parameters[i].name == paramName && parameters[i].type == paramType)
				return true;

		return false;
	}

	protected override void OnPaused()
	{
		// Optionally reset animation parameters when paused
		// For now, let animations continue but paused via Time.timeScale
		// Systems can call SetMovement(0, 0, 0, 0, 0) if needed
	}
}
