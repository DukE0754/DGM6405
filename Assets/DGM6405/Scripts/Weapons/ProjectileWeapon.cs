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
	
	/// <summary>
	/// Moved to LateUpdate to avoid firing before the animation modifications apply
	/// </summary>
	private bool _pendingFire;
	
	public Transform Muzzle => _muzzle;

	private void LateUpdate()
	{
		if (!_pendingFire) return;
		
		_pendingFire = false;
		
		if (_useArc) FireArc(_lastAimTarget);
		else Fire(_lastAimTarget);
	}

	// Event listeners
	public void OnSetAimTarget(Vector3 worldPosition)
	{
		_lastAimTarget = worldPosition;
	}

	public void OnFireProjectile()
	{
		_pendingFire = true;
	}

	public bool CanFire => true;

	public void Fire(Vector3 targetPosition)
	{
		if (_muzzle == null) return;

		var direction = targetPosition - _muzzle.position;

		if (direction.sqrMagnitude < 0.0001f)
		{
			Debug.LogWarning("ProjectileWeapon: Invalid fire direction.");
			return;
		}

		direction.Normalize();

		SpawnProjectile(direction);
	}

	public void Fire(Vector3 direction, bool useDirection)
	{
		if (_muzzle == null) return;
		SpawnProjectile(direction);
	}

	/// <summary>
	/// </summary>
	/// <param name="targetPosition"></param>
	public void FireArc(Vector3 targetPosition)
	{
		if (_muzzle == null) return;

		var velocity = CalculateArcVelocity(_muzzle.position, targetPosition, _arcHeight);

		var projectile = Instantiate(
			_projectilePrefab,
			_muzzle.position,
			Quaternion.LookRotation(velocity.normalized));

		projectile.LaunchWithVelocity(velocity, gameObject);
	}

	public void SetUseArc(bool useArc)
	{
		_useArc = useArc;
	}

	private void OnDrawGizmosSelected()
	{
		if (_muzzle == null) return;

		Gizmos.color = _useArc ? Color.cyan : Color.red;
		Gizmos.DrawLine(_muzzle.position, _lastAimTarget);
		Gizmos.DrawSphere(_lastAimTarget, GetProjectileGizmoRadius());
	}

	private void SpawnProjectile(Vector3 direction)
	{
		if (_projectilePrefab == null)
		{
			Debug.LogError("ProjectileWeapon: Projectile prefab not assigned.");
			return;
		}

		var rotation = Quaternion.LookRotation(direction);

		var projectile = Instantiate(
			_projectilePrefab,
			_muzzle.position,
			rotation);

		projectile.Launch(direction, gameObject);
	}

	private Vector3 CalculateArcVelocity(Vector3 start, Vector3 end, float height)
	{
		var displacementY = end.y - start.y;
		var displacementXZ = new Vector3(end.x - start.x, 0, end.z - start.z);

		var gravity = Physics.gravity.y;

		var time =
			Mathf.Sqrt(-2 * height / gravity) +
			Mathf.Sqrt(2 * (displacementY - height) / gravity);

		var velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * height);
		var velocityXZ = displacementXZ / time;

		return velocityXZ + velocityY * -Mathf.Sign(gravity);
	}

	private float GetProjectileGizmoRadius()
	{
		// if (_projectilePrefab == null) return 0.1f;
		//
		// var sphere = _projectilePrefab.GetComponentInChildren<SphereCollider>();
		// if (sphere != null)
		// {
		// 	var scale = sphere.transform.lossyScale;
		// 	var maxAxisScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
		// 	return sphere.radius * maxAxisScale;
		// }
		//
		// var capsule = _projectilePrefab.GetComponentInChildren<CapsuleCollider>();
		// if (capsule != null)
		// {
		// 	var scale = capsule.transform.lossyScale;
		// 	float axisScale = capsule.direction switch
		// 	{
		// 		0 => Mathf.Max(Mathf.Abs(scale.y), Mathf.Abs(scale.z)),
		// 		1 => Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z)),
		// 		_ => Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y))
		// 	};
		// 	return capsule.radius * axisScale;
		// }
		//
		// var box = _projectilePrefab.GetComponentInChildren<BoxCollider>();
		// if (box != null)
		// {
		// 	var size = Vector3.Scale(box.size, box.transform.lossyScale);
		// 	return Mathf.Max(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z)) * 0.5f;
		// }
		//
		// var collider = _projectilePrefab.GetComponentInChildren<Collider>();
		// if (collider != null) return collider.bounds.extents.magnitude;

		return 0.1f;
	}
}
