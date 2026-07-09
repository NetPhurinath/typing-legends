using System.Collections.Generic;
using UnityEngine;

public sealed class TypingStrategyProfiler : MonoBehaviour
{
    public static TypingStrategyProfiler Instance { get; private set; }

    private const string PrefsPrefix = "TypingLegends.Strategy.";
    private const string PrefsVersionKey = PrefsPrefix + "Version";
    private const int PrefsVersion = 1;
    private const string PrefsHasSamplesKey = PrefsPrefix + "HasSamples";
    private const string PrefsSampleCountKey = PrefsPrefix + "SampleCount";
    private const string PrefsPlanningKey = PrefsPrefix + "Planning";
    private const string PrefsMonitoringKey = PrefsPrefix + "Monitoring";
    private const string PrefsTrialAndErrorKey = PrefsPrefix + "TrialAndError";

    [Header("Profile smoothing")]
    [SerializeField, Range(0.05f, 0.5f)] private float emaAlpha = 0.20f;
    [SerializeField] private bool persistProfile = true;

    [Header("Debug (read-only)")]
    [SerializeField, Range(0f, 1f)] private float planningScore;
    [SerializeField, Range(0f, 1f)] private float monitoringScore;
    [SerializeField, Range(0f, 1f)] private float trialAndErrorScore;
    [SerializeField] private int sampleCount;
    [SerializeField] private float lastFirstInputDelay;
    [SerializeField] private float lastAverageInterval;
    [SerializeField] private float lastMistakeRate;

    private bool hasActiveAttempt;
    private AttemptSession activeAttempt;

    public bool HasSamples => sampleCount > 0;
    public float PlanningScore => planningScore;
    public float MonitoringScore => monitoringScore;
    public float TrialAndErrorScore => trialAndErrorScore;
    public int SampleCount => sampleCount;
    public float LastFirstInputDelay => lastFirstInputDelay;
    public float LastAverageInterval => lastAverageInterval;
    public float LastMistakeRate => lastMistakeRate;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("TypingStrategyProfiler: duplicate instance detected. Using the first instance found.");
            enabled = false;
            return;
        }

        Instance = this;
        LoadFromPrefs();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void OnDisable()
    {
        if (persistProfile)
        {
            SaveToPrefs();
            PlayerPrefs.Save();
        }
    }

    public void BeginAttempt(string word, float timeLimitSeconds)
    {
        activeAttempt = new AttemptSession(word, timeLimitSeconds);
        hasActiveAttempt = true;
    }

    public void RegisterKeyPress(bool isCorrect)
    {
        if (!hasActiveAttempt)
            return;

        activeAttempt.RegisterKeyPress(Time.time, isCorrect);
    }

    public void CompleteAttempt(bool completed, float timeTakenSeconds, int mistakes)
    {
        if (!hasActiveAttempt)
            return;

        var features = activeAttempt.BuildFeatures(completed, timeTakenSeconds, mistakes, Time.time);
        ApplyFeatures(features);
        hasActiveAttempt = false;

        if (persistProfile)
            SaveToPrefs();
    }

    public void ResetProfile(bool clearSavedState = false)
    {
        planningScore = 0f;
        monitoringScore = 0f;
        trialAndErrorScore = 0f;
        sampleCount = 0;
        lastFirstInputDelay = 0f;
        lastAverageInterval = 0f;
        lastMistakeRate = 0f;
        hasActiveAttempt = false;

        if (clearSavedState)
            ClearSavedState();
    }

    [ContextMenu("Reset Strategy Profile (Clear Saved Data)")]
    private void ResetStrategyProfileFromInspector()
    {
        ResetProfile(clearSavedState: true);
    }

    public bool HasSavedState => PlayerPrefs.HasKey(PrefsVersionKey);

    public void LoadFromPrefs()
    {
        if (!PlayerPrefs.HasKey(PrefsVersionKey))
            return;

        int version = PlayerPrefs.GetInt(PrefsVersionKey, 0);
        if (version != PrefsVersion)
            return;

        sampleCount = Mathf.Max(0, PlayerPrefs.GetInt(PrefsSampleCountKey, 0));
        planningScore = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefsPlanningKey, 0f));
        monitoringScore = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefsMonitoringKey, 0f));
        trialAndErrorScore = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefsTrialAndErrorKey, 0f));
    }

    public void SaveToPrefs()
    {
        PlayerPrefs.SetInt(PrefsVersionKey, PrefsVersion);
        PlayerPrefs.SetInt(PrefsHasSamplesKey, sampleCount > 0 ? 1 : 0);
        PlayerPrefs.SetInt(PrefsSampleCountKey, sampleCount);
        PlayerPrefs.SetFloat(PrefsPlanningKey, planningScore);
        PlayerPrefs.SetFloat(PrefsMonitoringKey, monitoringScore);
        PlayerPrefs.SetFloat(PrefsTrialAndErrorKey, trialAndErrorScore);
    }

    public void ClearSavedState()
    {
        PlayerPrefs.DeleteKey(PrefsVersionKey);
        PlayerPrefs.DeleteKey(PrefsHasSamplesKey);
        PlayerPrefs.DeleteKey(PrefsSampleCountKey);
        PlayerPrefs.DeleteKey(PrefsPlanningKey);
        PlayerPrefs.DeleteKey(PrefsMonitoringKey);
        PlayerPrefs.DeleteKey(PrefsTrialAndErrorKey);
        PlayerPrefs.Save();
    }

    private void ApplyFeatures(AttemptFeatures features)
    {
        if (!features.HasData)
            return;

        sampleCount++;
        lastFirstInputDelay = features.firstInputDelay;
        lastAverageInterval = features.averageInterval;
        lastMistakeRate = features.mistakeRate;

        planningScore = UpdateEma(planningScore, features.planningSignal, emaAlpha);
        monitoringScore = UpdateEma(monitoringScore, features.monitoringSignal, emaAlpha);
        trialAndErrorScore = UpdateEma(trialAndErrorScore, features.trialAndErrorSignal, emaAlpha);
    }

    private static float UpdateEma(float previous, float sample, float alpha)
    {
        alpha = Mathf.Clamp01(alpha);
        return (alpha * sample) + ((1f - alpha) * previous);
    }

    private readonly struct AttemptFeatures
    {
        public readonly bool HasData;
        public readonly float firstInputDelay;
        public readonly float averageInterval;
        public readonly float mistakeRate;
        public readonly float planningSignal;
        public readonly float monitoringSignal;
        public readonly float trialAndErrorSignal;

        public AttemptFeatures(
            bool hasData,
            float firstInputDelay,
            float averageInterval,
            float mistakeRate,
            float planningSignal,
            float monitoringSignal,
            float trialAndErrorSignal)
        {
            HasData = hasData;
            this.firstInputDelay = firstInputDelay;
            this.averageInterval = averageInterval;
            this.mistakeRate = mistakeRate;
            this.planningSignal = planningSignal;
            this.monitoringSignal = monitoringSignal;
            this.trialAndErrorSignal = trialAndErrorSignal;
        }
    }

    private sealed class AttemptSession
    {
        private readonly int wordLength;
        private readonly float startTime;
        private readonly float timeLimitSeconds;
        private readonly List<float> intervals = new List<float>();

        private int keyPressCount;
        private int correctPressCount;
        private int mistakeCount;
        private int earlyMistakeCount;
        private float firstInputTime = -1f;
        private float lastPressTime = -1f;
        private bool lastPressWasWrong;
        private float recoveryPauseSum;
        private int recoveryCount;

        public AttemptSession(string word, float timeLimitSeconds)
        {
            wordLength = Mathf.Max(1, string.IsNullOrEmpty(word) ? 0 : word.Length);
            this.timeLimitSeconds = Mathf.Max(0.01f, timeLimitSeconds);
            startTime = Time.time;
        }

        public void RegisterKeyPress(float currentTime, bool isCorrect)
        {
            if (firstInputTime < 0f)
                firstInputTime = currentTime;

            if (lastPressTime >= 0f)
                intervals.Add(Mathf.Max(0f, currentTime - lastPressTime));

            if (!isCorrect)
            {
                mistakeCount++;
                if (keyPressCount < Mathf.CeilToInt(wordLength * 0.3f))
                    earlyMistakeCount++;
                lastPressWasWrong = true;
            }
            else
            {
                correctPressCount++;
                if (lastPressWasWrong && lastPressTime >= 0f)
                {
                    recoveryPauseSum += Mathf.Max(0f, currentTime - lastPressTime);
                    recoveryCount++;
                }

                lastPressWasWrong = false;
            }

            keyPressCount++;
            lastPressTime = currentTime;
        }

        public AttemptFeatures BuildFeatures(bool completed, float timeTakenSeconds, int mistakes, float endTime)
        {
            if (keyPressCount <= 0)
                return new AttemptFeatures(false, 0f, 0f, 0f, 0f, 0f, 0f);

            float actualDuration = Mathf.Max(0.01f, endTime - startTime);
            float firstInputDelay = firstInputTime >= 0f ? Mathf.Max(0f, firstInputTime - startTime) : Mathf.Max(0f, timeTakenSeconds);
            float averageInterval = Mean(intervals);
            float intervalStdDev = StandardDeviation(intervals, averageInterval);
            float mistakeRate = Mathf.Clamp01((float)mistakeCount / Mathf.Max(1, keyPressCount));
            float earlyMistakeRate = Mathf.Clamp01((float)earlyMistakeCount / Mathf.Max(1, keyPressCount));
            float correctRate = Mathf.Clamp01((float)correctPressCount / Mathf.Max(1, keyPressCount));
            float recoveryAverage = recoveryCount > 0 ? recoveryPauseSum / recoveryCount : averageInterval;

            float delayScore = Normalize01(firstInputDelay, 0.08f, Mathf.Max(0.35f, timeLimitSeconds * 0.35f));
            float stabilityScore = 1f - Normalize01(intervalStdDev, 0.02f, Mathf.Max(0.18f, averageInterval * 1.5f));
            float recoveryScore = Normalize01(recoveryAverage, Mathf.Max(0.10f, averageInterval * 0.9f), Mathf.Max(0.25f, averageInterval * 2.5f));
            float fastStartScore = 1f - delayScore;
            float volatilityScore = Normalize01(intervalStdDev, 0.02f, Mathf.Max(0.15f, averageInterval * 1.25f));
            float completionScore = completed ? 1f : 0f;

            float planningSignal = Mathf.Clamp01((0.45f * delayScore) + (0.35f * stabilityScore) + (0.20f * correctRate));
            float monitoringSignal = Mathf.Clamp01((0.45f * recoveryScore) + (0.30f * completionScore) + (0.25f * (1f - earlyMistakeRate)));
            float trialAndErrorSignal = Mathf.Clamp01((0.50f * mistakeRate) + (0.25f * fastStartScore) + (0.25f * volatilityScore));

            if (!completed && actualDuration >= timeTakenSeconds)
            {
                monitoringSignal = Mathf.Clamp01(monitoringSignal * 0.85f);
                trialAndErrorSignal = Mathf.Clamp01(trialAndErrorSignal * 0.95f);
            }

            return new AttemptFeatures(
                true,
                firstInputDelay,
                averageInterval,
                mistakeRate,
                planningSignal,
                monitoringSignal,
                trialAndErrorSignal);
        }

        private static float Mean(List<float> values)
        {
            if (values == null || values.Count == 0)
                return 0f;

            float sum = 0f;
            for (int i = 0; i < values.Count; i++)
                sum += values[i];

            return sum / values.Count;
        }

        private static float StandardDeviation(List<float> values, float mean)
        {
            if (values == null || values.Count == 0)
                return 0f;

            float sum = 0f;
            for (int i = 0; i < values.Count; i++)
            {
                float delta = values[i] - mean;
                sum += delta * delta;
            }

            return Mathf.Sqrt(sum / values.Count);
        }

        private static float Normalize01(float value, float min, float max)
        {
            if (Mathf.Approximately(min, max))
                return 0f;

            if (min > max)
            {
                float swap = min;
                min = max;
                max = swap;
            }

            return Mathf.Clamp01((value - min) / (max - min));
        }
    }
}