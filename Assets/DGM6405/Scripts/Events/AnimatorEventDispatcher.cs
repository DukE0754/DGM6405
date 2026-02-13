using UnityEngine;

/// <summary>
///     Dispatches events from the Animator to the LocalEventBus.
///     This allows animations to trigger gameplay events like firing a projectile at a specific keyframe.
/// </summary>
public class AnimatorEventDispatcher : MonoBehaviour
{
	[SerializeField] private LocalEventBus _bus;

	private void Awake()
	{
		if (_bus == null)
			// Try to find the bus in parents, as the brain/root usually has it.
			_bus = GetComponentInParent<LocalEventBus>();

		if (_bus == null)
			Debug.LogWarning(
				$"[{name}] AnimatorEventDispatcher: LocalEventBus not found in parents. Animation events will not be dispatched.");
	}

	/// <summary>
	///     Called by Animation Event.
	/// </summary>
	public void FireProjectile()
	{
		_bus?.Raise<IFireProjectileListener>(l => l.OnFireProjectile());
	}
}
