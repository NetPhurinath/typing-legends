using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class LevelSelection : MonoBehaviour
{
    // unlocked = ปุ่มด่านนี้กดได้ไหม (true = เข้าได้, false = ล็อค)
    [SerializeField] private bool unlocked = false;

    [Header("UI")]
    // unlockImage = รูป/เลเยอร์รูปกุญแจ/ล็อค (แสดงตอนด่านถูกล็อค)
    public Image unlockImage;

    // clearImage = รูป/เลเยอร์ "CLEAR" (แสดงเมื่อด่านนี้เคยผ่านแล้ว)
    public Image clearImage;

    [Header("Optional (recommended)")]
    [Tooltip("If set, used to detect level number for clear/unlock (e.g. 'Level 1').")]
    // แนะนำให้กรอก: ชื่อซีนจริงใน Build Settings (เช่น "Level 1")
    // ถ้ากรอกแล้วสคริปต์จะไม่ต้องเดาจากชื่อ GameObject (ลดโอกาสเดาผิด)
    [SerializeField] private string levelSceneName;

    private int levelNum;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        // ถ้าใน Inspector ยังไม่ได้ผูก OnClick (แบบ Persistent) ไว้
        // จะผูกให้เอง เพื่อให้ด่านที่ปลดล็อคแล้วกดได้เลย
        // หมายเหตุ: ตรวจเฉพาะ persistent event (ที่เห็นใน Inspector)
        if (button != null && button.onClick.GetPersistentEventCount() == 0)
            button.onClick.AddListener(OnAutoClick);
    }

    // ตอน GameObject ถูกเปิดใช้งาน (เช่น เปิดหน้าเลือกด่าน, กลับมาจากด่าน)
    // จะรีเฟรชสถานะ/รูปทุกครั้ง
    private void OnEnable() => UpdateAll();

    private void UpdateAll()
    {
        // 1) หาเลขด่าน (levelNum)
        levelNum = ResolveLevelNumber();

        // 2) คำนวณว่าปลดล็อคหรือยัง (unlocked)
        UpdateLevelStatus();

        // 3) อัปเดต UI (โชว์ล็อค/โชว์ CLEAR)
        UpdateLevelImage();

        // 4) ปุ่มกดได้เฉพาะด่านที่ปลดล็อค
        if (button != null)
            button.interactable = unlocked;
    }

    public void Refresh() => UpdateAll();

    private void OnAutoClick()
    {
        // ใช้ชื่อซีนจาก levelSceneName ถ้ามี ไม่งั้นเดาเป็น "Level {เลข}"
        var target = ResolveTargetSceneName();
        if (!string.IsNullOrWhiteSpace(target))
            PressSelection(target);
    }

    private string ResolveTargetSceneName()
    {
        // ถ้าผู้ใช้ตั้งชื่อซีนไว้แล้ว ให้ใช้ตรงนั้นเลย (ชัวร์สุด)
        if (!string.IsNullOrWhiteSpace(levelSceneName))
            return levelSceneName;

        // ไม่ได้ตั้ง -> เดาจากเลขด่านที่หาได้ แล้วประกอบเป็น "Level {n}"
        int n = ResolveLevelNumber();
        return n > 0 ? $"Level {n}" : string.Empty;
    }

    // คีย์ที่ใช้เก็บความคืบหน้าใน PlayerPrefs
    // ตัวอย่าง: Lv1=1 แปลว่าเคลียร์ด่าน 1 แล้ว
    private static string LevelKey(int n) => "Lv" + n.ToString();

    private int ResolveLevelNumber()
    {
        // หน้าที่: หา "เลขด่าน" เพื่อเอาไปเช็คปลดล็อค/เคลียร์
        // ลำดับความน่าเชื่อถือ:
        // 1) ดึงจาก levelSceneName (ถ้ากำหนดไว้)
        // 2) ถ้าไม่มี ให้เดาจากชื่อ GameObject (เช่น "LevelButton3")

        // 1) Prefer explicit scene name (most reliable)
        if (!string.IsNullOrWhiteSpace(levelSceneName))
        {
            var m = Regex.Match(levelSceneName, @"\d+");
            if (m.Success && int.TryParse(m.Value, out int n) && n > 0) return n;
        }

        // 2) Fallback: parse from this button object's name
        {
            var m = Regex.Match(gameObject.name, @"\d+");
            if (m.Success && int.TryParse(m.Value, out int n) && n > 0) return n;
        }

        return 0;
    }

    private void UpdateLevelStatus()
    {
        // หน้าที่: ตั้งค่า unlocked
        // กติกา:
        // - ถ้าอ่านเลขด่านไม่ได้ -> ล็อค
        // - ด่าน 1 ปลดล็อคเสมอ
        // - ด่านอื่น ๆ ปลดล็อคเมื่อ "ด่านก่อนหน้า" เคลียร์แล้ว
        if (levelNum <= 0)
        {
            unlocked = false;
            return;
        }

        if (levelNum == 1)
        {
            unlocked = true;
            return;
        }

        // PlayerPrefs: Lv{n}=1 หมายถึงเคลียร์ด่าน n แล้ว
        unlocked = PlayerPrefs.GetInt(LevelKey(levelNum - 1), 0) > 0;
    }

    private void UpdateLevelImage()
    {
        // หน้าที่: โชว์/ซ่อนรูปล็อค และรูป CLEAR ตามสถานะ
        if (unlockImage != null)
            unlockImage.gameObject.SetActive(!unlocked);

        // cleared = เคยเคลียร์ด่านนี้แล้วไหม (ดูจาก Lv{levelNum})
        bool cleared = levelNum > 0 && PlayerPrefs.GetInt(LevelKey(levelNum), 0) > 0;
        if (clearImage != null)
            clearImage.gameObject.SetActive(cleared);

        // แก้ไขตรงนี้ได้อะไร:
        // - กันไม่ให้ Log สแปมตอนเล่นจริง (Build) แต่ยังช่วยดีบักใน Editor/Development ได้
        // - ถ้าอยากปิดดีบักเลย ให้คอมเมนต์ทั้งบล็อกนี้ออก
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            $"LevelSelection '{name}': levelNum={levelNum}, unlocked={unlocked}, " +
            $"prevKey={(levelNum > 1 ? LevelKey(levelNum - 1) : "-")}, " +
            $"thisKey={(levelNum > 0 ? LevelKey(levelNum) : "-")}, " +
            $"cleared={(clearImage ? clearImage.gameObject.activeSelf : false)}"
        );
#endif
    }

    public void PressSelection(string levelName)
    {
        // เผื่อกดตอนสถานะยังไม่อัปเดตล่าสุด ให้รีเฟรชก่อน
        UpdateAll();
        if (!unlocked) return;
        if (string.IsNullOrWhiteSpace(levelName)) return;

        // โหลดซีนด่าน
        SceneManager.LoadScene(levelName);
    }
}
