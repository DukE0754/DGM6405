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
		GameMgr.Instance.GameState = GameMgr.GameStates.Loading;
		StartCoroutine(PerformLoadSequence(sceneToLoad.ToString(), menuToOpen, onComplete));
	}

	public void LoadScene(string sceneToLoad, GameMenus menuToOpen, Action onComplete = null)
	{
		GameMgr.Instance.GameState = GameMgr.GameStates.Loading;
		StartCoroutine(PerformLoadSequence(sceneToLoad, menuToOpen, onComplete));
	}

	private IEnumerator PerformLoadSequence(string sceneToLoad, GameMenus menuToOpen, Action onComplete)
	{
		var waiting = true;

		UIMgr.Instance.CloseAllMenus();

		UIMgr.Instance.ShowMenu(GameMenus.Fader, () => waiting = false);

		yield return new WaitWhile(() => waiting);

		var asyncOperation = SceneManager.LoadSceneAsync(sceneToLoad);

		if (asyncOperation == null)
		{
			Debug.LogError($"SceneMgr: Failed to start loading scene '{sceneToLoad}'. Is it added to Build Settings?");
			yield break;
		}

		while (!asyncOperation.isDone) yield return null;

		UIMgr.Instance.HideMenu(GameMenus.Fader);

		UIMgr.Instance.ShowMenu(menuToOpen, () => onComplete?.Invoke());
	}
}
