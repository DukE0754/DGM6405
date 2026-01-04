using UnityEngine;
using UnityEngine.Serialization;
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
        SceneMgr.Instance.LoadScene(GameScenes.Gameplay, GameMenus.InGameUI);
    }

    public void ButtonSettings()
    {
        UIMgr.Instance.ShowMenu(GameMenus.SettingsMenu);
    }

    public void ButtonQuit()
    {
        Application.Quit();
    }
}
