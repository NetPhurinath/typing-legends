using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Reflection;

public class Typer : MonoBehaviour
{
    public Wordbank wordbank = null;
    [Header("Wordbank (any WordbankX)")]
    [SerializeField] private MonoBehaviour wordbankBehaviour = null;
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

    [Header("Item (tomyumshrimp)")]
    [SerializeField] private TomyumShrimpItem tomyumShrimpItem;

    [Header("Food")]
    [SerializeField] private int maxFood =3;

    // Kept for backwards compatibility / UI balancing. If an item is assigned,
    // the item's own healAmount will be used.
    [SerializeField] private int healPerFood =1;

    [Header("Food UI")]
    [SerializeField] private TMP_Text foodOutput;

    private int currentFood =0;

    private string remainingWord = string.Empty;
    private string currentWord = string.Empty;
    private int typedCount =0;
    private int score =0;

    public int Score => score;

    public int pointsPerWord =50;
    public float countdownTime =5f;
    private float timer;
    private bool isGameOver = false;

    private object resolvedWordbank = null;
    private MethodInfo resolvedGetWordMethod = null;
    private MethodInfo resolvedOnWordStartedMethod = null;
    private MethodInfo resolvedOnWordResultMethod = null;

    private int mistakesThisWord = 0;

    [Header("Health (optional)")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Monster (optional)")]
    [SerializeField] private MonsterHealth monsterHealth;
    [SerializeField] private int monsterDamagePerCorrectWord =1;

    [Header("Monster Portrait (optional)")]
    [SerializeField] private MonsterPortraitUI monsterPortraitUI;

    [Header("Time limit damage")]
    [SerializeField] private int slowWordDamage =1;

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

        if (monsterPortraitUI == null)
            monsterPortraitUI = Object.FindFirstObjectByType<MonsterPortraitUI>(FindObjectsInactive.Include);

        if (tomyumShrimpItem == null)
            tomyumShrimpItem = Object.FindFirstObjectByType<TomyumShrimpItem>(FindObjectsInactive.Include);
    }

    private void Start()
    {
        ResolveWordbankProvider();
        SetCurrentWord();
        UpdatePointDisplay();
        UpdateFoodDisplay();
        ResetTimer();
        AddFood(3);

        ScoreKeeper.Set(score);
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
        if (!ResolveWordbankProvider())
        {
            isGameOver = true;
            return;
        }

        typedCount = 0;
        mistakesThisWord = 0;
        currentWord = GetWordFromResolvedProvider();

        if (string.IsNullOrEmpty(currentWord))
        {
            Debug.LogError("Typer: Wordbank returned an empty word.");
            isGameOver = true;
            return;
        }

        SetRemainingWord(currentWord);
        ResetTimer();

        InvokeOnWordStarted(currentWord);
    }

    private bool ResolveWordbankProvider()
    {
        if (resolvedWordbank != null) return true;

        // 1) Explicit assignment: any MonoBehaviour with GetWord():string
        if (wordbankBehaviour != null)
        {
            if (TryResolveFromBehaviour(
                    wordbankBehaviour,
                    out resolvedWordbank,
                    out resolvedGetWordMethod,
                    out resolvedOnWordStartedMethod,
                    out resolvedOnWordResultMethod))
                return true;

            Debug.LogError("Typer: 'wordbankBehaviour' is set but does not have a GetWord() method that returns string.");
            return false;
        }

        // 2) Legacy assignment: Wordbank
        if (wordbank != null)
        {
            resolvedWordbank = wordbank;
            resolvedGetWordMethod = null;
            resolvedOnWordStartedMethod = null;
            resolvedOnWordResultMethod = null;
            return true;
        }

        // 3) Auto-find any Wordbank* component with GetWord
        var behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var behaviour in behaviours)
        {
            if (behaviour == null) continue;
            var typeName = behaviour.GetType().Name;
            if (!typeName.StartsWith("Wordbank")) continue;

                if (TryResolveFromBehaviour(
                    behaviour,
                    out resolvedWordbank,
                    out resolvedGetWordMethod,
                    out resolvedOnWordStartedMethod,
                    out resolvedOnWordResultMethod))
                return true;
        }

        Debug.LogError("Typer: No Wordbank component found. Assign a Wordbank/WordbankX on this GameObject or drag it into 'Wordbank Behaviour'.");
        return false;
    }

    private static bool TryResolveFromBehaviour(
        MonoBehaviour behaviour,
        out object provider,
        out MethodInfo getWordMethod,
        out MethodInfo onWordStartedMethod,
        out MethodInfo onWordResultMethod)
    {
        provider = null;
        getWordMethod = null;
        onWordStartedMethod = null;
        onWordResultMethod = null;

        if (behaviour == null) return false;

        var type = behaviour.GetType();
        getWordMethod = type.GetMethod(
            "GetWord",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types: System.Type.EmptyTypes,
            modifiers: null
        );

        if (getWordMethod == null) return false;
        if (getWordMethod.ReturnType != typeof(string)) return false;

        // Optional hooks
        onWordStartedMethod = type.GetMethod(
            "OnWordStarted",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types: new[] { typeof(string) },
            modifiers: null
        );
        if (onWordStartedMethod != null && onWordStartedMethod.ReturnType != typeof(void))
            onWordStartedMethod = null;

        onWordResultMethod = type.GetMethod(
            "OnWordResult",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types: new[] { typeof(string), typeof(float), typeof(int), typeof(bool) },
            modifiers: null
        );
        if (onWordResultMethod != null && onWordResultMethod.ReturnType != typeof(void))
            onWordResultMethod = null;

        provider = behaviour;
        return true;
    }

    private void InvokeOnWordStarted(string word)
    {
        if (string.IsNullOrEmpty(word)) return;
        if (resolvedWordbank == null) return;

        if (resolvedWordbank is AdaptiveWordbankAI adaptive)
        {
            adaptive.OnWordStarted(word);
            return;
        }

        if (resolvedOnWordStartedMethod != null)
            resolvedOnWordStartedMethod.Invoke(resolvedWordbank, new object[] { word });
    }

    private void InvokeOnWordResult(string word, float timeTakenSeconds, int mistakes, bool completed)
    {
        if (string.IsNullOrEmpty(word)) return;
        if (resolvedWordbank == null) return;

        if (resolvedWordbank is AdaptiveWordbankAI adaptive)
        {
            adaptive.OnWordResult(word, timeTakenSeconds, mistakes, completed);
            return;
        }

        if (resolvedOnWordResultMethod != null)
            resolvedOnWordResultMethod.Invoke(resolvedWordbank, new object[] { word, timeTakenSeconds, mistakes, completed });
    }

    private void NotifyWordResult(bool completed)
    {
        if (string.IsNullOrEmpty(currentWord)) return;

        float timeTakenSeconds = completed
            ? Mathf.Clamp(countdownTime - timer, 0.01f, countdownTime)
            : Mathf.Max(0.01f, countdownTime);

        InvokeOnWordResult(currentWord, timeTakenSeconds, mistakesThisWord, completed);
    }

    private string GetWordFromResolvedProvider()
    {
        if (resolvedWordbank == null) return string.Empty;

        if (resolvedWordbank is Wordbank typedWordbank)
            return typedWordbank.GetWord();

        if (resolvedGetWordMethod != null)
            return (string)resolvedGetWordMethod.Invoke(resolvedWordbank, null);

        return string.Empty;
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
                NotifyWordResult(completed: true);
                AddPoint(pointsPerWord);

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
            // wrong letter: no damage currently
            mistakesThisWord++;
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
        ScoreKeeper.Set(score);
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

        NotifyWordResult(completed: false);

        // Monster attacks when you're too slow
        if (monsterPortraitUI != null)
            monsterPortraitUI.PlayAttack();

        // Take health for being too slow on this word
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(slowWordDamage);

            if (playerHealth.CurrentHealth <= 0)
            {
                isGameOver = true;
                return;
            }
        }
        else
        {
            isGameOver = true;
            ScoreKeeper.Set(score);
            if (gameOverScreen != null) gameOverScreen.Show(score);
            else ReturnToMainMenu();
            return;
        }

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
        ScoreKeeper.Set(score);
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
        if (currentFood <=0) return;
        if (IsHealthFull()) return;

        currentFood--;

        // Prefer the separated item behaviour when present.
        if (tomyumShrimpItem != null)
        {
            // If for some reason the item refuses to use, fall back.
            if (!tomyumShrimpItem.TryUse(playerHealth))
                playerHealth.Heal(healPerFood);
        }
        else
        {
            playerHealth.Heal(healPerFood);
        }

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
