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

    private void Start()
    {
        UpdateLevelStatus();
        UpdateLevelImage();
    }

    private void UpdateLevelStatus()
    {
        string levelNumString = Regex.Match(gameObject.name, @"\d+").Value;
        int levelNum = int.Parse(levelNumString);

        if (levelNum == 1)
        {
            unlocked = true;
            return;
        }

        int previousLevelNum = levelNum - 1;
        if (PlayerPrefs.GetInt("Lv" + previousLevelNum.ToString()) > 0)
        {
            unlocked = true;
        }
    }

    private void UpdateLevelImage()
    {
        if (!unlocked)
        {
            unlockImage.gameObject.SetActive(true);
            foreach (var s in stars)
                s.SetActive(false);
        }
        else
        {
            unlockImage.gameObject.SetActive(false);
            foreach (var s in stars)
                s.SetActive(true);

            string levelNumString = Regex.Match(gameObject.name, @"\d+").Value;
            int starCount = PlayerPrefs.GetInt("Lv" + levelNumString);

            for (int i = 0; i < starCount && i < stars.Length; i++)
            {
                stars[i].GetComponent<Image>().sprite = starSprite;
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
