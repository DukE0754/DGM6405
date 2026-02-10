using UnityEngine;

public class SetAsPlayer : MonoBehaviour
{
	private void Start()
	{
		PlayerMgr.Instance.DebugAssignAsPlayer(gameObject);
	}
}
