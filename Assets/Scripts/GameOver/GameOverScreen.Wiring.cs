using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class GameOverScreen
{
    /// <summary>
    /// TryAutoWire(): พยายามหา/ผูก reference UI ให้อัตโนมัติ
    /// - หา titleText/pointsText (TMP) และ titleTextLegacy/pointsTextLegacy (Text เก่า)
    /// - หา restartButton/mainMenuButton
    /// - สำคัญ: จะเลือกเฉพาะ Component ที่อยู่ใต้ GameOverScreen object นี้เท่านั้น
    ///   เพื่อกันการไปหยิบ HUD/ข้อความที่อยู่นอกหน้าจอจบเกม (ซึ่งเคยทำให้คะแนนไม่ขึ้น)
    ///
    /// แก้เมธอดนี้แล้วได้อะไร:
    /// - ปรับหลักการ “เดาว่าอันไหนคือ title/points/buttons” จากชื่อ object
    /// - แก้ปัญหาฉากที่ลาก reference ผิด หรือไม่ได้ลากเลย
    /// </summary>
    private void TryAutoWire()
    {
        // =====================================================================
        // Auto-Wire (หาตัว UI ให้เอง)
        // - เป้าหมาย: ช่วยให้ฉากที่ลืมลาก reference ใน Inspector ยังทำงานได้
        // - ข้อสำคัญ: เราจำกัดให้ “หาเฉพาะลูกของ GameOverScreen ตัวนี้”
        //   เพื่อกันเคสที่ pointsText ไปอ้างผิดเป็น HUD ด้านบน (เคยทำให้คะแนนไม่ขึ้น)
        //
        // ถ้าคุณอยากคุมแบบชัวร์ที่สุด:
        // - ลาก titleText/pointsText/restartButton/mainMenuButton ใน Inspector ให้ครบ
        // - แล้ว TryAutoWire จะไม่ไปยุ่งกับของที่ตั้งไว้ถูกต้อง
        // =====================================================================

        // If scenes/prefabs accidentally reference HUD texts/buttons outside this screen,
        // clear them so we can re-wire to the correct children.
        // IsLocal: เช็คว่า Component อยู่ใน subtree ของหน้าจอนี้จริงไหม
        // (ถ้าไม่ใช่ => ให้ถือว่า reference ผิด/หลุด และเคลียร์ทิ้ง)
        bool IsLocal(Component c) => c != null && c.transform != null && (c.transform == transform || c.transform.IsChildOf(transform));

        if (titleText != null && !IsLocal(titleText)) titleText = null;
        if (pointsText != null && !IsLocal(pointsText)) pointsText = null;
        if (titleTextLegacy != null && !IsLocal(titleTextLegacy)) titleTextLegacy = null;
        if (pointsTextLegacy != null && !IsLocal(pointsTextLegacy)) pointsTextLegacy = null;
        if (restartButton != null && !IsLocal(restartButton)) restartButton = null;
        if (mainMenuButton != null && !IsLocal(mainMenuButton)) mainMenuButton = null;

        // canvas ใช้เป็น fallback เผื่อบางกรณีต้อง search กว้างขึ้น แต่สุดท้ายยัง filter ด้วย IsLocal
        var canvas = GetComponentInParent<Canvas>();

        // Prefer wiring within this screen object first (avoids grabbing unrelated HUD texts elsewhere).
        var localTmpTexts = GetComponentsInChildren<TMP_Text>(true);
        var localLegacyTexts = GetComponentsInChildren<Text>(true);
        var localButtons = GetComponentsInChildren<Button>(true);

        // 1) หา TMP_Text (titleText/pointsText)
        if (titleText == null || pointsText == null)
        {
            TMP_Text[] texts;
            if (localTmpTexts != null && localTmpTexts.Length > 0)
            {
                texts = localTmpTexts;
            }
            else
            {
                var all = canvas != null ? canvas.GetComponentsInChildren<TMP_Text>(true) : GetComponentsInChildren<TMP_Text>(true);
                if (all == null || all.Length == 0)
                {
                    texts = all;
                }
                else
                {
                    // Only consider TMP texts belonging to this screen.
                    var filtered = new System.Collections.Generic.List<TMP_Text>(all.Length);
                    foreach (var t in all)
                    {
                        if (IsLocal(t)) filtered.Add(t);
                    }
                    texts = filtered.ToArray();
                }
            }
            if (texts != null && texts.Length > 0)
            {
                if (titleText == null)
                {
                    foreach (var t in texts)
                    {
                        var n = t.name.ToLowerInvariant();
                        if (n.Contains("title") || n.Contains("result") || n.Contains("status"))
                        {
                            titleText = t;
                            break;
                        }
                    }

                    if (titleText == null && texts.Length >= 2) titleText = texts[0];
                }

                if (pointsText == null)
                {
                    foreach (var t in texts)
                    {
                        var n = t.name.ToLowerInvariant();
                        if ((n.Contains("point") || n.Contains("score")) && t.GetComponentInParent<Button>(true) == null)
                        {
                            pointsText = t;
                            break;
                        }
                    }

                    if (pointsText == null)
                    {
                        // Fallback: prefer a non-button text and not the same as titleText
                        foreach (var t in texts)
                        {
                            if (t == null) continue;
                            if (t == titleText) continue;
                            if (t.GetComponentInParent<Button>(true) != null) continue;
                            pointsText = t;
                            break;
                        }

                        if (pointsText == null)
                        {
                            if (texts.Length == 1) pointsText = texts[0];
                            else if (texts.Length >= 2) pointsText = texts[1];
                        }
                    }
                }
            }
        }

        // 2) หา Buttons (restartButton/mainMenuButton)
        if (restartButton == null || mainMenuButton == null)
        {
            Button[] buttons;
            if (localButtons != null && localButtons.Length > 0)
            {
                buttons = localButtons;
            }
            else
            {
                var all = canvas != null ? canvas.GetComponentsInChildren<Button>(true) : GetComponentsInChildren<Button>(true);
                if (all == null || all.Length == 0)
                {
                    buttons = all;
                }
                else
                {
                    var filtered = new System.Collections.Generic.List<Button>(all.Length);
                    foreach (var b in all)
                    {
                        if (IsLocal(b)) filtered.Add(b);
                    }
                    buttons = filtered.ToArray();
                }
            }
            if (buttons != null && buttons.Length > 0)
            {
                if (restartButton == null)
                {
                    foreach (var b in buttons)
                    {
                        var n = b.name.ToLowerInvariant();
                        if (n.Contains("restart") || n.Contains("again") || n.Contains("next"))
                        {
                            restartButton = b;
                            break;
                        }
                    }
                }

                if (mainMenuButton == null)
                {
                    foreach (var b in buttons)
                    {
                        var n = b.name.ToLowerInvariant();
                        if (n.Contains("menu") || n.Contains("main"))
                        {
                            mainMenuButton = b;
                            break;
                        }
                    }
                }
            }
        }

        // 3) หา Legacy Text (titleTextLegacy/pointsTextLegacy)
        if (titleTextLegacy == null || pointsTextLegacy == null)
        {
            Text[] legacyTexts;
            if (localLegacyTexts != null && localLegacyTexts.Length > 0)
            {
                legacyTexts = localLegacyTexts;
            }
            else
            {
                var all = canvas != null ? canvas.GetComponentsInChildren<Text>(true) : GetComponentsInChildren<Text>(true);
                if (all == null || all.Length == 0)
                {
                    legacyTexts = all;
                }
                else
                {
                    var filtered = new System.Collections.Generic.List<Text>(all.Length);
                    foreach (var t in all)
                    {
                        if (IsLocal(t)) filtered.Add(t);
                    }
                    legacyTexts = filtered.ToArray();
                }
            }
            if (legacyTexts != null && legacyTexts.Length > 0)
            {
                if (titleTextLegacy == null)
                {
                    foreach (var t in legacyTexts)
                    {
                        if (t == null) continue;
                        if (t.GetComponentInParent<Button>(true) != null) continue;

                        var n = t.name.ToLowerInvariant();
                        if (n == "text (legacy)" || n.Contains("title") || n.Contains("result") || n.Contains("status") || n.Contains("gameover"))
                        {
                            titleTextLegacy = t;
                            break;
                        }
                    }

                    if (titleTextLegacy == null)
                    {
                        foreach (var t in legacyTexts)
                        {
                            if (t == null) continue;
                            if (t.GetComponentInParent<Button>(true) != null) continue;
                            titleTextLegacy = t;
                            break;
                        }
                    }
                }

                if (pointsTextLegacy == null)
                {
                    // Best match: same parent container as title.
                    if (titleTextLegacy != null)
                    {
                        var titleParent = titleTextLegacy.transform.parent;
                        foreach (var t in legacyTexts)
                        {
                            if (t == null) continue;
                            if (t == titleTextLegacy) continue;
                            if (t.GetComponentInParent<Button>(true) != null) continue;
                            if (t.transform.parent != titleParent) continue;

                            var n = t.name.ToLowerInvariant();
                            if (n == "point" || n.Contains("point") || n.Contains("score"))
                            {
                                pointsTextLegacy = t;
                                break;
                            }
                        }
                    }

                    foreach (var t in legacyTexts)
                    {
                        if (t == null) continue;
                        if (t.GetComponentInParent<Button>(true) != null) continue;

                        var n = t.name.ToLowerInvariant();
                        if ((n == "point" || n.Contains("point") || n.Contains("score")) && t != titleTextLegacy)
                        {
                            pointsTextLegacy = t;
                            break;
                        }
                    }
                }
            }
        }

        // สุดท้าย: ผูก onClick ถ้ายังไม่ผูก
        EnsureButtonHooks();
    }
}
