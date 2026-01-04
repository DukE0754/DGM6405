using UnityEngine;
using UnityEngine.UI;

public class ButtonSounds : MonoBehaviour
{
    [Header("Only one needs to be assigned")]
    [SerializeField] private Button _button;
    [SerializeField] private Toggle _toggle;
    [SerializeField] private Slider _slider;
    
    [Header("Optional sound overrides, otherwise uses AudioMgr.SoundTypes")]
    [SerializeField] private AudioClip _clickSound;
    [SerializeField] private AudioClip _hoverSound;

    /// <summary>
    /// Called once
    /// </summary>
    private void Start()
    {
        if (_button) _button.onClick.AddListener(PlayClickSound);

        if (_toggle) _toggle.onValueChanged.AddListener(PlayClickSound);

        if (_slider) _slider.onValueChanged.AddListener(PlayClickSound);
    }

    /// <summary>
    /// Override for Toggles
    /// </summary>
    /// <param name="state"></param>
    private void PlayClickSound(bool state)
    {
        // Ensures only one toggle in a group plays a sound
        if (!_toggle) return;
        if (!state && _toggle.group && _toggle.group.AnyTogglesOn()) return;

        PlayClickSound(_clickSound);
    }

    /// <summary>
    /// Override for Sliders
    /// </summary>
    /// <param name="_"></param>
    private void PlayClickSound(float _)
    {
        PlayClickSound(_clickSound);
    }

    /// <summary>
    /// Override for Buttons
    /// </summary>
    private void PlayClickSound()
    {
        if (_clickSound != null)
            AudioMgr.Instance.PlaySound(_clickSound);
        else
            AudioMgr.Instance.PlaySound(AudioMgr.SoundTypes.ButtonSelect);
    }


    /// <summary>
    ///     Should be called from OnPointerEnter or OnSelected
    /// </summary>
    public void PlayHoverSound()
    {
        if (_hoverSound != null)
            AudioMgr.Instance.PlaySound(_hoverSound);
        else
            AudioMgr.Instance.PlaySound(AudioMgr.SoundTypes.ButtonHover);
    }
}