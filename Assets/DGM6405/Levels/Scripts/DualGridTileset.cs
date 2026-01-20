using UnityEngine;

/// <summary>
/// DualGridTileset
///
/// Maps canonical DualGridTile.ShapeType values to prefabs.
///
/// DESIGN GOAL:
/// ------------
/// This asset is PURE DATA:
/// - No knowledge of tilemaps
/// - No spawner logic
/// - No geometry rules
///
/// It simply answers:
///     "Given a ShapeType, what prefab should I spawn?"
///
/// This keeps:
/// - Biome swaps trivial
/// - Addressables support clean
/// - Prefab sets interchangeable
/// - Debug tile sets easy to author
/// </summary>
[CreateAssetMenu(menuName = "DualGrid/Tileset")]
public class DualGridTileset : ScriptableObject
{
	// =====================================================================
	// CANONICAL SHAPES
	// =====================================================================

	[Header("Canonical Shapes (Rotation Applied by Spawner)")]

	[Tooltip("0 solid quadrants")]
	public GameObject Empty;

	[Tooltip("1 solid quadrant")]
	public GameObject Convex;

	[Tooltip("2 adjacent solid quadrants")]
	public GameObject Edge;

	[Tooltip("2 opposite solid quadrants")]
	public GameObject Diagonal;

	[Tooltip("3 solid quadrants")]
	public GameObject Concave;

	[Tooltip("4 solid quadrants")]
	public GameObject Full;
	
	// =====================================================================
	// LOOKUP API
	// =====================================================================

	/// <summary>
	/// Returns the prefab for a given shape class.
	///
	/// The spawner will apply rotation.
	/// This method does NOT perform any orientation logic.
	/// </summary>
	public GameObject GetPrefab(DualGridTile.ShapeType shape)
	{
		return shape switch
		{
			DualGridTile.ShapeType.Empty => Empty,
			DualGridTile.ShapeType.Convex => Convex,
			DualGridTile.ShapeType.Edge => Edge,
			DualGridTile.ShapeType.Diagonal => Diagonal,
			DualGridTile.ShapeType.Concave => Concave,
			DualGridTile.ShapeType.Full => Full,
			_ => null
		};
	}

}
