using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject gameOverUI;
    [SerializeField] private GameOverScreen gameOverScreen;

    public void GameOver()
    {
        if (gameOverScreen == null)
            gameOverScreen = Object.FindFirstObjectByType<GameOverScreen>(FindObjectsInactive.Include);

        if (gameOverScreen != null)
        {
            gameOverScreen.Show(ScoreKeeper.LastScore);
            return;
        }

        if (gameOverUI != null) gameOverUI.SetActive(true);
        Time.timeScale = 0f; // หยุดเกม
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }
}
