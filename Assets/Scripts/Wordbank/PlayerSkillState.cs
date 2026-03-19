using UnityEngine;

public static class PlayerSkillState
{
    private const string PrefsPrefix = "TypingLegends.Skill.";
    private const string PrefsVersionKey = PrefsPrefix + "Version";
    private const int PrefsVersion = 1;
    private const string PrefsHasSamplesKey = PrefsPrefix + "HasSamples";
    private const string PrefsEmaWpmKey = PrefsPrefix + "EmaWpm";
    private const string PrefsEmaAccuracyKey = PrefsPrefix + "EmaAccuracy";

    private static bool hasSamples;
    private static float emaWpm;
    private static float emaAccuracy;

    public static bool HasSamples => hasSamples;
    public static float EstimatedWpm => emaWpm;
    public static float EstimatedAccuracy => emaAccuracy;

    public static bool HasSavedState => PlayerPrefs.HasKey(PrefsVersionKey);

    public static void Reset(float initialWpm = 0f, float initialAccuracy = 1f)
    {
        hasSamples = false;
        emaWpm = Mathf.Max(0f, initialWpm);
        emaAccuracy = Mathf.Clamp01(initialAccuracy);
    }

    public static void LoadFromPrefs()
    {
        if (!PlayerPrefs.HasKey(PrefsVersionKey))
            return;

        int version = PlayerPrefs.GetInt(PrefsVersionKey, 0);
        if (version != PrefsVersion)
            return;

        hasSamples = PlayerPrefs.GetInt(PrefsHasSamplesKey, 0) != 0;
        emaWpm = Mathf.Max(0f, PlayerPrefs.GetFloat(PrefsEmaWpmKey, 0f));
        emaAccuracy = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefsEmaAccuracyKey, 1f));
    }

    public static void SaveToPrefs()
    {
        PlayerPrefs.SetInt(PrefsVersionKey, PrefsVersion);
        PlayerPrefs.SetInt(PrefsHasSamplesKey, hasSamples ? 1 : 0);
        PlayerPrefs.SetFloat(PrefsEmaWpmKey, emaWpm);
        PlayerPrefs.SetFloat(PrefsEmaAccuracyKey, emaAccuracy);
    }

    public static void FlushPrefsToDisk()
    {
        PlayerPrefs.Save();
    }

    public static void ClearSavedState()
    {
        PlayerPrefs.DeleteKey(PrefsVersionKey);
        PlayerPrefs.DeleteKey(PrefsHasSamplesKey);
        PlayerPrefs.DeleteKey(PrefsEmaWpmKey);
        PlayerPrefs.DeleteKey(PrefsEmaAccuracyKey);
    }

    public static void UpdateFromWordResult(string word, float timeTakenSeconds, int mistakes, bool completed, float emaAlpha)
    {
        timeTakenSeconds = Mathf.Max(0.01f, timeTakenSeconds);
        emaAlpha = Mathf.Clamp01(emaAlpha);

        int correctChars = completed ? Mathf.Max(0, word?.Length ?? 0) : 0;
        int totalTyped = correctChars + Mathf.Max(0, mistakes);
        float accuracy = totalTyped <= 0 ? 0f : (float)correctChars / totalTyped;

        float minutes = timeTakenSeconds / 60f;
        float words = correctChars / 5f;
        float wpm = minutes <= 0f ? 0f : (words / minutes);

        if (!hasSamples)
        {
            emaWpm = wpm;
            emaAccuracy = accuracy;
            hasSamples = true;
            SaveToPrefs();
            return;
        }

        emaWpm = (emaAlpha * wpm) + ((1f - emaAlpha) * emaWpm);
        emaAccuracy = (emaAlpha * accuracy) + ((1f - emaAlpha) * emaAccuracy);

        SaveToPrefs();
    }
}
