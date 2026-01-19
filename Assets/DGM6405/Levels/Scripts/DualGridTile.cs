using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
///     DualGridTile
///     Shape-driven, canonical dual-grid tile contract.
///     The prefab declares WHAT it is (ShapeType),
///     and this script derives the 2x2 micro-mask algorithmically.
///     This guarantees:
///     - Only 6 shapes exist
///     - All rotations are mathematically consistent
///     - Artists never need to reason about bitmasks
///     - Spawner logic stays trivial and deterministic
/// </summary>
[ExecuteAlways]
public class DualGridTile : MonoBehaviour
{
	/// <summary>
	///     Micro-quadrants inside the prefab footprint.
	///     Layout (local space):
	///     Z+
	///     ^
	///     [ Q01 | Q11 ]
	///     [ Q00 | Q10 ] -> X+
	/// </summary>
	public enum Quad
	{
		Q00,
		Q10,
		Q01,
		Q11
	}
	// =====================================================================
	// SECTION A: SHAPE THEORY (6 CANONICAL SHAPES)
	// =====================================================================

	/// <summary>
	///     The six unique shape classes under 90° rotation symmetry.
	/// </summary>
	public enum ShapeType
	{
		Empty, // 0 solid
		Convex, // 1 solid
		Edge, // 2 adjacent solid
		Diagonal, // 2 opposite solid
		Concave, // 3 solid
		Full // 4 solid
	}

	// =====================================================================
	// SECTION B: PREFAB IDENTITY
	// =====================================================================

	[Header("Shape Declaration")]
	[Tooltip("Canonical shape class for this prefab. Mask is derived automatically.")]
	public ShapeType Shape = ShapeType.Empty;

	// =====================================================================
	// SECTION C: SPAWN KEY (RELOAD-SAFE IDENTITY)
	// =====================================================================

	[Header("Spawn Identity (Assigned by Spawner)")]
	public Vector3Int Key;
	public ShapeType SpawnedShape;

	// =====================================================================
	// GIZMOS (DERIVED FROM SHAPE)
	// =====================================================================

	[Header("Gizmos - Prefab Mask")]
	public bool DrawPrefabMaskGizmos = true;

	[Tooltip("Prefab footprint size in world units (quarter tile = 0.5).")]
	public float FootprintSize = 0.5f;

	[Tooltip("Lift gizmos above geometry so they're visible.")]
	public float GizmoHeightOffset = 0.05f;

	[Tooltip("Size of micro-quadrant cubes in the gizmo.")]
	public float MicroGizmoSize = 0.05f;

	[Header("Gizmos - Tilemap Overlay (Optional)")]
	public Tilemap MaskTilemap;

	public bool DrawPaintedCorners = true;
	public bool DrawDualCells = true;
	public bool DrawBounds;

	public float OverlayGizmoSize = 0.1f;
	public int MaxDrawCount = 500;


	private void OnDrawGizmos()
	{
		if (DrawPrefabMaskGizmos)
			DrawPrefabMask();

		if (MaskTilemap && (DrawPaintedCorners || DrawDualCells || DrawBounds))
			DrawTilemapOverlay();
	}

	// =====================================================================
	// MASK GENERATION (DERIVED, NOT AUTHORED)
	// =====================================================================

	/// <summary>
	///     Returns the DEFAULT (0°) mask for this prefab based on ShapeType.
	///     IMPORTANT:
	///     This mask is in CANONICAL ORIENTATION.
	///     The spawner will rotate this mask to match the world.
	///     Canonical definitions:
	///     - Empty   : 0000
	///     - Convex : 0001 (Q00 solid)
	///     - Edge   : 0011 (Q00 + Q10 solid)
	///     - Diagonal:0110 (Q10 + Q01 solid) // opposite corners
	///     - Concave:0111 (Q00 + Q10 + Q01 solid)
	///     - Full   : 1111
	///     Bit layout:
	///     1 = Q00
	///     2 = Q10
	///     4 = Q01
	///     8 = Q11
	/// </summary>
	public byte GetMask()
	{
		return Shape switch
		{
			ShapeType.Empty => 0,
			ShapeType.Convex => 1, // Q00
			ShapeType.Edge => 3, // Q00 + Q10
			ShapeType.Diagonal => 6, // Q10 + Q01
			ShapeType.Concave => 7, // Q00 + Q10 + Q01
			ShapeType.Full => 15, // All
			_ => 0
		};
	}

	// =====================================================================
	// ROTATION + NORMALIZATION (CORE ALGORITHM)
	// =====================================================================

	/// <summary>
	///     Rotates a 2x2 mask clockwise by 90 degrees.
	///     Quadrant movement:
	///     Q00 -> Q10
	///     Q10 -> Q11
	///     Q11 -> Q01
	///     Q01 -> Q00
	/// </summary>
	public static byte RotateMask90(byte m)
	{
		var q00 = (m & 1) != 0;
		var q10 = (m & 2) != 0;
		var q01 = (m & 4) != 0;
		var q11 = (m & 8) != 0;

		byte r = 0;
		if (q01) r |= 1;
		if (q00) r |= 2;
		if (q11) r |= 4;
		if (q10) r |= 8;

		return r;
	}

	/// <summary>
	///     Normalizes a mask into its CANONICAL FORM.
	///     Outputs:
	///     - canonicalMask: one of {0,1,3,5,7,15}
	///     - rotationSteps: number of clockwise 90° rotations
	/// </summary>
	public static void NormalizeMask(
		byte mask,
		out byte canonicalMask,
		out int rotationSteps
	)
	{
		canonicalMask = mask;
		rotationSteps = 0;

		var current = mask;

		for (var i = 1; i < 4; i++)
		{
			current = RotateMask90(current);

			if (current < canonicalMask)
			{
				canonicalMask = current;
				rotationSteps = i;
			}
		}
	}

	/// <summary>
	///     Converts a canonical mask into a shape class.
	/// </summary>
	public static ShapeType ShapeFromCanonical(byte canonicalMask)
	{
		return canonicalMask switch
		{
			0 => ShapeType.Empty,
			1 => ShapeType.Convex,
			3 => ShapeType.Edge,
			6 => ShapeType.Diagonal,
			7 => ShapeType.Concave,
			15 => ShapeType.Full,
			_ => ShapeType.Empty
		};
	}
	
	// Returns the canonical mask for a given ShapeType // comment every line
	public static byte GetCanonicalMask(ShapeType shape) // comment every line
	{
		return shape switch // comment every line
		{
			ShapeType.Empty    => 0,  // comment every line
			ShapeType.Convex   => 1,  // Q00 // comment every line
			ShapeType.Edge     => 3,  // Q00 + Q10 // comment every line
			ShapeType.Diagonal => 6,  // Q10 + Q01 // comment every line
			ShapeType.Concave  => 7,  // Q00 + Q10 + Q01 // comment every line
			ShapeType.Full     => 15, // comment every line
			_                  => 0  // comment every line
		};
	}

	// Normalizes a mask ONLY to its canonical value (no rotation info) // comment every line
	public static byte NormalizeToCanonicalMask(byte mask) // comment every line
	{
		var current = mask; // comment every line
		var min = mask; // comment every line

		for (var i = 0; i < 4; i++) // comment every line
		{
			if (current < min) // comment every line
				min = current; // comment every line

			current = RotateMask90(current); // comment every line
		}

		return min; // comment every line
	}


	// =====================================================================
	// PREFAB MASK GIZMOS
	// =====================================================================

	void DrawPrefabMask()
	{
		// We draw micro-quadrants in WORLD space, but their positions are defined
		// in the prefab's LOCAL space around its pivot.
		//
		// IMPORTANT CONTRACT (matches your working behavior):
		// - Prefab pivot (local 0,0,0) is the CENTER of the quarter-tile.
		// - The quarter-tile footprint is FootprintSize (default 0.5).
		// - Therefore the footprint spans:
		//     X: [-FootprintSize/2 .. +FootprintSize/2]
		//     Z: [-FootprintSize/2 .. +FootprintSize/2]
		//
		// Micro-quadrants are a 2x2 inside that footprint, so their centers are:
		//     +/- FootprintSize/4

		Vector3 basePos = transform.position + Vector3.up * GizmoHeightOffset;

		float half = FootprintSize * 0.5f;     // 0.25 when FootprintSize = 0.5
		float quarter = FootprintSize * 0.25f; // 0.125 when FootprintSize = 0.5

		// Local offsets around pivot (0,0,0)
		Vector3 q00 = new Vector3(-quarter, 0, -quarter);
		Vector3 q10 = new Vector3( quarter, 0, -quarter);
		Vector3 q01 = new Vector3(-quarter, 0,  quarter);
		Vector3 q11 = new Vector3( quarter, 0,  quarter);

		byte mask = GetMask();

		DrawMicro(basePos, q00, (mask & 1) != 0);
		DrawMicro(basePos, q10, (mask & 2) != 0);
		DrawMicro(basePos, q01, (mask & 4) != 0);
		DrawMicro(basePos, q11, (mask & 8) != 0);
	}


	private void DrawMicro(Vector3 basePos, Vector3 localOffset, bool solid)
	{
		var pos = basePos + transform.rotation * localOffset;

		Gizmos.color = solid ? Color.green : Color.red;
		Gizmos.DrawCube(pos, Vector3.one * MicroGizmoSize);
	}

	// =====================================================================
	// TILEMAP OVERLAY GIZMOS
	// =====================================================================

	private void DrawTilemapOverlay()
	{
		var bounds = MaskTilemap.cellBounds;
		var cellSize = MaskTilemap.layoutGrid.cellSize.x;
		var h = cellSize * 0.25f;

		var drawn = 0;

		for (var x = bounds.xMin; x < bounds.xMax; x++)
			for (var y = bounds.yMin; y < bounds.yMax; y++)
			{
				if (drawn >= MaxDrawCount)
					break;

				var cell = new Vector3Int(x, y, 0);

				if (!MaskTilemap.GetTile(cell))
					continue;

				drawn++;

				var cornerPos = MaskTilemap.GetCellCenterWorld(cell);
				cornerPos.y += GizmoHeightOffset;

				if (DrawPaintedCorners)
				{
					Gizmos.color = Color.yellow;
					Gizmos.DrawSphere(cornerPos, OverlayGizmoSize);
				}

				if (DrawDualCells)
				{
					DrawDualCross(cornerPos + new Vector3(-h, 0, -h), Color.cyan);
					DrawDualCross(cornerPos + new Vector3(h, 0, -h), Color.blue);
					DrawDualCross(cornerPos + new Vector3(-h, 0, h), Color.green);
					DrawDualCross(cornerPos + new Vector3(h, 0, h), Color.magenta);
				}
			}

		if (DrawBounds)
		{
			Gizmos.color = Color.white;

			var center = MaskTilemap.localBounds.center;
			var size = MaskTilemap.localBounds.size;

			Gizmos.matrix = MaskTilemap.transform.localToWorldMatrix;
			Gizmos.DrawWireCube(center, size);
			Gizmos.matrix = Matrix4x4.identity;
		}
	}

	private void DrawDualCross(Vector3 pos, Color color)
	{
		Gizmos.color = color;

		Gizmos.DrawLine(pos + Vector3.left * OverlayGizmoSize, pos + Vector3.right * OverlayGizmoSize);
		Gizmos.DrawLine(pos + Vector3.forward * OverlayGizmoSize, pos + Vector3.back * OverlayGizmoSize);
	}
}
