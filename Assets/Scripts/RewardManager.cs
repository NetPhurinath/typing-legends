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
        if (inventory == null)
            inventory = FindFirstObjectByType<FoodInventory>();

        if (inventory == null)
        {
            Debug.LogWarning("RewardManager: No FoodInventory found.");
            return null;
        }

        ItemType? reward = RollReward();

        if (!reward.HasValue)
        {
            Debug.Log("RewardManager: No reward granted.");
            return null;
        }

        // ถ้าไอเทมเต็ม ให้สุ่มใหม่
        if (inventory.IsItemFull(reward.Value))
        {
            Debug.Log($"RewardManager: {reward.Value} is full. Rolling again.");

            reward = RollAvailableReward();

            if (!reward.HasValue)
            {
                Debug.Log("RewardManager: All items are full. No reward granted.");
                return null;
            }
        }

        inventory.AddItem(reward.Value, 1);

        Debug.Log($"RewardManager: Granted {reward.Value}");

        return reward;
    }

    private ItemType? RollAvailableReward()
    {
        for (int i = 0; i < 100; i++)
        {
            ItemType? reward = RollReward();

            // สุ่มได้ Nothing
            if (!reward.HasValue)
                continue;

            // ไอเทมยังไม่เต็ม
            if (!inventory.IsItemFull(reward.Value))
                return reward;
        }

        return null;
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