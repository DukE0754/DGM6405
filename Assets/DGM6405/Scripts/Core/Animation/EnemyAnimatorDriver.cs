using UnityEngine;

/// <summary>
///     Centralizes animation calls for enemies.
///     Listens to LocalEventBus to react to gameplay events.
/// </summary>
public class EnemyAnimatorDriver : MonoBehaviour, IShootListener, IHealthListener, IMovementSpeedListener
{
	// Cached hashes for performance
	private static readonly int SpeedHash = Animator.StringToHash("Speed");
	private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
	private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");
	private static readonly int HitTriggerHash = Animator.StringToHash("Hit");
	private static readonly int DieTriggerHash = Animator.StringToHash("Die");
	[SerializeField] private Animator _animator;

	private void Awake()
	{
		if (_animator == null)
			_animator = GetComponentInChildren<Animator>();
	}

	void IHealthListener.OnHealthChanged(float current, float max)
	{
		// Optional: could drive a hit reaction based on delta, but keep minimal.
	}

	void IHealthListener.OnDied()
	{
		TriggerDie();
	}

	void IMovementSpeedListener.OnSpeedChanged(
		float speed, float animationBlend, float walkSpeed, float sprintSpeed, float velocityX, float velocityZ)
	{
		SetSpeed(speed);
	}

	// Event listeners
	void IShootListener.OnShoot(bool shootInput)
	{
		if (shootInput) TriggerAttack();
	}

	public void SetSpeed(float speed)
	{
		if (_animator == null) return;
		_animator.SetFloat(SpeedHash, speed);
		_animator.SetBool(IsMovingHash, speed > 0.1f);
	}

	public void TriggerAttack()
	{
		if (_animator == null) return;
		_animator.SetTrigger(AttackTriggerHash);
	}

	public void TriggerHit()
	{
		if (_animator == null) return;
		_animator.SetTrigger(HitTriggerHash);
	}

	public void TriggerDie()
	{
		if (_animator == null) return;
		_animator.SetTrigger(DieTriggerHash);
	}
}
