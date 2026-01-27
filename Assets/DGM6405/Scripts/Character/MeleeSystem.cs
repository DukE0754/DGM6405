using UnityEngine;

/// <summary>
/// Handles melee combat attacks.
/// Manages melee weapon visibility and attack animations.
/// </summary>
public class MeleeSystem : PausableBehaviour
{
    [Header("References")]
    [Tooltip("CharacterAnimationSystem for updating melee animations. Required.")]
    [SerializeField] private CharacterAnimationSystem _animationSystem;

    [Tooltip("WeaponHandSlots for managing weapon visibility. If null, will use from CharacterContext.")]
    [SerializeField] private WeaponHandSlots _weaponHandSlots;

    [Tooltip("CharacterSoundSystem for playing melee sounds. Optional.")]
    [SerializeField] private CharacterSoundSystem _soundSystem;

    [Tooltip("CharacterContext component. If null, will try to find on same GameObject.")]
    [SerializeField] private CharacterContext _context;

    [Header("Debug Gizmos")]
    [Tooltip("Show melee system gizmos in scene view when selected.")]
    [SerializeField] private bool _showGizmos = true;

    [Tooltip("Melee attack range.")]
    [SerializeField] private float _meleeRange = 2f;

    [Tooltip("Melee attack arc angle in degrees.")]
    [Range(0f, 360f)]
    [SerializeField] private float _meleeArcAngle = 90f;

    // Internal state
    private bool _isMeleeAttacking;

    // Public properties
    public bool IsMeleeAttacking => _isMeleeAttacking;

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
                $"[{name}] MeleeSystem: CharacterAnimationSystem is required! " +
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

        // Get sound system if not assigned
        if (_soundSystem == null)
        {
            _soundSystem = GetComponent<CharacterSoundSystem>();
        }
    }

    /// <summary>
    /// Attempts melee attack based on input command.
    /// </summary>
    /// <param name="isMeleeAttacking">Whether melee input is active.</param>
    public void TryMelee(bool isMeleeAttacking)
    {
        // Check game state
        if (GameMgr.Instance != null && !GameMgr.Instance.IsGameRunning)
        {
            // Cancel melee attack if game is not running
            if (_isMeleeAttacking)
            {
                _isMeleeAttacking = false;
                UpdateMeleeState();
            }
            return;
        }

        // Update melee attack state
        bool wasMeleeAttacking = _isMeleeAttacking;
        _isMeleeAttacking = isMeleeAttacking;

        // If just started attacking, play sound
        if (_isMeleeAttacking && !wasMeleeAttacking)
        {
            if (_soundSystem != null)
            {
                _soundSystem.PlayMelee();
            }
        }

        // Update animation and weapon visibility
        UpdateMeleeState();
    }

    /// <summary>
    /// Updates animation and weapon slot visibility based on melee attack state.
    /// </summary>
    private void UpdateMeleeState()
    {
        // Update animation
        if (_animationSystem != null)
        {
            _animationSystem.SetMelee(_isMeleeAttacking);
        }

        // Update weapon slot visibility
        if (_weaponHandSlots != null)
        {
            if (_isMeleeAttacking)
            {
                _weaponHandSlots.SetActiveSlot(WeaponHandSlots.WeaponSlotType.Melee);
            }
            else
            {
                // Only clear if no other combat action is active
                // This will be handled by combat system coordination
                // For now, clear weapon when not attacking
                _weaponHandSlots.SetActiveSlot(WeaponHandSlots.WeaponSlotType.None);
            }
        }
    }

    protected override void OnPaused()
    {
        // Cancel melee attack when paused
        if (_isMeleeAttacking)
        {
            _isMeleeAttacking = false;
            UpdateMeleeState();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!_showGizmos)
            return;

        // Melee attack arc visualization
        if (_isMeleeAttacking)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f); // Red with transparency
        }
        else
        {
            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f); // Gray with transparency
        }

        // Draw arc in front of character
        float halfAngle = _meleeArcAngle * 0.5f;
        Vector3 forward = transform.forward;
        int segments = 16;
        for (int i = 0; i < segments; i++)
        {
            float t1 = (float)i / segments;
            float t2 = (float)(i + 1) / segments;
            float angle1 = -halfAngle + t1 * _meleeArcAngle;
            float angle2 = -halfAngle + t2 * _meleeArcAngle;
            Vector3 dir1 = Quaternion.AngleAxis(angle1, Vector3.up) * forward;
            Vector3 dir2 = Quaternion.AngleAxis(angle2, Vector3.up) * forward;
            Gizmos.DrawLine(transform.position + dir1 * _meleeRange, transform.position + dir2 * _meleeRange);
        }

        // Draw forward direction
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, forward * _meleeRange);
    }

    private void OnValidate()
    {
        // Warn if animation system not assigned
        if (_animationSystem == null)
        {
            Debug.LogWarning($"[{name}] MeleeSystem: CharacterAnimationSystem reference not assigned in inspector.", this);
        }

        // Clamp values
        _meleeRange = Mathf.Max(0f, _meleeRange);
    }
}
