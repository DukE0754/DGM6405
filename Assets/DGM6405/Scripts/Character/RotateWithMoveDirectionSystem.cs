using UnityEngine;

/// <summary>
///     Rotates character to face movement direction.
///     Default rotation behavior.
/// </summary>
public class RotateWithMoveDirectionSystem : PausableBehaviour, IMovementListener, IRotationListener
{
	[Header("Rotation Settings")]
	[Tooltip("How fast the character turns to face movement direction")]
	[Range(0.0f, 0.3f)]
	[SerializeField] private float _rotationSmoothTime = 0.12f;

	[Header("References")]
	[Tooltip("CharacterContext component. If null, will try to find on same GameObject.")]
	[SerializeField] private CharacterContext _context;

	[Tooltip("Main camera transform. If null, will try to find Camera.main.")]
	[SerializeField] private Transform _mainCamera;

	private bool _isEnabled = true;
	private float _rotationVelocity;
	private Vector2 _lastMoveInput;

	private void Awake()
	{
		if (_context == null) _context = GetComponent<CharacterContext>();

		if (_mainCamera == null)
		{
			var mainCam = Camera.main;
			if (mainCam != null) _mainCamera = mainCam.transform;
		}
	}

	void IMovementListener.OnMove(Vector2 moveInput, bool isSprinting)
	{
		_lastMoveInput = moveInput;
	}

	void IRotationListener.OnRotate(Vector3 direction)
	{
		// Manual rotation override for AI
		if (direction != Vector3.zero)
		{
			var targetRotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
			
			var rotation = Mathf.SmoothDampAngle(
				transform.eulerAngles.y, targetRotation, ref _rotationVelocity,
				_rotationSmoothTime);

			transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
		}
	}

	void IRotationListener.SetRotateToMovement(bool enable)
	{
		_isEnabled = enable;
	}

	protected override void PausableUpdate()
	{
		// Manual rotation from AI (OnRotate) takes precedence if called in same frame, 
		// but here we just handle the movement-based rotation.
		if (!_isEnabled || _lastMoveInput == Vector2.zero)
			return;

		// Calculate target rotation relative to camera
		var inputDirection = new Vector3(_lastMoveInput.x, 0.0f, _lastMoveInput.y).normalized;
		var yawOffset = _mainCamera != null ? _mainCamera.eulerAngles.y : 0f;
		var targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + yawOffset;

		var rotation = Mathf.SmoothDampAngle(
			transform.eulerAngles.y, targetRotation, ref _rotationVelocity,
			_rotationSmoothTime);

		// Rotate character
		transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
	}
}
