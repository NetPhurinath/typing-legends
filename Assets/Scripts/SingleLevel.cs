using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToMainMenu : MonoBehaviour
{
    [SerializeField] private string mainMenuScene = "MainMenu";

    public void BackToMainMenu()
    {
        if (!SceneExists(mainMenuScene))
        {
            Debug.LogError($"MainMenu scene '{mainMenuScene}' not found in Build Settings.");
            return;
        }
        SceneManager.LoadSceneAsync(mainMenuScene);
    }

    private bool SceneExists(string name)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
            if (sceneName == name) return true;
        }
        return false;
    }
}