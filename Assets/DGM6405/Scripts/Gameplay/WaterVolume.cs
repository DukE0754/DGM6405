using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
///     Detects character entry/exit and raises IWaterVolumeListener events.
///     Attach this to a trigger collider on the Water layer.
/// </summary>
public class WaterVolume : MonoBehaviour
{
	[FormerlySerializedAs("_settings")] [SerializeField]
	private WaterData Data;

	private void OnTriggerEnter(Collider other)
	{
		if (ColliderMgr.Instance.TryGetEventBus(other, out var bus))
			// Use calculated surface height
			bus.Raise<IWaterVolumeListener>(l => l.OnEnteredWater(GetSurfaceHeight()));
	}

	private void OnTriggerExit(Collider other)
	{
		if (ColliderMgr.Instance.TryGetEventBus(other, out var bus))
			bus.Raise<IWaterVolumeListener>(l => l.OnExitedWater());
	}

	private void OnTriggerStay(Collider other)
	{
		if (Data == null) return;

		// Death check if player falls too deep
		var surfaceHeight = GetSurfaceHeight();
		var deathThreshold = surfaceHeight - Data.DeathDepth;

		if (other.transform.position.y < deathThreshold)
			if (ColliderMgr.Instance.TryGetDamageReceiver(other, out var receiver))
				receiver.ApplyDamage(
					new DamageInfo
					{
						Amount = 9999,
						Source = gameObject,
						HitPoint = other.transform.position
					});
	}

	private float GetSurfaceHeight()
	{
		var surfaceHeight = transform.position.y;

		if (TryGetComponent<BoxCollider>(out var box))
			surfaceHeight = transform.position.y + (box.center.y + box.size.y * 0.5f) * transform.lossyScale.y;
		else if (TryGetComponent<SphereCollider>(out var sphere))
			surfaceHeight = transform.position.y + (sphere.center.y + sphere.radius) * transform.lossyScale.y;
		else if (TryGetComponent<CapsuleCollider>(out var capsule))
			surfaceHeight = transform.position.y + (capsule.center.y + capsule.height * 0.5f) * transform.lossyScale.y;

		if (Data != null) surfaceHeight += Data.SurfaceOffset;

		return surfaceHeight;
	}
}
