using UnityEngine;

/// <summary>
///     Handles detection logic including range and line-of-sight checks.
/// </summary>
public class DetectionSystem : MonoBehaviour
{
	[SerializeField] private Transform _eyePosition;
	[SerializeField] private float _detectionRange = 15f;
	[SerializeField] private float _fieldOfView = 120f;
	[SerializeField] private LayerMask _obstructionMask;

	public float DetectionRange => _detectionRange;

	private void Awake()
	{
		if (_eyePosition == null) _eyePosition = transform;
	}

	private void OnDrawGizmosSelected()
	{
		// Draw detection range
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, _detectionRange);

		// Draw FOV cone
		var leftDir = Quaternion.Euler(0, -_fieldOfView / 2, 0) * transform.forward;
		var rightDir = Quaternion.Euler(0, _fieldOfView / 2, 0) * transform.forward;
		Gizmos.color = Color.blue;
		Gizmos.DrawRay(transform.position, leftDir * _detectionRange);
		Gizmos.DrawRay(transform.position, rightDir * _detectionRange);
	}

	public bool IsTargetInDetectionRange(Transform target)
	{
		if (target == null) return false;
		return Vector3.Distance(transform.position, target.position) <= _detectionRange;
	}

	public bool HasLineOfSight(Transform target)
	{
		if (target == null) return false;

		var directionToTarget = (target.position - _eyePosition.position).normalized;
		var distanceToTarget = Vector3.Distance(_eyePosition.position, target.position);

		// Check if target is within FOV
		if (Vector3.Angle(transform.forward, directionToTarget) > _fieldOfView / 2f) return false;

		// Raycast to check for obstructions
		if (Physics.Raycast(
				_eyePosition.position, directionToTarget,
				out var hit, distanceToTarget, _obstructionMask))
			// If we hit something other than the target
			// (or a child of the target), then LOS is blocked
			if (hit.transform != target && !hit.transform.IsChildOf(target))
				return false;

		return true;
	}
}
