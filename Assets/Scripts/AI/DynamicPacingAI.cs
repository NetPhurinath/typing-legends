using UnityEngine;

/// <summary>
/// Dynamic Pacing AI - ระบบปรับจังหวะของเกมตามฝีมือผู้เล่น
/// 
/// หน้าที่:
/// - ดูความเร็ว (WPM) และความแม่นยำของผู้เล่น
/// - ปรับเวลาต่อคำ, ความกดดัน, และแรงกดดันของศัตรู
/// - เปลี่ยน state ตาม performance ของผู้เล่น (Normal/Pressure/Recovery/Burst)
/// 
/// ไม่ทับกับ TypeMasterAI:
/// - TypeMasterAI = ปรับความยากของ "คำ"
/// - DynamicPacingAI = ปรับ "จังหวะและความกดดัน" ของเกม
/// </summary>
public class DynamicPacingAI : MonoBehaviour
{
    public enum PacingState
    {
        Normal,      // ปกติ เล่นเหมือนเดิม
        Pressure,    // กดดัน เร่งเกม ท้าทายมากขึ้น
        Recovery,    // ช่วยเหลือ ผ่อนเกมลง
        Burst        // โหมดพิเศษ เร่งแบบสั้น ๆ เพื่อให้ตื่นเต้น
    }

    [Header("อ้างอิงคอมโพเนนต์")]
    [SerializeField] private TypingStrategyProfiler strategyProfiler;
    [SerializeField] private Typer typer;
    [SerializeField] private MonsterHealth monsterHealth;

    [Header("ตั้งค่าการเปลี่ยน State")]
    [Tooltip("WPM ต้องมากกว่านี้ เกมจึงเข้า Pressure state")]
    [Min(0f)] public float pressureWpmThreshold = 30f;

    [Tooltip("Accuracy ต้องมากกว่านี้ เกมจึงเข้า Pressure state")]
    [Range(0f, 1f)] public float pressureAccuracyThreshold = 0.85f;

    [Tooltip("Mistake rate ต้องเกินนี้ จึงเข้า Recovery state")]
    [Range(0f, 1f)] public float recoveryMistakeThreshold = 0.3f;

    [Tooltip("ต้องพิมพ์ดีต่อเนื่องกี่ครั้งถึง Burst")]
    [Min(1)] public int burstRequiredStreak = 3;

    [Header("ปรับเวลา")]
    [Tooltip("ลดเวลาต่อคำลง % เมื่อ Pressure")]
    [Range(0f, 0.5f)] public float timerReducePercentPressure = 0.2f;

    [Tooltip("เพิ่มเวลาต่อคำ % เมื่อ Recovery")]
    [Range(0f, 0.5f)] public float timerIncreasePercentRecovery = 0.25f;

    [Header("ปรับศัตรู")]
    [Tooltip("ศัตรูโจมตีเร็วกว่านี้เปอร์เซ็นต์ในโหมด Pressure")]
    [Range(0f, 1f)] public float enemyAttackRateMultiplierPressure = 1.3f;

    [Tooltip("ศัตรูโจมตีช้าลงในโหมด Recovery")]
    [Range(0.5f, 1f)] public float enemyAttackRateMultiplierRecovery = 0.8f;

    [Header("Debug (อ่านอย่างเดียว)")]
    [SerializeField] private PacingState currentState = PacingState.Normal;
    [SerializeField] private int consecutiveGoodWords = 0;
    [SerializeField] private int consecutiveMistakes = 0;
    [SerializeField] private float debugLastWpm = 0f;
    [SerializeField] private float debugLastAccuracy = 0f;

    private float originalCountdownTime;
    private int stateChangeCounter = 0;
    private float lastRecordedWpm = 0f;
    private float lastRecordedAccuracy = 0f;

    private void Awake()
    {
        if (strategyProfiler == null)
            strategyProfiler = Object.FindFirstObjectByType<TypingStrategyProfiler>(FindObjectsInactive.Include);

        if (typer == null)
            typer = GetComponent<Typer>();

        if (typer == null)
            typer = Object.FindFirstObjectByType<Typer>(FindObjectsInactive.Include);

        if (monsterHealth == null)
            monsterHealth = Object.FindFirstObjectByType<MonsterHealth>(FindObjectsInactive.Include);

        if (typer != null)
            originalCountdownTime = typer.countdownTime;
    }

    private void Start()
    {
        // ถ้า Typer เป็น WordbankBehaviour ที่มี OnWordResult hook
        // Typer จะเรียก OnWordResult ของ DynamicPacingAI โดยอัตโนมัติ
        Debug.Log("DynamicPacingAI initialized and ready for word result callbacks");
    }

    private void Update()
    {
        if (typer == null) return;

        UpdateState();
        ApplyPacingEffects();
    }

    private void UpdateState()
    {
        PacingState newState = PacingState.Normal;

        float performanceWpm = lastRecordedWpm;
        float performanceAccuracy = lastRecordedAccuracy;

        // ถ้าพลาดติดกัน 1 ครั้ง ให้เข้า Recovery ทันที
        if (consecutiveMistakes >= 1)
        {
            newState = PacingState.Recovery;
        }
        // ถ้าทำดีติดกัน 3 ครั้งขึ้นไป ให้เข้า Burst
        else if (consecutiveGoodWords >= burstRequiredStreak)
        {
            newState = PacingState.Burst;
            consecutiveGoodWords = 0;
        }
        // ถ้าผู้เล่นทำได้ดีติดกัน 2 ครั้ง หรือทำงานเร็วแม่นยำตามเกณฑ์ ให้เข้า Pressure
        else if (consecutiveGoodWords >= 2 || (performanceWpm >= pressureWpmThreshold && performanceAccuracy >= pressureAccuracyThreshold))
        {
            newState = PacingState.Pressure;
        }
        else if (performanceAccuracy > 0f && performanceAccuracy < 0.75f)
        {
            newState = PacingState.Recovery;
        }

        if (newState != currentState)
        {
            currentState = newState;
            stateChangeCounter++;
            Debug.Log($"DynamicPacingAI: State changed to {currentState} (#{stateChangeCounter}). Good streak: {consecutiveGoodWords}, Mistake streak: {consecutiveMistakes}, WPM: {performanceWpm:F1}, Accuracy: {performanceAccuracy:P0}");
        }
    }

    private void ApplyPacingEffects()
    {
        if (typer == null) return;

        switch (currentState)
        {
            case PacingState.Normal:
                ResetTimer();
                break;

            case PacingState.Pressure:
                ApplyPressure();
                break;

            case PacingState.Recovery:
                ApplyRecovery();
                break;

            case PacingState.Burst:
                ApplyBurst();
                break;
        }
    }

    private void ResetTimer()
    {
        typer.SetCountdownTime(originalCountdownTime);
    }

    private void ApplyPressure()
    {
        float reducedTime = originalCountdownTime * (1f - timerReducePercentPressure);
        typer.SetCountdownTime(Mathf.Max(1f, reducedTime));

        if (monsterHealth != null)
        {
            // สามารถเพิ่มดาเมจศัตรูได้ถ้าต้องการ
            // monsterHealth.AttackRateMultiplier = enemyAttackRateMultiplierPressure;
        }
    }

    private void ApplyRecovery()
    {
        float extendedTime = originalCountdownTime * (1f + timerIncreasePercentRecovery);
        typer.SetCountdownTime(extendedTime);

        if (monsterHealth != null)
        {
            // ลดแรงกดดันศัตรู
            // monsterHealth.AttackRateMultiplier = enemyAttackRateMultiplierRecovery;
        }
    }

    private void ApplyBurst()
    {
        // Burst = เร่งแบบสั้น ๆ เพื่อให้ตื่นเต้น
        float burstTime = originalCountdownTime * 0.85f;
        typer.SetCountdownTime(Mathf.Max(1f, burstTime));
    }

    public PacingState CurrentState => currentState;
    public int ConsecutiveGoodWords => consecutiveGoodWords;
    public int ConsecutiveMistakes => consecutiveMistakes;

    /// <summary>
    /// Hook ที่ Typer จะเรียก หลังจากแต่ละคำเสร็จสิ้น
    /// ใช้สำหรับติดตามผลการพิมพ์ของผู้เล่นแต่ละครั้ง
    /// 
    /// วิธีใช้:
    /// Typer จะหา OnWordResult method โดยอัตโนมัติ ถ้า DynamicPacingAI ถูก assign ใน Wordbank Behaviour
    /// </summary>
    public void OnWordResult(string word, float timeTakenSeconds, int mistakes, bool completed)
    {
        if (!completed)
        {
            // ผู้เล่นไม่ทันพิมพ์คำให้จบ
            consecutiveMistakes++;
            consecutiveGoodWords = 0;
            UpdatePerformanceSnapshot(word, timeTakenSeconds, mistakes, completed);
            return;
        }

        // ผู้เล่นพิมพ์คำให้จบสำเร็จ
        if (mistakes == 0)
        {
            // ไม่มีความผิด เป็นคำที่ดี
            consecutiveGoodWords++;
            consecutiveMistakes = 0;
        }
        else if (mistakes <= 1)
        {
            // ผิดนิดหน่อย ยังถือว่าดี
            consecutiveGoodWords++;
            consecutiveMistakes = 0;
        }
        else
        {
            // ผิดมากกว่า 1 ครั้ง นับเป็นการพลาด
            consecutiveMistakes++;
            consecutiveGoodWords = 0;
        }

        UpdatePerformanceSnapshot(word, timeTakenSeconds, mistakes, completed);
        Debug.Log($"DynamicPacingAI: Word '{word}' completed. Good: {consecutiveGoodWords}, Mistakes: {consecutiveMistakes}, WPM: {lastRecordedWpm:F1}, Accuracy: {lastRecordedAccuracy:P0}");
    }

    private void UpdatePerformanceSnapshot(string word, float timeTakenSeconds, int mistakes, bool completed)
    {
        if (string.IsNullOrEmpty(word))
        {
            debugLastWpm = 0f;
            debugLastAccuracy = 0f;
            lastRecordedWpm = 0f;
            lastRecordedAccuracy = 0f;
            return;
        }

        int totalChars = Mathf.Max(1, word.Length);
        float safeSeconds = Mathf.Max(0.25f, timeTakenSeconds);
        float rawWpm = (totalChars / 5f) / (safeSeconds / 60f);
        float accuracy = completed
            ? Mathf.Clamp01(1f - (mistakes / (float)totalChars))
            : Mathf.Clamp01(1f - (mistakes / (float)Mathf.Max(1, totalChars)));

        lastRecordedWpm = rawWpm;
        lastRecordedAccuracy = accuracy;
        debugLastWpm = rawWpm;
        debugLastAccuracy = accuracy;
    }

    /// <summary>
    /// Optional hook: Typer อาจเรียกนี้ตอนเริ่มพิมพ์คำใหม่
    /// </summary>
    public void OnWordStarted(string word)
    {
        // เว้นไว้สำหรับอนาคต เช่น เก็บเวลาเริ่ม หรือทำ VFX
    }
    //////////////////////////////////////////////////
    private void OnGUI()
    {
        // Debug display บนหน้าจอเพื่อแสดงสถานะ AI
        GUI.color = Color.white;
        GUIStyle debugStyle = new GUIStyle(GUI.skin.label);
        debugStyle.fontSize = 16;
        debugStyle.richText = true;
        
        string timerText = (typer != null) ? typer.countdownTime.ToString("F2") + "s" : "N/A";
        string originalText = originalCountdownTime.ToString("F2") + "s";
        
        string debugInfo = 
            $"<b>DynamicPacingAI Debug</b>\n" +
            $"State: <color=yellow>{currentState}</color>\n" +
            $"Good Streak: {consecutiveGoodWords} | Mistakes: {consecutiveMistakes}\n" +
            $"WPM: {debugLastWpm:F1} | Accuracy: {debugLastAccuracy:P0}\n" +
            $"Timer: {timerText} (Original: {originalText})\n" +
            $"Reduce: -{(timerReducePercentPressure * 100):F0}% | Add: +{(timerIncreasePercentRecovery * 100):F0}%\n" +
            $"References: StrategyProfiler={(strategyProfiler != null ? "✓" : "✗")}, Typer={(typer != null ? "✓" : "✗")}, MonsterHealth={(monsterHealth != null ? "✓" : "✗")}";
        
        GUI.Label(new Rect(10, 10, 500, 150), debugInfo);
    }
    ////////////////////////////////////////////////////////
}
