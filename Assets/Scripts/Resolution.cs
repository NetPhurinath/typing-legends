using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;


public class ResolutionManager : MonoBehaviour
{
    public TMP_Dropdown ResDropdown;
    public Toggle fullscreenToggle;
    UnityEngine.Resolution[] Allresolutions;
    int currentResolutionIndex;
    bool isFullscreen;
    List<UnityEngine.Resolution> SelectedResolutionList = new List<UnityEngine.Resolution>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       isFullscreen = true;
        Allresolutions = Screen.resolutions;
        ResDropdown.ClearOptions();
        List<string> resolutionStringList = new List<string>();
        string newRes;
        foreach (UnityEngine.Resolution res in Allresolutions)
        {
            newRes = res.width.ToString() + " x " + res.height.ToString();
            if (!resolutionStringList.Contains(newRes))
            {
                resolutionStringList.Add(newRes);
                SelectedResolutionList.Add(res);
            }  
        }
        ResDropdown.AddOptions(resolutionStringList);
    }

    public void ChangeResolution(int resolutionIndex)
    {
        currentResolutionIndex = ResDropdown.value;
        Screen.SetResolution(SelectedResolutionList[currentResolutionIndex].width, SelectedResolutionList[currentResolutionIndex].height, isFullscreen);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        this.isFullscreen = fullscreenToggle.isOn;
        Screen.SetResolution(SelectedResolutionList[currentResolutionIndex].width, SelectedResolutionList[currentResolutionIndex].height, isFullscreen);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
