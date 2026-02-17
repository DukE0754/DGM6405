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

		var chapters = LevelMgr.Instance.LevelData.Chapters;
		var bestTimes = SaveUtil.SavedValues.BestTimeMs;

		var absoluteIndex = 0;
		for (var c = 0; c < chapters.Length; c++)
		{
			var chapter = chapters[c];
			if (chapter.Levels == null) continue;

			for (var l = 0; l < chapter.Levels.Length; l++)
			{
				var level = chapter.Levels[l];
				var isUnlocked = LevelMgr.Instance.IsLevelUnlocked(absoluteIndex);
				var bestTime = bestTimes != null && absoluteIndex < bestTimes.Length ? bestTimes[absoluteIndex] : 0;
				_entries.Add(
					new LevelSelectEntry
					{
						ChapterIndex = c,
						ChapterName = chapter.ChapterName,
						Index = absoluteIndex,
						LevelName = level.LevelName,
						ParTimeMs = level.ParTimeMs,
						BestTimeMs = bestTime,
						IsUnlocked = isUnlocked,
						IsCompleted = SaveUtil.SavedValues.HighestLevelCompleted >= absoluteIndex
					});
				absoluteIndex++;
			}
		}
	}

	private void EnsureGridItemPool()
	{
		if (_gridItemPrefab == null || _gridParent == null)
			return;

		// Find the max number of items in any chapter to ensure we have enough pool
		var maxLevelsInChapter = 0;
		if (LevelMgr.Instance.HasValidLevelData)
		{
			foreach (var chapter in LevelMgr.Instance.LevelData.Chapters)
			{
				if (chapter.Levels != null && chapter.Levels.Length > maxLevelsInChapter)
					maxLevelsInChapter = chapter.Levels.Length;
			}
		}

		while (_gridItems.Count < maxLevelsInChapter)
		{
			var item = Instantiate(_gridItemPrefab, _gridParent);
			item.gameObject.SetActive(false);
			_gridItems.Add(item);
		}
	}

	private void ShowPage(int chapterIndex)
	{
		EnsureGridItemPool();

		if (_entries.Count == 0)
		{
			foreach (var item in _gridItems)
				item.gameObject.SetActive(false);
			UpdatePageLabel(0, 0);
			return;
		}

		var chaptersCount = LevelMgr.Instance.LevelData.Chapters.Length;
		_currentPageIndex = Mathf.Clamp(chapterIndex, 0, Mathf.Max(0, chaptersCount - 1));

		var chapterEntries = _entries.FindAll(e => e.ChapterIndex == _currentPageIndex);
		var chapterName = LevelMgr.Instance.LevelData.Chapters[_currentPageIndex].ChapterName;

		for (var i = 0; i < _gridItems.Count; i++)
		{
			if (i < chapterEntries.Count)
			{
				var entry = chapterEntries[i];
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

		UpdatePageLabel(_currentPageIndex + 1, chaptersCount, chapterName);
	}

	private void UpdatePageLabel(int page, int totalPages, string chapterName = "")
	{
		if (_pageLabel != null)
			_pageLabel.text = totalPages <= 0 ? "0/0" : $"{chapterName} ({page}/{totalPages})";
	}

	private void UpdatePagingButtons()
	{
		if (!LevelMgr.Instance.HasValidLevelData)
		{
			if (_previousPageButton != null) _previousPageButton.interactable = false;
			if (_nextPageButton != null) _nextPageButton.interactable = false;
			return;
		}

		var totalPages = LevelMgr.Instance.LevelData.Chapters.Length;
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
		public int ChapterIndex;
		public string ChapterName;
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
		GlobalEventBus.Instance.Raise<IUIEventListener>(l => l.OnHideMenu(GameMenus.LevelSelectMenu));
	}

#endregion
}
