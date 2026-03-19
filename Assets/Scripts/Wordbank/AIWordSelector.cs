using System;
using System.Collections.Generic;
using UnityEngine;

public static class AIWordSelector
{
    [Serializable]
    public struct Settings
    {
        [Header("Skill normalization")]
        [Min(0f)] public float wpmMin;
        [Min(0f)] public float wpmMax;
        [Range(0f, 1f)] public float accuracyMin;
        [Range(0f, 1f)] public float accuracyMax;
        [Range(0f, 1f)] public float wpmWeight;

        [Header("Exploration")]
        [Range(0f, 1f)] public float neighborChance;
        [Range(0f, 1f)] public float randomBucketChance;

        public static Settings Default => new Settings
        {
            wpmMin = 10f,
            wpmMax = 45f,
            accuracyMin = 0.75f,
            accuracyMax = 0.97f,
            wpmWeight = 0.6f,
            neighborChance = 0.15f,
            randomBucketChance = 0.05f,
        };
    }

    public static int ChooseBucketIndex(int bucketCount, float wpm, float accuracy, bool hasSamples, Settings settings)
    {
        if (bucketCount <= 1) return 0;
        if (!hasSamples) return 0;

        float wpmT = InverseLerpSafe(settings.wpmMin, settings.wpmMax, wpm);
        float accT = InverseLerpSafe(settings.accuracyMin, settings.accuracyMax, accuracy);

        float wpmWeight = Mathf.Clamp01(settings.wpmWeight);
        float score = (wpmWeight * wpmT) + ((1f - wpmWeight) * accT);
        score = Mathf.Clamp01(score);

        int baseIndex = Mathf.Clamp(Mathf.RoundToInt(score * (bucketCount - 1)), 0, bucketCount - 1);

        float roll = UnityEngine.Random.value;
        if (roll < Mathf.Clamp01(settings.randomBucketChance))
            return UnityEngine.Random.Range(0, bucketCount);

        if (roll < Mathf.Clamp01(settings.randomBucketChance) + Mathf.Clamp01(settings.neighborChance))
        {
            int delta = UnityEngine.Random.value < 0.5f ? -1 : 1;
            return Mathf.Clamp(baseIndex + delta, 0, bucketCount - 1);
        }

        return baseIndex;
    }

    public static int ChooseBucketIndex(int bucketCount, Settings settings = default)
    {
        if (settings.wpmMax <= 0f && settings.accuracyMax <= 0f)
            settings = Settings.Default;

        return ChooseBucketIndex(
            bucketCount,
            PlayerSkillState.EstimatedWpm,
            PlayerSkillState.EstimatedAccuracy,
            PlayerSkillState.HasSamples,
            settings
        );
    }

    public static List<List<string>> PartitionWordsByLength(IReadOnlyList<string> words, int bucketCount)
    {
        bucketCount = Mathf.Max(1, bucketCount);
        var result = new List<List<string>>(bucketCount);
        for (int i = 0; i < bucketCount; i++) result.Add(new List<string>());

        if (words == null || words.Count == 0) return result;

        var sorted = new List<string>(words.Count);
        for (int i = 0; i < words.Count; i++)
        {
            if (!string.IsNullOrEmpty(words[i])) sorted.Add(words[i]);
        }

        sorted.Sort((a, b) => a.Length.CompareTo(b.Length));

        if (sorted.Count == 0) return result;

        for (int i = 0; i < sorted.Count; i++)
        {
            int bucket = Mathf.FloorToInt(((float)i / sorted.Count) * bucketCount);
            bucket = Mathf.Clamp(bucket, 0, bucketCount - 1);
            result[bucket].Add(sorted[i]);
        }

        return result;
    }

    private static float InverseLerpSafe(float a, float b, float value)
    {
        if (Mathf.Approximately(a, b)) return 0f;
        if (a > b) (a, b) = (b, a);
        return Mathf.Clamp01((value - a) / (b - a));
    }
}
