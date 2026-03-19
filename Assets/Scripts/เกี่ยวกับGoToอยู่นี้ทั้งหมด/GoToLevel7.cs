using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToLevel7 : MonoBehaviour
{
    public void ReturnToMenu()
    {
        SceneManager.LoadSceneAsync("Level 7");
    }

   
}
