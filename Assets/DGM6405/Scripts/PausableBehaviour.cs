using UnityEngine;

public abstract class PausableBehaviour : MonoBehaviour
{
	private bool _isPaused;

	protected bool IsPaused => _isPaused;

	protected virtual void OnEnable()
	{
		if (GameMgr.Instance == null)
			return;

		_isPaused = GameMgr.Instance.IsPaused;
		GameMgr.Instance.PauseStateChanged += HandlePauseStateChanged;
	}

	protected virtual void OnDisable()
	{
		if (GameMgr.Instance == null)
			return;

		GameMgr.Instance.PauseStateChanged -= HandlePauseStateChanged;
	}

	private void HandlePauseStateChanged(bool paused)
	{
		if (_isPaused == paused)
			return;

		_isPaused = paused;

		if (paused)
			OnPaused();
		else
			OnResumed();
	}

	private void Update()
	{
		if (!_isPaused)
			PausableUpdate();
	}

	private void LateUpdate()
	{
		if (!_isPaused)
			PausableLateUpdate();
	}

	private void FixedUpdate()
	{
		if (!_isPaused)
			PausableFixedUpdate();
	}

	protected virtual void OnPaused() { }
	protected virtual void OnResumed() { }
	protected virtual void PausableUpdate() { }
	protected virtual void PausableLateUpdate() { }
	protected virtual void PausableFixedUpdate() { }
}
