using UnityEngine;

public class RewardManager : MonoBehaviour
{
    private const float NothingChance = 0.5f;
    private const float FoodChance = 0.125f;
    private const float ShieldChance = 0.125f;
    private const float TimeChance = 0.125f;

    [SerializeField] private FoodInventory inventory;

    private void Awake()
    {
        if (inventory == null)
            inventory = GetComponent<FoodInventory>();

        if (inventory == null)
            inventory = FindFirstObjectByType<FoodInventory>();
    }

    public ItemType? GrantLevelReward()
    {
        ItemType? reward = RollReward();

        if (reward.HasValue)
        {
            if (inventory == null)
                inventory = FindFirstObjectByType<FoodInventory>();

            if (inventory != null)
            {
                inventory.AddItem(reward.Value, 1);
                Debug.Log($"RewardManager: Granted {reward.Value}");
            }
            else
            {
                Debug.LogWarning("RewardManager: No FoodInventory found to receive the reward.");
            }
        }
        else
        {
            Debug.Log("RewardManager: No reward granted.");
        }

        return reward;
    }

    public ItemType? RollReward()
    {
        float roll = Random.value;

        if (roll < NothingChance)
            return null;

        roll -= NothingChance;

        if (roll < FoodChance)
            return ItemType.Food;

        roll -= FoodChance;

        if (roll < ShieldChance)
            return ItemType.Shield;

        roll -= ShieldChance;

        if (roll < TimeChance)
            return ItemType.Time;

        return ItemType.SkipWord;
    }
}