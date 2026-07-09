using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class FoodInventory : MonoBehaviour
{
    private const string InventoryKeyPrefix = "Inventory_";

    private static readonly Dictionary<ItemType, int> sharedItems = new();
    private static bool sharedItemsInitialized;

    private Dictionary<ItemType, int> items => sharedItems;

    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Typer typer;

    [Header("Food")]
    [SerializeField] private int maxFood = 3;
    [SerializeField] private int healPerFood = 1;
    [SerializeField] private int shieldPerUse = 1;
    [SerializeField] private float timeBonusSeconds = 5f;
    

    [Header("Food UI")]
    [SerializeField] private TMP_Text foodOutput;
    [SerializeField] private GameObject foodIcon;

    [Header("Shield UI")]
    [SerializeField] private TMP_Text shieldOutput;
    [SerializeField] private GameObject shieldIcon;

    [Header("Time UI")]
    [SerializeField] private TMP_Text timeOutput;
    [SerializeField] private GameObject timeIcon;

    [Header("Skip Word UI")]
    [SerializeField] private TMP_Text skipWordOutput;
    [SerializeField] private GameObject skipWordIcon;

    [Header("Starting Items")]
    [SerializeField] private int startFood = 0;
    [SerializeField] private int startShield = 0;
    [SerializeField] private int startTime = 0;
    [SerializeField] private int startSkipWord = 0;

    private static void EnsureSharedItemsInitialized()
    {
        if (sharedItemsInitialized) return;

        foreach (ItemType type in System.Enum.GetValues(typeof(ItemType)))
        {
            sharedItems[type] = Mathf.Max(0, PlayerPrefs.GetInt(GetItemKey(type), 0));
        }

        sharedItemsInitialized = true;
    }

    private static string GetItemKey(ItemType type)
    {
        return InventoryKeyPrefix + type;
    }

    private static void SaveSharedInventory()
    {
        EnsureSharedItemsInitialized();

        foreach (ItemType type in System.Enum.GetValues(typeof(ItemType)))
        {
            PlayerPrefs.SetInt(GetItemKey(type), Mathf.Max(0, sharedItems[type]));
        }

        PlayerPrefs.Save();
    }

    public static void ResetSharedInventory()
    {
        EnsureSharedItemsInitialized();

        foreach (ItemType type in System.Enum.GetValues(typeof(ItemType)))
        {
            sharedItems[type] = 0;
        }

        SaveSharedInventory();
    }

    private void Awake()
    {
        EnsureSharedItemsInitialized();

        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (typer == null)
            typer = GetComponent<Typer>();

        if (typer == null)
            typer = FindFirstObjectByType<Typer>();
    }

    private void Start()
    {
        AddItem(ItemType.Food, startFood);
        AddItem(ItemType.Shield, startShield);
        AddItem(ItemType.Time, startTime);
        AddItem(ItemType.SkipWord, startSkipWord);

        RefreshAllUI();
    }

    public void OnFoodIconClicked()
    {
        UseItem(ItemType.Food);
        Debug.Log("Food Clicked");
    }

    public void OnShieldIconClicked()
    {
        UseItem(ItemType.Shield);
    }

    public void OnTimeIconClicked()
    {
        UseItem(ItemType.Time);
    }

    public void OnSkipWordIconClicked()
    {
        UseItem(ItemType.SkipWord);
    }

    public int GetItemCount(ItemType type)
    {
        return items.TryGetValue(type, out int count) ? count : 0;
    }

    public void AddItem(ItemType type, int amount = 1)
    {
        if (amount <= 0) return;

        if (!items.ContainsKey(type))
            items[type] = 0;

        if (type == ItemType.Food)
            items[type] = Mathf.Min(maxFood, items[type] + amount);
        else
            items[type] += amount;

        SaveSharedInventory();
        RefreshUI(type);
    }

    public void RefreshAllUI()
    {
        RefreshUI(ItemType.Food);
        RefreshUI(ItemType.Shield);
        RefreshUI(ItemType.Time);
        RefreshUI(ItemType.SkipWord);
    }

    public bool RemoveItem(ItemType type, int amount = 1)
    {
        if (!items.ContainsKey(type))
            return false;

        if (items[type] < amount)
            return false;

        items[type] -= amount;

        SaveSharedInventory();
        RefreshUI(type);

        return true;
    }

    public bool UseItem(ItemType type)
    {
        if (!items.ContainsKey(type)) return false;
        if (items[type] <= 0) return false;

        switch (type)
        {
            case ItemType.Food:

                if (playerHealth == null) return false;
                if (playerHealth.CurrentHealth >= playerHealth.MaxHealth) return false;

                playerHealth.Heal(healPerFood);
                break;

            case ItemType.Shield:

                if (playerHealth == null) return false;

                if (playerHealth.ShieldCount > 0)
                    return false;

                playerHealth.AddShield(shieldPerUse);
                break;

            case ItemType.Time:
                if (typer == null) return false;
                typer.AddTime(timeBonusSeconds);
                break;

            case ItemType.SkipWord:
                if (typer == null) return false;
                if (!typer.SkipCurrentWord()) return false;
                break;
        }

        RemoveItem(type);

        RefreshUI(type);

        Debug.Log("Use " + type);

        return true;
    }

    public bool HasItem(ItemType type)
    {
        return GetItemCount(type) > 0;
    }

    private void RefreshUI(ItemType type)
    {
        if (type == ItemType.Food)
        {
            if (foodOutput != null)
                foodOutput.text = items[type].ToString();

            if (foodIcon != null)
                foodIcon.SetActive(items[type] > 0);
        }
        else if (type == ItemType.Shield)
        {
            if (shieldOutput != null)
                shieldOutput.text = items[type].ToString();

            if (shieldIcon != null)
                shieldIcon.SetActive(items[type] > 0);
        }
        else if (type == ItemType.Time)
        {
            if (timeOutput != null)
                timeOutput.text = items[type].ToString();

            if (timeIcon != null)
                timeIcon.SetActive(items[type] > 0);
        }
        else if (type == ItemType.SkipWord)
        {
            if (skipWordOutput != null)
                skipWordOutput.text = items[type].ToString();

            if (skipWordIcon != null)
                skipWordIcon.SetActive(items[type] > 0);
        }
    }
}