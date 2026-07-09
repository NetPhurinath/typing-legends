using UnityEngine;

public class TypeMasterAI : MonoBehaviour
{
    [Header("อ้างอิงคอมโพเนนต์")]
    [Tooltip("ชุด Wordbank ทั้ง 10 ระดับ: เปลี่ยนตรงนี้ = เปลี่ยนว่าระดับไหนใช้ Wordbank ไหน")]
    [SerializeField] private TypeMasterAIWordbankTiers wordbankTiers;

    [Tooltip("การตั้งค่าเกณฑ์ปรับระดับ: เปลี่ยนตรงนี้ = เปลี่ยนความเร็ว/ความแม่นยำที่ใช้เลื่อนขึ้น-ลง")]
    [SerializeField] private TypeMasterAIDifficultySettings difficulty;

    [Tooltip("สัญญาณพฤติกรรมจากเกมพิมพ์: ถ้ามี จะใช้ช่วยปรับเกณฑ์เลื่อนระดับแบบอ่อน ๆ")]
    [SerializeField] private TypingStrategyProfiler strategyProfiler;

    [Header("Debug (อ่านอย่างเดียว)")]
    [SerializeField] private int currentTierIndex = 0;
    [SerializeField] private float emaWpm = 0f;
    [SerializeField, Range(0f, 1f)] private float emaAccuracy = 1f;
    [SerializeField, Range(0f, 1f)] private float strategyPlanningBias = 0f;
    [SerializeField, Range(0f, 1f)] private float strategyMonitoringBias = 0f;
    [SerializeField, Range(0f, 1f)] private float strategyTrialAndErrorBias = 0f;

    private int promoteCounter = 0;
    private int demoteCounter = 0;

    public int CurrentTierIndex => currentTierIndex;
    public float CurrentEstimatedWpm => emaWpm;
    public float CurrentEstimatedAccuracy => emaAccuracy;

    private void Awake()
    {
        if (wordbankTiers == null) wordbankTiers = GetComponent<TypeMasterAIWordbankTiers>();
        if (difficulty == null) difficulty = GetComponent<TypeMasterAIDifficultySettings>();
        if (strategyProfiler == null) strategyProfiler = Object.FindFirstObjectByType<TypingStrategyProfiler>(FindObjectsInactive.Include);

        if (wordbankTiers == null)
            Debug.LogError("TypeMasterAI: ต้องมีคอมโพเนนต์ TypeMasterAIWordbankTiers อยู่ด้วย");
        if (difficulty == null)
            Debug.LogError("TypeMasterAI: ต้องมีคอมโพเนนต์ TypeMasterAIDifficultySettings อยู่ด้วย");

        if (difficulty != null)
            currentTierIndex = Mathf.Clamp(difficulty.startTierIndex, 0, 9);
        else
            currentTierIndex = Mathf.Clamp(currentTierIndex, 0, 9);
    }

    // Typer จะเรียก GetWord() เหมือน Wordbank ปกติ
    public string GetWord()
    {
        if (wordbankTiers == null)
        {
            Debug.LogError("TypeMasterAI: wordbankTiers is null");
            return string.Empty;
        }

        int start = Mathf.Clamp(currentTierIndex, 0, 9);

        // ลองระดับปัจจุบันก่อน ถ้าไม่ได้ค่อย fallback ไปหา level อื่น
        for (int attempt = 0; attempt < wordbankTiers.TierCount; attempt++)
        {
            int idx = (start + attempt) % wordbankTiers.TierCount;
            if (wordbankTiers.TryGetWord(idx, out var word))
                return word;
        }

        Debug.LogError("TypeMasterAI: ไม่มี Wordbank ที่ใช้งานได้ (ทุกระดับว่างหรือไม่มี GetWord)");
        return string.Empty;
    }

    // Optional hook (Typer จะเรียกถ้ามี)
    public void OnWordStarted(string word)
    {
        // เว้นไว้เผื่ออนาคต (เช่น เก็บสถิติ/ทำ UI)
    }

    // Optional hook (Typer จะเรียกถ้ามี)
    public void OnWordResult(string word, float timeTakenSeconds, int mistakes, bool completed)
    {
        if (difficulty == null) return;

        timeTakenSeconds = Mathf.Max(0.01f, timeTakenSeconds);
        int correctChars = completed ? Mathf.Max(0, word?.Length ?? 0) : 0;
        int totalTyped = correctChars + Mathf.Max(0, mistakes);
        float accuracy = totalTyped <= 0 ? 0f : (float)correctChars / totalTyped;

        float minutes = timeTakenSeconds / 60f;
        float words = correctChars / 5f; // สูตร WPM มาตรฐาน (5 ตัวอักษร = 1 คำ)
        float wpm = minutes <= 0f ? 0f : (words / minutes);

        emaWpm = UpdateEma(emaWpm, wpm, difficulty.emaAlpha);
        emaAccuracy = UpdateEma(emaAccuracy, accuracy, difficulty.emaAlpha);

        UpdateTier();
    }

    private void UpdateTier()
    {
        if (difficulty == null) return;

        currentTierIndex = Mathf.Clamp(currentTierIndex, 0, 9);
        if (wordbankTiers != null && wordbankTiers.TierCount <= 1) return;

        var rule = difficulty.GetRule(currentTierIndex);
        bool canPromote = currentTierIndex < 9;
        bool canDemote = currentTierIndex > 0;

        UpdateStrategyBias();

        float promoteWpmThreshold = ApplyPlanningBias(rule.promoteWpm);
        float promoteAccuracyThreshold = ApplyPlanningAccuracyBias(rule.promoteAccuracy);
        float demoteWpmThreshold = ApplyTrialAndErrorWpmBias(rule.demoteWpm);
        float demoteAccuracyThreshold = ApplyTrialAndErrorAccuracyBias(rule.demoteAccuracy);

        bool promote = canPromote && emaWpm >= promoteWpmThreshold && emaAccuracy >= promoteAccuracyThreshold;
        bool demote = canDemote && (emaWpm <= demoteWpmThreshold || emaAccuracy <= demoteAccuracyThreshold);

        if (promote)
        {
            promoteCounter++;
            demoteCounter = 0;
        }
        else if (demote)
        {
            demoteCounter++;
            promoteCounter = 0;
        }
        else
        {
            promoteCounter = 0;
            demoteCounter = 0;
        }

        if (canPromote && promoteCounter >= difficulty.promoteStreak)
        {
            currentTierIndex++;
            promoteCounter = 0;
            demoteCounter = 0;
            return;
        }

        if (canDemote && demoteCounter >= difficulty.demoteStreak)
        {
            currentTierIndex--;
            promoteCounter = 0;
            demoteCounter = 0;
        }
    }

    private void UpdateStrategyBias()
    {
        if (strategyProfiler == null)
        {
            strategyPlanningBias = 0f;
            strategyMonitoringBias = 0f;
            strategyTrialAndErrorBias = 0f;
            return;
        }

        strategyPlanningBias = Mathf.Clamp01(strategyProfiler.PlanningScore);
        strategyMonitoringBias = Mathf.Clamp01(strategyProfiler.MonitoringScore);
        strategyTrialAndErrorBias = Mathf.Clamp01(strategyProfiler.TrialAndErrorScore);
    }

    private float ApplyPlanningBias(float threshold)
    {
        float easing = Mathf.Lerp(1f, 0.9f, strategyPlanningBias);
        return Mathf.Max(0f, threshold * easing);
    }

    private float ApplyPlanningAccuracyBias(float threshold)
    {
        float easing = Mathf.Lerp(0f, 0.03f, strategyPlanningBias);
        return Mathf.Clamp01(threshold - easing);
    }

    private float ApplyTrialAndErrorWpmBias(float threshold)
    {
        float tightening = Mathf.Lerp(1f, 1.15f, strategyTrialAndErrorBias);
        return Mathf.Max(0f, threshold * tightening);
    }

    private float ApplyTrialAndErrorAccuracyBias(float threshold)
    {
        float tightening = Mathf.Lerp(0f, 0.05f, strategyTrialAndErrorBias);
        return Mathf.Clamp01(threshold + tightening);
    }

    private static float UpdateEma(float previous, float sample, float alpha)
    {
        alpha = Mathf.Clamp01(alpha);
        return (alpha * sample) + ((1f - alpha) * previous);
    }
}
