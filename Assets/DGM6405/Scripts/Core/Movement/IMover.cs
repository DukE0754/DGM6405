using UnityEngine;

/// <summary>
///     Interface for movement execution.
/// </summary>
public interface IMover
{
	Vector3 Velocity { get; }
	float RemainingDistance { get; }
	bool IsAtDestination { get; }
	void SetDestination(Vector3 destination);
	void Stop();
	void Resume();
	void SetEnabled(bool value);
}
