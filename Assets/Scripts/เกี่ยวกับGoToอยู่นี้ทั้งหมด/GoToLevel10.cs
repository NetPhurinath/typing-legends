using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToLevel10 : MonoBehaviour
{
    public void ReturnToMenu()
    {
        SceneManager.LoadSceneAsync("Level 10");
    }

   
}
