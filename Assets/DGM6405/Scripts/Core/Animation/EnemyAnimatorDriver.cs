using UnityEngine;

/// <summary>
///     Centralizes animation calls for enemies.
///     Listens to LocalEventBus to react to gameplay events.
/// </summary>
public class EnemyAnimatorDriver : MonoBehaviour, IShootListener, IFireProjectileListener, IHealthListener, IMovementSpeedListener
{
	// Cached hashes for performance
	private static readonly int SpeedHash = Animator.StringToHash("Speed");
	private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
	private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");
	private static readonly int HitTriggerHash = Animator.StringToHash("Hit");
	private static readonly int DieTriggerHash = Animator.StringToHash("Die");
	[SerializeField] private Animator _animator;
	[SerializeField] private ProjectileWeapon _projectileWeapon;
	[SerializeField] private AudioClip[] _attackAudioClips;
	[SerializeField] [Range(0f, 1f)] private float _attackAudioVolume = 0.75f;

	private void Awake()
	{
		if (_animator == null)
			_animator = GetComponentInChildren<Animator>();
		if (_projectileWeapon == null)
			_projectileWeapon = GetComponent<ProjectileWeapon>();
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

	void IFireProjectileListener.OnFireProjectile()
	{
		PlayAttackSound();
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

	private void PlayAttackSound()
	{
		if (GameMgr.Instance != null && !GameMgr.Instance.IsGameRunning)
			return;

		if (_attackAudioClips == null || _attackAudioClips.Length == 0)
			return;

		var clip = _attackAudioClips[Random.Range(0, _attackAudioClips.Length)];
		if (clip == null)
			return;

		var position = _projectileWeapon != null && _projectileWeapon.Muzzle != null
			? _projectileWeapon.Muzzle.position
			: transform.position;

		AudioSource.PlayClipAtPoint(clip, position, _attackAudioVolume);
	}
}
