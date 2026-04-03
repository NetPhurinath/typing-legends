using UnityEngine;

/// <summary>
/// Simple SFX helper for UI/gameplay one-shot sounds.
/// Attach to a GameObject with an AudioSource (recommended on a persistent "SFX" object).
/// </summary>
[DisallowMultipleComponent]
public class SfxPlayer : MonoBehaviour
{
 [Header("Audio")]
 [SerializeField] private AudioSource audioSource;

 [Header("Clips")]
 [SerializeField] private AudioClip playerHitMonsterClip;
 [SerializeField] private AudioClip monsterHitPlayerClip;

 private void Awake()
 {
 if (audioSource == null) audioSource = GetComponent<AudioSource>();
 }

 public void PlayPlayerHitMonster()
 {
 PlayOneShot(playerHitMonsterClip);
 }

 public void PlayMonsterHitPlayer()
 {
 PlayOneShot(monsterHitPlayerClip);
 }

 /// <summary>
 /// Play an arbitrary one-shot clip via the configured AudioSource.
 /// </summary>
 public void PlayClip(AudioClip clip)
 {
 PlayOneShot(clip);
 }

 private void PlayOneShot(AudioClip clip)
 {
 if (clip == null) return;

 if (audioSource != null)
 {
 audioSource.PlayOneShot(clip);
 return;
 }

 // Fallback: play at listener position.
 var listener = FindFirstObjectByType<AudioListener>();
 if (listener != null)
 AudioSource.PlayClipAtPoint(clip, listener.transform.position);
 }
}
