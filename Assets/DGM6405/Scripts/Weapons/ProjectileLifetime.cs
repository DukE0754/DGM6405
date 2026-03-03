using UnityEngine;

public class ProjectileLifetime : PausableBehaviour
{
	[SerializeField] private float lifeTimeSeconds = 3f;

	protected override void OnEnable()
	{
		base.OnEnable();
		if (lifeTimeSeconds <= 0f) lifeTimeSeconds = 0.1f;
		StartCoroutine(DestroyAfterDelay(lifeTimeSeconds));
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
