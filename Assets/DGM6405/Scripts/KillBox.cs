using UnityEngine;

public class KillBox : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			GameMgr.Instance.GameOver();
		}
	}
}
