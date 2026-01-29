using UnityEngine;
using UnityEngine.SceneManagement;

public class Restart : MonoBehaviour
{
    [SerializeField] private bool enableKeyboardRestart = true;
    [SerializeField] private KeyCode restartKey = KeyCode.R;

    void Update()
    {
        if (!enableKeyboardRestart) return;
        if (Input.GetKeyDown(restartKey))
        {
            ReloadScene();
        }
    }

    public void RestartGame()
    {
        ReloadScene();
    }

    private void ReloadScene()
    {
        Time.timeScale = 1f;
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }
}
