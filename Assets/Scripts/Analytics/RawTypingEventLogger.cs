using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class RawTypingEventLogger : MonoBehaviour
{
    public static RawTypingEventLogger Instance { get; private set; }

    [Header("Logging")]
    [SerializeField] private bool persistToDisk = true;
    [SerializeField] private bool logSessionStartAndEnd = true;

    private string sessionId;
    private string sessionStartUtc;
    private string sceneName;
    private string logFilePath;
    private int attemptId;
    private int currentAttemptId;
    private string currentWord = string.Empty;
    private float sessionStartRealtime;
    private float attemptStartRealtime;
    private float lastKeyPressRealtime = -1f;
    private bool sessionStarted;

    public string CurrentLogFilePath => logFilePath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("RawTypingEventLogger: duplicate instance detected. Using the first instance found.");
            enabled = false;
            return;
        }

        Instance = this;
        StartSessionIfNeeded();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (sessionStarted && logSessionStartAndEnd)
            WriteEvent(BuildEventLine("session_end", string.Empty, string.Empty, false, 0, 0, 0f, 0f, -1f, -1f, 0f, 0f, 0f, 0f));
    }

    public void StartSessionIfNeeded()
    {
        if (sessionStarted)
            return;

        sessionStarted = true;
        sessionId = Guid.NewGuid().ToString("N");
        sessionStartUtc = DateTime.UtcNow.ToString("o");
        sceneName = SceneManager.GetActiveScene().name;
        sessionStartRealtime = Time.realtimeSinceStartup;
        attemptId = 0;
        currentAttemptId = 0;
        currentWord = string.Empty;
        lastKeyPressRealtime = -1f;

        var folder = Path.Combine(Application.persistentDataPath, "TypingLegendsLogs");
        Directory.CreateDirectory(folder);
        logFilePath = Path.Combine(folder, $"raw-events-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{sessionId}.jsonl");

        if (logSessionStartAndEnd)
        {
            WriteEvent(BuildEventLine(
                "session_start",
                string.Empty,
                string.Empty,
                false,
                0,
                0,
                0f,
                0f,
                -1f,
                -1f,
                0f,
                0f,
                0f,
                0f));
        }
    }

    public void LogAttemptStarted(string word, float timeLimitSeconds)
    {
        StartSessionIfNeeded();

        attemptId++;
        currentAttemptId = attemptId;
        currentWord = word ?? string.Empty;
        attemptStartRealtime = Time.realtimeSinceStartup;
        lastKeyPressRealtime = -1f;

        WriteEvent(BuildEventLine(
            "attempt_start",
            string.Empty,
            string.Empty,
            false,
            0,
            0,
            0f,
            0f,
            -1f,
            0f,
            timeLimitSeconds,
            0f,
            0f,
            0f));
    }

    public void LogKeyPress(char typedChar, char expectedChar, int typedIndexBefore, bool isCorrect, int mistakesSoFar)
    {
        StartSessionIfNeeded();

        float now = Time.realtimeSinceStartup;
        float timeSinceSessionStart = now - sessionStartRealtime;
        float timeSinceAttemptStart = attemptStartRealtime > 0f ? now - attemptStartRealtime : 0f;
        float timeSinceLastKey = lastKeyPressRealtime >= 0f ? now - lastKeyPressRealtime : -1f;

        lastKeyPressRealtime = now;

        WriteEvent(BuildEventLine(
            "key_press",
            typedChar == '\0' ? string.Empty : typedChar.ToString(),
            expectedChar == '\0' ? string.Empty : expectedChar.ToString(),
            isCorrect,
            typedIndexBefore,
            mistakesSoFar,
            timeSinceSessionStart,
            timeSinceAttemptStart,
            timeSinceLastKey,
            -1f,
            0f,
            0f,
            0f,
            0f));
    }

    public void LogAttemptEnded(bool completed, float timeTakenSeconds, int mistakes, int typedCount, string word)
    {
        StartSessionIfNeeded();

        WriteEvent(BuildEventLine(
            completed ? "attempt_end_completed" : "attempt_end_timeout",
            string.Empty,
            string.Empty,
            completed,
            typedCount,
            mistakes,
            Time.realtimeSinceStartup - sessionStartRealtime,
            Time.realtimeSinceStartup - attemptStartRealtime,
            -1f,
            timeTakenSeconds,
            0f,
            0f,
            0f,
            0f,
            word ?? string.Empty));

        currentWord = string.Empty;
        currentAttemptId = 0;
        lastKeyPressRealtime = -1f;
    }

    public void LogItemUsed(string itemName, bool success, string wordContext)
    {
        StartSessionIfNeeded();

        WriteEvent(BuildEventLine(
            "item_used",
            itemName ?? string.Empty,
            wordContext ?? string.Empty,
            success,
            0,
            0,
            Time.realtimeSinceStartup - sessionStartRealtime,
            attemptStartRealtime > 0f ? Time.realtimeSinceStartup - attemptStartRealtime : 0f,
            -1f,
            0f,
            0f,
            0f,
            0f,
            0f));
    }

    private void WriteEvent(string jsonLine)
    {
        if (!persistToDisk)
            return;

        if (string.IsNullOrEmpty(logFilePath))
            return;

        File.AppendAllText(logFilePath, jsonLine + Environment.NewLine, Encoding.UTF8);
    }

    private string BuildEventLine(
        string eventType,
        string typedChar,
        string expectedChar,
        bool isCorrect,
        int typedIndex,
        int mistakes,
        float timeSinceSessionStart,
        float timeSinceAttemptStart,
        float timeSinceLastKey,
        float timeTakenSeconds,
        float timeLimitSeconds,
        float firstInputDelay,
        float averageInterval,
        float mistakeRate,
        string wordOverride = null)
    {
        var entry = new RawTypingEvent
        {
            sessionId = sessionId,
            sessionStartUtc = sessionStartUtc,
            sceneName = sceneName,
            attemptId = currentAttemptId,
            eventType = eventType,
            word = wordOverride ?? currentWord,
            typedChar = typedChar,
            expectedChar = expectedChar,
            isCorrect = isCorrect,
            typedIndex = typedIndex,
            mistakes = mistakes,
            timeSinceSessionStart = timeSinceSessionStart,
            timeSinceAttemptStart = timeSinceAttemptStart,
            timeSinceLastKey = timeSinceLastKey,
            timeTakenSeconds = timeTakenSeconds,
            timeLimitSeconds = timeLimitSeconds,
            firstInputDelay = firstInputDelay,
            averageInterval = averageInterval,
            mistakeRate = mistakeRate,
        };

        return JsonUtility.ToJson(entry);
    }

    [Serializable]
    private sealed class RawTypingEvent
    {
        public string sessionId;
        public string sessionStartUtc;
        public string sceneName;
        public int attemptId;
        public string eventType;
        public string word;
        public string typedChar;
        public string expectedChar;
        public bool isCorrect;
        public int typedIndex;
        public int mistakes;
        public float timeSinceSessionStart;
        public float timeSinceAttemptStart;
        public float timeSinceLastKey;
        public float timeTakenSeconds;
        public float timeLimitSeconds;
        public float firstInputDelay;
        public float averageInterval;
        public float mistakeRate;
    }
}