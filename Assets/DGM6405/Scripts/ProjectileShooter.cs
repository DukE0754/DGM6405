using UnityEngine;

public class ProjectileShooter : MonoBehaviour
{
	[Header("Projectile")]
	[SerializeField] private GameObject projectilePrefab;

	[SerializeField] private Transform firePoint;

	[Header("Shooting")]
	[SerializeField] private float projectileSpeed = 15f;

	[SerializeField] private float fireCooldown = 0.5f;

	private float _nextFireTime;

	private Transform Origin => firePoint != null ? firePoint : transform;

	public bool CanShoot()
	{
		return Time.time >= _nextFireTime && projectilePrefab != null;
	}

	public void ShootForward()
	{
		ShootDirection(Origin.forward);
	}

	public void ShootAt(Vector3 aimPoint)
	{
		var dir = aimPoint - Origin.position;
		ShootDirection(dir);
	}

	private void ShootDirection(Vector3 direction)
	{
		if (!CanShoot()) return;
		if (direction.sqrMagnitude < 0.0001f) return;

		_nextFireTime = Time.time + fireCooldown;
		direction.Normalize();

		var origin = Origin;

		var projectile = Instantiate(
			projectilePrefab,
			origin.position,
			Quaternion.LookRotation(direction, Vector3.up)
		);

		// 🔥 IGNORE COLLISION WITH SHOOTER
		IgnoreSelfCollisions(projectile);

		var rb = projectile.GetComponent<Rigidbody>();
		if (rb != null)
			// Unity 6
			rb.linearVelocity = direction * projectileSpeed;
	}

	private void IgnoreSelfCollisions(GameObject projectile)
	{
		var shooterColliders = GetComponentsInChildren<Collider>();
		var projectileColliders = projectile.GetComponentsInChildren<Collider>();

		foreach (var shooterCol in shooterColliders)
		{
			foreach (var projCol in projectileColliders) Physics.IgnoreCollision(shooterCol, projCol, true);
		}
	}
}
