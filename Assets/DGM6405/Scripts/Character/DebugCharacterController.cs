using UnityEngine;

[ExecuteAlways]
public class DebugCharacterController : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private CharacterController _controller;

	[SerializeField] private Transform _leftFootBone;
	[SerializeField] private Transform _rightFootBone;

	[SerializeField] private Transform _leftFootTarget;
	[SerializeField] private Transform _rightFootTarget;

	[Header("Gizmo Settings")]
	[SerializeField] private bool _drawGizmos = true;

	[SerializeField] private float _axisLength = 0.15f;

	private void OnDrawGizmos()
	{
		if (!_drawGizmos || _controller == null)
			return;

		DrawControllerCapsule();
		DrawFeet();
	}

	// --------------------------------------------------
	// Capsule Gizmos
	// --------------------------------------------------

	private void DrawControllerCapsule()
	{
		var t = _controller.transform;

		var height = _controller.height;
		var radius = _controller.radius;
		var skin = _controller.skinWidth;
		var center = _controller.center;

		var trueRadius = radius + skin;

		var worldCenter = t.TransformPoint(center);

		var cylinderHeight = Mathf.Max(0, height - radius * 2f);

		var up = t.up;

		var top = worldCenter + up * (cylinderHeight / 2f);
		var bottom = worldCenter - up * (cylinderHeight / 2f);

		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(top, trueRadius);
		Gizmos.DrawWireSphere(bottom, trueRadius);

		var forward = t.forward * trueRadius;
		var right = t.right * trueRadius;

		Gizmos.DrawLine(top + forward, bottom + forward);
		Gizmos.DrawLine(top - forward, bottom - forward);
		Gizmos.DrawLine(top + right, bottom + right);
		Gizmos.DrawLine(top - right, bottom - right);

		var capsuleBottom =
			worldCenter - up * (height / 2f) - up * skin;

		Gizmos.color = Color.green;
		Gizmos.DrawSphere(capsuleBottom, 0.04f);

		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(worldCenter, 0.035f);
	}

	// --------------------------------------------------
	// Feet Gizmos
	// --------------------------------------------------

	private void DrawFeet()
	{
		DrawFoot(_leftFootBone, _leftFootTarget, Color.blue);
		DrawFoot(_rightFootBone, _rightFootTarget, Color.red);
	}

	private void DrawFoot(Transform bone, Transform target, Color color)
	{
		if (bone != null)
		{
			Gizmos.color = color;
			Gizmos.DrawSphere(bone.position, 0.04f);
			DrawAxes(bone);
		}

		if (target != null)
		{
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireSphere(target.position, 0.05f);
			DrawAxes(target);
		}
	}

	private void DrawAxes(Transform t)
	{
		if (t == null) return;

		Gizmos.color = Color.red; // Forward
		Gizmos.DrawLine(t.position, t.position + t.forward * _axisLength);

		Gizmos.color = Color.green; // Up
		Gizmos.DrawLine(t.position, t.position + t.up * _axisLength);

		Gizmos.color = Color.blue; // Right
		Gizmos.DrawLine(t.position, t.position + t.right * _axisLength);
	}

	// --------------------------------------------------
	// Logging
	// --------------------------------------------------

	public void PrintDebugInfo()
	{
		if (_controller == null)
		{
			Debug.LogWarning("CharacterController missing.");
			return;
		}

		var t = _controller.transform;

		Debug.Log("===== CHARACTER CONTROLLER DEBUG =====");
		Debug.Log($"Transform Position: {t.position}");
		Debug.Log($"Height: {_controller.height}");
		Debug.Log($"Radius: {_controller.radius}");
		Debug.Log($"Center: {_controller.center}");
		Debug.Log($"Skin Width: {_controller.skinWidth}");

		var worldCenter = t.TransformPoint(_controller.center);
		var bottom =
			worldCenter - t.up * (_controller.height / 2f);

		Debug.Log($"World Center: {worldCenter}");
		Debug.Log($"Capsule Bottom (no skin): {bottom}");
		Debug.Log($"Capsule Bottom (with skin): {bottom - t.up * _controller.skinWidth}");

		LogFoot("LEFT", _leftFootBone, _leftFootTarget);
		LogFoot("RIGHT", _rightFootBone, _rightFootTarget);

		Debug.Log("======================================");
	}

	private void LogFoot(string label, Transform bone, Transform target)
	{
		if (bone == null && target == null)
			return;

		Debug.Log($"---- {label} FOOT ----");

		if (bone != null)
		{
			Debug.Log($"Bone Position: {bone.position}");
			Debug.Log($"Bone Rotation (Euler): {bone.rotation.eulerAngles}");
			Debug.Log($"Bone Forward: {bone.forward}");
			Debug.Log($"Bone Up: {bone.up}");
		}

		if (target != null)
		{
			Debug.Log($"Target Position: {target.position}");
			Debug.Log($"Target Rotation (Euler): {target.rotation.eulerAngles}");
			Debug.Log($"Target Forward: {target.forward}");
			Debug.Log($"Target Up: {target.up}");
		}

		if (bone != null && target != null)
		{
			var upAngle =
				Vector3.Angle(bone.up, target.up);

			var forwardAngle =
				Vector3.Angle(bone.forward, target.forward);

			Debug.Log($"Up Angle Difference: {upAngle}");
			Debug.Log($"Forward Angle Difference: {forwardAngle}");
		}
	}
}
