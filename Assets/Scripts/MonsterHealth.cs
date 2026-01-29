using System;
using UnityEngine;

public class MonsterHealth : MonoBehaviour
{
 [Header("Health")]
 [SerializeField] private int maxHealth =5;

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
 if (maxHealth <1) maxHealth =1;
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
 if (amount <=0) return;

 currentHealth -= amount;
 if (currentHealth <=0)
 {
 currentHealth =0;
 HealthChanged?.Invoke(currentHealth, maxHealth);
 Die();
 return;
 }

 HealthChanged?.Invoke(currentHealth, maxHealth);
 }

 public void Heal(int amount)
 {
 if (isDead) return;
 if (amount <=0) return;

 currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
 HealthChanged?.Invoke(currentHealth, maxHealth);
 }

 private void Die()
 {
 isDead = true;

 // Treat monster death as win/level clear. If there is a UI, show it; otherwise go next level.
 if (gameOverScreen != null)
 {
 int points =0;
 if (typer != null) points = typer.Score;
 gameOverScreen.Show(points);
 }
 else
 {
 Debug.Log("Monster died.");
 }
 }
}
