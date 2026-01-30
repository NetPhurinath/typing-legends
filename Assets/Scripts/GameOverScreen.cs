using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameOverScreen : MonoBehaviour
{
    [Header("Text (optional)")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text pointsText;

    [Header("Text (Legacy UI Text) - optional")]
    [SerializeField] private Text titleTextLegacy;
    [SerializeField] private Text pointsTextLegacy;

    [Header("Score Source")]
    [SerializeField] private bool autoFetchScoreOnEnable = true;
    [SerializeField] private bool autoCreatePointsTextIfMissing = true;

    [Header("Score Display")]
    [SerializeField] private bool forcePointsStyleFromTitle = true;

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
    private bool hasExplicitPoints;
    private int lastShownPoints;
    private bool restartHooked;
    private bool mainMenuHooked;

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
        var canvas = GetComponentInParent<Canvas>();

        // Prefer wiring within this screen object first (avoids grabbing unrelated HUD texts elsewhere).
        var localTmpTexts = GetComponentsInChildren<TMP_Text>(true);
        var localLegacyTexts = GetComponentsInChildren<Text>(true);
        var localButtons = GetComponentsInChildren<Button>(true);

        if (titleText == null || pointsText == null)
        {
            var texts = (localTmpTexts != null && localTmpTexts.Length > 0)
                ? localTmpTexts
                : (canvas != null ? canvas.GetComponentsInChildren<TMP_Text>(true) : GetComponentsInChildren<TMP_Text>(true));
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
                        if ((n.Contains("point") || n.Contains("score")) && t.GetComponentInParent<Button>(true) == null)
                        {
                            pointsText = t;
                            break;
                        }
                    }

                    if (pointsText == null)
                    {
                        // Fallback: prefer a non-button text and not the same as titleText
                        foreach (var t in texts)
                        {
                            if (t == null) continue;
                            if (t == titleText) continue;
                            if (t.GetComponentInParent<Button>(true) != null) continue;
                            pointsText = t;
                            break;
                        }

                        if (pointsText == null)
                        {
                            if (texts.Length == 1) pointsText = texts[0];
                            else if (texts.Length >= 2) pointsText = texts[1];
                        }
                    }
                }
            }
        }

        if (restartButton == null || mainMenuButton == null)
        {
            var buttons = (localButtons != null && localButtons.Length > 0)
                ? localButtons
                : (canvas != null ? canvas.GetComponentsInChildren<Button>(true) : GetComponentsInChildren<Button>(true));
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

        if (titleTextLegacy == null || pointsTextLegacy == null)
        {
            var legacyTexts = (localLegacyTexts != null && localLegacyTexts.Length > 0)
                ? localLegacyTexts
                : (canvas != null ? canvas.GetComponentsInChildren<Text>(true) : GetComponentsInChildren<Text>(true));
            if (legacyTexts != null && legacyTexts.Length > 0)
            {
                if (titleTextLegacy == null)
                {
                    foreach (var t in legacyTexts)
                    {
                        if (t == null) continue;
                        if (t.GetComponentInParent<Button>(true) != null) continue;

                        var n = t.name.ToLowerInvariant();
                        if (n == "text (legacy)" || n.Contains("title") || n.Contains("result") || n.Contains("status") || n.Contains("gameover"))
                        {
                            titleTextLegacy = t;
                            break;
                        }
                    }

                    if (titleTextLegacy == null)
                    {
                        foreach (var t in legacyTexts)
                        {
                            if (t == null) continue;
                            if (t.GetComponentInParent<Button>(true) != null) continue;
                            titleTextLegacy = t;
                            break;
                        }
                    }
                }

                if (pointsTextLegacy == null)
                {
                    // Best match: same parent container as title.
                    if (titleTextLegacy != null)
                    {
                        var titleParent = titleTextLegacy.transform.parent;
                        foreach (var t in legacyTexts)
                        {
                            if (t == null) continue;
                            if (t == titleTextLegacy) continue;
                            if (t.GetComponentInParent<Button>(true) != null) continue;
                            if (t.transform.parent != titleParent) continue;

                            var n = t.name.ToLowerInvariant();
                            if (n == "point" || n.Contains("point") || n.Contains("score"))
                            {
                                pointsTextLegacy = t;
                                break;
                            }
                        }
                    }

                    foreach (var t in legacyTexts)
                    {
                        if (t == null) continue;
                        if (t.GetComponentInParent<Button>(true) != null) continue;

                        var n = t.name.ToLowerInvariant();
                        if ((n == "point" || n.Contains("point") || n.Contains("score")) && t != titleTextLegacy)
                        {
                            pointsTextLegacy = t;
                            break;
                        }
                    }
                }
            }
        }

        EnsureButtonHooks();
    }

    private void EnsureButtonHooks()
    {
        if (restartButton != null && !restartHooked)
        {
            restartButton.onClick.AddListener(OnRestartPressed);
            restartHooked = true;
        }

        if (mainMenuButton != null && !mainMenuHooked)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuPressed);
            mainMenuHooked = true;
        }
    }

    private void Awake()
    {
        // Migration / safety: older scenes may have serialized the previous default.
        if (string.IsNullOrWhiteSpace(winSceneName) || winSceneName == "Level 2")
            winSceneName = "LevelSelection";

        TryAutoWire();
        EnsurePointsText();

        // Hide at start
        gameObject.SetActive(false);

        EnsureButtonHooks();

        if (mainMenuButton == null)
            Debug.LogWarning($"{nameof(GameOverScreen)}: mainMenuButton is not set.", this);

        if (pointsText == null)
        {
            if (pointsTextLegacy == null)
                Debug.LogWarning($"{nameof(GameOverScreen)}: pointsText is not set (score will not display).", this);
        }
    }

    private void OnEnable()
    {
        TryAutoWire();
        EnsurePointsText();
        EnsureButtonHooks();

        if (!autoFetchScoreOnEnable) return;

        // If this screen was activated without calling Show(points), still show the latest score.
        if (!hasExplicitPoints)
        {
            UpdatePointsText(ScoreKeeper.LastScore);
            return;
        }

        // If Show() ran while inactive and texts weren't wired yet, refresh now.
        if (pointsText == null && pointsTextLegacy == null)
        {
            TryAutoWire();
            EnsurePointsText();
        }
        UpdatePointsText(lastShownPoints);
    }

    private void OnDisable()
    {
        // Next time we open, allow auto-fetch to populate if Show() isn't called.
        hasExplicitPoints = false;
    }

    private void UpdatePointsText(int points)
    {
        EnsurePointsText();
        if (pointsText == null && pointsTextLegacy == null) return;

        EnsurePointsTextStyle();

        string text;
        try { text = string.Format(pointsFormat, points); }
        catch { text = points + " POINTS"; }

        if (pointsText != null)
        {
            if (!pointsText.gameObject.activeSelf) pointsText.gameObject.SetActive(true);
            if (!pointsText.enabled) pointsText.enabled = true;
            var c = pointsText.color;
            if (c.a <= 0.01f) pointsText.color = new Color(c.r, c.g, c.b, 1f);
            pointsText.text = text;
        }

        if (pointsTextLegacy != null)
        {
            if (!pointsTextLegacy.gameObject.activeSelf) pointsTextLegacy.gameObject.SetActive(true);
            if (!pointsTextLegacy.enabled) pointsTextLegacy.enabled = true;
            var c = pointsTextLegacy.color;
            if (c.a <= 0.01f) pointsTextLegacy.color = new Color(c.r, c.g, c.b, 1f);
            pointsTextLegacy.text = text;
        }
    }

    private void EnsurePointsTextStyle()
    {
        if (!forcePointsStyleFromTitle) return;

        // If the scene already has a "Point" text, make sure it's positioned & styled to be visible.
        if (pointsText != null && titleText != null)
        {
            var titleRt = titleText.rectTransform;
            var rt = pointsText.rectTransform;

            if (rt.localScale.sqrMagnitude < 0.0001f) rt.localScale = Vector3.one;

            rt.anchorMin = titleRt.anchorMin;
            rt.anchorMax = titleRt.anchorMax;
            rt.pivot = titleRt.pivot;
            if (rt.sizeDelta == Vector2.zero) rt.sizeDelta = titleRt.sizeDelta;

            // If points is sitting on top of the title (common when manually duplicated), move it below.
            if (Vector2.Distance(rt.anchoredPosition, titleRt.anchoredPosition) < 1f)
                rt.anchoredPosition = titleRt.anchoredPosition + new Vector2(0f, -Mathf.Max(40f, titleRt.sizeDelta.y * 0.9f));

            pointsText.font = titleText.font;
            pointsText.fontSize = Mathf.Max(18f, titleText.fontSize * 0.6f);
            pointsText.color = titleText.color;
            pointsText.alignment = titleText.alignment;
            pointsText.raycastTarget = false;
        }

        if (pointsTextLegacy != null && titleTextLegacy != null)
        {
            var titleRt = (RectTransform)titleTextLegacy.transform;
            var rt = (RectTransform)pointsTextLegacy.transform;

            if (rt.localScale.sqrMagnitude < 0.0001f) rt.localScale = Vector3.one;

            rt.anchorMin = titleRt.anchorMin;
            rt.anchorMax = titleRt.anchorMax;
            rt.pivot = titleRt.pivot;
            if (rt.sizeDelta == Vector2.zero) rt.sizeDelta = titleRt.sizeDelta;

            if (Vector2.Distance(rt.anchoredPosition, titleRt.anchoredPosition) < 1f)
                rt.anchoredPosition = titleRt.anchoredPosition + new Vector2(0f, -Mathf.Max(40f, titleRt.sizeDelta.y * 0.9f));

            pointsTextLegacy.font = titleTextLegacy.font != null ? titleTextLegacy.font : Resources.GetBuiltinResource<Font>("Arial.ttf");
            pointsTextLegacy.fontSize = Mathf.Max(18, Mathf.RoundToInt(titleTextLegacy.fontSize * 0.6f));
            pointsTextLegacy.color = titleTextLegacy.color;
            pointsTextLegacy.alignment = titleTextLegacy.alignment;
            pointsTextLegacy.raycastTarget = false;
        }
    }

    // Show Game Over with points and pause gameplay
    public void Show(int points)
    {
        Show(points, false);
    }

    // Show Win/Game Over with points and pause gameplay
    public void Show(int points, bool isWin)
    {
        TryAutoWire();
        EnsureButtonHooks();

        showingWin = isWin;
        hasExplicitPoints = true;
        lastShownPoints = points;

        ScoreKeeper.Set(points);

        EnsurePointsText();

        SetRestartButtonLabel(isWin);

        if (titleText != null) titleText.text = isWin ? winTitle : gameOverTitle;
        if (titleTextLegacy != null) titleTextLegacy.text = isWin ? winTitle : gameOverTitle;

        UpdatePointsText(points);

        gameObject.SetActive(true);
        if (backgroundOverlay != null) backgroundOverlay.SetActive(true);

        // Pause time so gameplay stops when popup is shown
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
    }

    private void EnsurePointsText()
    {
        if (pointsText != null || pointsTextLegacy != null) return;
        if (!autoCreatePointsTextIfMissing) return;

        // Avoid creating duplicates if a child already exists.
        var existing = transform.Find("PointsText");
        if (existing != null)
        {
            pointsText = existing.GetComponent<TMP_Text>();
            if (pointsText != null) return;

            pointsTextLegacy = existing.GetComponent<Text>();
            if (pointsTextLegacy != null) return;
        }

        // Prefer using titleText as a template.
        if (titleText != null)
        {
            var parent = titleText.transform.parent != null ? titleText.transform.parent : transform;
            var go = new GameObject("PointsText", typeof(RectTransform), typeof(TMP_Text));
            go.transform.SetParent(parent, false);

            // Keep it near the title in draw order.
            go.transform.SetSiblingIndex(titleText.transform.GetSiblingIndex() + 1);

            var tmp = go.GetComponent<TMP_Text>();
            var rt = (RectTransform)go.transform;

            var titleRt = titleText.rectTransform;
            rt.anchorMin = titleRt.anchorMin;
            rt.anchorMax = titleRt.anchorMax;
            rt.pivot = titleRt.pivot;
            rt.sizeDelta = titleRt.sizeDelta;
            rt.anchoredPosition = titleRt.anchoredPosition + new Vector2(0f, -Mathf.Max(40f, titleRt.sizeDelta.y * 0.9f));

            tmp.font = titleText.font;
            tmp.fontSize = Mathf.Max(18f, titleText.fontSize * 0.6f);
            tmp.color = titleText.color;
            tmp.alignment = titleText.alignment;
            tmp.raycastTarget = false;

            pointsText = tmp;
            EnsurePointsTextStyle();
            return;
        }

        if (titleTextLegacy != null)
        {
            var parent = titleTextLegacy.transform.parent != null ? titleTextLegacy.transform.parent : transform;
            var go = new GameObject("PointsText", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);

            // Keep it near the title in draw order.
            go.transform.SetSiblingIndex(titleTextLegacy.transform.GetSiblingIndex() + 1);

            var txt = go.GetComponent<Text>();
            var rt = (RectTransform)go.transform;

            var titleRt = (RectTransform)titleTextLegacy.transform;
            rt.anchorMin = titleRt.anchorMin;
            rt.anchorMax = titleRt.anchorMax;
            rt.pivot = titleRt.pivot;
            rt.sizeDelta = titleRt.sizeDelta;
            rt.anchoredPosition = titleRt.anchoredPosition + new Vector2(0f, -Mathf.Max(40f, titleRt.sizeDelta.y * 0.9f));

            txt.font = titleTextLegacy.font != null ? titleTextLegacy.font : Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.fontSize = Mathf.Max(18, Mathf.RoundToInt(titleTextLegacy.fontSize * 0.6f));
            txt.color = titleTextLegacy.color;
            txt.alignment = titleTextLegacy.alignment;
            txt.raycastTarget = false;

            pointsTextLegacy = txt;
            EnsurePointsTextStyle();
            return;
        }

        // Last resort: create a basic legacy Text so something shows.
        {
            var go = new GameObject("PointsText", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(transform, false);

            var txt = go.GetComponent<Text>();
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(400f, 80f);
            rt.anchoredPosition = new Vector2(0f, 140f);

            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.fontSize = 28;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.raycastTarget = false;

            pointsTextLegacy = txt;
        }
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
