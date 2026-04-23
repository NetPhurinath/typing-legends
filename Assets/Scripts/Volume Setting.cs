using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicSetting : MonoBehaviour
{
    private const string MusicVolumeKey = "musicVolume";

    [SerializeField]
    private Slider volumeSlider;

    private void OnEnable()
    {
        // When returning to the Options scene, the UI is recreated. Reload saved value.
        Load();
        ApplyMusicVolume();
    }

    void Start()
    {
        if (!PlayerPrefs.HasKey(MusicVolumeKey))
            PlayerPrefs.SetFloat(MusicVolumeKey, 1f);

        Load();
        ApplyMusicVolume();
    }

    // Hook this to the slider OnValueChanged.
    public void ChangeVolume()
    {
        ApplyMusicVolume();
        Save();
    }

    private void ApplyMusicVolume()
    {
        var v = volumeSlider != null ? volumeSlider.value : PlayerPrefs.GetFloat(MusicVolumeKey, 1f);

        if (MusicManager.Instance == null) return;

        // Apply to all BGM sources so switching tracks keeps the same volume.
        if (MusicManager.Instance.menuMusic != null) MusicManager.Instance.menuMusic.volume = v;
        if (MusicManager.Instance.levelSelectMusic != null) MusicManager.Instance.levelSelectMusic.volume = v;
        if (MusicManager.Instance.forestMusic != null) MusicManager.Instance.forestMusic.volume = v;
        if (MusicManager.Instance.seaMusic != null) MusicManager.Instance.seaMusic.volume = v;
        if (MusicManager.Instance.cityMusic != null) MusicManager.Instance.cityMusic.volume = v;
    }

    private void Load()
    {
        if (volumeSlider != null)
            volumeSlider.value = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
    }

    private void Save()
    {
        if (volumeSlider != null)
        {
            PlayerPrefs.SetFloat(MusicVolumeKey, volumeSlider.value);
            PlayerPrefs.Save();
        }
    }
}