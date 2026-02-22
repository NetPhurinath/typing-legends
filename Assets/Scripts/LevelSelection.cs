using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class LevelSelection : MonoBehaviour
{
    [SerializeField] private bool unlocked = false;

    [Header("UI")]
    public Image unlockImage;   // lock overlay (shown when locked)
    public Image clearImage;    // "CLEAR" overlay (hidden by default)

    [Header("Optional (recommended)")]
    [Tooltip("If set, used to detect level number for clear/unlock (e.g. 'Level 1').")]
    [SerializeField] private string levelSceneName;

    private int levelNum;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        // If the scene's Button OnClick isn't wired in the inspector,
        // auto-wire it so unlocked levels are clickable.
        if (button != null && button.onClick.GetPersistentEventCount() == 0)
            button.onClick.AddListener(OnAutoClick);
    }

    private void OnEnable() => UpdateAll();

    private void UpdateAll()
    {
        levelNum = ResolveLevelNumber();
        UpdateLevelStatus();
        UpdateLevelImage();

        if (button != null)
            button.interactable = unlocked;
    }

    public void Refresh() => UpdateAll();

    private void OnAutoClick()
    {
        var target = ResolveTargetSceneName();
        if (!string.IsNullOrWhiteSpace(target))
            PressSelection(target);
    }

    private string ResolveTargetSceneName()
    {
        if (!string.IsNullOrWhiteSpace(levelSceneName))
            return levelSceneName;

        int n = ResolveLevelNumber();
        return n > 0 ? $"Level {n}" : string.Empty;
    }

    private static string LevelKey(int n) => "Lv" + n.ToString();

    private int ResolveLevelNumber()
    {
        // 1) Prefer explicit scene name (most reliable)
        if (!string.IsNullOrWhiteSpace(levelSceneName))
        {
            var m = Regex.Match(levelSceneName, @"\d+");
            if (m.Success && int.TryParse(m.Value, out int n) && n > 0) return n;
        }

        // 2) Fallback: parse from this button object's name
        {
            var m = Regex.Match(gameObject.name, @"\d+");
            if (m.Success && int.TryParse(m.Value, out int n) && n > 0) return n;
        }

        return 0;
    }

    private void UpdateLevelStatus()
    {
        if (levelNum <= 0)
        {
            unlocked = false;
            return;
        }

        if (levelNum == 1)
        {
            unlocked = true;
            return;
        }

        unlocked = PlayerPrefs.GetInt(LevelKey(levelNum - 1), 0) > 0;
    }

    private void UpdateLevelImage()
    {
        if (unlockImage != null)
            unlockImage.gameObject.SetActive(!unlocked);

        bool cleared = levelNum > 0 && PlayerPrefs.GetInt(LevelKey(levelNum), 0) > 0;
        if (clearImage != null)
            clearImage.gameObject.SetActive(cleared);

        Debug.Log($"LevelSelection '{name}': levelNum={levelNum}, unlocked={unlocked}, Lv{levelNum}={PlayerPrefs.GetInt(LevelKey(levelNum), 0)}, clearImage={(clearImage ? clearImage.gameObject.activeSelf : false)}");
    }

    public void PressSelection(string levelName)
    {
        UpdateAll();
        if (!unlocked) return;
        if (string.IsNullOrWhiteSpace(levelName)) return;

        SceneManager.LoadScene(levelName);
    }
}
