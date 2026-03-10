using System;
using UnityEngine;
using UnityEngine.Rendering;

public class GlobalVolumeSwapper : MonoBehaviour, IHealthListener
{
    [SerializeField] private Volume _globalVolume;
	[SerializeField] private VolumeProfile _defaultProfile;
	[SerializeField] private VolumeProfile _deathProfile;
	
	private void Start()
	{
		if (_globalVolume == null)
		{
			Debug.LogError("[GlobalVolumeSwapper.Start] No global volume found.");
			return;
		}
		_globalVolume.profile = _defaultProfile;
	}

	private void OnEnable()
	{
		GlobalEventBus.Instance?.Register<IHealthListener>(this);
	}

	private void OnDisable()
	{
		GlobalEventBus.Instance?.Unregister<IHealthListener>(this);
	}

	public void OnDied()
	{
		_globalVolume.profile = _deathProfile;
	}
}
