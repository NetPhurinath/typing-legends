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

    private void Start() => UpdateAll();
    private void OnEnable() => UpdateAll();

    private void UpdateAll()
    {
        levelNum = ResolveLevelNumber();
        UpdateLevelStatus();
        UpdateLevelImage();
    }

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

        unlocked = PlayerPrefs.GetInt("Lv" + (levelNum - 1).ToString(), 0) > 0;
    }

    private void UpdateLevelImage()
    {
        if (unlockImage != null)
            unlockImage.gameObject.SetActive(!unlocked);

        bool cleared = levelNum > 0 && PlayerPrefs.GetInt("Lv" + levelNum.ToString(), 0) > 0;
        if (clearImage != null)
            clearImage.gameObject.SetActive(cleared);

        Debug.Log($"LevelSelection '{name}': levelNum={levelNum}, unlocked={unlocked}, Lv{levelNum}={PlayerPrefs.GetInt("Lv" + levelNum, 0)}, clearImage={(clearImage ? clearImage.gameObject.activeSelf : false)}");
    }

    public void PressSelection(string levelName)
    {
        if (unlocked)
            SceneManager.LoadScene(levelName);
    }
}
