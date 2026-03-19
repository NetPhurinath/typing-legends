using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToLevel4 : MonoBehaviour
{
    public void ReturnToMenu()
    {
        SceneManager.LoadSceneAsync("Level 4");
    }

   
}
