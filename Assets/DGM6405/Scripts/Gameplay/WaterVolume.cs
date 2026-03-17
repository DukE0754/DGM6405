using UnityEngine;

/// <summary>
///     Detects character entry/exit and raises IWaterVolumeListener events.
///     Attach this to a trigger collider on the Water layer.
/// </summary>
public class WaterVolume : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (ColliderMgr.Instance.TryGetEventBus(other, out var bus))
        {
            // Use current Y position as surface height, or specify offset if needed
            bus.Raise<IWaterVolumeListener>(l => l.OnEnteredWater(transform.position.y));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (ColliderMgr.Instance.TryGetEventBus(other, out var bus))
        {
            bus.Raise<IWaterVolumeListener>(l => l.OnExitedWater());
        }
    }
}
