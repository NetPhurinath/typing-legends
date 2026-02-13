using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToLevel5 : MonoBehaviour
{
    public void ReturnToMenu()
    {
        SceneManager.LoadSceneAsync("Level 5");
    }

   
}
