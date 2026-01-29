using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMusicController : MonoBehaviour
{
    private void Start()
    {
        SceneManager.LoadScene("MainMenu");
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (MusicManager.Instance == null) return;

        switch (scene.name)
        {
            case "MainMenu":
            case "Options":
                MusicManager.Instance.PlayMenu();
                break;

            case "LevelSelection":
                MusicManager.Instance.PlayLevelSelect();
                break;
        }

        Debug.Log("Scene loaded: " + scene.name);
    }
}
