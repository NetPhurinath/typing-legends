using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private TMP_Text pointsText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private GameObject backgroundOverlay; // Optional background panel (e.g., dim screen)

    private float previousTimeScale = 1f;

    private void Awake()
    {
        // Hide at start
        gameObject.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartPressed);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuPressed);
    }

    // Show Game Over with points and pause gameplay
    public void Show(int points)
    {
        if (pointsText != null)
            pointsText.text = points + " POINTS";

        gameObject.SetActive(true);
        if (backgroundOverlay != null) backgroundOverlay.SetActive(true);

        // Pause time so gameplay stops when popup is shown
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
    }

    // Hide Game Over and resume time
    public void Hide()
    {
        gameObject.SetActive(false);
        if (backgroundOverlay != null) backgroundOverlay.SetActive(false);
        Time.timeScale = previousTimeScale;
    }

    // Restart: reload current scene and resume time
    public void OnRestartPressed()
    {
        if (backgroundOverlay != null) backgroundOverlay.SetActive(false);
        Time.timeScale = previousTimeScale;
        var current = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(current);
    }

    // Main menu: go back to main menu scene and resume time
    public void OnMainMenuPressed()
    {
        if (backgroundOverlay != null) backgroundOverlay.SetActive(false);
        Time.timeScale = previousTimeScale;
        SceneManager.LoadScene("MainMenu");
    }
}
