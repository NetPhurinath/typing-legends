using System.Collections.Generic;
using UnityEngine;

public abstract class AdaptiveWordbankAI : MonoBehaviour
{
    [Header("AI selection")]
    [Tooltip("Optional: explicit difficulty tiers (easy/medium/hard). If assigned, AI will pick from these buckets instead of splitting by word length.")]
    [SerializeField] private WordbankTieredList tieredList;

    [Tooltip("How many difficulty buckets to split this word list into (by word length).")]
    [SerializeField, Min(1)] private int bucketCount = 3;

    [Tooltip("EMA alpha for skill tracking: higher = adapts faster, lower = smoother.")]
    [SerializeField, Range(0.05f, 0.5f)] private float skillEmaAlpha = 0.20f;

    [SerializeField] private AIWordSelector.Settings selectorSettings = AIWordSelector.Settings.Default;

    [Header("Debug (read-only)")]
    [SerializeField] private bool debugHasSamples;
    [SerializeField] private float debugEstimatedWpm;
    [SerializeField, Range(0f, 1f)] private float debugEstimatedAccuracy;

    private WordBag[] buckets;
    private bool initialized;

    protected abstract IReadOnlyList<string> OriginalWords { get; }

    // Override in a specific Wordbank to keep it from auto-loading the shared tiered list.
    protected virtual bool AutoLoadDefaultTieredListFromResources => true;

    protected virtual void Awake()
    {
        InitializeIfNeeded();
    }

    private void Update()
    {
        debugHasSamples = PlayerSkillState.HasSamples;
        debugEstimatedWpm = PlayerSkillState.EstimatedWpm;
        debugEstimatedAccuracy = PlayerSkillState.EstimatedAccuracy;
    }

    public string GetWord()
    {
        InitializeIfNeeded();
        if (buckets == null || buckets.Length == 0) return string.Empty;

        int chosen = AIWordSelector.ChooseBucketIndex(buckets.Length, selectorSettings);

        // Fallback to a non-empty bucket if needed
        for (int attempt = 0; attempt < buckets.Length; attempt++)
        {
            int idx = (chosen + attempt) % buckets.Length;
            if (buckets[idx] != null && buckets[idx].Count > 0)
                return buckets[idx].GetNext();
        }

        return string.Empty;
    }

    // Optional hook (Typer will call if present)
    public void OnWordResult(string word, float timeTakenSeconds, int mistakes, bool completed)
    {
        PlayerSkillState.UpdateFromWordResult(word, timeTakenSeconds, mistakes, completed, skillEmaAlpha);
    }

    // Optional hook (Typer will call if present)
    public void OnWordStarted(string word) { }

    private void InitializeIfNeeded()
    {
        if (initialized) return;
        initialized = true;

        // If not assigned in the Inspector, try the default Resources asset.
        if (tieredList == null && AutoLoadDefaultTieredListFromResources)
            tieredList = Resources.Load<WordbankTieredList>("Wordbanks/Ramayana_TieredList");

        // Prefer explicit tier buckets (easy/medium/hard) if provided
        if (tieredList != null)
        {
            buckets = new WordBag[tieredList.BucketCount];
            for (int i = 0; i < tieredList.BucketCount; i++)
                buckets[i] = new WordBag(new List<string>(tieredList.GetBucket(i)));
            return;
        }

        // Otherwise, auto-partition by word length
        var words = OriginalWords;
        var partitions = AIWordSelector.PartitionWordsByLength(words, bucketCount);
        buckets = new WordBag[partitions.Count];

        for (int i = 0; i < partitions.Count; i++)
            buckets[i] = new WordBag(partitions[i]);
    }

    private sealed class WordBag
    {
        private readonly List<string> original;
        private readonly List<string> bag;
        private int index;

        public int Count => original?.Count ?? 0;

        public WordBag(List<string> words)
        {
            original = words ?? new List<string>();
            bag = new List<string>(original.Count);
            RefillAndShuffle();
        }

        public string GetNext()
        {
            if (bag.Count == 0) return string.Empty;

            if (index >= bag.Count)
                RefillAndShuffle();

            var word = bag[index];
            index++;
            return word;
        }

        private void RefillAndShuffle()
        {
            bag.Clear();
            bag.AddRange(original);
            Shuffle(bag);
            index = 0;
        }

        private static void Shuffle(List<string> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int randomIndex = Random.Range(i, list.Count);
                (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
            }
        }
    }
}
