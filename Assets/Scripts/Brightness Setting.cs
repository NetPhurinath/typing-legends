using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Brightness control via a fullscreen UI overlay.
/// 
/// Persistence:
/// - This object is kept across scene loads.
/// - The dim overlay is created automatically if not assigned.
/// - Brightness is re-applied on every scene load.
/// 
/// UI interaction:
/// - The overlay must NOT block UI interaction.
/// </summary>
public class BrightnessSetting : MonoBehaviour
{
 private const string BrightnessKey = "brightness";

 [Header("UI")]
 [SerializeField] private Slider brightnessSlider;

 [Tooltip("Full-screen Image used as a black overlay. Color should be black; alpha is controlled by this script.")]
 [SerializeField] private Image dimOverlayImage;

 [Header("Range")]
 [Tooltip("Minimum dim amount (overlay alpha) when slider is at max brightness.")]
 [Range(0f,1f)]
 [SerializeField] private float minDim =0f;

 [Tooltip("Maximum dim amount (overlay alpha) when slider is at min brightness.")]
 [Range(0f,1f)]
 [SerializeField] private float maxDim =0.6f;

 private static BrightnessSetting instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            if (brightnessSlider != null)
            {
                instance.brightnessSlider = brightnessSlider;

                // Re-wire the slider's OnValueChanged to the surviving instance
                brightnessSlider.onValueChanged.RemoveAllListeners();
                brightnessSlider.onValueChanged.AddListener(instance.ChangeBrightness);

                instance.RefreshFromPrefs();
            }

            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureOverlayExists();
        EnsureOverlayDoesNotBlockInput();
    }

 private void OnEnable()
 {
 SceneManager.sceneLoaded += OnSceneLoaded;
 RefreshFromPrefs();
 }

 private void OnDisable()
 {
 SceneManager.sceneLoaded -= OnSceneLoaded;
 }

 private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
 {
 EnsureOverlayExists();
 EnsureOverlayDoesNotBlockInput();
 ApplySavedBrightness();
 }

 private void Start()
 {
 if (!PlayerPrefs.HasKey(BrightnessKey))
 PlayerPrefs.SetFloat(BrightnessKey,1f);

 RefreshFromPrefs();
 }

 /// <summary>
 /// Call this if you want to force the Options UI to reload the saved brightness.
 /// </summary>
 public void RefreshFromPrefs()
 {
 Load();
 ApplySavedBrightness();
 }

 // Hook this to the slider OnValueChanged.
 public void ChangeBrightness()
 {
 // Save even if the overlay is controlled from this UI.
 SaveCurrentBrightness();
 ApplySavedBrightness();
 }

// Runtime listener overload (called when slider is re-wired via code)
public void ChangeBrightness(float value)
{
    ChangeBrightness();
}

    private void EnsureOverlayDoesNotBlockInput()
 {
 if (dimOverlayImage == null) return;

 dimOverlayImage.raycastTarget = false;

 // Also ensure any canvas group on the overlay image can't block.
 var cg = dimOverlayImage.GetComponent<CanvasGroup>();
 if (cg != null) cg.blocksRaycasts = false;

 // Ensure the canvas hierarchy is non-interactive even if someone re-enables a raycaster.
 var canvas = dimOverlayImage.GetComponentInParent<Canvas>();
 if (canvas != null)
 {
 var canvasCg = canvas.GetComponent<CanvasGroup>();
 if (canvasCg == null) canvasCg = canvas.gameObject.AddComponent<CanvasGroup>();
 canvasCg.interactable = false;
 canvasCg.blocksRaycasts = false;
 }

 var raycaster = dimOverlayImage.GetComponentInParent<GraphicRaycaster>();
 if (raycaster != null) raycaster.enabled = false;
 }

 private void EnsureOverlayExists()
 {
 if (dimOverlayImage != null) return;

 // If an overlay already exists under this persistent object, reuse it.
 var existing = transform.Find("BrightnessOverlayCanvas/DimOverlay");
 if (existing != null)
 {
 dimOverlayImage = existing.GetComponent<Image>();
 if (dimOverlayImage != null) return;
 }

 // Create a dedicated overlay Canvas so the setting works in every scene.
 var canvasGo = new GameObject("BrightnessOverlayCanvas");
 canvasGo.transform.SetParent(transform, false);

 var canvas = canvasGo.AddComponent<Canvas>();
 canvas.renderMode = RenderMode.ScreenSpaceOverlay;
 canvas.sortingOrder =10000; // high, but avoid max value edge cases

 // Make the whole overlay canvas non-interactive.
 var canvasCg = canvasGo.AddComponent<CanvasGroup>();
 canvasCg.interactable = false;
 canvasCg.blocksRaycasts = false;

 canvasGo.AddComponent<CanvasScaler>();
 canvasGo.AddComponent<GraphicRaycaster>();

 var overlayGo = new GameObject("DimOverlay");
 overlayGo.transform.SetParent(canvasGo.transform, false);

 var rect = overlayGo.AddComponent<RectTransform>();
 rect.anchorMin = Vector2.zero;
 rect.anchorMax = Vector2.one;
 rect.offsetMin = Vector2.zero;
 rect.offsetMax = Vector2.zero;

 dimOverlayImage = overlayGo.AddComponent<Image>();
 dimOverlayImage.color = new Color(0f,0f,0f,0f);
 dimOverlayImage.raycastTarget = false;

 // We don't need raycast handling on this canvas at all.
 var gr = canvasGo.GetComponent<GraphicRaycaster>();
 if (gr != null) gr.enabled = false;
 }

 private void ApplySavedBrightness()
 {
 if (dimOverlayImage == null) return;

 float brightness = Mathf.Clamp01(PlayerPrefs.GetFloat(BrightnessKey,1f));

 // Keep slider UI in sync when present (e.g., when returning to Options).
 if (brightnessSlider != null && !Mathf.Approximately(brightnessSlider.value, brightness))
 brightnessSlider.value = brightness;

 // Map brightness (1->0 dim,0->max dim)
 float dim = Mathf.Lerp(maxDim, minDim, brightness);

 var c = dimOverlayImage.color;
 c.r =0f;
 c.g =0f;
 c.b =0f;
 c.a = dim;
 dimOverlayImage.color = c;
 }

 private void Load()
 {
 if (brightnessSlider != null)
 brightnessSlider.value = PlayerPrefs.GetFloat(BrightnessKey,1f);
 }

 private void SaveCurrentBrightness()
 {
 float brightness = brightnessSlider != null ? brightnessSlider.value : PlayerPrefs.GetFloat(BrightnessKey,1f);
 PlayerPrefs.SetFloat(BrightnessKey, Mathf.Clamp01(brightness));
 PlayerPrefs.Save();
 }
}
