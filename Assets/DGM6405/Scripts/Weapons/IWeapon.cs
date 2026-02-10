using UnityEngine;

/// <summary>
///     Interface for weapon systems.
/// </summary>
public interface IWeapon
{
	bool CanFire { get; }
	void Fire(Vector3 targetPosition);
	void Fire(Vector3 direction, bool useDirection);
}
