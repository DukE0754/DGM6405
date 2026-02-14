using UnityEngine;

/// <summary>
///     Handles blocking/defending with shield.
///     Manages shield visibility and blocking animation state.
/// </summary>
public class BlockSystem : PausableBehaviour, IBlockListener
{
	[Header("References")]
	[Tooltip("CharacterContext component. If null, will try to find on same GameObject.")]
	[SerializeField] private CharacterContext _context;

	[Header("Debug Gizmos")]
	[Tooltip("Show block system gizmos in scene view when selected.")]
	[SerializeField] private bool _showGizmos = true;

	[Tooltip("Block coverage angle in degrees.")]
	[Range(0f, 360f)]
	[SerializeField] private float _blockArcAngle = 180f;

	// Internal state

	// Public properties
	public bool IsBlocking { get; private set; }

	private void Awake()
	{
		// Get context if not assigned
		if (_context == null) _context = GetComponent<CharacterContext>();
	}
	
	private void OnDrawGizmosSelected()
	{
		if (!_showGizmos)
			return;

		// Block arc visualization
		if (IsBlocking)
			Gizmos.color = new Color(0f, 1f, 1f, 0.5f); // Cyan with transparency
		else
			Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f); // Gray with transparency

		// Draw arc in front of character
		var halfAngle = _blockArcAngle * 0.5f;
		var forward = transform.forward;
		var right = transform.right;
		var arcStart = Quaternion.AngleAxis(-halfAngle, Vector3.up) * forward;
		var arcEnd = Quaternion.AngleAxis(halfAngle, Vector3.up) * forward;

		// Draw arc lines
		var segments = 16;
		for (var i = 0; i < segments; i++)
		{
			var t1 = (float) i / segments;
			var t2 = (float) (i + 1) / segments;
			var angle1 = -halfAngle + t1 * _blockArcAngle;
			var angle2 = -halfAngle + t2 * _blockArcAngle;
			var dir1 = Quaternion.AngleAxis(angle1, Vector3.up) * forward;
			var dir2 = Quaternion.AngleAxis(angle2, Vector3.up) * forward;
			Gizmos.DrawLine(transform.position + dir1 * 1.5f, transform.position + dir2 * 1.5f);
		}

		// Draw forward direction
		Gizmos.color = Color.blue;
		Gizmos.DrawRay(transform.position, forward * 1.5f);
	}

	void IBlockListener.OnBlock(bool blockInput)
	{
		SetBlocking(blockInput);
	}

	private void SetBlocking(bool isBlocking)
	{
		// Check game state
		if (GameMgr.Instance != null && !GameMgr.Instance.IsGameRunning)
		{
			// Cancel blocking if game is not running
			if (IsBlocking)
			{
				IsBlocking = false;
			}

			return;
		}

		// Update blocking state
		if (IsBlocking != isBlocking)
		{
			IsBlocking = isBlocking;
		}
	}

	protected override void OnPaused()
	{
		// Cancel blocking when paused
		if (IsBlocking)
		{
			IsBlocking = false;
		}
	}
}
