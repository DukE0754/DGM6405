using DGM6405.Events;
using UnityEngine;

public abstract class PausableBehaviour : MonoBehaviour, IGameStateListener
{
	protected bool IsPaused { get; private set; }

	private void Update()
	{
		if (!IsPaused)
			PausableUpdate();
	}

	private void FixedUpdate()
	{
		if (!IsPaused)
			PausableFixedUpdate();
	}

	private void LateUpdate()
	{
		if (!IsPaused)
			PausableLateUpdate();
	}

	protected virtual void OnEnable()
	{
		if (GlobalEventBus.Instance == null)
			return;

		if (GameMgr.Instance != null)
			IsPaused = GameMgr.Instance.IsPaused;

		GlobalEventBus.Instance.Register<IGameStateListener>(this);
	}

	protected virtual void OnDisable()
	{
		if (GlobalEventBus.Instance == null)
			return;

		GlobalEventBus.Instance.Unregister<IGameStateListener>(this);
	}

	public void OnPauseStateChanged(bool paused)
	{
		if (IsPaused == paused)
			return;

		IsPaused = paused;

		if (paused)
			OnPaused();
		else
			OnResumed();
	}

	protected virtual void OnPaused()
	{
	}

	protected virtual void OnResumed()
	{
	}

	protected virtual void PausableUpdate()
	{
	}

	protected virtual void PausableLateUpdate()
	{
	}

	protected virtual void PausableFixedUpdate()
	{
	}
}
