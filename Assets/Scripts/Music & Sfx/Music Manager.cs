using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    public AudioSource menuMusic;
    public AudioSource levelSelectMusic;
    public AudioSource forestMusic;
    public AudioSource seaMusic;
    public AudioSource cityMusic;

    private AudioSource current;

    // Tracks nested mute requests (pause overlay + game over overlay etc.)
    private int pauseRequests;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayMenu()
    {
        Debug.Log("PlayMusic: Menu");
        SwitchTo(menuMusic);
    }

    public void PlayLevelSelect()
    {
        Debug.Log("PlayMusic: LevelSelection");
        SwitchTo(levelSelectMusic);
    }

    public void PlayForest()
    {
        Debug.Log("PlayMusic: Forest");
        SwitchTo(forestMusic);
    }

    public void PlaySea() 
    {
        Debug.Log("PlayMusic: Sea");
        SwitchTo(seaMusic);
    }

    public void PlayCity() 
    {
        Debug.Log("PlayMusic: City");
        SwitchTo(cityMusic);
    }

    /// <summary>
    /// Temporarily silence the current BGM (used by pause/game-over overlays).
    /// Supports nesting: multiple PauseBgm() calls require the same number of ResumeBgm() calls.
    /// </summary>
    public void PauseBgm()
    {
        pauseRequests++;
        ApplyPauseState();
    }

    /// <summary>
    /// Undo a PauseBgm() request.
    /// </summary>
    public void ResumeBgm()
    {
        pauseRequests = Mathf.Max(0, pauseRequests -1);
        ApplyPauseState();
    }

    private void ApplyPauseState()
    {
        if (current == null) return;

        bool shouldMute = pauseRequests >0;

        // Use Pause/UnPause so playback resumes where it left off.
        if (shouldMute)
        {
            if (current.isPlaying)
                current.Pause();
        }
        else
        {
            // Only unpause if we have a clip assigned.
            if (current.clip != null)
                current.UnPause();
        }
    }

    void SwitchTo(AudioSource next)
    {
        if (next == null) return;
        if (current == next) return;

        if (current != null)
            current.Stop();

        current = next;
        current.loop = true;
        current.Play();

        // If we're currently paused by an overlay, keep BGM silenced.
        ApplyPauseState();
    }
}
