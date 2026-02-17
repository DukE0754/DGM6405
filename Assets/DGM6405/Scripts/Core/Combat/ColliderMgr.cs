using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Provides O(1) lookup for SimpleDamageReceiver and other components from Colliders.
///     Avoids the need for expensive GetComponent calls during collision or targeting.
/// </summary>
public class ColliderMgr : Singleton<ColliderMgr>
{
	private readonly Dictionary<Collider, SimpleDamageReceiver> _damageReceivers = new();

	public void Register(Collider col, SimpleDamageReceiver receiver)
	{
		if (col == null) return;
		_damageReceivers[col] = receiver;
	}

	public void Unregister(Collider col)
	{
		if (col == null) return;
		_damageReceivers?.Remove(col);
	}

	public SimpleDamageReceiver GetDamageReceiver(Collider col)
	{
		if (col == null) return null;
		_damageReceivers.TryGetValue(col, out var receiver);
		return receiver;
	}

	public bool TryGetDamageReceiver(Collider col, out SimpleDamageReceiver receiver)
	{
		if (col == null)
		{
			receiver = null;
			return false;
		}

		return _damageReceivers.TryGetValue(col, out receiver);
	}
}
