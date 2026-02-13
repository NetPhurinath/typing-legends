using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;

    [Header("References (optional)")]
    [SerializeField] private GameOverScreen gameOverScreen;
    [SerializeField] private Typer typer;

    private int currentHealth;
    private bool isDead;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public event Action<int, int> HealthChanged; // current, max

    private void Awake()
    {
        if (typer == null) typer = Object.FindFirstObjectByType<Typer>();
        if (gameOverScreen == null)
        {
            var endScreens = Object.FindObjectsByType<GameOverScreen>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var screen in endScreens)
            {
                if (screen is GameWinScreen) continue;
                gameOverScreen = screen;
                break;
            }

            if (gameOverScreen == null)
                gameOverScreen = Object.FindFirstObjectByType<GameOverScreen>(FindObjectsInactive.Include);
        }

        if (maxHealth < 1) maxHealth = 1;
        currentHealth = maxHealth;
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void ResetHealth()
    {
        isDead = false;
        currentHealth = maxHealth;
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;
        if (amount <= 0) return;

        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            HealthChanged?.Invoke(currentHealth, maxHealth);
            Die();
            return;
        }

        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        if (amount <= 0) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        isDead = true;

        if (gameOverScreen != null)
        {
            int points = 0;
            if (typer != null) points = typer.Score;
            else points = ScoreKeeper.LastScore;

            ScoreKeeper.Set(points);
            gameOverScreen.Show(points);
        }
        else
        {
            Debug.LogWarning("PlayerHealth: gameOverScreen is not set.");
        }
    }
}
