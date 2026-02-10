using UnityEngine;

/// <summary>
///     Brain for the Training Dummy.
///     Provides a stable test target that doesn't die.
///     Listens to health events via LocalEventBus.
/// </summary>
public class TrainingDummyBrain : MonoBehaviour, IHealthListener
{
	[SerializeField] private Health _health;
	[SerializeField] private EnemyAnimatorDriver _animatorDriver;
	[SerializeField] private float _resetDelay = 2f;
	[SerializeField] private bool _autoReset = true;

	private int _lastHealth;

	private void Awake()
	{
		if (_health == null) _health = GetComponent<Health>();
		if (_animatorDriver == null)
			_animatorDriver = GetComponent<EnemyAnimatorDriver>();
		_lastHealth = _health != null ? _health.CurrentHealth : 0;
	}

	public void OnHealthChanged(float current, float max)
	{
		if (current < _lastHealth)
		{
			Debug.Log($"[Dummy] Took damage. HP: {current}/{max}");
			_animatorDriver?.TriggerHit();
		}
		_lastHealth = (int)current;
	}

	public void OnDied()
	{
		Debug.Log("[Dummy] Died! Resetting...");
		if (_autoReset) Invoke(nameof(ResetDummy), _resetDelay);
	}

	private void ResetDummy()
	{
		_health.ResetHealth();
	}
}
