using UnityEngine;

public class LevelCompleteTrigger : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			var gameLoop = GameLoopManager.Instance;
			var timeMs = 0;

			if (gameLoop != null)
				timeMs = Mathf.RoundToInt(gameLoop.GameTimer * 1000f);
			else
				Debug.LogWarning("LevelCompleteTrigger: No GameLoopManager found in scene. Time will be 0.");

			GameMgr.Instance.NextLevel(timeMs);
		}
	}
}
