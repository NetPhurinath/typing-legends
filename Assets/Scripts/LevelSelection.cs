using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

public class LevelSelection : MonoBehaviour
{
    [SerializeField] private bool unlocked = false;
    public Image unlockImage;
    public GameObject[] stars;
    public Sprite starSprite;
    public Sprite emptyStarSprite;

    private void Start()
    {
        UpdateLevelStatus();
        UpdateLevelImage();
    }

    private void UpdateLevelStatus()
    {
        string levelNumString = Regex.Match(gameObject.name, @"\d+").Value;
        if (!int.TryParse(levelNumString, out int levelNum) || levelNum <= 0)
        {
            unlocked = false;
            return;
        }

        if (levelNum == 1)
        {
            unlocked = true;
            return;
        }

        int previousLevelNum = levelNum - 1;
        if (PlayerPrefs.GetInt("Lv" + previousLevelNum.ToString(), 0) > 0)
        {
            unlocked = true;
        }
    }

    private void UpdateLevelImage()
    {
        if (!unlocked)
        {
            if (unlockImage != null)
                unlockImage.gameObject.SetActive(true);
            foreach (var s in stars)
                s.SetActive(false);
        }
        else
        {
            if (unlockImage != null)
                unlockImage.gameObject.SetActive(false);
            foreach (var s in stars)
                s.SetActive(true);

            string levelNumString = Regex.Match(gameObject.name, @"\d+").Value;
            int starCount = PlayerPrefs.GetInt("Lv" + levelNumString, 0);
            if (starCount < 0) starCount = 0;

            // Reset all stars first (prevents stale sprites)
            if (emptyStarSprite != null)
            {
                for (int i = 0; i < stars.Length; i++)
                {
                    if (stars[i] == null) continue;
                    var img = stars[i].GetComponent<Image>();
                    if (img == null) img = stars[i].GetComponentInChildren<Image>(true);
                    if (img != null) img.sprite = emptyStarSprite;
                }
            }

            for (int i = 0; i < starCount && i < stars.Length; i++)
            {
                if (stars[i] == null) continue;
                var img = stars[i].GetComponent<Image>();
                if (img == null) img = stars[i].GetComponentInChildren<Image>(true);
                if (img != null && starSprite != null) img.sprite = starSprite;
            }
        }
    }

    public void PressSelection(string levelName)
    {
        if (unlocked)
        {
            SceneManager.LoadScene(levelName);
        }
    }
}
