using UnityEngine;

public class EnemyShooterAI : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private ProjectileShooter shooter;

	[SerializeField] private Transform target;

	[Header("Behavior")]
	[SerializeField] private float shootRange = 15f;

	[SerializeField] private float rotationSpeed = 6f;

	[Header("Aim")]
	[SerializeField] private Vector3 targetOffset = Vector3.up;

	[SerializeField] private bool requireTarget;

	[Header("Debug")]
	[SerializeField] private Color gizmoColor = Color.red;

	private void Reset()
	{
		shooter = GetComponent<ProjectileShooter>();
	}

	private void Update()
	{
		if (shooter == null) return;
		if (requireTarget)
		{
			if (target == null) return;
			var aimPoint = target.position + targetOffset;
			var dir = aimPoint - transform.position;

			var dist = dir.magnitude;
			if (dist > shootRange) return;
			if (dir.sqrMagnitude < 0.0001f) return;

			var desiredRotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
			transform.rotation = Quaternion.Slerp(
				transform.rotation,
				desiredRotation,
				rotationSpeed * Time.deltaTime
			);
		}


		shooter.ShootForward();
	}

	// 🔴 GIZMO: draw ray from shooter to target
	private void OnDrawGizmos()
	{
		if (target == null) return;

		Gizmos.color = gizmoColor;

		var start = transform.position;
		var end = target.position + targetOffset;

		Gizmos.DrawLine(start, end);
		Gizmos.DrawSphere(end, 0.15f); // small dot at aim point
	}
}
