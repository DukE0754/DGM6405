using UnityEngine;

/// <summary>
///     Handles projectile movement and collision.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Projectile : PausableBehaviour
{
	[SerializeField] private ProjectileData _data;

	private Rigidbody _rb;
	private GameObject _source;
	private Vector3 _savedVelocity;
	private Vector3 _savedAngularVelocity;

	private void Awake()
	{
		_rb = GetComponent<Rigidbody>();
		if (_data != null)
			_rb.useGravity = _data.UseGravity;
	}

	protected override void OnPaused()
	{
		_savedVelocity = _rb.linearVelocity;
		_savedAngularVelocity = _rb.angularVelocity;
		_rb.linearVelocity = Vector3.zero;
		_rb.angularVelocity = Vector3.zero;
		_rb.isKinematic = true;
	}

	protected override void OnResumed()
	{
		_rb.isKinematic = false;
		_rb.linearVelocity = _savedVelocity;
		_rb.angularVelocity = _savedAngularVelocity;
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
		StartCoroutine(DestroyAfterDelay(lifetime));
	}

	public void LaunchWithVelocity(Vector3 velocity, GameObject source)
	{
		_source = source;
		var lifetime = _data != null ? _data.Lifetime : 5f;

		_rb.linearVelocity = velocity;
		if (velocity.sqrMagnitude > 0.001f)
			transform.forward = velocity;
		_rb.useGravity = true; // Force gravity for arc shots
		StartCoroutine(DestroyAfterDelay(lifetime));
	}

	private System.Collections.IEnumerator DestroyAfterDelay(float delay)
	{
		var remaining = delay;
		while (remaining > 0)
		{
			if (!IsPaused)
				remaining -= Time.deltaTime;
			yield return null;
		}

		Destroy(gameObject);
	}
}
