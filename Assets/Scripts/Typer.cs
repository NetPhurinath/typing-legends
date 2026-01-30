using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Typer : MonoBehaviour
{
    public Wordbank wordbank = null;
    public TMP_Text wordOutput = null;
    public TMP_Text pointOutput = null;
    public TMP_Text timerOutput = null;
    public void OnFoodIconClicked()
    {
    ConsumeFood();
    }

    [SerializeField] private GameOverScreen gameOverScreen = null;
    [SerializeField] private GameWinScreen gameWinScreen = null;
    [Header("Food Icon")]
    [SerializeField] private GameObject foodIcon;

    [Header("Food")]
    [SerializeField] private int maxFood = 3;
    [SerializeField] private int healPerFood = 1;
    [Header("Food UI")]
    [SerializeField] private TMP_Text foodOutput;


    private int currentFood = 0;

    private string remainingWord = string.Empty;
    private string currentWord = string.Empty;
    private int typedCount = 0;
    private int score = 0;

    public int Score => score;

    public int pointsPerWord = 50;
    public float countdownTime = 5f;
    private float timer;
    private bool isGameOver = false;

    [Header("Health (optional)")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Monster (optional)")]
    [SerializeField] private MonsterHealth monsterHealth;
    [SerializeField] private int monsterDamagePerCorrectWord = 1;

    [Header("Time limit damage")]
    [SerializeField] private int slowWordDamage = 1;

    private void Awake()
    {
        if (gameWinScreen == null)
            gameWinScreen = Object.FindFirstObjectByType<GameWinScreen>(FindObjectsInactive.Include);

        if (gameOverScreen == null)
        {
            var endScreens = Object.FindObjectsByType<GameOverScreen>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var screen in endScreens)
            {
                if (screen is GameWinScreen) continue;
                gameOverScreen = screen;
                break;
            }

            if (gameOverScreen == null)
                gameOverScreen = Object.FindFirstObjectByType<GameOverScreen>(FindObjectsInactive.Include);
        }
    }

    private void Start()
    {
        SetCurrentWord();
        UpdatePointDisplay();
        UpdateFoodDisplay();
        ResetTimer();
        AddFood(3);
    }

    private void Update()
    {
        if (isGameOver) return;
        CheckInput();
        UpdateTimer();
        if (Input.GetKeyDown(KeyCode.H) && !IsHealthFull())
{
    ConsumeFood();
}


    }

    private void SetCurrentWord()
    {
        typedCount = 0;
        currentWord = wordbank.GetWord();

        SetRemainingWord(currentWord);
        ResetTimer();
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
        if (!Input.anyKeyDown) return;

        string keysPressed = Input.inputString;
        if (string.IsNullOrEmpty(keysPressed)) return;

        foreach (char c in keysPressed)
        {
            if (char.IsControl(c)) continue;
            EnterLetter(c.ToString());
            break;
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

                // Damage monster when player types the whole word correctly
                if (monsterHealth != null)
                {
                    monsterHealth.TakeDamage(monsterDamagePerCorrectWord);
                    if (monsterHealth.CurrentHealth <= 0)
                    {
                        isGameOver = true;
                        Win();
                        return;
                    }
                }

                SetCurrentWord();
            }
        }
        else
        {
            // Do nothing on wrong letter (no heart reduction)
            // Optional: you can add feedback here (sound/shake/etc.)
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

    private void ResetTimer()
    {
        timer = countdownTime;
        UpdateTimerDisplay();
    }

    private void UpdateTimer()
    {
        timer -= Time.deltaTime;
        if (timer < 0f) timer = 0f;
        UpdateTimerDisplay();

        if (timer <= 0f) OnWordTimedOut();
    }

    private void OnWordTimedOut()
    {
        if (isGameOver) return;

        // Take 1 health for being too slow on this word
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(slowWordDamage);

            // If health reached 0, PlayerHealth will show Game Over screen.
            if (playerHealth.CurrentHealth <= 0)
            {
                isGameOver = true;
                return;
            }
        }
        else
        {
            // Fallback behavior if no health system is assigned
            isGameOver = true;
            if (gameOverScreen != null) gameOverScreen.Show(score);
            else ReturnToMainMenu();
            return;
        }

        // Still alive: skip to next word and reset timer
        SetCurrentWord();
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
        if (gameWinScreen != null)
        {
            gameWinScreen.Show(score);
            return;
        }

        if (gameOverScreen != null)
        {
            gameOverScreen.Show(score, true);
            return;
        }

        SceneManager.LoadScene("Level 2");
    }

private bool IsHealthFull()
{
    if (playerHealth == null) return true;
    return playerHealth.CurrentHealth >= playerHealth.MaxHealth;
}

private void ConsumeFood()
{
    if (playerHealth == null) return;
    if (currentFood <= 0) return;
    if (IsHealthFull()) return;

    currentFood--;
    playerHealth.Heal(healPerFood);

    UpdateFoodDisplay();
    UpdateFoodIcon();
}


private void UpdateFoodIcon()
{
    if (foodIcon == null) return;

    foodIcon.SetActive(currentFood > 0);
}


private void AddFood(int amount)
{
    if (amount <= 0) return;

    currentFood = Mathf.Min(maxFood, currentFood + amount);
    UpdateFoodDisplay();
    UpdateFoodIcon();
   
}


private void UpdateFoodDisplay()
{
    if (foodOutput != null)
        foodOutput.text = currentFood.ToString();
}


}
