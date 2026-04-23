using UnityEngine;
using UnityEngine.UI;

public class SfxSetting : MonoBehaviour
{
 private const string SfxVolumeKey = "sfxVolume";

 [SerializeField] private Slider sfxSlider;

 private void OnEnable()
 {
 // When returning to the Options scene, the UI is recreated. Reload saved value.
 Load();
 ApplySfxVolume();
 }

 private void Start()
 {
 if (!PlayerPrefs.HasKey(SfxVolumeKey))
 PlayerPrefs.SetFloat(SfxVolumeKey,1f);

 Load();
 ApplySfxVolume();
 }

 // Hook this to the slider OnValueChanged.
 public void ChangeSfxVolume()
 {
 ApplySfxVolume();
 Save();
 }

 private void ApplySfxVolume()
 {
 float v = sfxSlider != null ? sfxSlider.value : PlayerPrefs.GetFloat(SfxVolumeKey,1f);

 var players = FindObjectsByType<SfxPlayer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
 foreach (var p in players)
 p.SetVolume(v);
 }

 private void Load()
 {
 if (sfxSlider != null)
 sfxSlider.value = PlayerPrefs.GetFloat(SfxVolumeKey,1f);
 }

 private void Save()
 {
 if (sfxSlider != null)
 {
 PlayerPrefs.SetFloat(SfxVolumeKey, sfxSlider.value);
 PlayerPrefs.Save();
 }
 }
}
