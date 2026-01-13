using UnityEngine;

public static class RuntimeBootstrapLoader
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Initialize()
	{
		if (GlobalsMgr.Instance)
			return;
		
		// ReSharper disable once Unity.UnknownResource
		var prefab = Resources.Load<GameObject>("GameGlobals");
		var instance = Object.Instantiate(prefab);
	}
}
