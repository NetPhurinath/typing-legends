using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToLevelSelection : MonoBehaviour
{
    public void ReturnToLevelSelection()
    {
        SceneManager.LoadSceneAsync("LevelSelection");
    }
}
