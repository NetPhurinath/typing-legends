using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToLevel3 : MonoBehaviour
{
    public void ReturnToMenu()
    {
        SceneManager.LoadSceneAsync("Level 3");
    }

   
}
