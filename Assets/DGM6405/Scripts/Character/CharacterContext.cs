using UnityEngine;

/// <summary>
///     Centralized context component that holds shared references for character systems.
///     Provides O(1) access to common components without GetComponent calls.
/// </summary>
public class CharacterContext : PausableBehaviour
{
	[Header("Events")]
	[Tooltip("LocalEventBus for entity-specific events. If null, will try to find on same GameObject.")]
	[SerializeField] private LocalEventBus _eventBus;

	[Header("Core Components")]
	[Tooltip("CharacterController component for movement. Required.")]
	[SerializeField] private CharacterController _controller;

	[Tooltip("Animator component for animations. Optional but recommended.")]
	[SerializeField] private Animator _animator;

	[Header("Camera")]
	[Tooltip("Main camera transform for player third person camera.")]
	[SerializeField] private Camera _mainCamera;

	[Tooltip("Cinemachine camera target transform for camera rotation.")]
	[SerializeField] private Transform _cameraTarget;

	[Header("Weapon Slots")]
	[Tooltip("WeaponHandSlots component managing weapon GameObjects.")]
	[SerializeField] private WeaponHandSlots _weaponHandSlots;

	[Header("Audio")]
	[Tooltip("Audio clips for footstep sounds.")]
	[SerializeField] private AudioClip[] _footstepAudioClips;

	[Tooltip("Audio clip for landing sound.")]
	[SerializeField] private AudioClip _landingAudioClip;

	[Tooltip("Volume for footstep and landing sounds.")]
	[Range(0f, 1f)]
	[SerializeField] private float _footstepAudioVolume = 0.5f;

	[Header("Debug")]
	[SerializeField] private string _currentStateSummary;
	[SerializeField] private string _healthState;
	[SerializeField] private string _brainState;
	[SerializeField] private string _aimState;
	[SerializeField] private string _movementState;

	// Public properties for O(1) access
	public CharacterController Controller => _controller;
	public Animator Animator => _animator;
	public LocalEventBus EventBus => _eventBus;
	public Camera CharacterCamera => _mainCamera;
	public Transform CameraTarget => _cameraTarget;
	public WeaponHandSlots WeaponHandSlots => _weaponHandSlots;
	public AudioClip[] FootstepAudioClips => _footstepAudioClips;
	public AudioClip LandingAudioClip => _landingAudioClip;
	public float FootstepAudioVolume => _footstepAudioVolume;

	private void Awake()
	{
		// Try to find event bus if not assigned
		if (_eventBus == null)
		{
			_eventBus = GetComponent<LocalEventBus>();
			if (_eventBus == null)
				Debug.LogWarning(
					$"[{name}] CharacterContext: LocalEventBus not found. Event-based systems will not work. " +
					"Assign LocalEventBus reference in inspector or add LocalEventBus component.",
					this
				);
		}

		// Validate required components
		if (_controller == null)
		{
			_controller = GetComponent<CharacterController>();
			if (_controller == null)
			{
				Debug.LogError(
					$"[{name}] CharacterContext: CharacterController is required! " +
					$"Either add CharacterController component to {gameObject.name} or assign reference in inspector.",
					this
				);
				enabled = false;
				return;
			}
		}

		// Try to find animator if not assigned
		if (_animator == null)
		{
			_animator = GetComponent<Animator>();
			if (_animator == null)
				Debug.LogWarning(
					$"[{name}] CharacterContext: Animator not found. Animation systems will not work. " +
					"Assign Animator reference in inspector or add Animator component.",
					this
				);
		}

		// Try to find weapon hand slots if not assigned
		if (_weaponHandSlots == null)
		{
			_weaponHandSlots = GetComponent<WeaponHandSlots>();
			if (_weaponHandSlots == null)
				Debug.LogWarning(
					$"[{name}] CharacterContext: WeaponHandSlots not found. Combat systems may not work correctly. " +
					"Assign WeaponHandSlots reference in inspector or add WeaponHandSlots component.",
					this
				);
		}
	}

	private void OnValidate()
	{
		// Validate controller
		if (_controller == null)
			Debug.LogWarning(
				$"[{name}] CharacterContext: CharacterController reference not assigned in inspector.", this);

		// Validate audio clips
		if (_footstepAudioClips == null || _footstepAudioClips.Length == 0)
			Debug.LogWarning($"[{name}] CharacterContext: No footstep audio clips assigned.", this);

		if (_landingAudioClip == null)
			Debug.LogWarning($"[{name}] CharacterContext: Landing audio clip not assigned.", this);
	}

	protected override void PausableUpdate()
	{
		UpdateDebugSummary();
	}

	private void UpdateDebugSummary()
	{
		// Try to find components if not cached (CharacterContext usually has them)
		var health = GetComponent<Health>();
		var brain = GetComponent<PlayerCommandBrain>();
		var aim = GetComponent<AimSystem>();
		var move = GetComponent<CharacterMovementSystem>();

		_healthState = health != null ? GetPrivateFieldValue<string>(health, "_currentStateDebug") : "N/A";
		_brainState = brain != null ? GetPrivateFieldValue<string>(brain, "_currentStateDebug") : "N/A";
		_aimState = aim != null ? GetPrivateFieldValue<string>(aim, "_currentStateDebug") : "N/A";
		
		float speed = move != null ? move.Speed : 0f;
		_movementState = move != null ? $"Speed: {speed:F2}" : "N/A";

		_currentStateSummary = $"H: {_healthState} | B: {_brainState} | A: {_aimState}";
	}

	private T GetPrivateFieldValue<T>(object obj, string fieldName)
	{
		if (obj == null) return default;
		var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		if (field != null) return (T)field.GetValue(obj);
		return default;
	}
}
