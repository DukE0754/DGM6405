using UnityEngine;

/// <summary>
///     Handles rotating the GameObject to face a target.
///     Useful for Bud (turret) or Snake (melee) during attacks.
/// </summary>
public class RotateToTarget : MonoBehaviour, IAimTargetListener
{
	[SerializeField] private float _rotationSpeed = 5f;
	[SerializeField] private bool _useSmoothing = true;

	public void OnSetAimTarget(Vector3 worldPosition)
	{
		FacePosition(worldPosition);
	}

	public void FacePosition(Vector3 position)
	{
		var direction = (position - transform.position).normalized;
		direction.y = 0; // Keep rotation on Y axis only

		if (direction.sqrMagnitude < 0.001f) return;

		var targetRotation = Quaternion.LookRotation(direction);

		if (_useSmoothing)
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
		else
			transform.rotation = targetRotation;
	}

	public void FaceDirection(Vector3 direction)
	{
		direction.y = 0;
		if (direction.sqrMagnitude < 0.001f) return;

		var targetRotation = Quaternion.LookRotation(direction);

		if (_useSmoothing)
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
		else
			transform.rotation = targetRotation;
	}
}
