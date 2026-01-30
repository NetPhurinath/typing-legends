using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameOverScreen : MonoBehaviour
{
    [Header("Text (optional)")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text pointsText;

    [Header("Titles")]
    [SerializeField] private string gameOverTitle = "GAME OVER";
    [SerializeField] private string winTitle = "YOU WIN";

    [Header("Score Text")]
    [SerializeField] private string pointsFormat = "{0} POINTS";

    [Header("Win (optional)")]
    [SerializeField] private string winSceneName = "LevelSelection";

    [Header("Buttons")]
    [SerializeField] private string restartButtonLabelGameOver = "Restart";
    [SerializeField] private string restartButtonLabelWin = "Next";

    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private GameObject backgroundOverlay; // Optional background panel (e.g., dim screen)

    private float previousTimeScale = 1f;
    private bool showingWin;

    private void SetRestartButtonLabel(bool isWin)
    {
        if (restartButton == null) return;

        string label = isWin ? restartButtonLabelWin : restartButtonLabelGameOver;
        if (string.IsNullOrWhiteSpace(label)) return;

        var tmp = restartButton.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null)
        {
            tmp.text = label;
            return;
        }

        var legacy = restartButton.GetComponentInChildren<Text>(true);
        if (legacy != null)
        {
            legacy.text = label;
        }
    }

    private void TryAutoWire()
    {
        if (titleText == null || pointsText == null)
        {
            var texts = GetComponentsInChildren<TMP_Text>(true);
            if (texts != null && texts.Length > 0)
            {
                if (titleText == null)
                {
                    foreach (var t in texts)
                    {
                        var n = t.name.ToLowerInvariant();
                        if (n.Contains("title") || n.Contains("result") || n.Contains("status"))
                        {
                            titleText = t;
                            break;
                        }
                    }

                    if (titleText == null && texts.Length >= 2) titleText = texts[0];
                }

                if (pointsText == null)
                {
                    foreach (var t in texts)
                    {
                        var n = t.name.ToLowerInvariant();
                        if (n.Contains("point") || n.Contains("score"))
                        {
                            pointsText = t;
                            break;
                        }
                    }

                    if (pointsText == null)
                    {
                        if (texts.Length == 1) pointsText = texts[0];
                        else if (texts.Length >= 2) pointsText = texts[1];
                    }
                }
            }
        }

        if (restartButton == null || mainMenuButton == null)
        {
            var buttons = GetComponentsInChildren<Button>(true);
            if (buttons != null && buttons.Length > 0)
            {
                if (restartButton == null)
                {
                    foreach (var b in buttons)
                    {
                        var n = b.name.ToLowerInvariant();
                        if (n.Contains("restart") || n.Contains("again") || n.Contains("next"))
                        {
                            restartButton = b;
                            break;
                        }
                    }
                }

                if (mainMenuButton == null)
                {
                    foreach (var b in buttons)
                    {
                        var n = b.name.ToLowerInvariant();
                        if (n.Contains("menu") || n.Contains("main"))
                        {
                            mainMenuButton = b;
                            break;
                        }
                    }
                }
            }
        }
    }

    private void Awake()
    {
        // Migration / safety: older scenes may have serialized the previous default.
        if (string.IsNullOrWhiteSpace(winSceneName) || winSceneName == "Level 2")
            winSceneName = "LevelSelection";

        TryAutoWire();

        // Hide at start
        gameObject.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartPressed);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuPressed);
        else
            Debug.LogWarning($"{nameof(GameOverScreen)}: mainMenuButton is not set.", this);

        if (pointsText == null)
            Debug.LogWarning($"{nameof(GameOverScreen)}: pointsText is not set (score will not display).", this);
    }

    // Show Game Over with points and pause gameplay
    public void Show(int points)
    {
        Show(points, false);
    }

    // Show Win/Game Over with points and pause gameplay
    public void Show(int points, bool isWin)
    {
        showingWin = isWin;

        SetRestartButtonLabel(isWin);

        if (titleText != null)
            titleText.text = isWin ? winTitle : gameOverTitle;

        if (pointsText != null)
        {
            try
            {
                pointsText.text = string.Format(pointsFormat, points);
            }
            catch
            {
                pointsText.text = points + " POINTS";
            }
        }

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

        if (showingWin && !string.IsNullOrWhiteSpace(winSceneName))
        {
            if (!Application.CanStreamedLevelBeLoaded(winSceneName))
            {
                Debug.LogError($"{nameof(GameOverScreen)}: Scene '{winSceneName}' cannot be loaded. Add it to File > Build Profiles/Settings > Scenes In Build.", this);
                return;
            }
            SceneManager.LoadScene(winSceneName);
            return;
        }

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
