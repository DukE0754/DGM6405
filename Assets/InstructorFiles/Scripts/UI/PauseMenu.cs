using UnityEngine;

public class PauseMenu : MenuBase
{
	public override GameMenus MenuType()
	{
		return GameMenus.PauseMenu;
	}
	
	private void OnEnable()
	{
		//Time.timeScale = 0f;
		GameMgr.Instance.GameState = GameMgr.GameStates.Paused;
	}

	private void OnDisable()
	{
		//Time.timeScale = 1f;
		if (GameMgr.Instance.GameState == GameMgr.GameStates.Paused)
			GameMgr.Instance.GameState = GameMgr.GameStates.InGame;
	}
	
	public void ButtonResume()
	{
		if (!Interactable) return;
		Interactable = false;
		UIMgr.Instance.HideMenu(GameMenus.PauseMenu);
	}

	public void ButtonRestart()
	{
		if (!Interactable) return;
		Interactable = false;
		SceneMgr.Instance.LoadScene(GameScenes.Gameplay, GameMenus.InGameUI, GameMgr.Instance.StartGame);
	}
	
	public void ButtonQuit()
	{
		if (!Interactable) return;
		Interactable = false;
		SceneMgr.Instance.LoadScene(GameScenes.MainMenu, GameMenus.MainMenu, () => GameMgr.Instance.GameState = GameMgr.GameStates.Menu);
	}

	public void ButtonGameOver()
	{
		if (!Interactable) return;
		Interactable = false;
		GameMgr.Instance.GameOver();
	}
}
