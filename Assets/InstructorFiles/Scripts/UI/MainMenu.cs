using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     Main menu entry point.
///     Supports New Game and Continue via LevelMgr.
/// </summary>
public class MainMenu : MenuBase
{
	private void OnEnable()
	{
		UpdateButtonStates();
		//_newGameButton?.Select();
	}

	public override GameMenus MenuType()
	{
		return GameMenus.MainMenu;
	}

#region Button State

	private void UpdateButtonStates()
	{
		if (_continueButton == null)
			return;

		// Continue is valid only if save exists and level data is valid
		var hasSave =
			LevelMgr.Instance.HasValidLevelData &&
			SaveUtil.SavedValues.HighestLevelCompleted >= 0;

		_continueButton.interactable = hasSave;
	}

#endregion

#region Serialized Fields

	[SerializeField] private Button _newGameButton;
	[SerializeField] private Button _continueButton;
	[SerializeField] private Button _levelSelectButton;
	[SerializeField] private Button _settingsButton;
	[SerializeField] private Button _quitButton;

#endregion

#region Button Callbacks

	public void ButtonNewGame()
	{
		if (!Interactable) return;
		Interactable = false;

		SaveUtil.DeleteSaveData();
		SaveUtil.Load();

		var levelIndex = LevelMgr.Instance.GetNewGameLevelIndex();
		LevelMgr.Instance.LoadLevel(levelIndex);
	}

	public void ButtonContinue()
	{
		if (!Interactable) return;
		Interactable = false;

		var levelIndex = LevelMgr.Instance.GetContinueLevelIndex();
		LevelMgr.Instance.LoadLevel(levelIndex);
	}

	public void ButtonLevelSelect()
	{
		if (!Interactable) return;
		Interactable = false;

		UIMgr.Instance.ShowMenu(GameMenus.LevelSelectMenu, () => Interactable = true);
	}

	public void ButtonSettings()
	{
		if (!Interactable) return;
		Interactable = false;

		UIMgr.Instance.ShowMenu(GameMenus.SettingsMenu, () => Interactable = true);
	}

	public void ButtonQuit()
	{
		if (!Interactable) return;
		Interactable = false;

#if UNITY_EDITOR
		EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}

#endregion
}
