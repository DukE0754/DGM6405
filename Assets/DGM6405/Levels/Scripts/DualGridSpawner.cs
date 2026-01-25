using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
///     CORNER-DRIVEN DUAL GRID SPAWNER (CANONICAL MASK VERSION)
///     UPDATE:
///     -------
///     - Keys are CELL-SPACE based (no world float rounding)
///     - Object names include REAL WORLD COORDS (2 decimal places)
///     - Shape reuse is based on ShapeType, NOT name matching
///     - Axis mapping is MEASURED, not assumed (fixes vertical flip bugs)
///     TILEMAP CONTRACT:
///     -----------------
///     - Painted tiles represent VERTICES (corners), not blocks
///     - Each painted tile spawns FOUR quarter-tiles around it
///     - Each quarter-tile resolves itself from a 2x2 pattern of painted corners
/// </summary>
[ExecuteAlways]
public class DualGridSpawner : MonoBehaviour
{
	// =====================================================================
	// REFERENCES
	// =====================================================================

	[Header("References")]
	[Tooltip("Tilemap used as CORNER OCCUPANCY mask")]
	public Tilemap MaskTilemap;

	[Tooltip("Tileset mapping ShapeType → Prefab")]
	public DualGridTileset Tileset;

	[Tooltip("Parent object for all spawned quarter tiles")]
	public Transform Root;

	// =====================================================================
	// SETTINGS
	// =====================================================================

	[Header("Settings")]
	[Tooltip("World Y position for spawned tiles")]
	public float TileHeight;

	[Tooltip("Continuously rebuild in editor/runtime")]
	public bool AutoRefresh = true;

	[Tooltip("Seconds between rebuilds in Edit Mode")]
	public float EditorRefreshInterval = 0.1f;

	// =====================================================================
	// INTERNAL STATE
	// =====================================================================

	/// <summary>
	///     Active spawned tiles indexed by dual-grid key
	/// </summary>
	private readonly Dictionary<Vector3Int, GameObject> _spawned = new();

	// How world axes map to tilemap cell axes (measured, not guessed)
	private Vector3Int _cellStepWorldX;
	private Vector3Int _cellStepWorldZ;

	private float _lastEditorRebuildTime;

	private void Update()
	{
#if UNITY_EDITOR
		if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
			return;
#endif
		
		if (!AutoRefresh)
			return;

		if (Application.isPlaying)
		{
			Rebuild();
			return;
		}

		if (Time.realtimeSinceStartup - _lastEditorRebuildTime >= EditorRefreshInterval)
		{
			_lastEditorRebuildTime = Time.realtimeSinceStartup;
			Rebuild();
		}
	}

	// =====================================================================
	// UNITY EVENTS
	// =====================================================================

	private void OnValidate()
	{
#if UNITY_EDITOR
		if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
			return;
#endif
		
		RequestRebuild();
	}

	// =====================================================================
	// REBUILD CONTROL
	// =====================================================================

	public void RequestRebuild()
	{
		_lastEditorRebuildTime = 0f;
	}

	public void ClearAllSpawnedTiles()
	{
		if (!Root)
			return;

		for (var i = Root.childCount - 1; i >= 0; i--)
		{
			var child = Root.GetChild(i);
			if (child)
				SafeDestroy(child.gameObject);
		}

		_spawned.Clear();
	}

	public void RebuildOnce()
	{
		Rebuild();
	}

	public void ClearAndRebuildOnce()
	{
		ClearAllSpawnedTiles();
		Rebuild();
	}

	public void ToggleAutoRefresh()
	{
		AutoRefresh = !AutoRefresh;
		RequestRebuild();
	}

	// =====================================================================
	// CORE REBUILD
	// =====================================================================

	public void Rebuild()
	{
#if UNITY_EDITOR
		if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
			return;
#endif

		if (!MaskTilemap || !Tileset || !Root)
			return;

		// Measure axis mapping once per rebuild
		CacheWorldToCellSteps();

		// Re-sync dictionary from scene (domain reload safe)
		RebuildSpawnIndex();

		var bounds = MaskTilemap.cellBounds;
		HashSet<Vector3Int> liveKeys = new();

		for (var x = bounds.xMin; x < bounds.xMax; x++)
			for (var y = bounds.yMin; y < bounds.yMax; y++)
			{
				var paintedCell = new Vector3Int(x, y, 0);

				// Only painted tiles act as vertices
				if (!HasTile(paintedCell))
					continue;

				ProcessCorner(paintedCell, liveKeys);
			}

		// Destroy anything not touched this pass
		CleanupDeadKeys(liveKeys);
	}

	// =====================================================================
	// AXIS MAPPING (THE IMPORTANT PART)
	// =====================================================================

	/// <summary>
	///     Measures how world +X and +Z map into tilemap cell space.
	///     This avoids assuming +worldZ == +cellY, which is false in your layout.
	/// </summary>
	private void CacheWorldToCellSteps()
	{
		var originWorld = MaskTilemap.transform.position;
		var s = MaskTilemap.layoutGrid.cellSize.x;

		var c0 = MaskTilemap.WorldToCell(originWorld);
		var cx = MaskTilemap.WorldToCell(originWorld + Vector3.right * s);
		var cz = MaskTilemap.WorldToCell(originWorld + Vector3.forward * s);

		_cellStepWorldX = cx - c0;
		_cellStepWorldZ = cz - c0;

		// Safety fallbacks
		if (_cellStepWorldX == Vector3Int.zero)
			_cellStepWorldX = new Vector3Int(1, 0, 0);

		if (_cellStepWorldZ == Vector3Int.zero)
			_cellStepWorldZ = new Vector3Int(0, 1, 0);
	}

	// =====================================================================
	// SCENE SYNC
	// =====================================================================

	private void RebuildSpawnIndex()
	{
		_spawned.Clear();

		for (var i = 0; i < Root.childCount; i++)
		{
			var child = Root.GetChild(i);
			var tile = child.GetComponent<DualGridTile>();
			if (!tile)
				continue;

			_spawned[tile.Key] = child.gameObject;
		}
	}

	private void CleanupDeadKeys(HashSet<Vector3Int> liveKeys)
	{
		var toRemove = new List<Vector3Int>();

		foreach (var kvp in _spawned)
			if (!liveKeys.Contains(kvp.Key))
			{
				SafeDestroy(kvp.Value);
				toRemove.Add(kvp.Key);
			}

		foreach (var k in toRemove)
			_spawned.Remove(k);
	}

	// =====================================================================
	// CORNER PROCESSING
	// =====================================================================

	private void ProcessCorner(Vector3Int cornerCell, HashSet<Vector3Int> liveKeys)
	{
		var cornerWorldPos = MaskTilemap.GetCellCenterWorld(cornerCell);
		cornerWorldPos.y = TileHeight;

		var cellSize = MaskTilemap.layoutGrid.cellSize.x;
		var h = cellSize * 0.25f;

		ProcessQuadrant(cornerCell, cornerWorldPos, new Vector3(-h, 0, -h), liveKeys);
		ProcessQuadrant(cornerCell, cornerWorldPos, new Vector3(h, 0, -h), liveKeys);
		ProcessQuadrant(cornerCell, cornerWorldPos, new Vector3(-h, 0, h), liveKeys);
		ProcessQuadrant(cornerCell, cornerWorldPos, new Vector3(h, 0, h), liveKeys);
	}

	// =====================================================================
	// QUADRANT RESOLUTION
	// =====================================================================

	private void ProcessQuadrant(
		Vector3Int cornerCell,
		Vector3 cornerWorldPos,
		Vector3 offset,
		HashSet<Vector3Int> liveKeys
	)
	{
		var worldPos = cornerWorldPos + offset;

		var key = MakeDualKey(cornerCell, offset);
		liveKeys.Add(key);

		// 1) Build WORLD MASK
		var worldMask = BuildWorldMask(cornerCell, offset);
		var rotationTargetMask = worldMask; // rotation mask defaults to world mask


		// 2) Determine shape from raw world mask
		// Normalize the raw world mask into its canonical form (rotation-invariant)
		var canonicalWorldMask = DualGridTile.NormalizeToCanonicalMask(worldMask); // comment every line

		// Resolve the base shape from the canonical mask
		var shape = DualGridTile.ShapeFromCanonical(canonicalWorldMask); // comment every line

		// Concave masks (3 solid corners) require directional ownership. // comment every line
		// Only ONE quadrant may legally be concave per inside corner. // comment every line
		if (shape == DualGridTile.ShapeType.Concave) // comment every line
		{
			// Determine which micro-quadrant is empty in WORLD mask space. // comment every line
			// (bit not set among Q00=1, Q10=2, Q01=4, Q11=8). // comment every line
			var emptyBit = (byte) (~worldMask & 0b1111); // comment every line

			// Determine which quadrant this spawned tile represents based on its offset. // comment every line
			var isRight = offset.x > 0f; // comment every line
			var isTop = offset.z > 0f; // comment every line

			// Map this quadrant to its "quadBit" (which corner of the 2x2 this quad corresponds to). // comment every line
			byte quadBit = 0; // comment every line
			if (!isRight && !isTop) quadBit = 1; // Q00 // comment every line
			if (isRight && !isTop) quadBit = 2; // Q10 // comment every line
			if (!isRight && isTop) quadBit = 4; // Q01 // comment every line
			if (isRight && isTop) quadBit = 8; // Q11 // comment every line

			// Map this quadrant to its expected EMPTY bit for concave ownership. // comment every line
			// Only the quad that faces the empty corner may remain concave. // comment every line
			var expectedEmptyBit = quadBit; // comment every line

			// If this quadrant does NOT correspond to the empty corner, it is NOT concave. // comment every line
			if ((emptyBit & expectedEmptyBit) == 0) // comment every line
			{
				// Determine the opposite corner bit (diagonal across the 2x2). // comment every line
				byte oppositeBit = 0; // comment every line
				if (emptyBit == 1) oppositeBit = 8; // Q00 <-> Q11 // comment every line
				if (emptyBit == 8) oppositeBit = 1; // Q11 <-> Q00 // comment every line
				if (emptyBit == 2) oppositeBit = 4; // Q10 <-> Q01 // comment every line
				if (emptyBit == 4) oppositeBit = 2; // Q01 <-> Q10 // comment every line

				// If this quad is diagonally opposite the empty corner, it is interior and should be FULL. // comment every line
				if (quadBit == oppositeBit) // comment every line
				{
					shape = DualGridTile.ShapeType.Full; // comment every line
					rotationTargetMask = worldMask; // comment every line
				}
				else // comment every line
				{
					// Otherwise this quad borders the empty corner and should be an EDGE.
					shape = DualGridTile.ShapeType.Edge;

					// Determine which TWO bits are adjacent to the empty corner
					byte adjA = 0;
					byte adjB = 0;

					switch (emptyBit)
					{
						case 1: // Q00 empty → adjacent Q10 & Q01
							adjA = 2; // Q10
							adjB = 4; // Q01
							break;

						case 2: // Q10 empty → adjacent Q00 & Q11
							adjA = 1; // Q00
							adjB = 8; // Q11
							break;

						case 4: // Q01 empty → adjacent Q00 & Q11
							adjA = 1; // Q00
							adjB = 8; // Q11
							break;

						case 8: // Q11 empty → adjacent Q10 & Q01
							adjA = 2; // Q10
							adjB = 4; // Q01
							break;
					}

					// Now choose WHICH edge based on which adjacent side this quad is on
					if (quadBit == adjA)
					{
						// Edge runs along adjB ↔ opposite(adjB)
						rotationTargetMask = (byte) (adjB | oppositeBit);
					}
					else
					{
						// Edge runs along adjA ↔ opposite(adjA)
						rotationTargetMask = (byte) (adjA | oppositeBit);
					}


				} // comment every line
			} // comment every line

			/*
			// =====================================================================
			// DEBUG: CONCAVE-ADJACENT EDGE BIT DUMP (PLACE INSIDE ProcessQuadrant)
			// =====================================================================
			// Put this RIGHT AFTER you compute: worldMask, canonicalWorldMask, shape, emptyBit, quadBit,
			// and RIGHT BEFORE you finalize rotation (before you rotate canonicalMask toward rotationTargetMask).

			// Only run this debug when we're in the concave-handling branch and we decide this quad becomes an EDGE.
			if (shape == DualGridTile.ShapeType.Edge) // comment every line
			{
				// Compute raw bits as 4-bit binary strings. // comment every line
				var wm = Convert.ToString(worldMask, 2).PadLeft(4, '0'); // comment every line
				var em = Convert.ToString((byte) (~worldMask & 0b1111), 2).PadLeft(4, '0'); // comment every line
				var qb = Convert.ToString(quadBit, 2).PadLeft(4, '0'); // comment every line
				var rt = Convert.ToString(rotationTargetMask, 2).PadLeft(4, '0'); // comment every line

				// Extract which bits are set for quick human comparison. // comment every line
				string BitsToList(byte m) // comment every line
				{
					// comment every line
					var s = ""; // comment every line
					if ((m & 1) != 0) s += "Q00 "; // comment every line
					if ((m & 2) != 0) s += "Q10 "; // comment every line
					if ((m & 4) != 0) s += "Q01 "; // comment every line
					if ((m & 8) != 0) s += "Q11 "; // comment every line
					return s.Trim(); // comment every line
				} // comment every line


				// Print a single line you can copy/paste into notes and compare side-by-side. // comment every line
				Debug.Log( // comment every line
					"[DualGrid][EDGE@CONCAVE] " + // comment every line
					$"Corner:{cornerCell} " + // comment every line
					$"Offset:({offset.x:F2},{offset.y:F2},{offset.z:F2}) " + // comment every line
					$"Side:{(isRight ? "R" : "L")}{(isTop ? "T" : "B")} " + // comment every line
					$"WorldMask:{wm}({BitsToList(worldMask)}) " + // comment every line
					$"EmptyBit:{em}({BitsToList(emptyBit)}) " + // comment every line
					$"QuadBit:{qb}({BitsToList(quadBit)}) " + // comment every line
					$"RotTarget:{rt}({BitsToList(rotationTargetMask)})" // comment every line
				); // comment every line
			} // comment every line

			*/
		} // comment every line


		// 3) Determine rotation by rotating CANONICAL mask into WORLD space // comment every line
		// This is the same rule used for Edge/Convex/Concave and matches the expected "pointing" behavior. // comment every line
		var canonicalMask = DualGridTile.GetCanonicalMask(shape); // comment every line
		var rotatedCanonicalMask = canonicalMask; // comment every line
		var rotSteps = 0; // comment every line

		for (var i = 0; i < 4; i++) // comment every line
		{
			if (rotatedCanonicalMask == rotationTargetMask)
				break; // comment every line

			rotatedCanonicalMask = DualGridTile.RotateMask90(rotatedCanonicalMask); // comment every line
			rotSteps++; // comment every line
		} // comment every line

		// 3b) DIAGONAL TIE-BREAK (0 vs 180 ambiguity) // comment every line
		// Diagonal masks can be identical under 180°, but your prefab is not. // comment every line
		// When ambiguous, pick rotation based on which painted corner is the driver. // comment every line
		if (shape == DualGridTile.ShapeType.Diagonal) // comment every line
		{
			var rightSide = offset.x > 0 ? 1 : 0; // comment every line
			var topSide = offset.z > 0 ? 1 : 0; // comment every line

			var basisRight = _cellStepWorldX; // comment every line
			var basisUp = _cellStepWorldZ; // comment every line

			var topRight = cornerCell + rightSide * basisRight + topSide * basisUp; // comment every line
			var topLeft = topRight - basisRight; // comment every line
			var botRight = topRight - basisUp; // comment every line
			var botLeft = topRight - basisRight - basisUp; // comment every line

			var paintedA = Vector3Int.zero; // comment every line
			var paintedB = Vector3Int.zero; // comment every line
			var paintedCount = 0; // comment every line

			if (HasTile(botLeft)) // comment every line
			{
				paintedA = botLeft; // comment every line
				paintedCount++; // comment every line
			} // comment every line

			if (HasTile(botRight)) // comment every line
			{
				if (paintedCount == 0) paintedA = botRight; // comment every line
				else paintedB = botRight; // comment every line
				paintedCount++; // comment every line
			} // comment every line

			if (HasTile(topLeft)) // comment every line
			{
				if (paintedCount == 0) paintedA = topLeft; // comment every line
				else paintedB = topLeft; // comment every line
				paintedCount++; // comment every line
			} // comment every line

			if (HasTile(topRight)) // comment every line
			{
				if (paintedCount == 0) paintedA = topRight; // comment every line
				else paintedB = topRight; // comment every line
				paintedCount++; // comment every line
			} // comment every line

			if (paintedCount == 2) // comment every line
			{
				var primary = paintedA; // comment every line

				if (paintedB.y > primary.y || (paintedB.y == primary.y && paintedB.x > primary.x)) // comment every line
					primary = paintedB; // comment every line

				if (cornerCell != primary) // comment every line
					rotSteps = (rotSteps + 2) % 4; // comment every line
			} // comment every line
		} // comment every line


		// 4) Prefab
		var prefab = Tileset.GetPrefab(shape);
		if (!prefab)
		{
			ClearAtKey(key);
			return;
		}

		// Convert rotation steps into Unity Y-rotation (degrees) // comment every line
		var finalSteps = (4 - rotSteps) % 4; // comment every line

		// Diagonal tiles have 180° visual asymmetry but 180° mask symmetry // comment every line
		// The canonical diagonal prefab is authored flipped relative to mask space // comment every line
		if (shape == DualGridTile.ShapeType.Diagonal) // comment every line
			// If rotation is EVEN (0° or 180°), correct handedness // comment every line
			if ((finalSteps & 1) == 0) // comment every line
				finalSteps = (finalSteps + 2) % 4; // flip by 180° // comment every line

		// Convert final rotation steps to degrees // comment every line
		var rotation = finalSteps * 90; // comment every line

		SpawnAtKey(key, prefab, worldPos, rotation, shape);
	}

	// =====================================================================
	// WORLD MASK (AXIS CORRECT)
	// =====================================================================

	private byte BuildWorldMask(Vector3Int cornerCell, Vector3 offset)
	{
		var rightSide = offset.x > 0 ? 1 : 0;
		var topSide = offset.z > 0 ? 1 : 0;

		var basisRight = _cellStepWorldX;
		var basisUp = _cellStepWorldZ;

		// Top-right of the 2x2 block
		var topRight =
			cornerCell +
			rightSide * basisRight +
			topSide * basisUp;

		var topLeft = topRight - basisRight;
		var botRight = topRight - basisUp;
		var botLeft = topRight - basisRight - basisUp;

		byte mask = 0;

		if (HasTile(botLeft)) mask |= 1; // Q00
		if (HasTile(botRight)) mask |= 2; // Q10
		if (HasTile(topLeft)) mask |= 4; // Q01
		if (HasTile(topRight)) mask |= 8; // Q11

		return mask;
	}

	// =====================================================================
	// KEYS (PURE CELL SPACE)
	// =====================================================================

	private Vector3Int MakeDualKey(Vector3Int cornerCell, Vector3 offset)
	{
		var sx = offset.x > 0 ? 1 : 0;
		var sy = offset.z > 0 ? 1 : 0;

		return new Vector3Int(
			cornerCell.x * 2 + sx,
			0,
			cornerCell.y * 2 + sy
		);
	}

	private bool HasTile(Vector3Int pos)
	{
		return MaskTilemap.GetTile(pos) != null;
	}

	// =====================================================================
	// SPAWNING / LIFETIME SAFETY
	// =====================================================================

	private void SpawnAtKey(
		Vector3Int key,
		GameObject prefab,
		Vector3 worldPos,
		int rotation,
		DualGridTile.ShapeType shape
	)
	{
		if (_spawned.TryGetValue(key, out var existing))
		{
			if (!existing || existing.transform.parent != Root)
			{
				_spawned.Remove(key);
			}
			else
			{
				var tile = existing.GetComponent<DualGridTile>();
				if (tile && tile.SpawnedShape == shape)
				{
					existing.transform.SetPositionAndRotation(
						worldPos,
						Quaternion.Euler(0, rotation, 0)
					);
					tile.Key = key;
					Rename(existing, key, worldPos, shape);
					return;
				}

				SafeDestroy(existing);
				_spawned.Remove(key);
			}
		}

		GameObject obj;

#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			obj = (GameObject) UnityEditor.PrefabUtility.InstantiatePrefab(prefab, Root);
			obj.transform.SetPositionAndRotation(
				worldPos,
				Quaternion.Euler(0, rotation, 0)
			);
			UnityEditor.Undo.RegisterCreatedObjectUndo(obj, "Spawn DualGrid Tile");
		}
		else
#endif
		{
			obj = Instantiate(
				prefab,
				worldPos,
				Quaternion.Euler(0, rotation, 0),
				Root
			);
		}


		var dgTile = obj.GetComponent<DualGridTile>();
		if (!dgTile)
			dgTile = obj.AddComponent<DualGridTile>();

		dgTile.Key = key;
		dgTile.SpawnedShape = shape;

		Rename(obj, key, worldPos, shape);
		_spawned[key] = obj;
	}

	private void Rename(GameObject obj, Vector3Int key, Vector3 worldPos, DualGridTile.ShapeType shape)
	{
		var wx = worldPos.x.ToString("F2");
		var wz = worldPos.z.ToString("F2");

		obj.name = $"{shape} [K {key.x},{key.z}] [W {wx},{wz}]";
	}

	private void ClearAtKey(Vector3Int key)
	{
		if (_spawned.TryGetValue(key, out var obj))
		{
			SafeDestroy(obj);
			_spawned.Remove(key);
		}
	}

	// ... inside DualGridSpawner class ...

#if UNITY_EDITOR

	// Tracks pending destroy requests as InstanceIDs (safe across domain/editor churn).
	private static readonly Queue<int>
		EditorDestroyQueue = new Queue<int>(); // Stores GameObject instance IDs to destroy later.

	// Ensures we only hook EditorApplication.update once.
	private static bool _editorDestroyHooked = false; // True once we've subscribed to the editor update pump.

#endif

	private void SafeDestroy(GameObject obj)
	{
		// If the reference is already null, there is nothing to destroy.
		if (!obj)
			return;

#if UNITY_EDITOR

		// If we are actually in play mode, use normal runtime destruction.
		if (Application.isPlaying)
		{
			// Runtime-safe destroy (deferred until end of frame).
			Destroy(obj);
			return;
		}

		// Convert the object to an instance ID so we don't capture a fragile reference in a lambda.
		int id = obj.GetInstanceID(); // Unique identifier for this UnityEngine.Object instance.

		// Queue this object for destruction during a safe editor update moment.
		EditorDestroyQueue.Enqueue(id); // Adds the instance ID to the pending destroy list.

		// Hook the editor update loop once so we can drain the queue safely.
		if (!_editorDestroyHooked) // Only subscribe a single time.
		{
			_editorDestroyHooked = true; // Mark as hooked so we don't double-subscribe.
			UnityEditor.EditorApplication.update += DrainEditorDestroyQueue; // Drain the queue every editor update.
		}

#else

	// Non-editor builds only need runtime destruction.
	Destroy(obj); // Deferred runtime destroy.

#endif
	}

#if UNITY_EDITOR
	private static void DrainEditorDestroyQueue()
	{
		// Never destroy objects while Unity is transitioning into/out of play mode.
		// This avoids inspector / serialization churn while Unity is rebinding objects.
		if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
			return;

		// If there's nothing to destroy, unhook and exit to avoid per-frame overhead.
		if (EditorDestroyQueue.Count == 0)
		{
			// Unsubscribe from the editor update loop since the queue is empty.
			UnityEditor.EditorApplication.update -= DrainEditorDestroyQueue;

			// Mark as unhooked so future SafeDestroy calls can re-hook.
			_editorDestroyHooked = false;

			// Exit early since there is no work to perform.
			return;
		}

		// Drain all pending destroy requests this tick so the scene stays in sync immediately.
		while (EditorDestroyQueue.Count > 0)
		{
			// Pull the next instance ID to destroy.
			int id = EditorDestroyQueue.Dequeue();

			// Resolve the instance ID back into a Unity object (may be null if already destroyed).
			UnityEngine.Object unityObj = UnityEditor.EditorUtility.InstanceIDToObject(id);

			// If the object no longer exists (already destroyed / unloaded), skip it.
			if (!unityObj)
				continue;

			// We only expect GameObjects here, but we cast defensively.
			GameObject go = unityObj as GameObject;

			// If this isn’t a GameObject, skip it (unexpected input).
			if (!go)
				continue;

			// If Unity has already marked it for destruction, skip it.
			if (!go)
				continue;

			// If the object (or a child) is selected, deselect it to prevent inspector binding errors.
			// This prevents the inspector from trying to draw a missing target mid-destroy.
			if (UnityEditor.Selection.activeObject == go)
				UnityEditor.Selection.activeObject = null;

			// Destroy using Undo so the user can Ctrl+Z in edit mode.
			// This also properly records prefab instance removals.
			UnityEditor.Undo.DestroyObjectImmediate(go);
		}
	}
#endif
}
