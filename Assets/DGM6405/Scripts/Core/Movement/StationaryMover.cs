using UnityEngine;

/// <summary>
///     A mover that doesn't actually move. Used for stationary enemies like Bud or Training Dummy.
///     Keeps the architecture consistent.
/// </summary>
public class StationaryMover : MonoBehaviour, IMover
{
	public Vector3 Velocity => Vector3.zero;
	public float RemainingDistance => 0f;
	public bool IsAtDestination => true;

	public void SetDestination(Vector3 destination)
	{
		/* Do nothing */
	}

	public void Stop()
	{
		/* Do nothing */
	}

	public void Resume()
	{
		/* Do nothing */
	}

	public void SetEnabled(bool value)
	{
		/* Do nothing */
	}
}
