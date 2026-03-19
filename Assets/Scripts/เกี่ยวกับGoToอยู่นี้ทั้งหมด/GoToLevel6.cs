using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToLevel6 : MonoBehaviour
{
    public void ReturnToMenu()
    {
        SceneManager.LoadSceneAsync("Level 6");
    }

   
}
