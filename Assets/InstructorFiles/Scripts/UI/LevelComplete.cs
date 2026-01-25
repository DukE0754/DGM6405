/// <summary>
/// Game over screen
/// Allows for quitting or retrying
/// </summary>
public class LevelComplete : MenuBase
{
	public override GameMenus MenuType()
	{
		return GameMenus.LevelCompleteMenu;
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
