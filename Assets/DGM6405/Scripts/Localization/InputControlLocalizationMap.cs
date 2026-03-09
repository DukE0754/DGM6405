using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;

[CreateAssetMenu(
	fileName = "InputControlLocalizationMap",
	menuName = "Scriptable Objects/Localization/Input Control Localization Map"
)]
public class InputControlLocalizationMap : ScriptableObject
{
#region Serialized Fields

	[SerializeField]
	private List<Rule> _rules = new();

#endregion

#region Public API

	/// <summary>
	///     Resolve localized binding text for the given action
	/// </summary>
	public string Resolve(InputAction action, PlayerInput playerInput)
	{
		if (playerInput == null)
			return action.GetBindingDisplayString();

		var scheme = playerInput.currentControlScheme;

		foreach (var binding in action.bindings)
			if (binding.isComposite)
			{
				foreach (var rule in _rules)
				{
					if (!rule.Composite)
						continue;

					if (action.name.ToLower().Contains(rule.PathContains.ToLower()))
						return ResolveForScheme(rule, scheme);
				}
			}
			else
			{
				//var path = binding.effectivePath.ToLower();

				foreach (var rule in _rules)
				{
					if (rule.Composite)
						continue;

					if (action.name.ToLower().Contains(rule.PathContains.ToLower()))
						return ResolveForScheme(rule, scheme);
				}
			}

		Debug.LogWarning($"No localization key found for action {action.name}");
		return action.GetBindingDisplayString();
	}

#endregion

#region Data Structures

	[Serializable]
	public struct ControlSchemeLocalization
	{
		/// <summary>
		///     Name of the control scheme (must match Input Actions asset)
		///     Example: "KeyboardMouse", "Gamepad"
		/// </summary>
		public string ControlScheme;

		/// <summary>
		///     Localization key used for this scheme
		/// </summary>
		public string LocalizationKey;
	}

	[Serializable]
	public struct Rule
	{
		/// <summary>
		///     Path or composite name match
		/// </summary>
		public string PathContains;

		/// <summary>
		///     True if this rule is for a composite binding
		/// </summary>
		public bool Composite;

		/// <summary>
		///     Localization keys per control scheme
		/// </summary>
		public List<ControlSchemeLocalization> SchemeKeys;
	}

#endregion

#region Private Methods

	private string ResolveForScheme(Rule rule, string scheme)
	{
		foreach (var schemeEntry in rule.SchemeKeys)
			if (schemeEntry.ControlScheme == scheme)
				return Localize(schemeEntry.LocalizationKey);

		Debug.LogWarning($"No localization key for control scheme {scheme} in {rule.PathContains}");
		return "";
	}

	private string Localize(string key)
	{
		return LocalizationSettings.StringDatabase.GetLocalizedString("Input", key);
	}

#endregion
}
