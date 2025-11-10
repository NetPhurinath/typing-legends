using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Typer : MonoBehaviour
{
    public Wordbank wordbank = null;
    public TMP_Text wordOutput = null;
    public TMP_Text pointOutput = null;
    public TMP_Text timerOutput = null; // เพิ่ม text สำหรับแสดงเวลานับถอยหลัง

    private string remainingWord = string.Empty;
    private string currentWord = string.Empty;
    private int typedCount = 0;
    private int score = 0;

    public int pointsPerWord = 50;
    public float countdownTime = 5f; // ตั้งเวลาเริ่มต้น 5 วิ
    private float timer;

    private void Start()
    {
        
        SetCurrentWord();
        UpdatePointDisplay();
        ResetTimer();
    }

    private void Update()
    {
        CheckInput();
        UpdateTimer();
    }

    private void SetCurrentWord()
    {
        typedCount = 0;
        currentWord = wordbank.GetWord();

         if (string.IsNullOrEmpty(currentWord))
            {
                Win();
            return;
            }

        SetRemainingWord(currentWord);
        ResetTimer(); // รีเซ็ตเวลาเมื่อเริ่มคำใหม่
    }

    private void SetRemainingWord(string newString)
    {
        remainingWord = newString;
        if (wordOutput != null)
        {
            wordOutput.richText = true;
            string colored = "";

            if (typedCount > 0)
                colored = "<color=#FFD700>" + remainingWord.Substring(0, typedCount) + "</color>";

            string rest = "";
            if (typedCount < remainingWord.Length)
                rest = remainingWord.Substring(typedCount);

            wordOutput.text = colored + rest;
        }
    }

    private void CheckInput()
    {
        if (Input.anyKeyDown)
        {
            string keysPressed = Input.inputString;
            if (string.IsNullOrEmpty(keysPressed)) return;

            foreach (char c in keysPressed)
            {
                if (char.IsControl(c)) continue;
                EnterLetter(c.ToString());
                break;
            }
        }
    }

    private void EnterLetter(string typedLetter)
    {
        if (IsCorrectLetter(typedLetter))
        {
            RemoveLetter();

            if (IsWordComplete())
            {
                AddPoint(pointsPerWord);
                SetCurrentWord();
            }
        }
    }

    private bool IsCorrectLetter(string letter)
    {
        if (string.IsNullOrEmpty(remainingWord)) return false;
        if (typedCount >= remainingWord.Length) return false;
        return char.ToLowerInvariant(letter[0]) == char.ToLowerInvariant(remainingWord[typedCount]);
    }

    private void RemoveLetter()
    {
        typedCount++;
        SetRemainingWord(remainingWord);
    }

    private bool IsWordComplete()
    {
        return typedCount >= remainingWord.Length;
    }

    private void AddPoint(int amount)
    {
        score += amount;
        UpdatePointDisplay();
    }

    private void UpdatePointDisplay()
    {
        if (pointOutput != null)
            pointOutput.text = "Score: " + score.ToString();
    }

    // ===== ระบบนับเวลา =====
    private void ResetTimer()
    {
        timer = countdownTime;
        UpdateTimerDisplay();
    }

    private void UpdateTimer()
    {
        timer -= Time.deltaTime;
        UpdateTimerDisplay();

        if (timer <= 0f)
        {
            ReturnToMainMenu();
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerOutput != null)
            timerOutput.text = "Time: " + Mathf.Ceil(timer).ToString();
    }

    private void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void Win()
    {
        // เมื่อสคริปต์ถูกปิด ให้กลับไปที่เมนูหลัก
        SceneManager.LoadScene("Level 2");
    }
}
