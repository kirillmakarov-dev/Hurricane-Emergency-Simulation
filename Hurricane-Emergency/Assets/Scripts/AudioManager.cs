using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string masterVolumeParam = "MasterVolume";
    [SerializeField] private string musicVolumeParam = "MusicVolume";
    [SerializeField] private string sfxVolumeParam = "SFXVolume";

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private AudioClip musicClip;

    [SerializeField] private Button soundMenuButton; // Reference to the button that opens the sound menu
    [SerializeField] private GameObject soundSettingsUI; // Reference to the sound settings UI panel
    [Header("Preferences")]
    private const string MasterVolKey = "audio.master";
    private const string MusicVolKey = "audio.music";
    private const string SfxVolKey = "audio.sfx";
    private const string MasterMuteKey = "audio.master.mute";
    private const float MinDb = -80f;
    private const float MaxDb = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (soundMenuButton != null)
        {
            soundMenuButton.onClick.AddListener(() =>
            {
                // Assuming you have a method to open the sound settings UI
                soundSettingsUI.SetActive(!soundSettingsUI.activeSelf); // Toggle the UI visibility
            });
        }
    }
    void OnDisable()
    {
        if (soundMenuButton != null)
        {
            soundMenuButton.onClick.RemoveAllListeners();
        }
    }

    private void Start()
    {
        LoadFromPrefs();
        SoundIsPlaying(true);
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null || musicSource == null) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.playOnAwake = true;
        musicSource.Play();
    }
    public void PlayAnnouncement()
    {
        musicSource.Stop();
        sfxSource.PlayOneShot(sfxSource.clip);
    }

    public void PlaySong()
    {
        musicSource.Stop();
        sfxSource.clip = musicClip;
        sfxSource.PlayOneShot(sfxSource.clip);
    }

    public void SetMasterVolume(float normalized)
    {
        SaveAndApplyVolume(MasterVolKey, masterVolumeParam, normalized, IsMuted());
    }

    public void SetMusicVolume(float normalized)
    {
        SaveAndApplyVolume(MusicVolKey, musicVolumeParam, normalized, false);
    }

    public void SetSfxVolume(float normalized)
    {
        SaveAndApplyVolume(SfxVolKey, sfxVolumeParam, normalized, false);
    }

    public void SetMasterMute(bool muted)
    {
        PlayerPrefs.SetInt(MasterMuteKey, muted ? 1 : 0);
        PlayerPrefs.Save();

        float master = PlayerPrefs.GetFloat(MasterVolKey, 1f);
        ApplyMixerValue(masterVolumeParam, muted ? MinDb : LinearToDb(master));
    }

    public float GetMasterVolume()
    {
        return PlayerPrefs.GetFloat(MasterVolKey, 1f);
    }

    public float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat(MusicVolKey, 1f);
    }

    public float GetSfxVolume()
    {
        return PlayerPrefs.GetFloat(SfxVolKey, 1f);
    }

    public bool GetMasterMute()
    {
        return IsMuted();
    }

    public void SoundIsPlaying(bool isPlaying)
    {
        AudioListener.pause = !isPlaying;

        if (musicSource != null)
        {
            if (isPlaying) musicSource.UnPause();
            else musicSource.Pause();
        }

        if (sfxSource != null)
        {
            if (isPlaying) sfxSource.UnPause();
            else sfxSource.Pause();
        }
    }

    private void LoadFromPrefs()
    {
        float master = PlayerPrefs.GetFloat(MasterVolKey, 1f);
        float music = PlayerPrefs.GetFloat(MusicVolKey, 1f);
        float sfx = PlayerPrefs.GetFloat(SfxVolKey, 1f);
        bool muted = IsMuted();

        ApplyMixerValue(masterVolumeParam, muted ? MinDb : LinearToDb(master));
        ApplyMixerValue(musicVolumeParam, LinearToDb(music));
        ApplyMixerValue(sfxVolumeParam, LinearToDb(sfx));
    }

    private void SaveAndApplyVolume(string key, string param, float normalized, bool forceMute)
    {
        float value = Mathf.Clamp01(normalized);
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();

        float db = forceMute ? MinDb : LinearToDb(value);
        ApplyMixerValue(param, db);
    }

    private void ApplyMixerValue(string param, float db)
    {
        if (audioMixer == null || string.IsNullOrWhiteSpace(param)) return;
        audioMixer.SetFloat(param, Mathf.Clamp(db, MinDb, MaxDb));
    }

    private static float LinearToDb(float value)
    {
        if (value <= 0.0001f) return MinDb;
        return Mathf.Log10(value) * 20f;
    }

    private static bool IsMuted()
    {
        return PlayerPrefs.GetInt(MasterMuteKey, 0) == 1;
    }
}
