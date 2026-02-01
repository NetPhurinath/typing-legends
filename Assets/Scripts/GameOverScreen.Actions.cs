using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public partial class GameOverScreen
{
    // =====================================================================
    // Actions (ปุ่ม/การนำทาง)
    // - Restart/Next: ถ้าแพ้ => โหลดฉากเดิม, ถ้าชนะ => โหลด winSceneName
    // - Main Menu: โหลดฉาก "MainMenu"
    //
    // จุดที่แก้บ่อย:
    // - winSceneName (ใน GameOverScreen.cs) : ชนะแล้วกด Next ไปฉากไหน
    // - restartButtonLabelGameOver/Win       : เปลี่ยนข้อความบนปุ่ม
    // - ชื่อฉาก MainMenu                     : ถ้าเปลี่ยนชื่อ scene ต้องแก้ใน OnMainMenuPressed
    // =====================================================================

    /// <summary>
    /// ตั้งข้อความบนปุ่ม restartButton ตามสถานะชนะ/แพ้
    /// - isWin=false => ใช้ restartButtonLabelGameOver (เช่น "Restart")
    /// - isWin=true  => ใช้ restartButtonLabelWin (เช่น "Next")
    ///
    /// แก้เมธอดนี้แล้วได้อะไร:
    /// - เปลี่ยนตรรกะการเลือก label, หรือเลือก TMP/Legacy Text ที่จะไปอัปเดต
    /// </summary>
    private void SetRestartButtonLabel(bool isWin)
    {
        if (restartButton == null) return;

        string label = isWin ? restartButtonLabelWin : restartButtonLabelGameOver;
        if (string.IsNullOrWhiteSpace(label)) return;

        var tmp = restartButton.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null)
        {
            tmp.text = label;
            return;
        }

        var legacy = restartButton.GetComponentInChildren<Text>(true);
        if (legacy != null)
        {
            legacy.text = label;
        }
    }

    /// <summary>
    /// ผูก event ของปุ่ม (onClick) ให้เรียกเมธอดของเรา
    /// - กันการผูกซ้ำด้วยตัวแปร restartHooked / mainMenuHooked
    ///
    /// แก้เมธอดนี้แล้วได้อะไร:
    /// - เพิ่มปุ่มใหม่, เปลี่ยน handler, หรือเปลี่ยนเงื่อนไขการผูกได้
    /// </summary>
    private void EnsureButtonHooks()
    {
        // กันการ AddListener ซ้ำ (จะทำให้กด 1 ครั้งแล้วโหลดซ้ำหลายครั้ง)
        if (restartButton != null && !restartHooked)
        {
            restartButton.onClick.AddListener(OnRestartPressed);
            restartHooked = true;
        }

        if (mainMenuButton != null && !mainMenuHooked)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuPressed);
            mainMenuHooked = true;
        }
    }

    // Restart/Next:
    // - เกมแพ้  : รีโหลดฉากปัจจุบัน
    // - เกมชนะ  : โหลด winSceneName (ต้องอยู่ใน Scenes In Build)
    // - ทุกกรณี : คืนค่า Time.timeScale ก่อน
    /// <summary>
    /// เมื่อกดปุ่ม Restart/Next
    /// - คืนค่าเวลา (Time.timeScale) ก่อนเสมอ เพื่อให้ฉากใหม่ไม่ค้างอยู่ที่ 0
    /// - ถ้าเป็นโหมดชนะ (showingWin=true) จะโหลด winSceneName
    /// - ถ้าเป็นโหมดแพ้ จะ reload ฉากปัจจุบัน
    ///
    /// แก้เมธอดนี้แล้วได้อะไร:
    /// - เปลี่ยนพฤติกรรมปุ่ม Next (เช่น ไปหน้าคัดด่าน/ไปด่านถัดไป)
    /// - เพิ่มเอฟเฟกต์/เสียงก่อนโหลดฉาก
    /// </summary>
    public void OnRestartPressed()
    {
        if (backgroundOverlay != null) backgroundOverlay.SetActive(false);
        Time.timeScale = previousTimeScale;

        if (showingWin)
        {
            // Win: Next -> try next level (Level N+1) first, then fallback to winSceneName
            if (autoAdvanceToNextLevelOnWin)
            {
                // Most reliable: go to next scene in Build Settings order.
                int currentIndex = SceneManager.GetActiveScene().buildIndex;
                int nextIndex = currentIndex + 1;
                if (currentIndex >= 0 && nextIndex < SceneManager.sceneCountInBuildSettings)
                {
                    SceneManager.LoadScene(nextIndex);
                    return;
                }

                // Fallback: derive next level name from the current scene name (e.g., "Level 1" -> "Level 2").
                if (TryGetNextLevelSceneName(out string nextLevelSceneName) &&
                    Application.CanStreamedLevelBeLoaded(nextLevelSceneName))
                {
                    SceneManager.LoadScene(nextLevelSceneName);
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(winSceneName))
            {
                Debug.LogError($"{nameof(GameOverScreen)}: winSceneName is empty.", this);
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(winSceneName))
            {
                Debug.LogError($"{nameof(GameOverScreen)}: Scene '{winSceneName}' cannot be loaded. Add it to File > Build Profiles/Settings > Scenes In Build.", this);
                return;
            }
            SceneManager.LoadScene(winSceneName);
            return;
        }

        var current = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(current);
    }

    private bool TryGetNextLevelSceneName(out string nextSceneName)
    {
        nextSceneName = null;

        if (string.IsNullOrWhiteSpace(nextLevelSceneNameFormat) || !nextLevelSceneNameFormat.Contains("{0}"))
            return false;

        var currentSceneName = SceneManager.GetActiveScene().name;
        var match = System.Text.RegularExpressions.Regex.Match(currentSceneName ?? string.Empty, @"\d+");
        if (!match.Success) return false;
        if (!int.TryParse(match.Value, out int currentLevelNumber)) return false;
        if (currentLevelNumber <= 0) return false;

        nextSceneName = string.Format(nextLevelSceneNameFormat, currentLevelNumber + 1);
        return !string.IsNullOrWhiteSpace(nextSceneName);
    }

    // Main menu: กลับหน้าเมนูหลัก และคืนค่าเวลา
    /// <summary>
    /// เมื่อกดปุ่ม Main Menu
    /// - คืนค่าเวลา
    /// - โหลดฉาก "MainMenu"
    ///
    /// แก้เมธอดนี้แล้วได้อะไร:
    /// - ถ้าเปลี่ยนชื่อ scene เมนู หรืออยากไปหน้าอื่น ให้แก้ชื่อที่ LoadScene
    /// </summary>
    public void OnMainMenuPressed()
    {
        if (backgroundOverlay != null) backgroundOverlay.SetActive(false);
        Time.timeScale = previousTimeScale;

        // In this project, the WIN screen's "MENU" should return to the level selection screen.
        // Reuse winSceneName as the configured target (default: "LevelSelection").
        var target = string.IsNullOrWhiteSpace(winSceneName) ? "LevelSelection" : winSceneName;
        if (!Application.CanStreamedLevelBeLoaded(target))
        {
            Debug.LogError($"{nameof(GameOverScreen)}: Scene '{target}' cannot be loaded. Add it to Scenes In Build.", this);
            return;
        }
        SceneManager.LoadScene(target);
    }
}
