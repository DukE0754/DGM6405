using UnityEngine;
using UnityEngine.Animations.Rigging;

public class FootGroundIK : MonoBehaviour
{
	private static readonly int LeftFootCurve = Animator.StringToHash("LeftFootPlant");
	private static readonly int RightFootCurve = Animator.StringToHash("RightFootPlant");

	[Header("References")]
	[SerializeField] private Animator _animator;

	[SerializeField] private Transform _hip;
	[SerializeField] private CharacterController _controller;
	[SerializeField] private Transform _visualRoot;

	[Header("Left Foot")]
	[SerializeField] private Transform _leftFootBone;

	[SerializeField] private Transform _leftTarget;
	[SerializeField] private TwoBoneIKConstraint _leftIK;

	[Header("Right Foot")]
	[SerializeField] private Transform _rightFootBone;

	[SerializeField] private Transform _rightTarget;
	[SerializeField] private TwoBoneIKConstraint _rightIK;

	[Header("Ground Settings")]
	[SerializeField] private LayerMask _groundMask;

	[SerializeField] private float _raycastDistance = 1.5f;
	[SerializeField] private float _raycastHeight = 0.5f;
	[SerializeField] private float _footOffset = 0.02f;

	[Header("Compensation")]
	[SerializeField] private bool _enableVerticalCompensation = true;

	[SerializeField] private float _compensationMultiplier = 2;

	[Header("Debug")]
	[SerializeField] private bool _drawGizmos = true;

	private float _lastVerticalLift;

	private RaycastHit? _leftHit;

	private Vector3 _originalVisualLocalPos;
	private RaycastHit? _rightHit;

	private void Awake()
	{
		if (_visualRoot != null)
			_originalVisualLocalPos = _visualRoot.localPosition;
	}

	private void LateUpdate()
	{
		if (_hip == null) return;

		_leftHit = SolveFoot(
			_leftFootBone,
			_leftTarget,
			_leftIK,
			_animator.GetFloat(LeftFootCurve));

		_rightHit = SolveFoot(
			_rightFootBone,
			_rightTarget,
			_rightIK,
			_animator.GetFloat(RightFootCurve));

		if (_enableVerticalCompensation)
			ApplyVerticalCompensation();
	}

	private RaycastHit? SolveFoot(
		Transform footBone,
		Transform target,
		TwoBoneIKConstraint constraint,
		float weight)
	{
		if (constraint == null || footBone == null || target == null)
			return null;

		var up = _hip.up;
		var down = -up;
		var origin = footBone.position + up * _raycastHeight;

		if (Physics.Raycast(
				origin, down, out var hit,
				_raycastDistance, _groundMask))
		{
			var footPosition = hit.point + up * _footOffset;
			target.position = footPosition;

			var projectedForward =
				Vector3.ProjectOnPlane(_hip.forward, hit.normal);

			target.rotation =
				Quaternion.LookRotation(projectedForward, hit.normal);

			constraint.weight = weight;
			return hit;
		}

		constraint.weight = 0f;
		return null;
	}

	private void ApplyVerticalCompensation()
	{
		if (_controller == null || _visualRoot == null)
			return;

		_visualRoot.localPosition = _originalVisualLocalPos;

		if (!_leftHit.HasValue && !_rightHit.HasValue)
			return;

		var verticalLift = 0f;

		if (_leftHit.HasValue)
			verticalLift = Mathf.Max(
				verticalLift,
				CalculateCompensation(_leftHit.Value));

		if (_rightHit.HasValue)
			verticalLift = Mathf.Max(
				verticalLift,
				CalculateCompensation(_rightHit.Value));

		_lastVerticalLift = verticalLift;

		_visualRoot.position -= _controller.transform.up * verticalLift;
	}

	private float CalculateCompensation(RaycastHit hit)
	{
		var controllerUp = _controller.transform.up;

		var alignment = Vector3.Dot(hit.normal, controllerUp);

		// Flat ground → 0
		// Slopes → positive
		var deviation = 1f - alignment;

		// Prevent negative values (in case of weird normals)
		deviation = Mathf.Max(0f, deviation);

		return _controller.skinWidth * _compensationMultiplier * deviation;
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		if (!_drawGizmos || _hip == null)
			return;

		DrawFootGizmos(_leftFootBone, _leftTarget, _leftHit, Color.blue);
		DrawFootGizmos(_rightFootBone, _rightTarget, _rightHit, Color.red);

		DrawControllerBottom();
		DrawCompensation();
	}

	private void DrawFootGizmos(
		Transform footBone,
		Transform target,
		RaycastHit? hit,
		Color color)
	{
		if (footBone == null) return;

		var up = _hip.up;
		var down = -up;

		var origin = footBone.position + up * _raycastHeight;
		var end = origin + down * _raycastDistance;

		Gizmos.color = color;
		Gizmos.DrawLine(origin, end);
		Gizmos.DrawSphere(origin, 0.02f);

		if (hit.HasValue)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(hit.Value.point, 0.03f);

			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(
				hit.Value.point,
				hit.Value.point + hit.Value.normal * 0.25f);
		}

		if (target != null)
		{
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireSphere(target.position, 0.04f);
		}
	}

	private void DrawControllerBottom()
	{
		if (_controller == null) return;

		var t = _controller.transform;

		var worldCenter =
			t.TransformPoint(_controller.center);

		var bottom =
			worldCenter - t.up * (_controller.height / 2f);

		var bottomWithSkin =
			bottom - t.up * _controller.skinWidth;

		Gizmos.color = Color.cyan;
		Gizmos.DrawSphere(bottomWithSkin, 0.05f);
	}

	private void DrawCompensation()
	{
		if (_visualRoot == null) return;

		Gizmos.color = Color.white;
		Gizmos.DrawLine(
			_visualRoot.position,
			_visualRoot.position + Vector3.up * _lastVerticalLift);
	}
#endif
}
