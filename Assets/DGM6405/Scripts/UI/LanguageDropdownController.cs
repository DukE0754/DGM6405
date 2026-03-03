using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

/// <summary>
///     Automatically populates a TMP dropdown with available locales.
///     Selecting a locale updates the game's language.
/// </summary>
public class LanguageDropdownController : MonoBehaviour
{
#region Serialized Fields

	[SerializeField] private TMP_Dropdown _dropdown;

#endregion

#region Unity Lifecycle

	private async void Start()
	{
		await LocalizationSettings.InitializationOperation.Task;

		PopulateDropdown();
		SetDropdownToCurrentLocale();

		_dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
	}

	private void OnDestroy()
	{
		_dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
	}

#endregion

#region Private Methods

	private void PopulateDropdown()
	{
		_dropdown.ClearOptions();

		var options = new List<string>();
		var locales = LocalizationSettings.AvailableLocales.Locales;

		foreach (var locale in locales)
		{
			// Native language name (best practice)
			var nativeName = locale.Identifier.CultureInfo.NativeName;
			options.Add(nativeName);
		}

		_dropdown.AddOptions(options);
	}

	private void SetDropdownToCurrentLocale()
	{
		var locales = LocalizationSettings.AvailableLocales.Locales;
		var currentLocale = LocalizationSettings.SelectedLocale;

		for (var i = 0; i < locales.Count; i++)
			if (locales[i] == currentLocale)
			{
				_dropdown.SetValueWithoutNotify(i);
				return;
			}
	}

	private void OnDropdownValueChanged(int index)
	{
		var locales = LocalizationSettings.AvailableLocales.Locales;

		if (index < 0 || index >= locales.Count)
			return;

		LocalizationSettings.SelectedLocale = locales[index];
	}

#endregion
}
