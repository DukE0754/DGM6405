using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     Controller for the level select menu (MVC: menu=controller, grid item=view, data=model).
/// </summary>
public class LevelSelectMenu : MenuBase
{
	private void OnEnable()
	{
		if (_itemsPerPage < 1)
			_itemsPerPage = 1;
		EnsureSaveLoaded();
		BuildModel();
		ShowPage(0);
		UpdatePagingButtons();
		_backButton?.Select();
	}

	private void OnDisable()
	{
		Interactable = false;
	}

	public override GameMenus MenuType()
	{
		return GameMenus.LevelSelectMenu;
	}

	private void EnsureSaveLoaded()
	{
		if (SaveUtil.SavedValues == null)
			SaveUtil.Load();
	}

	private void BuildModel()
	{
		_entries.Clear();

		if (!LevelMgr.Instance.HasValidLevelData)
			return;

		var levels = LevelMgr.Instance.LevelData.Levels;
		var bestTimes = SaveUtil.SavedValues.BestTimeMs;

		for (var i = 0; i < levels.Length; i++)
		{
			var isUnlocked = LevelMgr.Instance.IsLevelUnlocked(i);
			var bestTime = bestTimes != null && i < bestTimes.Length ? bestTimes[i] : 0;
			_entries.Add(
				new LevelSelectEntry
				{
					Index = i,
					LevelName = levels[i].LevelName,
					ParTimeMs = levels[i].ParTimeMs,
					BestTimeMs = bestTime,
					IsUnlocked = isUnlocked,
					IsCompleted = SaveUtil.SavedValues.HighestLevelCompleted >= i
				});
		}
	}

	private void EnsureGridItemPool()
	{
		if (_gridItemPrefab == null || _gridParent == null)
			return;

		while (_gridItems.Count < _itemsPerPage)
		{
			var item = Instantiate(_gridItemPrefab, _gridParent);
			item.gameObject.SetActive(false);
			_gridItems.Add(item);
		}
	}

	private void ShowPage(int pageIndex)
	{
		EnsureGridItemPool();

		if (_entries.Count == 0)
		{
			for (var i = 0; i < _gridItems.Count; i++)
				_gridItems[i].gameObject.SetActive(false);
			UpdatePageLabel(0, 0);
			return;
		}

		var totalPages = Mathf.CeilToInt(_entries.Count / (float) _itemsPerPage);
		_currentPageIndex = Mathf.Clamp(pageIndex, 0, Mathf.Max(0, totalPages - 1));

		for (var i = 0; i < _gridItems.Count; i++)
		{
			var entryIndex = _currentPageIndex * _itemsPerPage + i;
			if (entryIndex < _entries.Count)
			{
				var entry = _entries[entryIndex];
				_gridItems[i].gameObject.SetActive(true);
				_gridItems[i].Bind(
					entry.Index,
					entry.LevelName,
					entry.ParTimeMs,
					entry.BestTimeMs,
					entry.IsUnlocked,
					entry.IsCompleted,
					OnLevelSelected);
			}
			else
			{
				_gridItems[i].gameObject.SetActive(false);
			}
		}

		UpdatePageLabel(_currentPageIndex + 1, totalPages);
	}

	private void UpdatePageLabel(int page, int totalPages)
	{
		if (_pageLabel != null)
			_pageLabel.text = totalPages <= 0 ? "0/0" : $"{page}/{totalPages}";
	}

	private void UpdatePagingButtons()
	{
		if (_entries.Count == 0)
		{
			if (_previousPageButton != null) _previousPageButton.interactable = false;
			if (_nextPageButton != null) _nextPageButton.interactable = false;
			return;
		}

		var totalPages = Mathf.CeilToInt(_entries.Count / (float) _itemsPerPage);
		if (_previousPageButton != null)
			_previousPageButton.interactable = _currentPageIndex > 0;
		if (_nextPageButton != null)
			_nextPageButton.interactable = _currentPageIndex < totalPages - 1;
	}

	private void OnLevelSelected(int levelIndex)
	{
		if (!Interactable) return;
		Interactable = false;
		LevelMgr.Instance.LoadLevel(levelIndex);
	}

#region Serialized Fields

	[Header("Grid")]
	[SerializeField] private LevelSelectGridItem _gridItemPrefab;

	[SerializeField] private Transform _gridParent;
	[SerializeField] private int _itemsPerPage = 9;

	[Header("Paging")]
	[SerializeField] private Button _previousPageButton;

	[SerializeField] private Button _nextPageButton;
	[SerializeField] private TMP_Text _pageLabel;

	[Header("Navigation")]
	[SerializeField] private Button _backButton;

#endregion

#region Model

	private class LevelSelectEntry
	{
		public int BestTimeMs;
		public int Index;
		public bool IsCompleted;
		public bool IsUnlocked;
		public string LevelName;
		public int ParTimeMs;
	}

	private readonly List<LevelSelectEntry> _entries = new();

#endregion

#region View State

	private readonly List<LevelSelectGridItem> _gridItems = new();
	private int _currentPageIndex;

#endregion

#region Button Callbacks

	public void ButtonPreviousPage()
	{
		if (!Interactable) return;
		ShowPage(_currentPageIndex - 1);
		UpdatePagingButtons();
	}

	public void ButtonNextPage()
	{
		if (!Interactable) return;
		ShowPage(_currentPageIndex + 1);
		UpdatePagingButtons();
	}

	public void ButtonBack()
	{
		if (!Interactable) return;
		Interactable = false;
		UIMgr.Instance.HideMenu(GameMenus.LevelSelectMenu);
	}

#endregion
}
