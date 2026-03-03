using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

/// <summary>
///     View for a single level entry in the level select grid.
/// </summary>
public class LevelSelectGridItem : MonoBehaviour
{
#region Public API

	public void Bind(
		int levelIndex,
		string levelNameKey,
		int parTimeMs,
		int bestTimeMs,
		bool isUnlocked,
		bool isCompleted,
		Action<int> onSelected)
	{
		_levelIndex = levelIndex;
		_onSelected = onSelected;

		if (_levelNumberText != null)
			_levelNumberText.text = $"Level {levelIndex + 1}";

		BindLevelName(levelIndex, levelNameKey);

		if (_parTimeText != null)
			_parTimeText.text = "Par: " + (parTimeMs > 0 ? FormatTime(parTimeMs) : "--:--");

		if (_bestTimeText != null)
		{
			if (!isUnlocked)
				_bestTimeText.text = "Locked";
			else if (bestTimeMs > 0)
				_bestTimeText.text = "Best: " + FormatTime(bestTimeMs);
			else
				_bestTimeText.text = isCompleted ? "No Time" : "--:--";
		}

		if (_lockedRoot != null)
			_lockedRoot.SetActive(!isUnlocked);

		if (_button != null)
		{
			_button.onClick.RemoveAllListeners();
			_button.interactable = isUnlocked;
			if (isUnlocked)
				_button.onClick.AddListener(OnClick);
		}
	}

#endregion

#region Serialized Fields

	[SerializeField] private Button _button;
	[SerializeField] private TMP_Text _levelNumberText;
	[SerializeField] private TMP_Text _levelNameText;
	[SerializeField] private TMP_Text _parTimeText;
	[SerializeField] private TMP_Text _bestTimeText;
	[SerializeField] private GameObject _lockedRoot;

#endregion

#region Private Fields

	private int _levelIndex;
	private Action<int> _onSelected;
	private LocalizedString _localizedLevelName;

#endregion

#region Localization

	private async void BindLevelName(int levelIndex, string levelNameKey)
	{
		if (_levelNameText == null)
			return;

		if (string.IsNullOrWhiteSpace(levelNameKey))
		{
			_levelNameText.text = $"Stage {levelIndex + 1}";
			return;
		}

		await LocalizationSettings.InitializationOperation.Task;

		_localizedLevelName = new LocalizedString
		{
			TableReference = "UI",
			TableEntryReference = levelNameKey
		};

		_localizedLevelName.StringChanged += OnLevelNameChanged;
		_localizedLevelName.RefreshString();
	}

	private void OnLevelNameChanged(string value)
	{
		if (_levelNameText != null)
			_levelNameText.text = value;
	}
	
	private void OnDestroy()
	{
		if (_localizedLevelName != null)
			_localizedLevelName.StringChanged -= OnLevelNameChanged;
	}

#endregion

#region Private Methods

	private void OnClick()
	{
		_onSelected?.Invoke(_levelIndex);
	}

	private static string FormatTime(int timeMs)
	{
		if (timeMs <= 0)
			return "--:--";

		var totalSeconds = timeMs / 1000f;
		var minutes = Mathf.FloorToInt(totalSeconds / 60f);
		var seconds = Mathf.FloorToInt(totalSeconds % 60f);
		var millis = Mathf.FloorToInt(timeMs % 1000f);
		return $"{minutes:00}:{seconds:00}.{millis:000}";
	}

#endregion
}
