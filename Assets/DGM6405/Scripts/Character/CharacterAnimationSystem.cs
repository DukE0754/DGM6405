using UnityEngine;

/// <summary>
/// Centralized animation system that handles all animator parameter updates.
/// Separates animation logic from other systems for better maintainability.
/// </summary>
public class CharacterAnimationSystem : PausableBehaviour
{
    [Header("References")]
    [Tooltip("CharacterContext component. If null, will try to find on same GameObject.")]
    [SerializeField] private CharacterContext _context;

    [Tooltip("Animator component. If null, will use Animator from CharacterContext.")]
    [SerializeField] private Animator _animator;

    // Animation parameter IDs (cached for performance)
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;
    private int _animIDBlock;
    private int _animIDMelee;
    private int _animIDShoot;
    private int _animIDDodge;

    // Cached animator state
    private bool _hasAnimator;

    private void Awake()
    {

        // Get context if not assigned
        if (_context == null)
        {
            _context = GetComponent<CharacterContext>();
            if (_context == null)
            {
                Debug.LogWarning(
                    $"[{name}] CharacterAnimationSystem: CharacterContext not found. " +
                    "Assign CharacterContext reference in inspector or add CharacterContext component.",
                    this
                );
            }
        }

        // Get animator from context or direct reference
        if (_animator == null)
        {
            if (_context != null)
            {
                _animator = _context.Animator;
            }
            else
            {
                _animator = GetComponent<Animator>();
            }
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

    /// <summary>
    /// Assigns animation parameter IDs using StringToHash for performance.
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
    }

    /// <summary>
    /// Updates movement animation parameters.
    /// </summary>
    /// <param name="speedBlend">Blended speed value for animation.</param>
    /// <param name="motionSpeed">Motion speed multiplier (input magnitude).</param>
    public void SetMovement(float speedBlend, float motionSpeed)
    {
        if (!_hasAnimator || _animator == null)
            return;

        _animator.SetFloat(_animIDSpeed, speedBlend);
        _animator.SetFloat(_animIDMotionSpeed, motionSpeed);
    }

    /// <summary>
    /// Updates grounded state animation parameter.
    /// </summary>
    /// <param name="grounded">Whether the character is grounded.</param>
    public void SetGrounded(bool grounded)
    {
        if (!_hasAnimator || _animator == null)
            return;

        _animator.SetBool(_animIDGrounded, grounded);
    }

    /// <summary>
    /// Sets jump animation state.
    /// </summary>
    /// <param name="isJumping">Whether the character is jumping.</param>
    public void SetJumping(bool isJumping)
    {
        if (!_hasAnimator || _animator == null)
            return;

        _animator.SetBool(_animIDJump, isJumping);
    }

    /// <summary>
    /// Sets free fall animation state.
    /// </summary>
    /// <param name="isFreeFalling">Whether the character is in free fall.</param>
    public void SetFreeFall(bool isFreeFalling)
    {
        if (!_hasAnimator || _animator == null)
            return;

        _animator.SetBool(_animIDFreeFall, isFreeFalling);
    }

    /// <summary>
    /// Sets block animation state.
    /// </summary>
    /// <param name="isBlocking">Whether the character is blocking.</param>
    public void SetBlock(bool isBlocking)
    {
        if (!_hasAnimator || _animator == null)
            return;

        _animator.SetBool(_animIDBlock, isBlocking);
    }

    /// <summary>
    /// Sets melee attack animation state.
    /// </summary>
    /// <param name="isMeleeAttacking">Whether the character is performing melee attack.</param>
    public void SetMelee(bool isMeleeAttacking)
    {
        if (!_hasAnimator || _animator == null)
            return;

        _animator.SetBool(_animIDMelee, isMeleeAttacking);
    }

    /// <summary>
    /// Sets shoot animation state.
    /// </summary>
    /// <param name="isShooting">Whether the character is shooting.</param>
    public void SetShoot(bool isShooting)
    {
        if (!_hasAnimator || _animator == null)
            return;

        _animator.SetBool(_animIDShoot, isShooting);
    }

    /// <summary>
    /// Sets dodge animation state.
    /// </summary>
    /// <param name="isDodging">Whether the character is dodging.</param>
    public void SetDodge(bool isDodging)
    {
        if (!_hasAnimator || _animator == null)
            return;

        _animator.SetBool(_animIDDodge, isDodging);
    }

    protected override void OnPaused()
    {
        // Optionally reset animation parameters when paused
        // For now, let animations continue but paused via Time.timeScale
        // Systems can call SetMovement(0, 0) if needed
    }

    private void OnValidate()
    {
        // Warn if animator not assigned
        if (_animator == null && _context == null)
        {
            Debug.LogWarning(
                $"[{name}] CharacterAnimationSystem: Animator or CharacterContext reference not assigned in inspector.",
                this
            );
        }
    }
}
