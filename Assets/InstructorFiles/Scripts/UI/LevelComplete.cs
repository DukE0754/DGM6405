using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Game over screen
/// Allows for quitting or retrying
/// </summary>
public class LevelComplete : MenuBase
{
	[Header("Stats")]
	[SerializeField] private TMP_Text _currentTimeText;
	[SerializeField] private TMP_Text _bestTimeText;
	[SerializeField] private TMP_Text _parTimeText;

	public override GameMenus MenuType()
	{
		return GameMenus.LevelCompleteMenu;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		UpdateStats();
	}

	private void UpdateStats()
	{
		if (LevelMgr.Instance.TryGetCurrentLevelInfo(out var info))
		{
			// Get best time from SaveUtil
			int bestTimeMs = SaveUtil.SavedValues.BestTimeMs[LevelMgr.Instance.CurrentLevelIndex];
			
			// Par time from LevelData
			int parTimeMs = info.ParTimeMs;

			// Current time - this is tricky because we already transited scenes
			_currentTimeText.text = FormatTime(GameMgr.Instance.LastRunTimeMs);
			_bestTimeText.text = FormatTime(bestTimeMs);
			_parTimeText.text = FormatTime(parTimeMs);
		}
		else
		{
			_currentTimeText.text = "--:--.--";
			_bestTimeText.text = "--:--.--";
			_parTimeText.text = "--:--.--";
		}
	}

	private string FormatTime(int ms)
	{
		if (ms <= 0) return "--:--.--";
		var time = TimeSpan.FromMilliseconds(ms);
		return time.ToString(@"mm\:ss\.ff");
	}

	public void ButtonNextLevel()
	{
		if (!Interactable) return;
		Interactable = false;
		LevelMgr.Instance.LoadNextLevelOrFinish();
	}

	public void ButtonMainMenu()
	{
		if (!Interactable) return;
		Interactable = false;
		SceneMgr.Instance.LoadScene(GameScenes.MainMenu, GameMenus.MainMenu, () => GameMgr.Instance.GameState = GameMgr.GameStates.Menu);
	}
}
