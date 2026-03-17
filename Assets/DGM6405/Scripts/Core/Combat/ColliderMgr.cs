using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Provides O(1) lookup for SimpleDamageReceiver and other components from Colliders.
///     Avoids the need for expensive GetComponent calls during collision or targeting.
/// </summary>
public class ColliderMgr : Singleton<ColliderMgr>
{
	private readonly Dictionary<Collider, LocalEventBus> _eventBuses = new();
	private readonly Dictionary<Collider, IDamageReceiver> _damageReceivers = new();

	public void Register(Collider col, LocalEventBus bus)
	{
		if (col == null) return;
		_eventBuses[col] = bus;
	}

	public void Register(Collider col, IDamageReceiver receiver)
	{
		if (col == null) return;
		_damageReceivers[col] = receiver;
	}

	public void Unregister(Collider col)
	{
		if (col == null) return;
		_eventBuses.Remove(col);
		_damageReceivers.Remove(col);
	}

	public bool TryGetEventBus(Collider col, out LocalEventBus bus)
	{
		if (col == null)
		{
			bus = null;
			return false;
		}

		return _eventBuses.TryGetValue(col, out bus);
	}

	public bool TryGetDamageReceiver(Collider col, out IDamageReceiver receiver)
	{
		if (col == null)
		{
			receiver = null;
			return false;
		}

		return _damageReceivers.TryGetValue(col, out receiver);
	}
}
