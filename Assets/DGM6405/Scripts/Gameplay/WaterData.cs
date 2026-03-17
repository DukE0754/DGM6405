using UnityEngine;

[CreateAssetMenu(fileName = "WaterSettings", menuName = "DGM6405/Gameplay/WaterSettings")]
public class WaterData : ScriptableObject
{
	[Header("Surface Settings")]
	[Tooltip("Offset from the top of the collider for the water surface.")]
	public float SurfaceOffset;

	[Header("Death Settings")]
	[Tooltip("How deep the player can fall into the water before dying (relative to surface).")]
	public float DeathDepth = 0.5f;

	[Header("Visual Settings")]
	[Tooltip("Offset for the floating position after death.")]
	public float FloatOffset = 0.5f;

	[Tooltip("Speed at which the player floats to the surface after death.")]
	public float FloatSpeed = 2f;
}
