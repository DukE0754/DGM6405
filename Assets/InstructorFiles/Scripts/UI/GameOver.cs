/// <summary>
///     Game over screen
///     Allows for quitting or retrying
/// </summary>
public class GameOver : MenuBase
{
	public override GameMenus MenuType()
	{
		return GameMenus.GameOverMenu;
	}

	public void ButtonRetry()
	{
		if (!Interactable) return;
		Interactable = false;
		LevelMgr.Instance.ReloadCurrentLevel();
	}

	public void ButtonMainMenu()
	{
		if (!Interactable) return;
		Interactable = false;
		SceneMgr.Instance.LoadScene(
			GameScenes.MainMenu, GameMenus.MainMenu, () => GameMgr.Instance.GameState = GameMgr.GameStates.Menu);
	}
}
