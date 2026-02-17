public class PauseMenu : MenuBase
{
	private void OnEnable()
	{
		GameMgr.Instance?.SetPaused(true);
	}

	private void OnDisable()
	{
		GameMgr.Instance?.SetPaused(false);
	}

	public override GameMenus MenuType()
	{
		return GameMenus.PauseMenu;
	}

	public void ButtonResume()
	{
		if (!Interactable) return;
		Interactable = false;
		GameMgr.Instance.SetPaused(false);
	}

	public void ButtonRestart()
	{
		if (!Interactable) return;
		Interactable = false;
		LevelMgr.Instance.ReloadCurrentLevel();
	}

	public void ButtonSettings()
	{
		if (!Interactable) return;
		Interactable = false;
		GlobalEventBus.Instance.Raise<IUIEventListener>(l => l.OnShowMenu(
			GameMenus.SettingsMenu, () => Interactable = true));
	}

	public void ButtonQuit()
	{
		if (!Interactable) return;
		Interactable = false;
		SceneMgr.Instance.LoadScene(
			GameScenes.MainMenu, GameMenus.MainMenu,
			() => GameMgr.Instance.GameState = GameMgr.GameStates.Menu);
	}

	public void ButtonGameOver()
	{
		if (!Interactable) return;
		Interactable = false;
		GameMgr.Instance.GameOver();
	}
}
