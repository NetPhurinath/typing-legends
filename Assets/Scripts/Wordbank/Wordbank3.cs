using System.Collections.Generic;
using UnityEngine;

public class Wordbank3 : MonoBehaviour
{
    private List<string> originalWords = new List<string>()
    {
       "นารายณ์", "อิศวร", "พรหมา", "มนตรา", "อาคม", "พิธี", "สังเวย", "อัญเชิญ",
        "มงกุฎ", "กำเนิด", "บุญญา", "วิทยา", "ศักดิ์สิทธิ์", "อำนาจ", "มารยา", "วิชา",
        "เมตตา", "กตัญญู", "สัจจะ", "ธรรมะ"
    };

    private List<string> workingWords = new List<string>();
    private int currentIndex = 0;

    private void Awake()
    {
        RefillAndShuffle();
    }

    private void RefillAndShuffle()
    {
        workingWords.Clear();
        workingWords.AddRange(originalWords);
        ShuffleWords(workingWords);
        currentIndex = 0;
    }

    private void ShuffleWords(List<string> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            string temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public string GetWord()
    {
        if (currentIndex >= workingWords.Count)
        {
            RefillAndShuffle();
        }

        string newWord = workingWords[currentIndex];
        currentIndex++;
        return newWord;
    }
}
