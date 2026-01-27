using UnityEngine;

/// <summary>
/// Handles shooting/ranged combat.
/// Manages projectile spawning, weapon visibility, and shooting animations.
/// </summary>
public class ShootSystem : PausableBehaviour
{
    [Header("References")]
    [Tooltip("ProjectileShooter component for spawning projectiles. Required.")]
    [SerializeField] private ProjectileShooter _projectileShooter;

    [Tooltip("CharacterAnimationSystem for updating shoot animations. Required.")]
    [SerializeField] private CharacterAnimationSystem _animationSystem;

    [Tooltip("WeaponHandSlots for managing weapon visibility. If null, will use from CharacterContext.")]
    [SerializeField] private WeaponHandSlots _weaponHandSlots;

    [Tooltip("AimSystem for getting aim point. Optional, but recommended for accurate aiming.")]
    [SerializeField] private AimSystem _aimSystem;

    [Tooltip("CharacterSoundSystem for playing shoot sounds. Optional.")]
    [SerializeField] private CharacterSoundSystem _soundSystem;

    [Tooltip("CharacterContext component. If null, will try to find on same GameObject.")]
    [SerializeField] private CharacterContext _context;

    [Header("Debug Gizmos")]
    [Tooltip("Show shoot system gizmos in scene view when selected.")]
    [SerializeField] private bool _showGizmos = true;

    // Internal state
    private bool _isShooting;

    // Public properties
    public bool IsShooting => _isShooting;

    private void Awake()
    {

        // Get context if not assigned
        if (_context == null)
        {
            _context = GetComponent<CharacterContext>();
        }

        // Get projectile shooter if not assigned
        if (_projectileShooter == null)
        {
            _projectileShooter = GetComponent<ProjectileShooter>();
        }

        // Validate projectile shooter
        if (_projectileShooter == null)
        {
            Debug.LogError(
                $"[{name}] ShootSystem: ProjectileShooter is required! " +
                "Add ProjectileShooter component or assign reference in inspector.",
                this
            );
            enabled = false;
            return;
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
                $"[{name}] ShootSystem: CharacterAnimationSystem is required! " +
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

        // Get aim system if not assigned
        if (_aimSystem == null)
        {
            _aimSystem = GetComponent<AimSystem>();
        }

        // Get sound system if not assigned
        if (_soundSystem == null)
        {
            _soundSystem = GetComponent<CharacterSoundSystem>();
        }
    }

    /// <summary>
    /// Attempts to shoot based on input command.
    /// </summary>
    /// <param name="isShooting">Whether shoot input is active.</param>
    public void TryShoot(bool isShooting)
    {
        // Check game state
        if (GameMgr.Instance != null && !GameMgr.Instance.IsGameRunning)
        {
            // Cancel shooting if game is not running
            if (_isShooting)
            {
                _isShooting = false;
                UpdateShootState();
            }
            return;
        }

        // Update shooting state
        bool wasShooting = _isShooting;
        _isShooting = isShooting;

        // If just started shooting, fire projectile
        if (_isShooting && !wasShooting)
        {
            FireProjectile();
        }

        // Update animation and weapon visibility
        UpdateShootState();
    }

    /// <summary>
    /// Fires a projectile using ProjectileShooter.
    /// </summary>
    private void FireProjectile()
    {
        // Validate projectile shooter
        if (_projectileShooter == null)
        {
            Debug.LogError($"[{name}] ShootSystem: ProjectileShooter reference is null! Cannot shoot.", this);
            return;
        }

        // Get aim point from AimSystem if available, otherwise shoot forward
        if (_aimSystem != null)
        {
            Vector3 aimPoint = _aimSystem.GetAimPoint();
            _projectileShooter.ShootAt(aimPoint);
        }
        else
        {
            // Fallback to forward shooting if no aim system
            _projectileShooter.ShootForward();
        }

        // Play shoot sound
        if (_soundSystem != null)
        {
            _soundSystem.PlayShoot();
        }
    }

    /// <summary>
    /// Updates animation and weapon slot visibility based on shooting state.
    /// </summary>
    private void UpdateShootState()
    {
        // Update animation
        if (_animationSystem != null)
        {
            _animationSystem.SetShoot(_isShooting);
        }

        // Update weapon slot visibility
        if (_weaponHandSlots != null)
        {
            if (_isShooting)
            {
                _weaponHandSlots.SetActiveSlot(WeaponHandSlots.WeaponSlotType.Ranged);
            }
            else
            {
                // Only clear if no other combat action is active
                // This will be handled by combat system coordination
                // For now, clear weapon when not shooting
                _weaponHandSlots.SetActiveSlot(WeaponHandSlots.WeaponSlotType.None);
            }
        }
    }

    protected override void OnPaused()
    {
        // Cancel shooting when paused
        if (_isShooting)
        {
            _isShooting = false;
            UpdateShootState();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!_showGizmos)
            return;

        // Aim point visualization
        if (_aimSystem != null && _aimSystem.IsAiming)
        {
            Vector3 aimPoint = _aimSystem.GetAimPoint();
            Vector3 firePoint = _projectileShooter != null && _projectileShooter.transform != null
                ? _projectileShooter.transform.position
                : transform.position;

            // Aim line
            Gizmos.color = Color.red;
            Gizmos.DrawLine(firePoint, aimPoint);

            // Aim point sphere
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(aimPoint, 0.15f);
        }

        // Fire point position
        if (_projectileShooter != null && _projectileShooter.transform != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(_projectileShooter.transform.position, 0.1f);
        }
    }

    private void OnValidate()
    {
        // Warn if required components not assigned
        if (_projectileShooter == null)
        {
            Debug.LogWarning($"[{name}] ShootSystem: ProjectileShooter reference not assigned in inspector.", this);
        }

        if (_animationSystem == null)
        {
            Debug.LogWarning($"[{name}] ShootSystem: CharacterAnimationSystem reference not assigned in inspector.", this);
        }
    }
}
