using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     Settings menu
///     Should include sliders and toggles for player preferences
///     Such as audio settings or accessibility settings
/// </summary>
public class Settings : MenuBase
{
	[SerializeField] private Button _backButton;
	[SerializeField] private Slider _masterSlider;
	[SerializeField] private Slider _sfxSlider;
	[SerializeField] private Slider _musicSlider;
	[SerializeField] private Toggle _muteAllToggle;

	private bool _ignoreEvents;
	private bool _isMuted;

	private void Awake()
	{
		if (_masterSlider) _masterSlider.onValueChanged.AddListener(OnMasterChanged);
		if (_sfxSlider) _sfxSlider.onValueChanged.AddListener(OnSfxChanged);
		if (_musicSlider) _musicSlider.onValueChanged.AddListener(OnMusicChanged);
		if (_muteAllToggle) _muteAllToggle.onValueChanged.AddListener(OnMuteAllChanged);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		_isMuted = false;
		RefreshSlidersFromSaveData();
		if (_muteAllToggle) _muteAllToggle.isOn = false;
	}

	private void OnDestroy()
	{
		if (_masterSlider) _masterSlider.onValueChanged.RemoveListener(OnMasterChanged);
		if (_sfxSlider) _sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);
		if (_musicSlider) _musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
		if (_muteAllToggle) _muteAllToggle.onValueChanged.RemoveListener(OnMuteAllChanged);
	}

	private void RefreshSlidersFromSaveData()
	{
		if (SaveUtil.SavedValues == null)
		{
			Debug.LogError("Failed to find save data");
			return;
		}

		_ignoreEvents = true;
		if (_masterSlider) _masterSlider.value = SaveUtil.SavedValues.GlobalVolume;
		if (_sfxSlider) _sfxSlider.value = SaveUtil.SavedValues.SfxVolume;
		if (_musicSlider) _musicSlider.value = SaveUtil.SavedValues.MusicVolume;
		_ignoreEvents = false;
	}

	private void OnMasterChanged(float value)
	{
		if (_ignoreEvents) return;
		if (_isMuted)
			GlobalContext.Instance.AudioMgr.GlobalVolume = value;
		else
			GlobalEventBus.Instance.Raise<IAudioEventListener>(l => l.OnSetMasterVolume(value));
	}

	private void OnSfxChanged(float value)
	{
		if (_ignoreEvents) return;
		if (_isMuted)
			GlobalContext.Instance.AudioMgr.SfxVolume = value;
		else
			GlobalEventBus.Instance.Raise<IAudioEventListener>(l => l.OnSetSfxVolume(value));
	}

	private void OnMusicChanged(float value)
	{
		if (_ignoreEvents) return;
		if (_isMuted)
			GlobalContext.Instance.AudioMgr.MusicVolume = value;
		else
			GlobalEventBus.Instance.Raise<IAudioEventListener>(l => l.OnSetMusicVolume(value));
	}

	private void OnMuteAllChanged(bool isMuted)
	{
		if (_ignoreEvents) return;

		_isMuted = isMuted;
		if (_isMuted)
		{
			GlobalEventBus.Instance.Raise<IAudioEventListener>(l => l.OnSetMasterVolume(0f, false));
			GlobalEventBus.Instance.Raise<IAudioEventListener>(l => l.OnSetMusicVolume(0f, false));
			GlobalEventBus.Instance.Raise<IAudioEventListener>(l => l.OnSetSfxVolume(0f, false));
		}
		else
		{
			if (_masterSlider)
				GlobalEventBus.Instance.Raise<IAudioEventListener>(l => l.OnSetMasterVolume(_masterSlider.value));
			if (_musicSlider)
				GlobalEventBus.Instance.Raise<IAudioEventListener>(l => l.OnSetMusicVolume(_musicSlider.value));
			if (_sfxSlider) GlobalEventBus.Instance.Raise<IAudioEventListener>(l => l.OnSetSfxVolume(_sfxSlider.value));
		}
	}

	public override GameMenus MenuType()
	{
		return GameMenus.SettingsMenu;
	}

	public void Close()
	{
		SaveUtil.Save();
		GlobalEventBus.Instance.Raise<IUIEventListener>(l => l.OnHideMenu(GameMenus.SettingsMenu));
	}
}
