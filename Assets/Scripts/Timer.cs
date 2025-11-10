using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public float timeRemaining;

  
    private void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;

        }
        else if (timeRemaining <= 0)
        {
           timeRemaining = 0;
        }
        
        timerText.text = "Time: " + Mathf.Ceil(timeRemaining).ToString();
    }
}
