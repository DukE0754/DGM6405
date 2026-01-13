using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The main menu when starting the game
/// The simple entry point after the game loads and return point if exiting gameplay
/// </summary>
public class MainMenu : MenuBase
{
    [SerializeField] private Button _startButton;
    
    public override GameMenus MenuType()
    {
        return GameMenus.MainMenu;
    }

    private void OnEnable()
    {
        _startButton.Select();
    }

    public void ButtonStart()
	{
		if (!Interactable) return;
		Interactable = false;
		SceneMgr.Instance.LoadScene(GameScenes.Gameplay, GameMenus.InGameUI, GameMgr.Instance.StartGame);
    }

    public void ButtonSettings()
    {
		if (!Interactable) return;
		Interactable = false;
        UIMgr.Instance.ShowMenu(GameMenus.SettingsMenu);
    }

    public void ButtonQuit()
    {
		if (!Interactable) return;
		Interactable = false;
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
