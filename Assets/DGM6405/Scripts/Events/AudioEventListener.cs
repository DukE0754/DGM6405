using UnityEngine;

/// <summary>
///     Bridges GlobalEventBus events to AudioMgr.
///     Decouples other systems from direct AudioMgr.Instance calls.
/// </summary>
public class AudioEventListener : MonoBehaviour, IAudioEventListener
{
	private void OnEnable()
	{
		GlobalEventBus.Instance.Register<IAudioEventListener>(this);
	}

	private void OnDisable()
	{
		GlobalEventBus.Instance.Unregister<IAudioEventListener>(this);
	}

	public void OnPlaySound(AudioMgr.SoundTypes sound)
	{
		AudioMgr.Instance.PlaySound(sound);
	}

	public void OnPlaySoundClip(AudioClip clip)
	{
		AudioMgr.Instance.PlaySound(clip);
	}

	public void OnSetMasterVolume(float volume, bool save)
	{
		AudioMgr.Instance.SetMasterVolume(volume, save);
	}

	public void OnSetMusicVolume(float volume, bool save)
	{
		AudioMgr.Instance.SetMusicVolume(volume, save);
	}

	public void OnSetSfxVolume(float volume, bool save)
	{
		AudioMgr.Instance.SetSfxVolume(volume, save);
	}
}
