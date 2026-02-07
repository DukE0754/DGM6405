using DGM6405.Events;
using UnityEngine;

/// <summary>
///     Handles character sound effects including footsteps, landing, and combat sounds.
///     Called via animation events or directly from other systems.
/// </summary>
public class CharacterSoundSystem : PausableBehaviour,
	IShootListener, IMeleeListener, IBlockListener, IGroundListener, IJumpListener
{
	[Header("Audio Clips")]
	[Tooltip("Audio clips for footstep sounds. Randomly selected when playing footstep.")]
	[SerializeField] private AudioClip[] _footstepAudioClips;

	[Tooltip("Audio clip for landing sound.")]
	[SerializeField] private AudioClip _landingAudioClip;

	[Tooltip("Audio clip for block sound.")]
	[SerializeField] private AudioClip _blockAudioClip;

	[Tooltip("Audio clip for shoot sound.")]
	[SerializeField] private AudioClip _shootAudioClip;

	[Tooltip("Audio clip for melee attack sound.")]
	[SerializeField] private AudioClip _meleeAudioClip;

	[Tooltip("Audio clip for dodge sound.")]
	[SerializeField] private AudioClip _dodgeAudioClip;

	[Header("Audio Settings")]
	[Tooltip("Volume for footstep and landing sounds.")]
	[Range(0f, 1f)]
	[SerializeField] private float _footstepAudioVolume = 0.5f;

	[Tooltip("Volume for combat sounds (block, shoot, melee, dodge).")]
	[Range(0f, 1f)]
	[SerializeField] private float _combatAudioVolume = 0.7f;

	[Header("References")]
	[Tooltip("CharacterContext component. If null, will try to find on same GameObject.")]
	[SerializeField] private CharacterContext _context;

	[Tooltip("CharacterController for getting center position. If null, will use from CharacterContext.")]
	[SerializeField] private CharacterController _controller;

	private void Awake()
	{
		// Get context if not assigned
		if (_context == null) _context = GetComponent<CharacterContext>();

		// Get controller from context or direct reference
		if (_controller == null)
		{
			if (_context != null)
				_controller = _context.Controller;
			else
				_controller = GetComponent<CharacterController>();
		}

		// Validate controller
		if (_controller == null)
			Debug.LogWarning(
				$"[{name}] CharacterSoundSystem: CharacterController not found. " +
				"Sound positions may not be accurate. Assign CharacterController reference.",
				this
			);
	}

	private void OnValidate()
	{
		// Warn about missing clips
		if (_footstepAudioClips == null || _footstepAudioClips.Length == 0)
			Debug.LogWarning($"[{name}] CharacterSoundSystem: No footstep audio clips assigned.", this);

		if (_landingAudioClip == null)
			Debug.LogWarning($"[{name}] CharacterSoundSystem: Landing audio clip not assigned.", this);
	}

	public void OnBlock(bool blockInput)
	{
		if (blockInput) PlayBlock();
	}

	public void OnGroundedChanged(bool isGrounded)
	{
		if (isGrounded) PlayLanding();
	}

	public void OnJumpPerformed()
	{
		// We could play a jump sound here if we had one.
	}

	public void OnMelee(bool meleeInput)
	{
		if (meleeInput) PlayMelee();
	}

	public void OnShoot(bool shootInput)
	{
		if (shootInput) PlayShoot();
	}

	/// <summary>
	///     Plays a footstep sound. Called from animation events.
	/// </summary>
	/// <param name="animationEvent">Animation event data (optional, for weight checking).</param>
	public void PlayFootstep(AnimationEvent animationEvent = null)
	{
		// Check if game is running
		if (GameMgr.Instance != null && !GameMgr.Instance.IsGameRunning)
			return;

		// Check animation event weight if provided
		if (animationEvent != null && animationEvent.animatorClipInfo.weight <= 0.5f)
			return;

		// Validate clips
		if (_footstepAudioClips == null || _footstepAudioClips.Length == 0)
		{
			Debug.LogWarning($"[{name}] CharacterSoundSystem: No footstep audio clips assigned.", this);
			return;
		}

		// Select random clip
		var index = Random.Range(0, _footstepAudioClips.Length);
		var clip = _footstepAudioClips[index];
		if (clip == null)
		{
			Debug.LogWarning($"[{name}] CharacterSoundSystem: Footstep audio clip at index {index} is null.", this);
			return;
		}

		// Play sound at character position
		var position = GetSoundPosition();
		AudioSource.PlayClipAtPoint(clip, position, _footstepAudioVolume);
	}

	/// <summary>
	///     Plays a landing sound. Called from animation events.
	/// </summary>
	/// <param name="animationEvent">Animation event data (optional, for weight checking).</param>
	public void PlayLanding(AnimationEvent animationEvent = null)
	{
		// Check if game is running
		if (GameMgr.Instance != null && !GameMgr.Instance.IsGameRunning)
			return;

		// Validate clip
		if (_landingAudioClip == null)
		{
			Debug.LogWarning($"[{name}] CharacterSoundSystem: Landing audio clip not assigned.", this);
			return;
		}

		// Play sound at character position
		var position = GetSoundPosition();
		AudioSource.PlayClipAtPoint(_landingAudioClip, position, _footstepAudioVolume);
	}

	/// <summary>
	///     Plays a block sound.
	/// </summary>
	public void PlayBlock()
	{
		if (GameMgr.Instance != null && !GameMgr.Instance.IsGameRunning)
			return;

		if (_blockAudioClip == null)
		{
			Debug.LogWarning($"[{name}] CharacterSoundSystem: Block audio clip not assigned.", this);
			return;
		}

		var position = GetSoundPosition();
		AudioSource.PlayClipAtPoint(_blockAudioClip, position, _combatAudioVolume);
	}

	/// <summary>
	///     Plays a shoot sound.
	/// </summary>
	public void PlayShoot()
	{
		if (GameMgr.Instance != null && !GameMgr.Instance.IsGameRunning)
			return;

		if (_shootAudioClip == null)
		{
			Debug.LogWarning($"[{name}] CharacterSoundSystem: Shoot audio clip not assigned.", this);
			return;
		}

		var position = GetSoundPosition();
		AudioSource.PlayClipAtPoint(_shootAudioClip, position, _combatAudioVolume);
	}

	/// <summary>
	///     Plays a melee attack sound.
	/// </summary>
	public void PlayMelee()
	{
		if (GameMgr.Instance != null && !GameMgr.Instance.IsGameRunning)
			return;

		if (_meleeAudioClip == null)
		{
			Debug.LogWarning($"[{name}] CharacterSoundSystem: Melee audio clip not assigned.", this);
			return;
		}

		var position = GetSoundPosition();
		AudioSource.PlayClipAtPoint(_meleeAudioClip, position, _combatAudioVolume);
	}

	/// <summary>
	///     Plays a dodge sound.
	/// </summary>
	public void PlayDodge()
	{
		if (GameMgr.Instance != null && !GameMgr.Instance.IsGameRunning)
			return;

		if (_dodgeAudioClip == null)
		{
			Debug.LogWarning($"[{name}] CharacterSoundSystem: Dodge audio clip not assigned.", this);
			return;
		}

		var position = GetSoundPosition();
		AudioSource.PlayClipAtPoint(_dodgeAudioClip, position, _combatAudioVolume);
	}

	/// <summary>
	///     Gets the position where sounds should be played (character center).
	/// </summary>
	private Vector3 GetSoundPosition()
	{
		if (_controller != null) return transform.TransformPoint(_controller.center);
		return transform.position;
	}

	protected override void OnPaused()
	{
		// Sounds triggered by animation events will naturally pause when Time.timeScale = 0
		// Explicitly stopping looping sounds would go here if we add AudioSource components later
	}
}
