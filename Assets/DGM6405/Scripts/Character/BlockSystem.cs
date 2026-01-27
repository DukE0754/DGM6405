using UnityEngine;

/// <summary>
/// Handles blocking/defending with shield.
/// Manages shield visibility and blocking animation state.
/// </summary>
public class BlockSystem : PausableBehaviour
{
    [Header("References")]
    [Tooltip("CharacterContext component. If null, will try to find on same GameObject.")]
    [SerializeField] private CharacterContext _context;

    [Tooltip("CharacterAnimationSystem for updating block animations. Required.")]
    [SerializeField] private CharacterAnimationSystem _animationSystem;

    [Header("Debug Gizmos")]
    [Tooltip("Show block system gizmos in scene view when selected.")]
    [SerializeField] private bool _showGizmos = true;

    [Tooltip("Block coverage angle in degrees.")]
    [Range(0f, 360f)]
    [SerializeField] private float _blockArcAngle = 180f;

    [Tooltip("WeaponHandSlots for managing shield visibility. If null, will use from CharacterContext.")]
    [SerializeField] private WeaponHandSlots _weaponHandSlots;

    // Internal state
    private bool _isBlocking;

    // Public properties
    public bool IsBlocking => _isBlocking;

    private void Awake()
    {

        // Get context if not assigned
        if (_context == null)
        {
            _context = GetComponent<CharacterContext>();
        }

        // Get animation system if not assigned
        if (_animationSystem == null)
        {
            _animationSystem = GetComponent<CharacterAnimationSystem>();
        }

        // Validate animation system
        if (_animationSystem == null)
        {
            Debug.LogError(
                $"[{name}] BlockSystem: CharacterAnimationSystem is required! " +
                "Add CharacterAnimationSystem component or assign reference in inspector.",
                this
            );
            enabled = false;
            return;
        }

        // Get weapon hand slots from context or direct reference
        if (_weaponHandSlots == null)
        {
            if (_context != null)
            {
                _weaponHandSlots = _context.WeaponHandSlots;
            }
            else
            {
                _weaponHandSlots = GetComponent<WeaponHandSlots>();
            }
        }

        // Warn if weapon hand slots not found
        if (_weaponHandSlots == null)
        {
            Debug.LogWarning(
                $"[{name}] BlockSystem: WeaponHandSlots not found. Shield visibility will not be managed.",
                this
            );
        }
    }

    /// <summary>
    /// Sets blocking state based on input command.
    /// </summary>
    /// <param name="isBlocking">Whether blocking input is active.</param>
    public void SetBlocking(bool isBlocking)
    {
        // Check game state
        if (GameMgr.Instance != null && !GameMgr.Instance.IsGameRunning)
        {
            // Cancel blocking if game is not running
            if (_isBlocking)
            {
                _isBlocking = false;
                UpdateBlockState();
            }
            return;
        }

        // Update blocking state
        if (_isBlocking != isBlocking)
        {
            _isBlocking = isBlocking;
            UpdateBlockState();
        }
    }

    /// <summary>
    /// Updates animation and weapon slot visibility based on blocking state.
    /// </summary>
    private void UpdateBlockState()
    {
        // Update animation
        if (_animationSystem != null)
        {
            _animationSystem.SetBlock(_isBlocking);
        }

        // Update weapon slot visibility
        if (_weaponHandSlots != null)
        {
            if (_isBlocking)
            {
                _weaponHandSlots.SetActiveSlot(WeaponHandSlots.WeaponSlotType.Shield);
            }
            else
            {
                // Only clear if no other combat action is active
                // This will be handled by combat system coordination
                // For now, clear shield when not blocking
                _weaponHandSlots.SetActiveSlot(WeaponHandSlots.WeaponSlotType.None);
            }
        }
    }

    protected override void OnPaused()
    {
        // Cancel blocking when paused
        if (_isBlocking)
        {
            _isBlocking = false;
            UpdateBlockState();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!_showGizmos)
            return;

        // Block arc visualization
        if (_isBlocking)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.5f); // Cyan with transparency
        }
        else
        {
            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f); // Gray with transparency
        }

        // Draw arc in front of character
        float halfAngle = _blockArcAngle * 0.5f;
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        Vector3 arcStart = Quaternion.AngleAxis(-halfAngle, Vector3.up) * forward;
        Vector3 arcEnd = Quaternion.AngleAxis(halfAngle, Vector3.up) * forward;

        // Draw arc lines
        int segments = 16;
        for (int i = 0; i < segments; i++)
        {
            float t1 = (float)i / segments;
            float t2 = (float)(i + 1) / segments;
            float angle1 = -halfAngle + t1 * _blockArcAngle;
            float angle2 = -halfAngle + t2 * _blockArcAngle;
            Vector3 dir1 = Quaternion.AngleAxis(angle1, Vector3.up) * forward;
            Vector3 dir2 = Quaternion.AngleAxis(angle2, Vector3.up) * forward;
            Gizmos.DrawLine(transform.position + dir1 * 1.5f, transform.position + dir2 * 1.5f);
        }

        // Draw forward direction
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, forward * 1.5f);
    }

    private void OnValidate()
    {
        // Warn if animation system not assigned
        if (_animationSystem == null)
        {
            Debug.LogWarning($"[{name}] BlockSystem: CharacterAnimationSystem reference not assigned in inspector.", this);
        }
    }
}
