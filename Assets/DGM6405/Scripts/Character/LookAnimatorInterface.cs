using UnityEngine;
using FIMSpace.FLook;

/// <summary>
///     Interfaces with the third-party LookAnimator component.
///     Listens to aim updates to set look targets and health events to disable looking on death.
/// </summary>
public class LookAnimatorInterface : PausableBehaviour, IAimListener, IHealthListener, ILookListener, IRotationListener
{
	[Header("References")]
	[SerializeField] private CharacterContext _context;
	[SerializeField] private FLookAnimator _lookAnimator;

	[Header("Settings")]
	[Tooltip("If true, the look animator will only be active when the character is aiming.")]
	[SerializeField] private bool _onlyLookWhenAiming = false;

	[Header("Debug")]
	[Tooltip("Current state of the look system.")]
	[SerializeField] private string _currentStateDebug;

	private float _initialLookAmount;
	private bool _isDead;
	private bool _rotateToCamera;
	private bool _rotateToMovement = true; // Default behavior
	private bool _isLooking;

	private void Awake()
	{
		if (_context == null) _context = GetComponent<CharacterContext>();
		if (_lookAnimator == null) _lookAnimator = GetComponentInChildren<FLookAnimator>();

		if (_lookAnimator != null)
		{
			_initialLookAmount = _lookAnimator.LookAnimatorAmount;
			// Ensure follow mode is set to position if we want to drive it via aim point
			_lookAnimator.FollowMode = FLookAnimator.EFFollowMode.FollowJustPosition;
			_isLooking = _initialLookAmount > 0.01f;
		}
		else
		{
			Debug.LogWarning($"[{name}] LookAnimatorInterface: FLookAnimator component not found.", this);
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (_context != null && _context.EventBus != null)
		{
			_context.EventBus.Register<IAimListener>(this);
			_context.EventBus.Register<IHealthListener>(this);
			_context.EventBus.Register<ILookListener>(this);
			_context.EventBus.Register<IRotationListener>(this);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (_context != null && _context.EventBus != null)
		{
			_context.EventBus.Unregister<IAimListener>(this);
			_context.EventBus.Unregister<IHealthListener>(this);
			_context.EventBus.Unregister<ILookListener>(this);
			_context.EventBus.Unregister<IRotationListener>(this);
		}
	}

	public void OnAimUpdate(Vector3 aimPoint, bool isTargeting)
	{
		if (_isDead || _lookAnimator == null)
		{
			UpdateState("Dead/No Animator");
			return;
		}
		
		// 1. Always look if we have a locked target
		if (isTargeting)
		{
			_lookAnimator.FollowOffset = aimPoint;
			SetLooking(true, "Targeting");
			return;
		}

		// 2. If no target, check rotation mode and settings
		// If _onlyLookWhenAiming is true, and we aren't targeting, we don't look.
		if (_onlyLookWhenAiming)
		{
			SetLooking(false, "OnlyLookWhenAiming (No Target)");
			return;
		}

		// 3. Fallback behavior (looking where the camera points)
		// We only look at the camera fallback if we are in 'Face Camera' rotation mode.
		// If we are in 'Rotate with Movement' mode, looking at the camera fallback 
		// (which might be behind us) looks unnatural.
		if (_rotateToCamera)
		{
			_lookAnimator.FollowOffset = aimPoint;
			SetLooking(true, "RotateToCamera Fallback");
		}
		else
		{
			SetLooking(false, "RotateWithMovement (No Target)");
		}
	}

	private void SetLooking(bool enable, string reason)
	{
		UpdateState(reason);
		
		if (_lookAnimator == null) return;

		if (enable != _isLooking)
		{
			_isLooking = enable;
			// Use the recommended method for smooth transitions
			_lookAnimator.SwitchLooking(enable);
			
			// Also physically enable/disable the component if it's not a transition
			// But Look Animator seems to handle its own enablement via SwitchLooking coroutines.
			// However, if we want to be absolutely sure it's "off" when not needed:
			// _lookAnimator.enabled = enable; 
			// Wait, if we disable the component, the coroutine might stop.
			// The user expected "disabling the component".
		}

		// Even if we don't change the 'enabled' state, we ensure the weight is correct 
		// if SwitchLooking doesn't reach the target for some reason.
		if (!_isLooking)
		{
			_lookAnimator.LookAnimatorAmount = 0f;
		}
		else
		{
			// If it was 0, it won't look at all. 
			// SwitchLooking animates LookAnimatorAmount.
		}
	}

	private void UpdateState(string state)
	{
		_currentStateDebug = state;
	}

	// IRotationListener implementation
	public void SetRotateToMovement(bool enable)
	{
		_rotateToMovement = enable;
	}

	public void SetRotateToCamera(bool enable)
	{
		_rotateToCamera = enable;
	}

	public void OnRotate(Vector3 direction) { }

	/// <summary>
	///     Optional: Use look input to potentially influence the look animator if needed.
	///     Currently just ensuring we stay updated if AimSystem fallback logic is used.
	/// </summary>
	void ILookListener.OnLook(Vector2 lookInput, bool isMouse)
	{
		// This implementation can be used to enable/disable the animator 
		// if we define "looking" as receiving look input, but AimSystem 
		// is a more reliable source for the target point.
	}

	public void OnHealthChanged(float current, float max) { }
	public void OnDamageTaken(int amount, Vector3 direction) { }

	public void OnDied()
	{
		if (_isDead) return;
		_isDead = true;

		if (_lookAnimator != null)
		{
			_lookAnimator.SwitchLooking(false);
			_lookAnimator.LookAnimatorAmount = 0f;
			_lookAnimator.enabled = false;
		}
		UpdateState("Dead");
	}

	protected override void OnPaused()
	{
		// Optional: disable looking when paused if desired
	}

	private void OnValidate()
	{
		if (_context == null) _context = GetComponent<CharacterContext>();
		if (_lookAnimator == null) _lookAnimator = GetComponentInChildren<FLookAnimator>();
	}
}
