using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Pause overlay UI (similar to `GameOverScreen`'s overlay pattern).
///
/// Usage:
/// - Put this on a Canvas panel.
/// - Assign `backgroundOverlay` (optional) and buttons (optional).
/// - Press Escape (default) to toggle pause.
///
/// This pauses the game via `Time.timeScale =0`.
/// </summary>
public class PauseOverlay : MonoBehaviour
{
 [Header("Visibility")]
 [SerializeField] private GameObject backgroundOverlay;

 [Header("Text (optional)")]
 [SerializeField] private TMP_Text titleText;
 [SerializeField] private Text titleTextLegacy;
 [SerializeField] private string pausedTitle = "PAUSED";

 [Header("Buttons (main) - optional")]
 [SerializeField] private Button resumeButton;
 [SerializeField] private Button restartButton;
 [SerializeField] private Button mainMenuButton;

 [Header("Pages")]
 [Tooltip("Root panel that contains the main pause buttons (Resume/Restart/etc).")]
 [SerializeField] private GameObject mainPage;

 [Tooltip("Root panel for Items page.")]
 [SerializeField] private GameObject itemsPage;

 [Tooltip("Root panel for Options page.")]
 [SerializeField] private GameObject optionsPage;

 [Header("Buttons (navigation) - optional")]
 [SerializeField] private Button itemsButton;
 [SerializeField] private Button optionsButton;
 [SerializeField] private Button backButton;

 [Header("Items Page (optional)")]
 [Tooltip("Text that displays item description when hovering an item.")]
 [SerializeField] private TMP_Text itemsDescriptionText;

 [Header("Input")]
 [SerializeField] private bool enableToggleKey = true;
 [SerializeField] private KeyCode toggleKey = KeyCode.Escape;

 private enum Page
 {
 Main,
 Items,
 Options,
 }

 private CanvasGroup canvasGroup;
 private float previousTimeScale =1f;
 private bool isPaused;

 private bool resumeHooked;
 private bool restartHooked;
 private bool menuHooked;
 private bool itemsHooked;
 private bool optionsHooked;
 private bool backHooked;

 private Page currentPage = Page.Main;

 private void Awake()
 {
 // Ensure we can hide/show without disabling the GameObject.
 canvasGroup = GetComponent<CanvasGroup>();
 if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

 // Start hidden.
 SetPanelVisible(false);
 if (backgroundOverlay != null) backgroundOverlay.SetActive(false);

 ApplyTitle();
 EnsureButtonHooks();
 ShowPage(Page.Main);
 }

 private void Update()
 {
 if (!enableToggleKey) return;

 if (Input.GetKeyDown(toggleKey))
 {
 if (isPaused) Resume();
 else Pause();
 }
 }

 private void OnDisable()
 {
 // Safety: if this object is disabled while paused, resume time.
 if (isPaused)
 Resume();
 }

 // Call this from a UI Button OnClick()
 public void OpenFromButton() => Pause();

 // Optional: use one button to open/close
 public void ToggleFromButton() => Toggle();

 public void Pause()
 {
 if (isPaused) return;

 ApplyTitle();
 EnsureButtonHooks();
 ShowPage(Page.Main);

 previousTimeScale = Time.timeScale;
 Time.timeScale =0f;

 isPaused = true;
 if (backgroundOverlay != null) backgroundOverlay.SetActive(true);
 SetPanelVisible(true);
 }

 public void Resume()
 {
 if (!isPaused) return;

 isPaused = false;
 if (backgroundOverlay != null) backgroundOverlay.SetActive(false);
 SetPanelVisible(false);

 Time.timeScale = previousTimeScale;
 }

 public void Toggle()
 {
 if (isPaused) Resume();
 else Pause();
 }

 public void OnResumePressed() => Resume();

 public void OnRestartPressed()
 {
 // Always restore time before loading.
 Time.timeScale =1f;
 var scene = SceneManager.GetActiveScene();
 SceneManager.LoadScene(scene.buildIndex);
 }

 public void OnMainMenuPressed()
 {
 // Match the project convention used by `GameOverScreen`.
 Time.timeScale =1f;

 const string target = "LevelSelection";
 if (Application.CanStreamedLevelBeLoaded(target))
 {
 SceneManager.LoadScene(target);
 return;
 }

 // Fallback for older spaced name.
 const string fallback = "Level Selection";
 if (Application.CanStreamedLevelBeLoaded(fallback))
 {
 SceneManager.LoadScene(fallback);
 return;
 }

 Debug.LogError($"{nameof(PauseOverlay)}: Cannot load LevelSelection scene. Add it to Scenes In Build.");
 }

 // Navigation
 public void OnItemsPressed() => ShowPage(Page.Items);
 public void OnOptionsPressed() => ShowPage(Page.Options);
 public void OnBackPressed() => ShowPage(Page.Main);

 private void ShowPage(Page page)
 {
 currentPage = page;

 if (mainPage != null) mainPage.SetActive(page == Page.Main);
 if (itemsPage != null) itemsPage.SetActive(page == Page.Items);
 if (optionsPage != null) optionsPage.SetActive(page == Page.Options);

 // Back button visibility: only show when not on main.
 if (backButton != null)
 backButton.gameObject.SetActive(page != Page.Main);

 // If we have a description text, clear it when page changes.
 if (itemsDescriptionText != null && page != Page.Items)
 itemsDescriptionText.text = string.Empty;

 // Refresh items list when entering items.
 if (page == Page.Items)
 {
 TryWireItemsHoverTargets();
 }
 }

 private void TryWireItemsHoverTargets()
 {
 if (itemsDescriptionText == null) return;
 if (itemsPage == null) return;

 // Wire any manually placed hover elements.
 var hovers = itemsPage.GetComponentsInChildren<ItemTooltipOnHover>(true);
 if (hovers == null) return;

 foreach (var hover in hovers)
 {
 if (hover == null) continue;
 hover.SetDescriptionText(itemsDescriptionText);
 }
 }

 private void ApplyTitle()
 {
 if (titleText != null) titleText.text = pausedTitle;
 if (titleTextLegacy != null) titleTextLegacy.text = pausedTitle;
 }

 private void EnsureButtonHooks()
 {
 if (resumeButton != null && !resumeHooked)
 {
 resumeButton.onClick.RemoveAllListeners();
 resumeButton.onClick.AddListener(OnResumePressed);
 resumeHooked = true;
 }

 if (restartButton != null && !restartHooked)
 {
 restartButton.onClick.RemoveAllListeners();
 restartButton.onClick.AddListener(OnRestartPressed);
 restartHooked = true;
 }

 if (mainMenuButton != null && !menuHooked)
 {
 mainMenuButton.onClick.RemoveAllListeners();
 mainMenuButton.onClick.AddListener(OnMainMenuPressed);
 menuHooked = true;
 }

 if (itemsButton != null && !itemsHooked)
 {
 itemsButton.onClick.RemoveAllListeners();
 itemsButton.onClick.AddListener(OnItemsPressed);
 itemsHooked = true;
 }

 if (optionsButton != null && !optionsHooked)
 {
 optionsButton.onClick.RemoveAllListeners();
 optionsButton.onClick.AddListener(OnOptionsPressed);
 optionsHooked = true;
 }

 if (backButton != null && !backHooked)
 {
 backButton.onClick.RemoveAllListeners();
 backButton.onClick.AddListener(OnBackPressed);
 backHooked = true;
 }
 }

 private void SetPanelVisible(bool visible)
 {
 if (canvasGroup == null) return;
 canvasGroup.alpha = visible ?1f :0f;
 canvasGroup.interactable = visible;
 canvasGroup.blocksRaycasts = visible;
 }
}
