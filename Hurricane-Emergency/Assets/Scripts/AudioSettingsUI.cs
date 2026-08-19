using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle masterMuteToggle;

    private void Start()
    {
        var audioManager = AudioManager.Instance;
        if (audioManager == null) return;

        if (masterSlider != null)
        {
            masterSlider.value = audioManager.GetMasterVolume();
            masterSlider.onValueChanged.AddListener(audioManager.SetMasterVolume);
        }

        if (musicSlider != null)
        {
            musicSlider.value = audioManager.GetMusicVolume();
            musicSlider.onValueChanged.AddListener(audioManager.SetMusicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = audioManager.GetSfxVolume();
            sfxSlider.onValueChanged.AddListener(audioManager.SetSfxVolume);
        }

        if (masterMuteToggle != null)
        {
            masterMuteToggle.isOn = audioManager.GetMasterMute();
            masterMuteToggle.onValueChanged.AddListener(audioManager.SetMasterMute);
        }
    }
}
