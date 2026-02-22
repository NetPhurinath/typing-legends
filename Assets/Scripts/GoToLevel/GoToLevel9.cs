using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToLevel9 : MonoBehaviour
{
    public void ReturnToMenu()
    {
        SceneManager.LoadSceneAsync("Level 9");
    }

   
}
