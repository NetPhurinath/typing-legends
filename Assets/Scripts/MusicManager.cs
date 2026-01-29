using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    public AudioSource menuMusic;
    public AudioSource levelSelectMusic;

    private AudioSource current;

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

    void SwitchTo(AudioSource next)
    {
        if (next == null) return;
        if (current == next) return;

        if (current != null)
            current.Stop();

        current = next;
        current.loop = true;
        current.Play();
    }
}
