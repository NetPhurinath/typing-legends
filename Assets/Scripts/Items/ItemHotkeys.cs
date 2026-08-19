using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Allow using items by pressing F1..F4 mapped to inventory slots.
/// Default mapping:
///  F1 -> ItemType.Food
///  F2 -> ItemType.Shield
///  F3 -> ItemType.Time
///  F4 -> ItemType.SkipWord
///
/// Usage:
/// - Attach to a persistent GameObject (e.g. a UI manager or the FoodInventory object).
/// - Optionally assign `inventory` in the Inspector; if left empty the script will auto-find one.
/// - Toggle `allowWhenPaused` if you want hotkeys to work while Time.timeScale == 0.
/// - Prevents triggering while typing into UI input fields.
/// </summary>
[DisallowMultipleComponent]
public class ItemHotkeys : MonoBehaviour
{
    [Header("References (optional)")]
    [SerializeField] private FoodInventory inventory;

    [Header("Behavior")]
    [Tooltip("Allow item hotkeys even when game is paused (Time.timeScale == 0).")]
    [SerializeField] private bool allowWhenPaused = false;

    [Tooltip("Ignore hotkeys when an UI input/selectable is focused.")]
    [SerializeField] private bool ignoreWhenUIFocused = true;

    [Header("Slot mapping (F1..F4)")]
    [SerializeField] private ItemType[] slotMapping = new ItemType[]
    {
        ItemType.Food,     // F1
        ItemType.Shield,   // F2
        ItemType.Time,     // F3
        ItemType.SkipWord  // F4
    };

    private void Awake()
    {
        if (inventory == null)
        {
            // Try to auto-find the inventory (matches project pattern used elsewhere)
#if UNITY_2023_1_OR_NEWER
            inventory = Object.FindFirstObjectByType<FoodInventory>(FindObjectsInactive.Include);
#else
            inventory = FindObjectOfType<FoodInventory>();
#endif
        }
    }

    private void Update()
    {
        if (!allowWhenPaused && Time.timeScale <= 0f)
            return;

        if (ignoreWhenUIFocused && IsUIInputFocused())
            return;

        // Check F1..F4
        if (Input.GetKeyDown(KeyCode.F1)) TryUseSlot(0);
        if (Input.GetKeyDown(KeyCode.F2)) TryUseSlot(1);
        if (Input.GetKeyDown(KeyCode.F3)) TryUseSlot(2);
        if (Input.GetKeyDown(KeyCode.F4)) TryUseSlot(3);
    }

    private bool IsUIInputFocused()
    {
        // If EventSystem says a selectable is selected, avoid hotkeys (so typing into fields won't trigger)
        if (EventSystem.current == null) return false;
        var selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null) return false;

        // If selected is an input field / selectable, treat as focused.
        // We don't explicitly depend on TMP types here to avoid hard dependency.
        return selected.GetComponent<UnityEngine.UI.InputField>() != null
            || selected.GetComponent<TMPro.TMP_InputField>() != null
            || selected.GetComponent<UnityEngine.UI.Selectable>() != null;
    }

    private void TryUseSlot(int slotIndex)
    {
        if (slotMapping == null || slotIndex < 0 || slotIndex >= slotMapping.Length)
            return;

        var item = slotMapping[slotIndex];

        if (inventory == null)
        {
            Debug.LogWarning($"ItemHotkeys: No FoodInventory assigned/found when trying to use {item} (slot {slotIndex + 1}).");
            return;
        }

        bool success = inventory.UseItem(item);

        // Log for diagnostics if raw logger exists in project
        var logger = RawTypingEventLogger.Instance;
        if (logger != null)
        {
            logger.LogItemUsed(item.ToString(), success, string.Empty);
        }

        if (success)
        {
            Debug.Log($"ItemHotkeys: Used {item} from slot {slotIndex + 1}.");
        }
        else
        {
            Debug.Log($"ItemHotkeys: Failed to use {item} from slot {slotIndex + 1}.");
        }
    }
}