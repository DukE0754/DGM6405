using UnityEngine;

public class KillBox : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (ColliderMgr.Instance.TryGetDamageReceiver(other, out var receiver))
		{
			receiver.ApplyDamage(new DamageInfo
			{
				Amount = 9999,
				Source = gameObject,
				HitPoint = other.transform.position
			});
			return;
		}

		var health = other.GetComponentInParent<Health>();
		if (health != null)
		{
			health.ApplyDamage(new DamageInfo
			{
				Amount = 9999,
				Source = gameObject,
				HitPoint = other.transform.position
			});
		}
	}
}
