using UnityEngine;

/// <summary>
/// Plays one-shot SFX when the game ends (win or lose).
/// Attach this to the same GameObject as `GameOverScreen`.
/// 
/// This script waits until `GameOverScreen.Show(...)` has been called
/// (HasExplicitPoints == true) to avoid playing at level start.
/// </summary>
[DisallowMultipleComponent]
public class GameEndSfx : MonoBehaviour
{
  [Header("SFX")]
  [SerializeField] private SfxPlayer sfxPlayer;

  [SerializeField] private AudioClip winClip;
  [SerializeField] private AudioClip loseClip;

  [Header("Behavior")]
  [Tooltip("If true: plays once each time the end screen is shown.")]
  [SerializeField] private bool playOncePerShow = true;

  private GameOverScreen screen;
  private bool playedForThisShow;

  private void Awake()
  {
    if (sfxPlayer == null)
      sfxPlayer = FindFirstObjectByType<SfxPlayer>(FindObjectsInactive.Include);

    screen = GetComponent<GameOverScreen>();
  }

  private void Update()
  {
    if (screen == null) return;

    // Gate: only consider this "shown" after Show(...) was called.
    if (!screen.HasExplicitPoints)
    {
      playedForThisShow = false;
      return;
    }

    // Then check if it is currently visible.
    var cg = screen.GetComponent<CanvasGroup>();
    bool isVisible = cg != null && (cg.interactable || cg.blocksRaycasts) && cg.alpha >0.001f;
    if (!isVisible)
    {
      playedForThisShow = false;
      return;
    }

    if (playOncePerShow && playedForThisShow) return;

    var clip = screen.IsWin ? winClip : loseClip;
    if (clip != null && sfxPlayer != null)
      sfxPlayer.PlayClip(clip);

    playedForThisShow = true;
  }
}
