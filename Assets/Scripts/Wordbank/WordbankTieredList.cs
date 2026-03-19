using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Typing Legends/Wordbank Tiered List", fileName = "WordbankTieredList")]
public class WordbankTieredList : ScriptableObject
{
    [Header("Easy (Bucket 0)")]
    public List<string> easy = new List<string>();

    [Header("Medium (Bucket 1)")]
    public List<string> medium = new List<string>();

    [Header("Hard (Bucket 2)")]
    public List<string> hard = new List<string>();

    public int BucketCount => 3;

    public IReadOnlyList<string> GetBucket(int index)
    {
        return index switch
        {
            0 => easy,
            1 => medium,
            2 => hard,
            _ => easy,
        };
    }
}
