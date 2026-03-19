using System;
using UnityEngine;

public class TypeMasterAIDifficultySettings : MonoBehaviour
{
    [Serializable]
    public class TierRule
    {
        [Header("เลื่อนขึ้น (ยากขึ้น)")]
        [Tooltip("พิมพ์เร็ว (WPM) ถึงเกณฑ์นี้ + ความแม่นยำถึงเกณฑ์ → มีสิทธิ์เลื่อนไประดับที่ยากขึ้น")]
        [Min(0f)] public float promoteWpm = 25f;

        [Tooltip("ความแม่นยำขั้นต่ำ (0-1) สำหรับเลื่อนขึ้น: 1 = ไม่พลาดเลย")]
        [Range(0f, 1f)] public float promoteAccuracy = 0.90f;

        [Header("เลื่อนลง (ง่ายลง)")]
        [Tooltip("ถ้า WPM ต่ำกว่าหรือเท่ากับค่านี้ (หรือความแม่นยำต่ำกว่าเกณฑ์) → มีสิทธิ์ลดระดับ")]
        [Min(0f)] public float demoteWpm = 15f;

        [Tooltip("ความแม่นยำต่ำกว่าค่านี้ → มีสิทธิ์ลดระดับ (ช่วยกันไม่ให้คำยากเกิน)")]
        [Range(0f, 1f)] public float demoteAccuracy = 0.80f;
    }

    [Header("เริ่มต้น")]
    [Tooltip("เริ่มเกมที่ระดับนี้ (0 ง่ายสุด, 9 ยากสุด): เปลี่ยนตรงนี้ = เริ่มคำยาก/ง่ายขึ้นทันที")]
    [Range(0, 9)] public int startTierIndex = 0;

    [Header("ความนิ่งของการปรับระดับ")]
    [Tooltip("ต้องทำได้ 'ต่อเนื่องกี่คำ' ก่อนเลื่อนขึ้น (ยิ่งมาก ยิ่งนิ่ง แต่ปรับช้าลง)")]
    [Min(1)] public int promoteStreak = 3;

    [Tooltip("ต้องพลาด/ช้าต่อเนื่องกี่คำก่อนลดระดับ (ช่วยกันแกว่ง)")]
    [Min(1)] public int demoteStreak = 2;

    [Tooltip("EMA alpha: ยิ่งมาก AI ยิ่งตอบสนองเร็ว แต่ค่า WPM/Accuracy จะแกว่งง่าย")]
    [Range(0.05f, 0.5f)] public float emaAlpha = 0.20f;

    [Header("กฎรายระดับ (10 ระดับ)")]
    [Tooltip("แก้ค่าของระดับ i = เปลี่ยนเงื่อนไขเลื่อนขึ้น/ลง 'ตอนที่ AI อยู่ระดับ i'")]
    public TierRule[] tierRules = new TierRule[10];

    private void Reset()
    {
        EnsureLength();
    }

    private void OnValidate()
    {
        EnsureLength();
    }

    private void EnsureLength()
    {
        if (tierRules == null || tierRules.Length != 10)
        {
            var newRules = new TierRule[10];
            if (tierRules != null)
            {
                for (int i = 0; i < Mathf.Min(10, tierRules.Length); i++)
                    newRules[i] = tierRules[i];
            }

            for (int i = 0; i < 10; i++)
                newRules[i] ??= new TierRule();

            tierRules = newRules;
        }

        for (int i = 0; i < 10; i++)
            tierRules[i] ??= new TierRule();
    }

    public TierRule GetRule(int tierIndex)
    {
        EnsureLength();
        tierIndex = Mathf.Clamp(tierIndex, 0, 9);
        return tierRules[tierIndex];
    }
}
