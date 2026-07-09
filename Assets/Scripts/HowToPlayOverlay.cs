using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// How-To-Play overlay with page navigation.
///
/// Setup (in Unity scene):
/// - Put this on a panel (overlay root) under a Canvas.
/// - Assign `backgroundOverlay` (optional dim background).
/// - Create3 page GameObjects (page1/page2/page3) and assign them.
/// - Assign `leftArrowButton`, `rightArrowButton`, `closeButton`.
/// - Hook your "How To Play" menu button to `OpenFromButton()`.
///
/// Behavior:
/// - Shows page1 on open.
/// - Page1: no left arrow.
/// - Page3: no right arrow.
/// - Close hides the overlay.
/// </summary>
[DisallowMultipleComponent]
public class HowToPlayOverlay : MonoBehaviour
{
    [Header("Visibility")]
    [SerializeField] private GameObject backgroundOverlay;

    [Header("Pages")]
    [SerializeField] private GameObject[] pages;

    [Header("Buttons")]
    [SerializeField] private Button leftArrowButton;
    [SerializeField] private Button rightArrowButton;
    [SerializeField] private Button closeButton;

    [Header("Options")]
    [SerializeField, Min(0)] private int startPageIndex = 0;

    private CanvasGroup canvasGroup;
    private int currentPageIndex;

    [SerializeField] private bool openOnStart = true;

    private void Start()
    {
    if (PlayerPrefs.GetInt("HowToPlayShown", 0) == 0)
    {
        Open();
    }
    }

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        HookButtons();

        // Start hidden.
        SetPanelVisible(false);
        if (backgroundOverlay != null) backgroundOverlay.SetActive(false);

        // Ensure page state is consistent in editor/runtime.
        currentPageIndex = Mathf.Clamp(startPageIndex, 0, Mathf.Max(0, (pages?.Length ?? 0) - 1));
        RefreshPages();
    }

    // Call from UI button
    public void OpenFromButton() => Open();

    public void Open()
    {
        currentPageIndex = Mathf.Clamp(startPageIndex, 0, Mathf.Max(0, (pages?.Length ?? 0) - 1));
        RefreshPages();

        if (backgroundOverlay != null) backgroundOverlay.SetActive(true);
        SetPanelVisible(true);
    }

    public void Close()
    {
        SetPanelVisible(false);
        if (backgroundOverlay != null) backgroundOverlay.SetActive(false);
        PlayerPrefs.SetInt("HowToPlayShown", 1);
    }

    public void NextPage()
    {
        if (pages == null || pages.Length == 0) return;
        currentPageIndex = Mathf.Min(currentPageIndex + 1, pages.Length - 1);
        RefreshPages();
    }

    public void PreviousPage()
    {
        if (pages == null || pages.Length == 0) return;
        currentPageIndex = Mathf.Max(currentPageIndex - 1, 0);
        RefreshPages();
    }

    private void HookButtons()
    {
        // Only add runtime listeners if there are no persistent listeners added in the Inspector.
        if (leftArrowButton != null)
        {
            if (leftArrowButton.onClick.GetPersistentEventCount() == 0)
                leftArrowButton.onClick.AddListener(PreviousPage);
        }

        if (rightArrowButton != null)
        {
            if (rightArrowButton.onClick.GetPersistentEventCount() == 0)
                rightArrowButton.onClick.AddListener(NextPage);
        }

        if (closeButton != null)
        {
            if (closeButton.onClick.GetPersistentEventCount() == 0)
                closeButton.onClick.AddListener(Close);
        }
    }

    private void RefreshPages()
    {
        if (pages != null)
        {
            for (int i = 0; i < pages.Length; i++)
            {
                if (pages[i] == null) continue;
                pages[i].SetActive(i == currentPageIndex);
            }
        }

        if (leftArrowButton != null)
            leftArrowButton.gameObject.SetActive(currentPageIndex > 0);

        if (rightArrowButton != null)
            rightArrowButton.gameObject.SetActive(pages != null && currentPageIndex < pages.Length - 1);
    }

    private void SetPanelVisible(bool visible)
    {
        if (canvasGroup == null) return;
        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }
}
