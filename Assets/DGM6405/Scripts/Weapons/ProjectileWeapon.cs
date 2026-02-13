using UnityEngine;

/// <summary>
///     A weapon that fires projectiles.
///     Supports direct fire and arc fire.
///     Listens to `IAimTargetListener` to cache aim and `IFireProjectileListener` to trigger firing.
/// </summary>
public class ProjectileWeapon : MonoBehaviour, IWeapon, IAimTargetListener, IFireProjectileListener
{
	[SerializeField] private Projectile _projectilePrefab;
	[SerializeField] private Transform _muzzle;
	[SerializeField] private float _arcHeight = 2f;
	[SerializeField] private bool _useArc;

	private Vector3 _lastAimTarget;

	public bool CanFire => true;

	public void Fire(Vector3 targetPosition)
	{
		if (_muzzle == null) return;

		var direction = (targetPosition - _muzzle.position).normalized;
		SpawnProjectile(direction);
	}

	public void Fire(Vector3 direction, bool useDirection)
	{
		if (_muzzle == null) return;
		SpawnProjectile(direction);
	}

	public void FireArc(Vector3 targetPosition)
	{
		if (_muzzle == null) return;

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

	public void OnFireProjectile()
	{
		if (_useArc) FireArc(_lastAimTarget);
		else Fire(_lastAimTarget);
	}
}
