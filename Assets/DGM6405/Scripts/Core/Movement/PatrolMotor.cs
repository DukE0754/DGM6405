using UnityEngine;

/// <summary>
///     Handles patrol logic by choosing waypoints and telling a mover where to go.
/// </summary>
public class PatrolMotor : MonoBehaviour
{
	[SerializeField] private Transform[] _waypoints;
	[SerializeField] private float _waitTime = 1f;
	[SerializeField] private bool _randomize;
	private int _currentWaypointIndex = -1;
	private bool _isWaiting;

	private IMover _mover;
	private float _nextPatrolTime;

	private void Awake()
	{
		_mover = GetComponent<IMover>();
	}

	private void Update()
	{
		if (_waypoints == null || _waypoints.Length == 0) return;

		if (_isWaiting)
		{
			if (Time.time >= _nextPatrolTime)
			{
				_isWaiting = false;
				GoToNextWaypoint();
			}

			return;
		}

		if (_mover.IsAtDestination)
		{
			_isWaiting = true;
			_nextPatrolTime = Time.time + _waitTime;
		}
	}

	private void OnDrawGizmos()
	{
		if (_waypoints == null || _waypoints.Length < 2) return;

		Gizmos.color = Color.green;
		for (var i = 0; i < _waypoints.Length; i++)
		{
			if (_waypoints[i] == null) continue;

			Gizmos.DrawSphere(_waypoints[i].position, 0.3f);

			var next = (i + 1) % _waypoints.Length;
			if (_waypoints[next] != null) Gizmos.DrawLine(_waypoints[i].position, _waypoints[next].position);
		}
	}

	private void GoToNextWaypoint()
	{
		if (_randomize)
			_currentWaypointIndex = Random.Range(0, _waypoints.Length);
		else
			_currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Length;

		_mover.SetDestination(_waypoints[_currentWaypointIndex].position);
	}
}
