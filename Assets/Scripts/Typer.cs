using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Reflection;

public class Typer : MonoBehaviour
{
    public Wordbank wordbank = null;
    [Header("Wordbank (any WordbankX)")]
    [SerializeField] private MonoBehaviour wordbankBehaviour = null;
    [Header("Strategy profiling (optional)")]
    [SerializeField] private TypingStrategyProfiler strategyProfiler = null;
    [Header("Raw event logging (optional)")]
    [SerializeField] private RawTypingEventLogger rawEventLogger = null;
    public TMP_Text wordOutput = null;
    public TMP_Text pointOutput = null;
    public TMP_Text timerOutput = null;
   

    [SerializeField] private GameOverScreen gameOverScreen = null;
    [SerializeField] private GameWinScreen gameWinScreen = null;
   


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

    [Header("Rewards (optional)")]
    [SerializeField] private RewardManager rewardManager;

    [Header("AI (optional)")]
    [SerializeField] private DynamicPacingAI dynamicPacingAI;

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

        if (rewardManager == null)
            rewardManager = GetComponent<RewardManager>();

        if (rewardManager == null)
            rewardManager = Object.FindFirstObjectByType<RewardManager>(FindObjectsInactive.Include);

        if (dynamicPacingAI == null)
            dynamicPacingAI = GetComponent<DynamicPacingAI>();

        if (dynamicPacingAI == null)
            dynamicPacingAI = Object.FindFirstObjectByType<DynamicPacingAI>(FindObjectsInactive.Include);
    }

    private void Start()
    {
        ResolveWordbankProvider();
        SetCurrentWord();
        UpdatePointDisplay();
        ResetTimer();

        ScoreKeeper.Set(score);
    }

    private void Update()
    {
        if (isGameOver) return;
        CheckInput();
        UpdateTimer();
        
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

        if (rawEventLogger != null)
            rawEventLogger.LogAttemptStarted(currentWord, countdownTime);

        if (strategyProfiler != null)
            strategyProfiler.BeginAttempt(currentWord, countdownTime);

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

        // Call DynamicPacingAI if assigned
        if (dynamicPacingAI != null && dynamicPacingAI.gameObject.activeInHierarchy)
        {
            dynamicPacingAI.OnWordStarted(word);
        }

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

        // Call DynamicPacingAI if assigned
        if (dynamicPacingAI != null && dynamicPacingAI.gameObject.activeInHierarchy)
        {
            dynamicPacingAI.OnWordResult(word, timeTakenSeconds, mistakes, completed);
        }

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

        if (rawEventLogger != null)
            rawEventLogger.LogAttemptEnded(completed, timeTakenSeconds, mistakesThisWord, typedCount, currentWord);

        if (strategyProfiler != null)
            strategyProfiler.CompleteAttempt(completed, timeTakenSeconds, mistakesThisWord);

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
        bool isCorrect = IsCorrectLetter(typedLetter);

        char typedChar = !string.IsNullOrEmpty(typedLetter) ? typedLetter[0] : '\0';
        char expectedChar = (!string.IsNullOrEmpty(remainingWord) && typedCount < remainingWord.Length)
            ? remainingWord[typedCount]
            : '\0';

        if (rawEventLogger != null)
            rawEventLogger.LogKeyPress(typedChar, expectedChar, typedCount, isCorrect, mistakesThisWord);

        if (strategyProfiler != null)
            strategyProfiler.RegisterKeyPress(isCorrect);

        if (isCorrect)
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

    public void SetCountdownTime(float newTime)
    {
        float previousCountdownTime = countdownTime;
        countdownTime = Mathf.Max(0.5f, newTime);

        if (!string.IsNullOrEmpty(currentWord) && previousCountdownTime > 0f)
        {
            float remainingRatio = Mathf.Clamp01(timer / previousCountdownTime);
            timer = Mathf.Max(0f, countdownTime * remainingRatio);
        }
        else
        {
            timer = countdownTime;
        }

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

    public bool SkipCurrentWord()
    {
        if (isGameOver) return false;
        if (string.IsNullOrEmpty(currentWord)) return false;

        SetCurrentWord();
        return true;
    }

    private void UpdateTimerDisplay()
    {
        if (timerOutput != null)
            timerOutput.text = "Time: " + Mathf.Ceil(timer).ToString();
    }

    public void AddTime(float amount)
    {
        if (isGameOver) return;
        if (amount <= 0f) return;

        timer += amount;
        UpdateTimerDisplay();
    }

    private void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void Win()
    {
        if (rewardManager != null)
        {
            Debug.Log("RewardManager Found");
            rewardManager.GrantLevelReward();
        }
        else
        {
            Debug.Log("RewardManager NULL");
        }

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
}
