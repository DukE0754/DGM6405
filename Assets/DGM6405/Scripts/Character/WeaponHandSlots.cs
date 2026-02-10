using System;
using UnityEngine;

/// <summary>
///     Manages weapon hand slot GameObjects for character animations.
///     Handles showing/hiding different weapon types (shield, melee, ranged).
/// </summary>
public class WeaponHandSlots : MonoBehaviour, IWeaponSlotListener
{
	/// <summary>
	///     Enum representing different weapon slot types.
	/// </summary>
	public enum WeaponSlotType
	{
		None = -1,
		Shield = 0,
		Melee = 1,
		Ranged = 2
	}

	[Header("Weapon Slots")]
	[Tooltip("Array of weapon slot GameObjects. Index corresponds to WeaponSlotType enum.")]
	[SerializeField] private GameObject[] _slots;

	private void OnValidate()
	{
		// Warn if slots array is empty
		if (_slots == null || _slots.Length == 0)
			Debug.LogWarning(
				$"[{name}] WeaponHandSlots: _slots array is empty. No weapon slots will be available.",
				this
			);
		else
			// Warn about null entries in array
			for (var i = 0; i < _slots.Length; i++)
				if (_slots[i] == null)
					Debug.LogWarning(
						$"[{name}] WeaponHandSlots: Slot at index {i} is null. Assign in inspector.",
						this
					);
	}

	private void Awake()
	{
		SetActiveSlot(WeaponSlotType.None);
	}

	public void OnWeaponSlotChanged(WeaponSlotType slotType)
	{
		SetActiveSlot(slotType);
	}

	/// <summary>
	///     Sets the active weapon slot, hiding all others.
	/// </summary>
	/// <param name="slotType">The weapon slot type to activate. Use None to hide all slots.</param>
	public void SetActiveSlot(WeaponSlotType slotType)
	{
		// Validate slots array exists
		if (_slots == null)
		{
			Debug.LogWarning($"[{name}] WeaponHandSlots: _slots array is null. Cannot set active slot.", this);
			return;
		}

		// Validate slot index
		var slotIndex = (int) slotType;
		if (slotIndex < -1 || slotIndex >= _slots.Length)
		{
			Debug.LogWarning(
				$"[{name}] WeaponHandSlots: Invalid slot index {slotIndex} for slot type {slotType}. " +
				$"Array length is {_slots.Length}.",
				this
			);
			return;
		}

		// Activate/deactivate slots
		for (var i = 0; i < _slots.Length; i++)
			// Null check for each slot
			if (_slots[i] != null)
				_slots[i].SetActive(i == slotIndex);
			else if (i == slotIndex)
				Debug.LogWarning(
					$"[{name}] WeaponHandSlots: Slot at index {i} ({slotType}) is null. Cannot activate.",
					this
				);
	}

	/// <summary>
	///     Gets the GameObject for a specific weapon slot type.
	/// </summary>
	/// <param name="slotType">The weapon slot type to retrieve.</param>
	/// <returns>The GameObject for the slot, or null if invalid or not found.</returns>
	public GameObject GetSlot(WeaponSlotType slotType)
	{
		var slotIndex = (int) slotType;
		if (_slots == null || slotIndex < 0 || slotIndex >= _slots.Length) return null;
		return _slots[slotIndex];
	}
}
