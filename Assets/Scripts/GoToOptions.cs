using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToOptions : MonoBehaviour
{
    public void ReturnToOptions()
    {
        SceneManager.LoadSceneAsync("Options");
    }


}