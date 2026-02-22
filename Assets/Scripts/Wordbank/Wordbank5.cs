using System.Collections.Generic;
using UnityEngine;

public class Wordbank5 : MonoBehaviour
{
    private List<string> originalWords = new List<string>()
    {
        "สหัสเดชะ", "มหิทธานุภาพ", "ธรรมานุภาพ", "ปรมัตถธรรม", "อนัตตลักษณะ", "สังขารธรรม", "อิทธิบาท", "พุทธานุภาพ",
        "ชัยมงคล", "สรรพสิ่ง", "สมเด็จ", "พระบารมี", "วิสุทธิธรรม", "อนุเคราะห์", "อนุสรณ์", "มหากรุณา",
        "อนุโมทนา", "มโนธรรม", "อุปสรรค", "มหาปรารถนา"
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
