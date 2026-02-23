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

            case "Level 1":
            case "Level 2":
            case "Level 3":
                MusicManager.Instance.PlayForest();
                break;

            case "Level 4":
            case "Level 5":
            case "Level 6":
                MusicManager.Instance.PlaySea();
                break;

            case "Level 7":
            case "Level 8":
            case "Level 9":
            case "Level 10":
                MusicManager.Instance.PlayCity();
                break;
        }

        Debug.Log("Scene loaded: " + scene.name);
    }
}
