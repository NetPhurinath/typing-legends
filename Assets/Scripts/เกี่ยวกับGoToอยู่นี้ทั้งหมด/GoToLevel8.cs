using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToLevel8 : MonoBehaviour
{
    public void ReturnToMenu()
    {
        SceneManager.LoadSceneAsync("Level 8");
    }

   
}
