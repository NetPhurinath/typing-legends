using UnityEngine;

/// <summary>
/// Item definition + behaviour for "tomyumshrimp".
/// Keeps the same properties as the current hard-coded food behaviour:
/// - Heals +1 health when used.
///
/// Attach this to a GameObject (or keep it as a reference on another script)
/// and call <see cref="TryUse"/> when the item is activated.
/// </summary>
public class TomyumShrimpItem : MonoBehaviour
{
 [Header("Item")]
 [SerializeField] private string itemName = "tomyumshrimp";

 [Header("Properties")]
 [SerializeField] private int healAmount =1;

 public string ItemName => itemName;
 public int HealAmount => healAmount;

 /// <summary>
 /// Attempts to use the item.
 /// Returns true if the item was consumed/activated.
 /// </summary>
 public bool TryUse(PlayerHealth playerHealth)
 {
 if (playerHealth == null) return false;
 if (playerHealth.CurrentHealth >= playerHealth.MaxHealth) return false;

 playerHealth.Heal(healAmount);
 return true;
 }
}
