using UnityEngine;
using UnityEngine.UI;

public class MonsterHeartUI : MonoBehaviour
{
 [SerializeField] private MonsterHealth monsterHealth;
 [SerializeField] private Image[] hearts; //5 hearts in order

 private void OnEnable()
 {
 if (monsterHealth != null)
 monsterHealth.HealthChanged += OnHealthChanged;

 Refresh();
 }

 private void OnDisable()
 {
 if (monsterHealth != null)
 monsterHealth.HealthChanged -= OnHealthChanged;
 }

 private void OnHealthChanged(int current, int max) => Refresh();

 private void Refresh()
 {
 if (monsterHealth == null || hearts == null) return;

 for (int i =0; i < hearts.Length; i++)
 {
 if (hearts[i] == null) continue;
 hearts[i].enabled = i < monsterHealth.CurrentHealth;
 }
 }
}
