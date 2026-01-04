using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

/// <summary>
/// A general audio manager for playing music and sounds
/// Contains its own list of reusable sounds, but accepts clips as well
/// </summary>
public class AudioMgr : Singleton<AudioMgr>
{
    /// <summary>
    /// Reused music clips enum, these should match <see cref="AudioMgr._reusableMusicClips"/>
    /// </summary>
    public enum MusicTypes
    {
        MainMenu = 0,
        Gameplay = 1
    }
    
    /// <summary>
    ///  Reused sound clips enum, these should match <see cref="AudioMgr._reusableSoundClips"/>
    /// </summary>
    public enum SoundTypes
    {
        ButtonSelect = 0,
        ButtonHover = 1,
        ButtonError = 2,
    }
    
    /// <summary>
    /// Main audio mixer
    /// </summary>
    [Header("Mixer")]
    [SerializeField] private AudioMixer _mixer;
    
    /// <summary>
    /// Audio sources for splitting sound channels
    /// </summary>
    [Header("Sources")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;
    
    /// <summary>
    /// Actual audio clips to align with <see cref="AudioMgr.MusicTypes"/>
    /// </summary>
    [Header("Reusable Clips")] [SerializeField]
    private AudioClip[] _reusableMusicClips;
    /// <summary>
    /// Actual audio clips to align with <see cref="AudioMgr.SoundTypes"/>
    /// </summary>
    [SerializeField] private AudioClip[] _reusableSoundClips;

    /// <summary>
    /// The player for music clips
    /// </summary>
    private AudioSource MusicPlayer => _musicSource;

    /// <summary>
    /// The player sfx clips
    /// </summary>
    private AudioSource SfxPlayer => _sfxSource;
    
    /// <summary>
    /// Volumes to get and set from the save data
    /// </summary>
    public float GlobalVolume
    {
        set => SaveUtil.SavedValues.GlobalVolume = value;
        get => SaveUtil.SavedValues.GlobalVolume;
    }
    
    /// <summary>
    /// Volumes to get and set from the save data
    /// </summary>
    public float MusicVolume
    {
        set => SaveUtil.SavedValues.MusicVolume = value;
        get => SaveUtil.SavedValues.MusicVolume;
    }
    
    /// <summary>
    /// Volumes to get and set from the save data
    /// </summary>
    public float SfxVolume
    {
        set => SaveUtil.SavedValues.SfxVolume = value;
        get => SaveUtil.SavedValues.SfxVolume;
    }
    
    /// <summary>
    /// Only have one audio manager
    /// Wait for the data load to set the volume
    /// </summary>
    private void Start()
    {
        SaveUtil.OnLoadCompleted += OnDataLoadComplete;
        SaveUtil.Load();
    }
    
    /// <summary>
    /// Once save data is loaded, update the volumes
    /// TODO maybe don't play any sounds until we know what volume to play at?
    /// </summary>
    private void OnDataLoadComplete()
    {
        SaveUtil.OnLoadCompleted -= OnDataLoadComplete;
    
        UpdateVolumeFromSaveData();
    }
    
    /// <summary>
    /// Update the master, music, and sfx volumes from the saved values
    /// </summary>
    private void UpdateVolumeFromSaveData()
    {
        _mixer.SetFloat("MasterVol", GlobalVolume);
        _mixer.SetFloat("MusicVol", MusicVolume);
        _mixer.SetFloat("SfxVol", SfxVolume);
    }
    
    /// <summary>
    /// Play a looping music by its <see cref="MusicTypes"/>
    /// </summary>
    /// <param name="music"></param>
    /// <param name="volumeMod"></param>
    [UsedImplicitly] // Use when appropriate
    public void PlayMusic(MusicTypes music, float volumeMod)
    {
        var index = (int) music;
        if (_reusableMusicClips.Length < index)
        {
            Debug.LogWarning($"Music type {music.ToString()} not found in music clips");
            return;
        }
    
        PlayMusic(_reusableMusicClips[(int) music], volumeMod);
    }
    
    /// <summary>
    /// Play a looping music by passing the audio clip
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="volumeMod"></param>
    [UsedImplicitly] // Use when appropriate
    public void PlayMusic(AudioClip clip, float volumeMod)
    {
        if (volumeMod <= 0f) return;
        MusicPlayer.clip = clip;
        MusicPlayer.volume = volumeMod;
        MusicPlayer.loop = true;
        MusicPlayer.Play();
    }
    
    /// <summary>
    /// Pause the music player
    /// Probably should be used during gameplay
    /// But maybe pause menu if there is no pause music?
    /// </summary>
    public void PauseMusic()
    {
        MusicPlayer.Pause();
    }
    
    /// <summary>
    /// Resume the music player
    /// </summary>
    public void ResumeMusic()
    {
        MusicPlayer.Play();
    }
    
    /// <summary>
    /// Player a non-looping music clip on the music channel
    /// Note that this will leave dead air once the clip ends
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="volumeMod"></param>
    public void PlayOneShotMusic(AudioClip clip, float volumeMod)
    {
        MusicPlayer.Stop();
        if (volumeMod <= 0f) return;
        MusicPlayer.PlayOneShot(clip, volumeMod);
    }
    
    /// <summary>
    /// Play a sound by its <see cref="SoundTypes"/> but only if there is no existing sound clip playing
    /// </summary>
    /// <param name="sound"></param>
    /// <param name="volumeMod"></param>
    public void PlaySoundNoOverlap(SoundTypes sound, float volumeMod)
    {
        if (!SfxPlayer.isPlaying)
            PlaySound(sound, volumeMod);
    }
    
    /// <summary>
    /// Play a sound by its <see cref="SoundTypes"/>
    /// </summary>
    /// <param name="sound"></param>
    /// <param name="volumeMod"></param>
    public void PlaySound(SoundTypes sound, float volumeMod = 1f)
    {
        var index = (int) sound;
        if (_reusableSoundClips.Length < index)
        {
            Debug.LogWarning($"Sound type {sound.ToString()} not found in sound clips");
            return;
        }
    
        PlaySound(_reusableSoundClips[(int) sound], volumeMod);
    }
    
    /// <summary>
    /// Play a sound clip by passing it directly
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="volumeMod"></param>
    public void PlaySound(AudioClip clip, float volumeMod = 1f)
    {
        if (volumeMod <= 0f) return;
        if (clip != null) SfxPlayer.PlayOneShot(clip, volumeMod);
    }
}
