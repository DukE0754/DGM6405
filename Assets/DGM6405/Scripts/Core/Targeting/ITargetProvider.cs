using UnityEngine;

/// <summary>
///     Interface for obtaining a target for AI.
/// </summary>
public interface ITargetProvider
{
	bool HasTarget { get; }
	Transform GetTarget();
	Vector3 GetTargetOffset();
	Vector3 GetTargetPosition();
}
