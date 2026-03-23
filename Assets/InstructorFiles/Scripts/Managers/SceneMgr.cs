using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
///     Manages the current scene and scene transitions
///     Ensures the UI clears and opens to the correct menu after loading is complete
/// </summary>
public class SceneMgr : Singleton<SceneMgr>
{
	public void LoadScene(GameScenes sceneToLoad, GameMenus menuToOpen, Action onComplete = null)
	{
		GlobalContext.Instance.GameMgr.GameState = GameMgr.GameStates.Loading;
		StartCoroutine(PerformLoadSequence(sceneToLoad.ToString(), menuToOpen, onComplete));
	}

	public void LoadScene(string sceneToLoad, GameMenus menuToOpen, Action onComplete = null)
	{
		GlobalContext.Instance.GameMgr.GameState = GameMgr.GameStates.Loading;
		StartCoroutine(PerformLoadSequence(sceneToLoad, menuToOpen, onComplete));
	}

	private IEnumerator PerformLoadSequence(string sceneToLoad, GameMenus menuToOpen, Action onComplete)
	{
		var waiting = true;

		GlobalEventBus.Instance.Raise<IUIEventListener>(l => l.OnCloseAllMenus());

		GlobalEventBus.Instance.Raise<IUIEventListener>(l => l.OnShowMenu(GameMenus.Fader, () => waiting = false));

		yield return new WaitWhile(() => waiting);

		var asyncOperation = SceneManager.LoadSceneAsync(sceneToLoad);

		if (asyncOperation == null)
		{
			Debug.LogError($"SceneMgr: Failed to start loading scene '{sceneToLoad}'. Is it added to Build Settings?");
			yield break;
		}

		while (!asyncOperation.isDone) yield return null;

		UpdateMusicForScene(sceneToLoad);

		GlobalEventBus.Instance.Raise<IUIEventListener>(l => l.OnHideMenu(GameMenus.Fader));

		GlobalEventBus.Instance.Raise<IUIEventListener>(l => l.OnShowMenu(menuToOpen, () => onComplete?.Invoke()));
	}

	private void UpdateMusicForScene(string sceneToLoad)
	{
		if (AudioMgr.Instance == null)
			return;

		switch (sceneToLoad)
		{
			case nameof(GameScenes.Gameplay):
				AudioMgr.Instance.PlayMusic(AudioMgr.MusicTypes.Gameplay, 1f);
				break;
			case nameof(GameScenes.MainMenu):
			case nameof(GameScenes.GameOver):
				AudioMgr.Instance.PlayMusic(AudioMgr.MusicTypes.MainMenu, 1f);
				break;
		}
	}
}
