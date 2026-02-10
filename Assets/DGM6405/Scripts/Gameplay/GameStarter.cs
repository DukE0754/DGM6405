using System.Collections;
using UnityEngine;

public class GameStarter : MonoBehaviour
{
	private void Start()
	{
		StartCoroutine(StartWhenReady());
	}

	private IEnumerator StartWhenReady()
	{
		Debug.Log("GameStarter: Waiting for level to finish loading...");
		yield return new WaitUntil(() => LevelMgr.Instance.IsLevelLoaded);
		
		Debug.Log("GameStarter: Spawning player");
		var spawnPoint = PlayerSpawnPoint.Instance;
		if (spawnPoint == null)
			Debug.LogError("GameStarter: No spawn point found!");
		else
			PlayerMgr.Instance.SpawnPlayer(
				spawnPoint.transform.position,
				spawnPoint.transform.rotation);

		Debug.Log("GameStarter: Waiting for player spawn...");
		yield return new WaitUntil(() => PlayerMgr.Instance.HasSpawnedPlayer);

		GlobalEventBus.Instance.Raise<ILevelListener>(l => l.OnLevelReady());
		
		Debug.Log("Game starting in 3 seconds...");
		yield return new WaitForSeconds(1f);
		Debug.Log("Game starting in 2 seconds...");
		yield return new WaitForSeconds(1f);
		Debug.Log("Game starting in 1 second...");
		yield return new WaitForSeconds(1f);

		GameMgr.Instance.StartGame();
	}
}
