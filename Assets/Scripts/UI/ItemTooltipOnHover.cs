using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attach to an item UI element (e.g. a Button/Image).
/// When the pointer hovers over it, it will show a tooltip/description text.
/// </summary>
public class ItemTooltipOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
 [TextArea]
 [SerializeField] private string description;

 [Header("UI Target")]
 [Tooltip("Text to show description (e.g. a TMP_Text on the right side of the items page).")]
 [SerializeField] private TMP_Text descriptionText;

 [Header("Behavior")]
 [SerializeField] private bool clearOnExit = true;
 [SerializeField] private string emptyText = "";

 private string previousText;
 private bool hadPrevious;

 public void SetDescriptionText(TMP_Text target) => descriptionText = target;

 public void OnPointerEnter(PointerEventData eventData)
 {
 if (descriptionText == null) return;

 previousText = descriptionText.text;
 hadPrevious = true;

 descriptionText.text = description;
 }

 public void OnPointerExit(PointerEventData eventData)
 {
 if (descriptionText == null) return;

 if (clearOnExit)
 {
 descriptionText.text = emptyText;
 return;
 }

 if (hadPrevious)
 descriptionText.text = previousText;
 }
}
