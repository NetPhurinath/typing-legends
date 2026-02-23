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
