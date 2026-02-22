using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class GameOverScreen
{
    // =====================================================================
    // Points/Score Display (การแสดงคะแนน)
    //
    // แนวทางหลักที่ใช้ตอนนี้:
    // - ถ้าใน UI มีข้อความ label ประมาณ "POINTS :" อยู่แล้ว => เราจะอัปเดต label นั้นให้กลายเป็น
    //   "POINTS : 250" (ใช้ Text เดียว) เพื่อ “กันตัวเลขซ้อนทับ” แบบถาวร
    // - ถ้าไม่มี label => ใช้ pointsText/pointsTextLegacy แสดงตาม pointsFormat
    //
    // ถ้าต้องการปรับข้อความ/รูปแบบ:
    // - แก้ pointsFormat ใน Inspector (อยู่ใน GameOverScreen.cs)
    // - หรือแก้ข้อความ label "POINTS :" ในฉาก (Text/TMP) ได้เลย
    // =====================================================================

    private string pointsInlineLabelBaseText;

    /// <summary>
    /// StripTrailingNumber: ตัด “ตัวเลขท้ายสุด” ออกจากข้อความ
    /// ตัวอย่าง: "POINTS : 250" -> "POINTS :"
    /// ใช้เพื่อเก็บ baseText ของ label แล้วเอาคะแนนใหม่ไปต่อท้ายได้เรื่อย ๆ
    /// </summary>
    private static string StripTrailingNumber(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;

        var trimmed = s.TrimEnd();
        int i = trimmed.Length - 1;
        while (i >= 0 && char.IsDigit(trimmed[i])) i--;
        while (i >= 0 && char.IsWhiteSpace(trimmed[i])) i--;
        return trimmed.Substring(0, i + 1).TrimEnd();
    }

    /// <summary>
    /// TryUpdateInlinePointsLabel: ถ้าใน UI มี label "POINTS :" อยู่แล้ว
    /// จะอัปเดต label นั้นเป็น "POINTS : <points>" และ return true
    ///
    /// ผลลัพธ์สำคัญ:
    /// - ใช้ Text เดียว => ไม่มีปัญหาเลขซ้อนทับกับคำว่า POINTS
    /// - จะซ่อน pointsText/PointsText ที่เคยถูกสร้างแยกไว้ (ถ้ามี) เพื่อลดความสับสน
    /// </summary>
    private bool TryUpdateInlinePointsLabel(int points)
    {
        // เคส “มี label POINTS :” => อัปเดต label นั้นเป็น "POINTS : <points>" แล้วจบ
        // ข้อดี: ไม่ต้องคำนวณตำแหน่ง 2 text และไม่เกิดการซ้อนทับ
        FindPointsLabel(out var tmpLabel, out var legacyLabel);
        if (tmpLabel == null && legacyLabel == null) return false;

        // Prefer TMP if both exist.
        if (tmpLabel != null)
        {
            if (string.IsNullOrWhiteSpace(pointsInlineLabelBaseText))
                pointsInlineLabelBaseText = StripTrailingNumber(tmpLabel.text);

            var baseText = string.IsNullOrWhiteSpace(pointsInlineLabelBaseText)
                ? "POINTS :"
                : pointsInlineLabelBaseText;

            tmpLabel.text = baseText + " " + points;
            tmpLabel.alignment = TextAlignmentOptions.Center;

            var rt = tmpLabel.rectTransform;
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0.5f, rt.anchorMin.y);
                rt.anchorMax = new Vector2(0.5f, rt.anchorMax.y);
                rt.pivot = new Vector2(0.5f, rt.pivot.y);
                rt.anchoredPosition = new Vector2(0f, rt.anchoredPosition.y);
            }

            if (pointsText != null && pointsText != tmpLabel && pointsText.transform.IsChildOf(transform))
                pointsText.gameObject.SetActive(false);
            if (pointsTextLegacy != null && pointsTextLegacy.transform.IsChildOf(transform))
                pointsTextLegacy.gameObject.SetActive(false);

            // If we previously auto-created a separate value text, hide it.
            var all = GetComponentsInChildren<Transform>(true);
            if (all != null)
            {
                foreach (var t in all)
                {
                    if (t == null) continue;
                    if (t.name != "PointsText") continue;
                    if (t == tmpLabel.transform) continue;
                    t.gameObject.SetActive(false);
                }
            }

            return true;
        }

        // Legacy label
        if (string.IsNullOrWhiteSpace(pointsInlineLabelBaseText))
            pointsInlineLabelBaseText = StripTrailingNumber(legacyLabel.text);

        var legacyBaseText = string.IsNullOrWhiteSpace(pointsInlineLabelBaseText)
            ? "POINTS :"
            : pointsInlineLabelBaseText;

        legacyLabel.text = legacyBaseText + " " + points;
        legacyLabel.alignment = TextAnchor.MiddleCenter;

        var legacyRt = (RectTransform)legacyLabel.transform;
        if (legacyRt != null)
        {
            legacyRt.anchorMin = new Vector2(0.5f, legacyRt.anchorMin.y);
            legacyRt.anchorMax = new Vector2(0.5f, legacyRt.anchorMax.y);
            legacyRt.pivot = new Vector2(0.5f, legacyRt.pivot.y);
            legacyRt.anchoredPosition = new Vector2(0f, legacyRt.anchoredPosition.y);
        }

        if (pointsText != null && pointsText.transform.IsChildOf(transform))
            pointsText.gameObject.SetActive(false);
        if (pointsTextLegacy != null && pointsTextLegacy != legacyLabel && pointsTextLegacy.transform.IsChildOf(transform))
            pointsTextLegacy.gameObject.SetActive(false);

        var allLegacy = GetComponentsInChildren<Transform>(true);
        if (allLegacy != null)
        {
            foreach (var t in allLegacy)
            {
                if (t == null) continue;
                if (t.name != "PointsText") continue;
                if (t == legacyLabel.transform) continue;
                t.gameObject.SetActive(false);
            }
        }

        return true;
    }

    /// <summary>
    /// LooksLikePointsLabel: heuristic เดาว่า Text ชิ้นนี้คือ label ของคะแนนหรือไม่
    /// - ดูจากมีคำว่า point/score และมี ':' หรือจบด้วย points
    ///
    /// แก้เมธอดนี้แล้วได้อะไร:
    /// - ปรับว่า “ข้อความแบบไหน” ถึงถือว่าเป็น label (รองรับภาษาไทย/ชื่ออื่นได้)
    /// </summary>
    private static bool LooksLikePointsLabel(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        var lower = s.ToLowerInvariant();
        if (!(lower.Contains("point") || lower.Contains("score"))) return false;
        return s.Contains(":") || lower.Trim() == "points" || lower.Trim().EndsWith("points");
    }

    /// <summary>
    /// FindPointsLabel: ค้นหา Text ที่เป็น label คะแนนภายในลูก ๆ ของหน้าจอนี้
    /// - คืนทั้ง TMP_Text และ legacy Text (ถ้ามี)
    /// - จะไม่เลือกข้อความที่อยู่ในปุ่ม (เพื่อกันไปหยิบ label บนปุ่ม)
    /// </summary>
    private void FindPointsLabel(out TMP_Text tmpLabel, out Text legacyLabel)
    {
        tmpLabel = null;
        legacyLabel = null;

        var tmpTexts = GetComponentsInChildren<TMP_Text>(true);
        if (tmpTexts != null)
        {
            foreach (var t in tmpTexts)
            {
                if (t == null) continue;
                if (t == pointsText) continue;
                if (t.GetComponentInParent<Button>(true) != null) continue;
                if (LooksLikePointsLabel(t.text))
                {
                    tmpLabel = t;
                    break;
                }
            }
        }

        var legacyTexts = GetComponentsInChildren<Text>(true);
        if (legacyTexts != null)
        {
            foreach (var t in legacyTexts)
            {
                if (t == null) continue;
                if (t == pointsTextLegacy) continue;
                if (t.GetComponentInParent<Button>(true) != null) continue;
                if (LooksLikePointsLabel(t.text))
                {
                    legacyLabel = t;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// UpdatePointsText: อัปเดตการแสดงคะแนนบนหน้าจอ
    /// ลำดับการทำงาน:
    /// 1) ถ้ามี label "POINTS :" => อัปเดต label นั้นเป็น "POINTS : <points>" แล้วจบ
    /// 2) ถ้าไม่มี label => แสดงผ่าน pointsText/pointsTextLegacy ตาม pointsFormat
    ///
    /// แก้เมธอดนี้แล้วได้อะไร:
    /// - เปลี่ยน rule ว่าจะเลือกแสดงคะแนนแบบไหน
    /// - เปลี่ยน format/การ fallback ในกรณีอ้างอิง UI ไม่ครบ
    /// </summary>
    private void UpdatePointsText(int points)
    {
        // If there is a dedicated label like "POINTS :", update that single label.
        // This avoids fragile dual-text layout and prevents overlap.
        if (TryUpdateInlinePointsLabel(points)) return;

        // เคส fallback: ไม่มี label => แสดงผ่าน pointsText/pointsTextLegacy

        EnsurePointsText();
        if (pointsText == null && pointsTextLegacy == null) return;

        EnsurePointsTextStyle();

        string text;
        var format = pointsFormat;
        if (string.IsNullOrWhiteSpace(format))
            format = "{0} POINTS";

        // If the inspector format is a label like "POINTS :" (no {0}), append the score.
        // หมายเหตุ: ถ้า pointsFormat ไม่มี {0} เราจะเอาคะแนนไปต่อท้ายให้เอง
        if (format.Contains("{0}"))
        {
            try { text = string.Format(format, points); }
            catch { text = points + " POINTS"; }
        }
        else
        {
            var trimmed = format.TrimEnd();
            text = trimmed.Length == 0 ? (points + " POINTS") : (trimmed + " " + points);
        }

        if (pointsText != null)
        {
            if (!pointsText.gameObject.activeSelf) pointsText.gameObject.SetActive(true);
            if (!pointsText.enabled) pointsText.enabled = true;
            var c = pointsText.color;
            if (c.a <= 0.01f) pointsText.color = new Color(c.r, c.g, c.b, 1f);
            pointsText.text = text;
        }

        if (pointsTextLegacy != null)
        {
            if (!pointsTextLegacy.gameObject.activeSelf) pointsTextLegacy.gameObject.SetActive(true);
            if (!pointsTextLegacy.enabled) pointsTextLegacy.enabled = true;
            var c = pointsTextLegacy.color;
            if (c.a <= 0.01f) pointsTextLegacy.color = new Color(c.r, c.g, c.b, 1f);
            pointsTextLegacy.text = text;
        }
    }

    /// <summary>
    /// EnsurePointsTextStyle: ปรับสไตล์/ตำแหน่งของ pointsText ให้ “มองเห็นแน่นอน”
    /// ใช้เฉพาะกรณี fallback ที่ต้องใช้ pointsText แสดงคะแนน (ไม่มี label)
    ///
    /// แก้เมธอดนี้แล้วได้อะไร:
    /// - ปรับการจัดวาง/ฟอนต์/สีของคะแนนให้เข้ากับ UI ของคุณ
    /// - ถ้าคุณคุมสไตล์จาก Inspector เองทั้งหมด สามารถปิด forcePointsStyleFromTitle ได้
    /// </summary>
    private void EnsurePointsTextStyle()
    {
        if (!forcePointsStyleFromTitle) return;

        // ใช้เฉพาะกรณี fallback (ไม่มี label) และมี pointsText อยู่
        // เป้าหมาย: กันเคสที่ฉากอ้างผิด/สไตล์เพี้ยน ทำให้คะแนนมองไม่เห็น

        // If the scene already has a "Point" text, make sure it's positioned & styled to be visible.
        if (pointsText != null && titleText != null)
        {
            var titleRt = titleText.rectTransform;
            var rt = pointsText.rectTransform;

            if (rt.localScale.sqrMagnitude < 0.0001f) rt.localScale = Vector3.one;

            rt.anchorMin = titleRt.anchorMin;
            rt.anchorMax = titleRt.anchorMax;
            rt.pivot = titleRt.pivot;
            if (rt.sizeDelta == Vector2.zero) rt.sizeDelta = titleRt.sizeDelta;

            // If points is sitting on top of the title (common when manually duplicated), move it below.
            if (Vector2.Distance(rt.anchoredPosition, titleRt.anchoredPosition) < 1f)
                rt.anchoredPosition = titleRt.anchoredPosition + new Vector2(0f, -Mathf.Max(40f, titleRt.sizeDelta.y * 0.9f));

            pointsText.font = titleText.font;
            pointsText.fontSize = Mathf.Max(18f, titleText.fontSize * 0.6f);
            pointsText.color = titleText.color;
            pointsText.alignment = titleText.alignment;
            pointsText.raycastTarget = false;
        }

        if (pointsTextLegacy != null && titleTextLegacy != null)
        {
            var titleRt = (RectTransform)titleTextLegacy.transform;
            var rt = (RectTransform)pointsTextLegacy.transform;

            if (rt.localScale.sqrMagnitude < 0.0001f) rt.localScale = Vector3.one;

            rt.anchorMin = titleRt.anchorMin;
            rt.anchorMax = titleRt.anchorMax;
            rt.pivot = titleRt.pivot;
            if (rt.sizeDelta == Vector2.zero) rt.sizeDelta = titleRt.sizeDelta;

            if (Vector2.Distance(rt.anchoredPosition, titleRt.anchoredPosition) < 1f)
                rt.anchoredPosition = titleRt.anchoredPosition + new Vector2(0f, -Mathf.Max(40f, titleRt.sizeDelta.y * 0.9f));

            pointsTextLegacy.font = titleTextLegacy.font != null ? titleTextLegacy.font : Resources.GetBuiltinResource<Font>("Arial.ttf");
            pointsTextLegacy.fontSize = Mathf.Max(18, Mathf.RoundToInt(titleTextLegacy.fontSize * 0.6f));
            pointsTextLegacy.color = titleTextLegacy.color;
            pointsTextLegacy.alignment = titleTextLegacy.alignment;
            pointsTextLegacy.raycastTarget = false;
        }
    }

    /// <summary>
    /// EnsurePointsText: สร้าง/หา pointsText ให้พร้อมใช้งาน (เฉพาะเมื่อจำเป็น)
    /// - ถ้า pointsText มีอยู่แล้ว จะไม่ทำอะไร
    /// - ถ้าใน UI มี label "POINTS :" จะไม่สร้าง Text แยก (เพราะเราใช้ label เดียว)
    /// - ถ้าไม่มี label และไม่มี pointsText: จะพยายามสร้างจาก titleText/titleTextLegacy หรือสร้างแบบ last resort
    ///
    /// แก้เมธอดนี้แล้วได้อะไร:
    /// - คุมว่า fallback จะสร้าง Text แบบไหน/วางตำแหน่งตรงไหน
    /// </summary>
    private void EnsurePointsText()
    {
        // สร้าง pointsText ให้เอง (เฉพาะกรณีจำเป็น) เพื่อกันคะแนนหาย
        if (pointsText != null || pointsTextLegacy != null) return;
        if (!autoCreatePointsTextIfMissing) return;

        // If this UI has a "POINTS :" label, prefer updating that label instead of creating
        // a separate value text (which can overlap due to layout/width measurement).
        FindPointsLabel(out var tmpLabelInline, out var legacyLabelInline);
        if (tmpLabelInline != null || legacyLabelInline != null) return;

        // Avoid creating duplicates if one already exists anywhere under this screen.
        Transform existing = null;
        var allChildren = GetComponentsInChildren<Transform>(true);
        if (allChildren != null)
        {
            foreach (var t in allChildren)
            {
                if (t != null && t.name == "PointsText")
                {
                    existing = t;
                    break;
                }
            }
        }
        if (existing != null)
        {
            pointsText = existing.GetComponent<TMP_Text>();
            if (pointsText != null) return;

            pointsTextLegacy = existing.GetComponent<Text>();
            if (pointsTextLegacy != null) return;
        }

        // Prefer using titleText as a template.
        if (titleText != null)
        {
            var parent = titleText.transform.parent != null ? titleText.transform.parent : transform;
            var go = new GameObject("PointsText", typeof(RectTransform), typeof(TMP_Text));
            go.transform.SetParent(parent, false);

            // Keep it near the title in draw order.
            go.transform.SetSiblingIndex(titleText.transform.GetSiblingIndex() + 1);

            var tmp = go.GetComponent<TMP_Text>();
            var rt = (RectTransform)go.transform;

            var titleRt = titleText.rectTransform;
            rt.anchorMin = titleRt.anchorMin;
            rt.anchorMax = titleRt.anchorMax;
            rt.pivot = titleRt.pivot;
            rt.sizeDelta = titleRt.sizeDelta;
            rt.anchoredPosition = titleRt.anchoredPosition + new Vector2(0f, -Mathf.Max(40f, titleRt.sizeDelta.y * 0.9f));

            tmp.font = titleText.font;
            tmp.fontSize = Mathf.Max(18f, titleText.fontSize * 0.6f);
            tmp.color = titleText.color;
            tmp.alignment = titleText.alignment;
            tmp.raycastTarget = false;

            pointsText = tmp;
            EnsurePointsTextStyle();
            return;
        }

        if (titleTextLegacy != null)
        {
            var parent = titleTextLegacy.transform.parent != null ? titleTextLegacy.transform.parent : transform;
            var go = new GameObject("PointsText", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);

            // Keep it near the title in draw order.
            go.transform.SetSiblingIndex(titleTextLegacy.transform.GetSiblingIndex() + 1);

            var txt = go.GetComponent<Text>();
            var rt = (RectTransform)go.transform;

            var titleRt = (RectTransform)titleTextLegacy.transform;
            rt.anchorMin = titleRt.anchorMin;
            rt.anchorMax = titleRt.anchorMax;
            rt.pivot = titleRt.pivot;
            rt.sizeDelta = titleRt.sizeDelta;
            rt.anchoredPosition = titleRt.anchoredPosition + new Vector2(0f, -Mathf.Max(40f, titleRt.sizeDelta.y * 0.9f));

            txt.font = titleTextLegacy.font != null ? titleTextLegacy.font : Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.fontSize = Mathf.Max(18, Mathf.RoundToInt(titleTextLegacy.fontSize * 0.6f));
            txt.color = titleTextLegacy.color;
            txt.alignment = titleTextLegacy.alignment;
            txt.raycastTarget = false;

            pointsTextLegacy = txt;
            EnsurePointsTextStyle();
            return;
        }

        // Last resort: create a basic legacy Text so something shows.
        {
            var go = new GameObject("PointsText", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(transform, false);

            var txt = go.GetComponent<Text>();
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(400f, 80f);
            rt.anchoredPosition = new Vector2(0f, 140f);

            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.fontSize = 28;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.raycastTarget = false;

            pointsTextLegacy = txt;
        }
    }
}
