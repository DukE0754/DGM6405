using UnityEngine;

public class LevelCompleteTrigger : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			var gameLoop = FindFirstObjectByType<GameLoopManager>();
			var timeMs = 0;

			if (gameLoop != null)
			{
				timeMs = Mathf.RoundToInt(gameLoop.GameTimer * 1000f);
			}

			GameMgr.Instance.NextLevel(timeMs);
		}
	}
}
