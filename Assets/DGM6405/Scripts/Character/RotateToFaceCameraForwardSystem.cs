using UnityEngine;

/// <summary>
///     Rotates character to face camera forward direction.
///     Useful when blocking or aiming.
/// </summary>
public class RotateToFaceCameraForwardSystem : PausableBehaviour, IRotationListener
{
	[Header("Rotation Settings")]
	[Tooltip("How fast the character turns to face camera forward")]
	[Range(0.0f, 0.3f)]
	[SerializeField] private float _rotationSmoothTime = 0.05f;

	[Header("References")]
	[Tooltip("CharacterContext component. If null, will try to find on same GameObject.")]
	[SerializeField] private CharacterContext _context;

	private bool _isEnabled;

	private Transform _mainCamera;
	private float _rotationVelocity;

	private void Awake()
	{
		if (_context == null) _context = GetComponent<CharacterContext>();

		_mainCamera = _context?.CharacterCamera?.transform;
	}

	void IRotationListener.SetRotateToCamera(bool enable)
	{
		_isEnabled = enable;
	}

	void IRotationListener.OnRotate(Vector3 direction)
	{
		// Not used by this system, which strictly follows camera
	}

	protected override void PausableUpdate()
	{
		if (!_isEnabled || _mainCamera == null)
			return;

		// Face camera forward direction (yaw only)
		var targetRotation = _mainCamera.eulerAngles.y;

		var rotation = Mathf.SmoothDampAngle(
			transform.eulerAngles.y, targetRotation, ref _rotationVelocity,
			_rotationSmoothTime);

		// Rotate character
		transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
	}
}
