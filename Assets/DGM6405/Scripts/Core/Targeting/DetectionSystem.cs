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
	[SerializeField] private LayerMask _targetMask;
	
	[Header("Debug")]
	[SerializeField] private string _currentStateDebug;

	public float DetectionRange => _detectionRange;

	private readonly Collider[] _hitColliders = new Collider[10];

	private void Awake()
	{
		if (_eyePosition == null) _eyePosition = transform;
	}

	private void Update()
	{
		UpdateDebugState();
	}

	private void UpdateDebugState()
	{
		var target = GetBestTarget();
		_currentStateDebug = target != null ? $"Best Target: {target.name}" : "No Target";
	}

	private void OnDrawGizmosSelected()
	{
		// Draw detection range
		Gizmos.color = new Color(1, 1, 0, 0.3f);
		Gizmos.DrawWireSphere(transform.position, _detectionRange);

		// Draw FOV cone
		var leftDir = Quaternion.Euler(0, -_fieldOfView / 2, 0) * transform.forward;
		var rightDir = Quaternion.Euler(0, _fieldOfView / 2, 0) * transform.forward;
		Gizmos.color = Color.blue;
		Gizmos.DrawRay(transform.position, leftDir * _detectionRange);
		Gizmos.DrawRay(transform.position, rightDir * _detectionRange);
	}

	public Transform GetBestTarget()
	{
		int numColliders = Physics.OverlapSphereNonAlloc(transform.position, _detectionRange, _hitColliders, _targetMask);
		Transform bestTarget = null;
		float minAngle = float.MaxValue;

		for (int i = 0; i < numColliders; i++)
		{
			var target = _hitColliders[i].transform;
			if (IsTargetInDetectionRange(target) && HasLineOfSight(target))
			{
				var directionToTarget = (target.position - _eyePosition.position).normalized;
				float angle = Vector3.Angle(transform.forward, directionToTarget);
				if (angle < minAngle)
				{
					minAngle = angle;
					bestTarget = target;
				}
			}
		}

		return bestTarget;
	}

	public bool IsTargetInDetectionRange(Transform target)
	{
		if (target == null) return false;
		return Vector3.Distance(transform.position, target.position) <= _detectionRange;
	}

	public bool HasLineOfSight(Transform target)
	{
		if (target == null) return false;

		// Aim at target center for LOS check
		Vector3 targetCenter = target.position + Vector3.up * 1f;
		var colliders = target.GetComponentsInChildren<Collider>();
		foreach (var col in colliders)
		{
			if (ColliderMgr.Instance != null && ColliderMgr.Instance.TryGetDamageReceiver(col, out _))
			{
				targetCenter = col.bounds.center;
				break;
			}
		}

		var directionToTarget = (targetCenter - _eyePosition.position).normalized;
		var distanceToTarget = Vector3.Distance(_eyePosition.position, targetCenter);

		// Check if target is within FOV (3D check)
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
