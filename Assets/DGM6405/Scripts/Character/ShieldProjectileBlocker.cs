using UnityEngine;

/// <summary>
///     Detects projectile collisions on the shield and raises a local block-hit event.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ShieldProjectileBlocker : MonoBehaviour
{
	[Header("References")]
	[Tooltip("CharacterContext used to raise local block-hit events.")]
	[SerializeField] private CharacterContext _context;

	[Tooltip("Optional explicit BlockSystem reference. If not assigned, found in parent.")]
	[SerializeField] private BlockSystem _blockSystem;

	private void Awake()
	{
		if (_context == null) _context = GetComponentInParent<CharacterContext>();
		if (_blockSystem == null) _blockSystem = GetComponentInParent<BlockSystem>();
		ValidateColliderSetup();
	}

	private void OnValidate()
	{
		if (_context == null) _context = GetComponentInParent<CharacterContext>();
		if (_blockSystem == null) _blockSystem = GetComponentInParent<BlockSystem>();
		ValidateColliderSetup();
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!IsActivelyBlocking()) return;

		if (!collision.gameObject.TryGetComponent<Projectile>(out _)) return;

		if (_context?.EventBus == null) return;

		var hitPoint = collision.contactCount > 0 ? collision.contacts[0].point : transform.position;
		var hitNormal = collision.contactCount > 0 ? collision.contacts[0].normal : -transform.forward;
		var source = collision.gameObject;

		_context.EventBus.Raise<IBlockHitListener>(l => l.OnBlockHit(hitPoint, hitNormal, source));
	}

	private bool IsActivelyBlocking()
	{
		if (_blockSystem != null) return _blockSystem.IsBlocking;

		// Fallback: when shield slot is active, it is expected to represent the block state.
		return gameObject.activeInHierarchy;
	}

	private void ValidateColliderSetup()
	{
		if (!TryGetComponent<Collider>(out var collider)) return;
		if (collider.isTrigger)
			Debug.LogWarning(
				$"[{name}] ShieldProjectileBlocker: Collider is set as trigger. Use a non-trigger collider to block projectile collisions.",
				this
			);
	}
}
