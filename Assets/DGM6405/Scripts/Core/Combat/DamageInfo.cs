using UnityEngine;

/// <summary>
///     Data payload containing information about a damage event.
/// </summary>
public struct DamageInfo
{
	public int Amount;
	public GameObject Source;
	public Vector3 HitPoint;

	public Vector3 HitNormal;
	// We can add DamageType here later if needed
}
