using UnityEngine;

/// <summary>
///     Handles projectile movement and collision.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
	[SerializeField] private ProjectileData _data;

	private Rigidbody _rb;
	private GameObject _source;

	private void Awake()
	{
		_rb = GetComponent<Rigidbody>();
		if (_data != null)
			_rb.useGravity = _data.UseGravity;
	}

	private void OnCollisionEnter(Collision collision)
	{
		// Don't hit the source
		if (collision.gameObject == _source) return;

		// Check if we hit something damageable
		if (collision.gameObject.TryGetComponent(out SimpleDamageReceiver damageReceiver))
			if (damageReceiver != null)
			{
				var info = new DamageInfo
				{
					Amount = _data != null ? _data.Damage : 0,
					Source = _source,
					HitPoint = collision.contacts[0].point,
					HitNormal = collision.contacts[0].normal
				};
				damageReceiver.ApplyDamage(info);
			}

		// Destroy on impact
		Destroy(gameObject);
	}

	public void Launch(Vector3 direction, GameObject source)
	{
		_source = source;
		var speed = _data != null ? _data.Speed : 0f;
		var lifetime = _data != null ? _data.Lifetime : 5f;

		_rb.linearVelocity = direction.normalized * speed;
		transform.forward = direction;
		Destroy(gameObject, lifetime); // Simple destruction for now
	}

	public void LaunchWithVelocity(Vector3 velocity, GameObject source)
	{
		_source = source;
		var lifetime = _data != null ? _data.Lifetime : 5f;

		_rb.linearVelocity = velocity;
		if (velocity.sqrMagnitude > 0.001f)
			transform.forward = velocity;
		_rb.useGravity = true; // Force gravity for arc shots
		Destroy(gameObject, lifetime);
	}
}
