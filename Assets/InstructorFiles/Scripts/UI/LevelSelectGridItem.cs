using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// View for a single level entry in the level select grid.
/// </summary>
public class LevelSelectGridItem : MonoBehaviour
{
	[SerializeField] private Button _button;
	[SerializeField] private TMP_Text _levelNumberText;
	[SerializeField] private TMP_Text _levelNameText;
	[SerializeField] private TMP_Text _parTimeText;
	[SerializeField] private TMP_Text _bestTimeText;
	[SerializeField] private GameObject _lockedRoot;

	private int _levelIndex;
	private Action<int> _onSelected;

	public void Bind(
		int levelIndex,
		string levelName,
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

		if (_levelNameText != null)
			_levelNameText.text = string.IsNullOrWhiteSpace(levelName) ? $"Stage {levelIndex + 1}" : levelName;

		if (_parTimeText != null)
			_parTimeText.text = $"Par: " + (parTimeMs > 0 ? FormatTime(parTimeMs) : "--:--");

		if (_bestTimeText != null)
		{
			if (!isUnlocked)
				_bestTimeText.text = "Locked";
			else if (bestTimeMs > 0)
				_bestTimeText.text = $"Best: " + FormatTime(bestTimeMs);
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
}
