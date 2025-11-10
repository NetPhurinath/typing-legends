using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Wordbank : MonoBehaviour
{
    private List<string> originalWords = new List<string>()
    {
        "ราม", "ลิง", "ศึก", "ดาบ", "ศร", "วัด", "เมฆ", "ฟ้า",
        "ยักษ์", "น้ำ", "ไฟ", "ดิน", "บิน", "รบ", "นาค", "โยธา",
        "ทัพ", "ชัย", "ม้า", "พร"
    };

    private List<string> workingWords = new List<string>();
    private int wordsGiven = 0;          // นับจำนวนคำที่ให้ไปแล้ว
    public int maxWords = 5;             // จำกัดแค่ 5 คำ

    private void Awake()
    {
        workingWords.AddRange(originalWords);
        ShuffleWords(workingWords);
    }

    private void ShuffleWords(List<string> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            string temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public string GetWord()
    {
        // ถ้าครบ 5 คำแล้ว ให้ return string ว่าง (หรือ null ก็ได้)
        if (wordsGiven >= maxWords)
        {
            return string.Empty; // หรือจะใส่ "END" เพื่อให้ Typer เช็คได้
        }

        // ดึงคำถัดไป
        string newWord = workingWords[wordsGiven];
        wordsGiven++;
        return newWord;
    }

    // เผื่อกรณีอยากรีเซ็ตเล่นใหม่
    public void ResetWords()
    {
        wordsGiven = 0;
        workingWords.Clear();
        workingWords.AddRange(originalWords);
        ShuffleWords(workingWords);
    }
}
