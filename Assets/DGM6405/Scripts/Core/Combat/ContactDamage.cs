using UnityEngine;

/// <summary>
///     Applies damage to objects on contact (collision or trigger).
/// </summary>
public class ContactDamage : MonoBehaviour
{
	[SerializeField] private int _damage = 10;
	[SerializeField] private float _cooldown = 1f;

	private float _nextDamageTime;

	private void OnCollisionEnter(Collision collision)
	{
		TryApplyDamage(collision.gameObject);
	}

	private void OnCollisionStay(Collision collision)
	{
		TryApplyDamage(collision.gameObject);
	}

	private void OnTriggerEnter(Collider other)
	{
		TryApplyDamage(other.gameObject);
	}

	private void OnTriggerStay(Collider other)
	{
		TryApplyDamage(other.gameObject);
	}

	private void TryApplyDamage(GameObject target)
	{
		if (Time.time < _nextDamageTime) return;

		var damageReceiver = target.GetComponent<IDamageReceiver>();
		if (damageReceiver != null)
		{
			var info = new DamageInfo
			{
				Amount = _damage,
				Source = gameObject,
				HitPoint = target.transform.position,
				HitNormal = Vector3.up
			};
			damageReceiver.ApplyDamage(info);
			_nextDamageTime = Time.time + _cooldown;
			Debug.Log($"[ContactDamage] Damaged {target.name} for {_damage}");
		}
	}
}
