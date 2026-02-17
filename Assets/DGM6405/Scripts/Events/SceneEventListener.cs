using UnityEngine;

/// <summary>
///     Bridges GlobalEventBus events to SceneMgr.
///     Decouples other systems from direct SceneMgr.Instance calls.
/// </summary>
public class SceneEventListener : MonoBehaviour, ISceneEventListener
{
	private void OnEnable()
	{
		GlobalEventBus.Instance.Register<ISceneEventListener>(this);
	}

	private void OnDisable()
	{
		GlobalEventBus.Instance.Unregister<ISceneEventListener>(this);
	}

	public void OnLoadScene(GameScenes scene, GameMenus menu, System.Action onComplete)
	{
		SceneMgr.Instance.LoadScene(scene, menu, onComplete);
	}
}
