using UnityEngine;

/// <summary>
///     Handles camera rotation for character.
///     Updates Cinemachine camera target rotation based on look input.
/// </summary>
public class CameraRotationSystem : PausableBehaviour, ILookListener
{
	private const float THRESHOLD = 0.01f;

	[Header("Camera Settings")]
	[Tooltip("How far in degrees can you move the camera up")]
	[SerializeField] private float _topClamp = 70.0f;

	[Tooltip("How far in degrees can you move the camera down")]
	[SerializeField] private float _bottomClamp = -30.0f;

	[Tooltip("Additional degrees to override the camera. Useful for fine tuning camera position when locked")]
	[SerializeField] private float _cameraAngleOverride;

	[Tooltip("For locking the camera position on all axis")]
	[SerializeField] private bool _lockCameraPosition;

	[Header("Debug Gizmos")]
	[Tooltip("Show camera gizmos in scene view when selected.")]
	[SerializeField] private bool _showGizmos = true;

	[Header("References")]
	[Tooltip("CharacterContext component. If null, will try to find on same GameObject.")]
	[SerializeField] private CharacterContext _context;

	private Transform _cameraTarget;

	private float _cinemachineTargetPitch;

	// Cinemachine rotation state
	private float _cinemachineTargetYaw;

	private void Awake()
	{
		// Get context if not assigned
		if (_context == null) _context = GetComponent<CharacterContext>();

		// Get camera target from context
		_cameraTarget = _context?.CameraTarget;

		// Validate camera target
		if (_cameraTarget == null)
		{
			Debug.LogError(
				$"[{name}] CameraRotationSystem: Camera target transform is required! " +
				"Assign camera target reference in inspector or CharacterContext.",
				this
			);
			enabled = false;
			return;
		}

		// Initialize rotation from current camera target rotation
		_cinemachineTargetYaw = _cameraTarget.transform.rotation.eulerAngles.y;
	}

	private void OnDrawGizmosSelected()
	{
		if (!_showGizmos || _cameraTarget == null)
			return;

		// Look direction line
		Gizmos.color = Color.cyan;
		var forward = _cameraTarget.forward;
		Gizmos.DrawRay(_cameraTarget.position, forward * 5f);

		// Camera target position
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(_cameraTarget.position, 0.2f);

		// Rotation limits visualization (simplified)
		Gizmos.color = Color.green;
		var upLimit = Quaternion.Euler(_topClamp, _cinemachineTargetYaw, 0f) * Vector3.forward;
		var downLimit = Quaternion.Euler(_bottomClamp, _cinemachineTargetYaw, 0f) * Vector3.forward;
		Gizmos.DrawRay(_cameraTarget.position, upLimit * 3f);
		Gizmos.DrawRay(_cameraTarget.position, downLimit * 3f);
	}

	/// <summary>
	///     Applies look input to camera rotation.
	/// </summary>
	/// <param name="lookInput">Look input delta (x, y).</param>
	/// <param name="isMouse">Whether input is from mouse (affects deltaTime multiplier).</param>
	void ILookListener.OnLook(Vector2 lookInput, bool isMouse)
	{
		ApplyLook(lookInput, isMouse);
	}

	protected override void PausableLateUpdate()
	{
		// Camera rotation is applied via ApplyLook() called by command brain
		// This update loop can be used for continuous rotation if needed
	}

	private void ApplyLook(Vector2 lookInput, bool isMouse)
	{
		// Validate camera target
		if (_cameraTarget == null)
		{
			Debug.LogError(
				$"[{name}] CameraRotationSystem: Camera target reference is null! Assign in inspector.", this);
			return;
		}

		// If there is an input and camera position is not fixed
		if (lookInput.sqrMagnitude >= THRESHOLD && !_lockCameraPosition)
		{
			// Don't multiply mouse input by Time.deltaTime
			var deltaTimeMultiplier = isMouse ? 1.0f : Time.deltaTime;
			var sensitivity = SaveUtil.SavedValues?.LookSensitivity ?? 1.0f;

			_cinemachineTargetYaw += lookInput.x * deltaTimeMultiplier * sensitivity;
			_cinemachineTargetPitch += lookInput.y * deltaTimeMultiplier * sensitivity;
		}

		// Clamp our rotations so our values are limited 360 degrees
		_cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
		_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, _bottomClamp, _topClamp);

		// Cinemachine will follow this target
		_cameraTarget.transform.rotation = Quaternion.Euler(
			_cinemachineTargetPitch + _cameraAngleOverride,
			_cinemachineTargetYaw, 0.0f);
	}

	/// <summary>
	///     Clamps angle between min and max, handling 360-degree wrapping.
	/// </summary>
	private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
	{
		if (lfAngle < -360f)
			lfAngle += 360f;
		if (lfAngle > 360f)
			lfAngle -= 360f;
		return Mathf.Clamp(lfAngle, lfMin, lfMax);
	}

	protected override void OnPaused()
	{
		// Camera rotation naturally stops when paused since PausableLateUpdate() won't be called
		// Optionally lock camera position here if needed
	}
}
