using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameOverScreen gameOverScreen; // Reference to the pop-up
    [SerializeField] private float timeRemaining = 60f; // Default to 60 or set in Inspector

    private bool isGameOver = false; // Flag to prevent triggering multiple times

    private void Update()
    {
        // If the game is already over, do nothing
        if (isGameOver) return;

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
        }
        else
        {
            // Time is up!
            timeRemaining = 0;
            isGameOver = true; // Set flag so this only runs once

            // Trigger the Game Over screen
            // Pass the score or '0' if you don't calculate points yet
            gameOverScreen.Show(100);
        }

        // Update the UI text
        if (timerText != null)
        {
            timerText.text = "Time: " + Mathf.Ceil(timeRemaining).ToString();
        }
    }
}