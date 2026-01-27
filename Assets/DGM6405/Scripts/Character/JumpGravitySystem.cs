using UnityEngine;

/// <summary>
/// Handles vertical movement including jumping and gravity.
/// Manages ground detection and vertical velocity.
/// </summary>
public class JumpGravitySystem : PausableBehaviour
{
    [Header("Jump Settings")]
    [Tooltip("The height the player can jump")]
    [SerializeField] private float _jumpHeight = 1.2f;

    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    [SerializeField] private float _gravity = -15.0f;

    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    [SerializeField] private float _jumpTimeout = 0.50f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    [SerializeField] private float _fallTimeout = 0.15f;

    [Header("Ground Detection")]
    [Tooltip("Useful for rough ground")]
    [SerializeField] private float _groundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    [SerializeField] private float _groundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    [SerializeField] private LayerMask _groundLayers;

    [Header("Debug Gizmos")]
    [Tooltip("Show jump/gravity gizmos in scene view when selected.")]
    [SerializeField] private bool _showGizmos = true;

    [Header("References")]
    [Tooltip("CharacterContext component. If null, will try to find on same GameObject.")]
    [SerializeField] private CharacterContext _context;

    [Tooltip("CharacterController component. If null, will use from CharacterContext.")]
    [SerializeField] private CharacterController _controller;

    [Tooltip("CharacterAnimationSystem for updating jump/fall animations. Optional.")]
    [SerializeField] private CharacterAnimationSystem _animationSystem;

    // Internal state
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;
    private bool _isGrounded;

    // Public properties
    public bool IsGrounded => _isGrounded;
    public float VerticalVelocity => _verticalVelocity;

    private void Awake()
    {

        // Get context if not assigned
        if (_context == null)
        {
            _context = GetComponent<CharacterContext>();
        }

        // Get controller from context or direct reference
        if (_controller == null)
        {
            if (_context != null)
            {
                _controller = _context.Controller;
            }
            else
            {
                _controller = GetComponent<CharacterController>();
            }
        }

        // Validate controller
        if (_controller == null)
        {
            Debug.LogError(
                $"[{name}] JumpGravitySystem: CharacterController is required! " +
                "Either add CharacterController component or assign CharacterContext with controller reference.",
                this
            );
            enabled = false;
            return;
        }

        // Find animation system if not assigned
        if (_animationSystem == null)
        {
            _animationSystem = GetComponent<CharacterAnimationSystem>();
        }

        // Reset timeouts on start
        _jumpTimeoutDelta = _jumpTimeout;
        _fallTimeoutDelta = _fallTimeout;
    }

    protected override void PausableUpdate()
    {
        // Check game state
        if (GameMgr.Instance == null)
        {
            Debug.LogWarning($"[{name}] JumpGravitySystem: GameMgr.Instance is null. Skipping update.", this);
            return;
        }

        if (!GameMgr.Instance.IsGameRunning)
            return;

        // Vertical movement is updated via TickVertical() called by command brain
        // This update loop can be used for continuous updates if needed
    }

    /// <summary>
    /// Updates vertical movement including ground check, jump, and gravity.
    /// Should be called once per frame by command brain.
    /// </summary>
    /// <param name="jumpRequested">Whether jump input is active.</param>
    public void TickVertical(bool jumpRequested)
    {
        // Validate controller
        if (_controller == null)
        {
            Debug.LogError($"[{name}] JumpGravitySystem: CharacterController reference is null! Assign in inspector.", this);
            return;
        }

        // Perform ground check
        GroundedCheck();

        if (_isGrounded)
        {
            // Reset the fall timeout timer
            _fallTimeoutDelta = _fallTimeout;

            // Update animator if using character
            if (_animationSystem != null)
            {
                _animationSystem.SetJumping(false);
                _animationSystem.SetFreeFall(false);
            }

            // Stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // Jump
            if (jumpRequested && _jumpTimeoutDelta <= 0.0f)
            {
                // The square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);

                // Update animator if using character
                if (_animationSystem != null)
                {
                    _animationSystem.SetJumping(true);
                }
            }

            // Jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // Reset the jump timeout timer
            _jumpTimeoutDelta = _jumpTimeout;

            // Fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                // Update animator if using character
                if (_animationSystem != null)
                {
                    _animationSystem.SetFreeFall(true);
                }
            }
        }

        // Apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += _gravity * Time.deltaTime;
        }
    }

    /// <summary>
    /// Performs ground detection using sphere cast.
    /// </summary>
    private void GroundedCheck()
    {
        // Set sphere position, with offset
        Vector3 spherePosition = new Vector3(
            transform.position.x, transform.position.y - _groundedOffset,
            transform.position.z);
        _isGrounded = Physics.CheckSphere(
            spherePosition, _groundedRadius, _groundLayers,
            QueryTriggerInteraction.Ignore);

        // Update animator if using character
        if (_animationSystem != null)
        {
            _animationSystem.SetGrounded(_isGrounded);
        }
    }

    protected override void OnPaused()
    {
        // Freeze vertical velocity when paused
        _verticalVelocity = 0f;
    }

    private void OnDrawGizmosSelected()
    {
        if (!_showGizmos)
            return;

        // Ground check visualization
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (_isGrounded)
            Gizmos.color = transparentGreen;
        else
            Gizmos.color = transparentRed;

        // Draw sphere at ground check position
        Vector3 spherePos = new Vector3(transform.position.x, transform.position.y - _groundedOffset, transform.position.z);
        Gizmos.DrawSphere(spherePos, _groundedRadius);

        // Draw line from character center to sphere center
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, spherePos);

        // Vertical velocity indicator
        if (Mathf.Abs(_verticalVelocity) > 0.01f)
        {
            Gizmos.color = _verticalVelocity > 0f ? Color.blue : Color.red;
            Vector3 velStart = transform.position;
            Vector3 velEnd = velStart + Vector3.up * _verticalVelocity * 0.5f;
            Gizmos.DrawLine(velStart, velEnd);
            Gizmos.DrawWireSphere(velEnd, 0.1f);
        }

        // Terminal velocity limit
        Gizmos.color = Color.gray;
        float terminalY = transform.position.y + _terminalVelocity * 0.5f;
        Gizmos.DrawLine(
            transform.position + Vector3.left * 0.5f,
            transform.position + Vector3.right * 0.5f);
    }

    private void OnValidate()
    {
        // Clamp values to valid ranges
        _jumpHeight = Mathf.Max(0f, _jumpHeight);
        _jumpTimeout = Mathf.Max(0f, _jumpTimeout);
        _fallTimeout = Mathf.Max(0f, _fallTimeout);
        _groundedRadius = Mathf.Max(0f, _groundedRadius);
    }
}
