using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class StrategyDebugUI : MonoBehaviour
{
    [SerializeField] private TypingStrategyProfiler profiler;
    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private GameObject panel;
    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (profiler == null)
            profiler = TypingStrategyProfiler.Instance;

        if (profiler == null)
            profiler = Object.FindFirstObjectByType<TypingStrategyProfiler>(FindObjectsInactive.Include);

        if (panel == null)
            panel = gameObject;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        ConfigureText();

        ApplyVisibility(false);
    }

    private void Update()
    {
        TogglePanel();
        UpdateDebugText();
    }

    private void TogglePanel()
    {
        if (!Input.GetKeyDown(KeyCode.F1))
            return;

        if (canvasGroup != null)
        {
            bool shouldShow = canvasGroup.alpha <= 0.01f;
            ApplyVisibility(shouldShow);
            return;
        }

        if (panel != null && panel != gameObject)
            panel.SetActive(!panel.activeSelf);
    }

    private void UpdateDebugText()
    {
        if (profiler == null || debugText == null)
            return;

        debugText.text =
            $"Profile Average\n" +
            $"Planning Score: {profiler.PlanningScore:P0}\n" +
            $"Monitoring Score: {profiler.MonitoringScore:P0}\n" +
            $"Trial-and-Error Score: {profiler.TrialAndErrorScore:P0}\n" +
            $"Sample Count: {profiler.SampleCount}\n\n" +
            $"Latest Attempt\n" +
            $"First Input Delay: {profiler.LastFirstInputDelay:0.00}s\n" +
            $"Average Interval: {profiler.LastAverageInterval:0.00}s\n" +
            $"Mistake Rate: {profiler.LastMistakeRate:P0}";
    }

    private void ConfigureText()
    {
        if (debugText == null)
            return;

        debugText.textWrappingMode = TextWrappingModes.Normal;
        debugText.overflowMode = TextOverflowModes.Overflow;
        debugText.alignment = TextAlignmentOptions.TopLeft;
        debugText.enableAutoSizing = false;

        debugText.fontSize = 14f;
        debugText.margin = new Vector4(8f, 8f, 8f, 8f);

        var textRect = debugText.rectTransform;
        if (textRect != null)
        {
            textRect.anchorMin = new Vector2(0f, 1f);
            textRect.anchorMax = new Vector2(0f, 1f);
            textRect.pivot = new Vector2(0f, 1f);
            textRect.anchoredPosition = new Vector2(16f, -16f);
            textRect.sizeDelta = new Vector2(320f, 180f);
        }
    }

    private void ApplyVisibility(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }

        if (debugText != null)
            debugText.enabled = visible;

        if (panel != null && panel != gameObject && canvasGroup == null)
            panel.SetActive(visible);
    }
}
