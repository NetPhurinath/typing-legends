using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// =================================================================================================
// GameOver / Win Screen (ตัวจบเกม)
// - ไฟล์นี้คือ “แกนหลัก” ของหน้าจอ GameOver/Win: เปิด/ปิดหน้าจอ, pause เวลา, ตั้งหัวข้อ, ส่งคะแนนไปแสดง
// - โค้ดถูกแยกเป็นหลายไฟล์ด้วย partial class เพื่อแก้ง่าย:
//   - GameOverScreen.cs            : โครงหลัก + fields + Show/Hide
//   - GameOverScreen.Wiring.cs     : Auto-wire หา Text/Buttons จากลูก ๆ ของหน้าจอนี้
//   - GameOverScreen.Points.cs     : กติกาการแสดงคะแนน (POINTS)
//   - GameOverScreen.Actions.cs    : ปุ่ม Restart/Next/MainMenu และการโหลด Scene
//
// จุดที่แก้บ่อย (แก้แล้วได้อะไร):
// - gameOverTitle / winTitle      : เปลี่ยนข้อความหัวข้อ
// - pointsFormat                  : เปลี่ยนรูปแบบคะแนน (เฉพาะกรณีที่ไม่มี label "POINTS :" ใน UI)
// - winSceneName                  : ตอนชนะแล้วกด Next จะไปฉากไหน
// - restartButtonLabelGameOver/Win: เปลี่ยนชื่อปุ่ม Restart/Next
// =================================================================================================
public partial class GameOverScreen : MonoBehaviour
{
    [Header("Text (optional)")]
    // titleText: ข้อความหัวข้อ (เช่น GAME OVER / YOU WIN)
    [SerializeField] private TMP_Text titleText;
    // pointsText: ข้อความคะแนน (ถ้าใน UI มี label "POINTS :" แยกต่างหาก ระบบจะไปอัปเดตที่ label นั้นแทน)
    [SerializeField] private TMP_Text pointsText;

    [Header("Text (Legacy UI Text) - optional")]
    // รองรับ UI Text แบบเก่า (ไม่ใช่ TMP) เผื่อฉากเก่าบางฉากยังใช้
    [SerializeField] private Text titleTextLegacy;
    [SerializeField] private Text pointsTextLegacy;

    [Header("Score Source")]
    // true = เปิดหน้าจอเมื่อไหร่ (OnEnable) จะดึง ScoreKeeper.LastScore มาแสดงให้อัตโนมัติ
    [SerializeField] private bool autoFetchScoreOnEnable = true;
    // true = ถ้าไม่มี pointsText/pointsTextLegacy ระบบจะสร้าง Text ใหม่ให้เองเพื่อให้คะแนนไม่หาย
    [SerializeField] private bool autoCreatePointsTextIfMissing = true;

    [Header("Score Display")]
    // true = ถ้าจำเป็นจะพยายามปรับสไตล์คะแนนให้เข้ากับ titleText (สี/ฟอนต์/ตำแหน่ง)
    [SerializeField] private bool forcePointsStyleFromTitle = true;

    [Header("Titles")]
    [SerializeField] private string gameOverTitle = "GAME OVER";
    [SerializeField] private string winTitle = "YOU WIN";

    [Header("Score Text")]
    // ใช้เฉพาะกรณีที่ใน UI ไม่มี label "POINTS :" แยกไว้
    // - ถ้ามี {0} จะ string.Format ใส่คะแนนให้ เช่น "{0} POINTS"
    // - ถ้าไม่มี {0} จะเอาคะแนนไปต่อท้าย เช่น "POINTS :" -> "POINTS : 250"
    [SerializeField] private string pointsFormat = "{0} POINTS";

    [Header("Win (optional)")]
    // ตอนชนะแล้วกด Next จะโหลดฉากนี้ (ต้องอยู่ใน Scenes In Build)
    [SerializeField] private string winSceneName = "LevelSelection";

    [Header("Win Navigation (optional)")]
    // ถ้าเปิดใช้งาน: ตอนชนะแล้วกด Next จะพยายามไปด่านถัดไป (เช่น "Level 1" -> "Level 2")
    // ถ้าไม่สามารถโหลดด่านถัดไปได้ จะ fallback ไป winSceneName
    [SerializeField] private bool autoAdvanceToNextLevelOnWin = true;

    // รูปแบบชื่อ scene ของด่านถัดไป (ต้องมี {0}) เช่น "Level {0}"
    [SerializeField] private string nextLevelSceneNameFormat = "Level {0}";

    [Header("Buttons")]
    [SerializeField] private string restartButtonLabelGameOver = "Restart";
    [SerializeField] private string restartButtonLabelWin = "Next";

    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private GameObject backgroundOverlay; // Optional background panel (e.g., dim screen)

    private float previousTimeScale = 1f;
    private bool showingWin;
    private bool hasExplicitPoints;
    private int lastShownPoints;
    private bool restartHooked;
    private bool mainMenuHooked;


    /// <summary>
    /// Awake: รันครั้งแรกตอนสร้าง object
    /// - ปรับค่า winSceneName กันฉากเก่าที่ serialize ค่าเดิมผิด
    /// - เรียก TryAutoWire/EnsurePointsText เพื่อให้ reference พร้อม
    /// - ปิดหน้าจอไว้ก่อน (gameObject.SetActive(false))
    ///
    /// แก้เมธอดนี้แล้วได้อะไร:
    /// - เปลี่ยนค่า default/migration, เปลี่ยนพฤติกรรมเริ่มต้นของหน้าจอ
    /// </summary>
    private void Awake()
    {
        // Migration / safety: older scenes may have serialized the previous default.
        if (string.IsNullOrWhiteSpace(winSceneName) || winSceneName == "Level 2")
            winSceneName = "LevelSelection";

        TryAutoWire();
        EnsurePointsText();

        // Hide at start
        gameObject.SetActive(false);

        EnsureButtonHooks();

        if (mainMenuButton == null)
            Debug.LogWarning($"{nameof(GameOverScreen)}: mainMenuButton is not set.", this);

        if (pointsText == null)
        {
            if (pointsTextLegacy == null)
                Debug.LogWarning($"{nameof(GameOverScreen)}: pointsText is not set (score will not display).", this);
        }
    }

    /// <summary>
    /// OnEnable: รันทุกครั้งที่หน้าจอนี้ถูกเปิด (SetActive(true))
    /// - auto-wire + ensure text/button พร้อม
    /// - ถ้าไม่เคยเรียก Show(points) มาก่อน จะดึง ScoreKeeper.LastScore มาแสดงแทน
    ///   (ช่วยกรณีเปิดหน้าจอผ่าน inspector/trigger โดยไม่ได้ส่งคะแนน)
    ///
    /// แก้เมธอดนี้แล้วได้อะไร:
    /// - คุมว่าเปิดหน้าจอแล้วจะแสดงคะแนนจากไหน/เวลาไหน
    /// </summary>
    private void OnEnable()
    {
        TryAutoWire();
        EnsurePointsText();
        EnsureButtonHooks();

        if (!autoFetchScoreOnEnable) return;

        // If this screen was activated without calling Show(points), still show the latest score.
        if (!hasExplicitPoints)
        {
            UpdatePointsText(ScoreKeeper.LastScore);
            return;
        }

        // If Show() ran while inactive and texts weren't wired yet, refresh now.
        if (pointsText == null && pointsTextLegacy == null)
        {
            TryAutoWire();
            EnsurePointsText();
        }
        UpdatePointsText(lastShownPoints);
    }

    /// <summary>
    /// OnDisable: รันเมื่อหน้าจอนี้ถูกปิด (SetActive(false))
    /// - รีเซ็ต flag เพื่อให้การเปิดครั้งถัดไปสามารถ auto-fetch คะแนนได้
    /// </summary>
    private void OnDisable()
    {
        // Next time we open, allow auto-fetch to populate if Show() isn't called.
        hasExplicitPoints = false;
    }



    // Show: เปิดหน้าจอ GameOver พร้อมคะแนน (และหยุดเวลาเกม)
    /// <summary>
    /// Show(points): API แบบสั้นสำหรับ “แพ้”
    /// - เทียบเท่า Show(points, false)
    ///
    /// แก้เมธอดนี้แล้วได้อะไร:
    /// - ถ้าอยากให้เรียก Show(points) แล้วเป็นโหมดอื่น ก็ปรับที่นี่ได้
    /// </summary>
    public void Show(int points)
    {
        Show(points, false);
    }

    // Show(points, isWin): ใช้ร่วมกันทั้งแพ้/ชนะ
    // - isWin=false => GAME OVER + ปุ่มเป็น Restart
    // - isWin=true  => YOU WIN    + ปุ่มเป็น Next และโหลด winSceneName
    /// <summary>
    /// Show(points, isWin): เปิดหน้าจอจบเกม พร้อมคะแนน และ pause เกม
    /// ผลที่เกิดขึ้น:
    /// - ตั้งสถานะ showingWin
    /// - บันทึกคะแนนลง ScoreKeeper
    /// - ตั้ง title และข้อความคะแนน
    /// - เปิดหน้าจอ + backgroundOverlay (ถ้ามี)
    /// - pause เกมด้วย Time.timeScale = 0 (และจำค่าเดิมไว้ใน previousTimeScale)
    ///
    /// แก้เมธอดนี้แล้วได้อะไร:
    /// - เปลี่ยน UI ตอนชนะ/แพ้, เปลี่ยนเวลาที่ pause, หรือเพิ่มเอฟเฟกต์ก่อนแสดงผล
    /// </summary>
    public void Show(int points, bool isWin)
    {
        TryAutoWire();
        EnsureButtonHooks();

        showingWin = isWin;
        hasExplicitPoints = true;
        lastShownPoints = points;

        ScoreKeeper.Set(points);

        // Persist basic progression on win.
        // We store "Lv{N}" >= 1 when the current level is completed.
        // LevelSelection uses Lv(N-1) > 0 to unlock level N.
        if (isWin)
            SaveLevelCompletedFlagForCurrentScene();

        EnsurePointsText();

        SetRestartButtonLabel(isWin);

        if (titleText != null) titleText.text = isWin ? winTitle : gameOverTitle;
        if (titleTextLegacy != null) titleTextLegacy.text = isWin ? winTitle : gameOverTitle;

        UpdatePointsText(points);

        gameObject.SetActive(true);
        if (backgroundOverlay != null) backgroundOverlay.SetActive(true);

        // Pause time so gameplay stops when popup is shown
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
    }

    private static void SaveLevelCompletedFlagForCurrentScene()
    {
        // Parse level number from scene name (e.g. "Level 1" -> 1)
        var sceneName = SceneManager.GetActiveScene().name;
        var match = System.Text.RegularExpressions.Regex.Match(sceneName ?? string.Empty, @"\d+");
        if (!match.Success) return;
        if (!int.TryParse(match.Value, out int levelNumber)) return;
        if (levelNumber <= 0) return;

        string key = $"Lv{levelNumber}";
        int oldValue = PlayerPrefs.GetInt(key, 0);
        if (oldValue < 1) PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
    }

    // Hide: ปิดหน้าจอ และคืนค่าเวลาเดิม
    /// <summary>
    /// Hide(): ปิดหน้าจอจบเกม และ unpause โดยคืนค่า Time.timeScale
    ///
    /// แก้เมธอดนี้แล้วได้อะไร:
    /// - ถ้าอยากให้ปิดแล้วทำอย่างอื่นต่อ (เช่น เล่นต่อ) สามารถเพิ่ม logic ที่นี่ได้
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
        if (backgroundOverlay != null) backgroundOverlay.SetActive(false);
        Time.timeScale = previousTimeScale;
    }

}
