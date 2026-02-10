using UnityEngine;

/// <summary>
///     A weapon that fires projectiles.
///     Supports direct fire and arc fire.
///     Listens to `IAimTargetListener` to cache aim and `IShootListener` to trigger firing.
/// </summary>
public class ProjectileWeapon : MonoBehaviour, IWeapon, IAimTargetListener, IShootListener
{
	[SerializeField] private Projectile _projectilePrefab;
	[SerializeField] private Transform _muzzle;
	[SerializeField] private float _fireRate = 1f;
	[SerializeField] private float _arcHeight = 2f;
	[SerializeField] private bool _useArc;

	private float _nextFireTime;
	private Vector3 _lastAimTarget;

	public bool CanFire => Time.time >= _nextFireTime;

	public void Fire(Vector3 targetPosition)
	{
		if (!CanFire) return;
		if (_muzzle == null) return;

		_nextFireTime = Time.time + 1f / _fireRate;

		var direction = (targetPosition - _muzzle.position).normalized;
		SpawnProjectile(direction);
	}

	public void Fire(Vector3 direction, bool useDirection)
	{
		if (!CanFire) return;
		if (_muzzle == null) return;

		_nextFireTime = Time.time + 1f / _fireRate;
		SpawnProjectile(direction);
	}

	public void FireArc(Vector3 targetPosition)
	{
		if (!CanFire) return;
		if (_muzzle == null) return;

		_nextFireTime = Time.time + 1f / _fireRate;

		var velocity = CalculateArcVelocity(_muzzle.position, targetPosition, _arcHeight);
		var projectile = Instantiate(_projectilePrefab, _muzzle.position, _muzzle.rotation);
		projectile.LaunchWithVelocity(velocity, gameObject);
	}

	public void SetUseArc(bool useArc)
	{
		_useArc = useArc;
	}

	private void SpawnProjectile(Vector3 direction)
	{
		var projectile = Instantiate(_projectilePrefab, _muzzle.position,
			Quaternion.LookRotation(direction));
		projectile.Launch(direction, gameObject);
	}

	private Vector3 CalculateArcVelocity(Vector3 start, Vector3 end, float height)
	{
		var displacementY = end.y - start.y;
		var displacementXZ = new Vector3(end.x - start.x, 0, end.z - start.z);
		var gravity = Physics.gravity.y;

		var time = Mathf.Sqrt(-2 * height / gravity) +
					Mathf.Sqrt(2 * (displacementY - height) / gravity);
		var velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * height);
		var velocityXZ = displacementXZ / time;

		return velocityXZ + velocityY * -Mathf.Sign(gravity);
	}

	// Event listeners
	public void OnSetAimTarget(Vector3 worldPosition)
	{
		_lastAimTarget = worldPosition;
	}

	public void OnShoot(bool shootInput)
	{
		if (!shootInput) return;
		if (_useArc) FireArc(_lastAimTarget);
		else Fire(_lastAimTarget);
	}
}
