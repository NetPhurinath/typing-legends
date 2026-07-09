using UnityEngine;
using UnityEngine.UI;

public class HeartUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image[] hearts; // Heart1, Heart2, Heart3 (in order)
    [SerializeField] private Image shieldHeart;

        private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.HealthChanged += OnHealthChanged;
            playerHealth.ShieldChanged += Refresh;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.HealthChanged -= OnHealthChanged;
            playerHealth.ShieldChanged -= Refresh;
        }
    }
    private void OnHealthChanged(int current, int max) => Refresh();

    private void Refresh()
    {
        if (playerHealth == null || hearts == null) return;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;
            hearts[i].enabled = i < playerHealth.CurrentHealth; // disappear when lost
        }

        if (shieldHeart != null)
        {
            shieldHeart.enabled = playerHealth.ShieldCount > 0;
        }
    }
}