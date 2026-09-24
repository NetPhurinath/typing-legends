using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

// Collect the entire stage; consume a fixed batch prepared by the previous stage.
public class ApiWordbank : MonoBehaviour
{
    [SerializeField] private string endpoint = "http://127.0.0.1:3000/api/create-practice";
    private readonly List<Attempt> attempts = new List<Attempt>();
    private readonly Dictionary<string, int> errorsByCharacter = new Dictionary<string, int>();
    private string[] batch = Array.Empty<string>();
    private int index;
    private int level = 1;
    private bool submitted;
    private float wordStartedAt;
    private float lastKeyAt;
    private float firstInputDelay;
    private float intervalTotal;
    private int keyCount;
    private int intervalCount;
    public int BufferedWordCount => batch.Length;
    public string Status { get; private set; } = "เก็บพฤติกรรมระหว่างด่าน";
    public string LastServedWord { get; private set; }

    [Serializable]
    public class Attempt
    {
        public string word;
        public string outcome;
        public float seconds;
        public int correctCharacters;
        public int mistakes;
        public int keyPresses;
        public float firstInputDelay;
        public float averageKeyInterval;
    }
    [Serializable]
    public class MistakeCount { public string character; public int count; }
    [Serializable]
    public class Stats
    {
        public float accuracy;
        public float averageTime;
        public string[] mistakes;
        public int currentLevel;
        public int sourceLevel;
        public int difficultyTier;
        public int sampleCount;
        public int completedWords;
        public int timedOutWords;
        public int skippedWords;
        public int totalKeyPresses;
        public Attempt[] attempts;
        public MistakeCount[] mistakeCounts;
    }

    public void Initialize(int levelNumber)
    {
        level = levelNumber;
        batch = NextLevelWords.GetBatch(level);
        Status = batch.Length > 0 ? "ใช้ชุดคำจากผลด่านก่อนหน้า" : "ใช้คำเดิมและเก็บพฤติกรรมทั้งด่าน";
    }

    public void BeginWord()
    {
        wordStartedAt = Time.time;
        lastKeyAt = -1;
        firstInputDelay = -1;
        keyCount = intervalCount = 0;
        intervalTotal = 0;
    }

    public void RecordKey(char expected, bool correct)
    {
        float now = Time.time;
        if (keyCount == 0) firstInputDelay = now - wordStartedAt;
        if (lastKeyAt >= 0) { intervalTotal += now - lastKeyAt; intervalCount++; }
        lastKeyAt = now;
        keyCount++;
        if (!correct && !char.IsControl(expected))
        {
            string character = expected.ToString();
            errorsByCharacter.TryGetValue(character, out int count);
            errorsByCharacter[character] = count + 1;
        }
    }

    public void RecordResult(string word, int correctCharacters, int mistakes, string outcome)
    {
        attempts.Add(new Attempt {
            word = word, outcome = outcome, seconds = Mathf.Max(0, Time.time - wordStartedAt),
            correctCharacters = correctCharacters, mistakes = mistakes, keyPresses = keyCount,
            firstInputDelay = firstInputDelay, averageKeyInterval = intervalCount == 0 ? 0 : intervalTotal / intervalCount
        });
    }

    public Stats GetStats(int targetLevel, int tier)
    {
        var stats = new Stats { currentLevel = targetLevel, sourceLevel = level, difficultyTier = Mathf.Clamp(tier, 0, 9),
            sampleCount = attempts.Count, attempts = attempts.ToArray() };
        int correct = 0, errors = 0;
        float seconds = 0;
        foreach (var attempt in attempts)
        {
            correct += attempt.correctCharacters; errors += attempt.mistakes; seconds += attempt.seconds;
            stats.totalKeyPresses += attempt.keyPresses;
            if (attempt.outcome == "completed") stats.completedWords++;
            else if (attempt.outcome == "timeout") stats.timedOutWords++;
            else if (attempt.outcome == "skipped") stats.skippedWords++;
        }
        stats.accuracy = correct + errors == 0 ? 0 : 100f * correct / (correct + errors);
        stats.averageTime = attempts.Count == 0 ? 0 : seconds / attempts.Count;
        var counts = new List<MistakeCount>();
        foreach (var pair in errorsByCharacter) counts.Add(new MistakeCount { character = pair.Key, count = pair.Value });
        counts.Sort((a, b) => b.count.CompareTo(a.count));
        stats.mistakeCounts = counts.ToArray();
        stats.mistakes = counts.ConvertAll(item => item.character).ToArray();
        return stats;
    }

    public void CompleteLevel(int targetLevel, int tier)
    {
        if (submitted || targetLevel > 10 || targetLevel <= level) return;
        submitted = true;
        Status = "จบด่านแล้ว กำลังเตรียมคำสำหรับด่าน " + targetLevel;
        NextLevelWords.Prepare(endpoint, GetStats(targetLevel, tier));
    }

    public bool TryTakeWord(int levelNumber, int tier, string previousWord, out string word)
    {
        word = null;
        if (!isActiveAndEnabled) return false;
        if (batch.Length == 0 && NextLevelWords.IsPending(level)) return false;
        if (batch.Length == 0)
        {
            // The batch may finish loading after the stage has already started.
            batch = NextLevelWords.GetBatch(level);
            if (batch.Length == 0) return false;
            Status = "ใช้ชุดคำจากผลด่านก่อนหน้า";
        }
        // Reuse this fixed batch for the whole stage, including retries; never refill from the API.
        for (int i = 0; i < batch.Length; i++)
        {
            var candidate = batch[index++ % batch.Length];
            if (candidate == previousWord && batch.Length > 1) continue;
            LastServedWord = word = candidate;
            return true;
        }
        return false;
    }

    public static string[] ParseWords(string text)
    {
        var result = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var part in (text ?? "").Split(new[] { ',', '，', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var word = part.Trim().Normalize(NormalizationForm.FormC);
            if (word.Length >= 2 && word.Length <= 32 && Regex.IsMatch(word, @"^[\u0E01-\u0E2E\u0E40-\u0E44][\u0E01-\u0E3A\u0E40-\u0E4E]+$") && seen.Add(word)) result.Add(word);
            if (result.Count == 10) break;
        }
        return result.ToArray();
    }
}
