# DynamicPacingAI - การเทสและปรับค่า Parameter

## 📋 ขั้นตอนที่ 1: เตรียมการติดตั้ง

### วิธีอัตโนมัติ (แนะนำ)

**Step A: เปิด Unity Editor**
```
Open the project in Unity 6.0.2 (or compatible version)
```

**Step B: รัน Editor Tool**
ไปที่ **Tools** → **Typing Legends** → **Setup DynamicPacingAI**

คุณจะเห็นตัวเลือก:
1. **Setup Current Scene** - ติดตั้งบน scene ที่เปิดอยู่
2. **Setup All Level Scenes** - ติดตั้งบนทุก level (Level 1-10)

**Step C: เลือก "Setup All Level Scenes"**
- Tool จะ auto-add DynamicPacingAI component บนทั้งหมด
- ตั้งค่า default parameters
- Save ทั้งหมด

---

## 📊 ขั้นตอนที่ 2: เทสเล่นเกม

### Test Scenario 1: Pressure State ✅
**วัตถุประสงค์**: ทดสอบว่า state เปลี่ยนเป็น Pressure เมื่อผู้เล่นทำดี

**วิธีทำ**:
1. Load **Level 1**
2. พิมพ์คำให้ดีติดกัน **3 ครั้ง** (ไม่ผิด)
3. **ตรวจสอบ**:
   - [ ] Timer ลดลงจาก 5s → 4.5s (ลด 10%)
   - [ ] ศัตรูโจมตีเร็วขึ้น (1.2x)
   - [ ] Console log: `State changed to Pressure`

---

### Test Scenario 2: Recovery State 🛡️
**วัตถุประสงค์**: ทดสอบว่า state เปลี่ยนเป็น Recovery เมื่อผู้เล่นพลาด

**วิธีทำ**:
1. Load **Level 1**
2. พิมพ์คำให้ผิดติดกัน **2 ครั้ง**
3. **ตรวจสอบ**:
   - [ ] Timer เพิ่มขึ้นจาก 5s → 5.75s (เพิ่ม 15%)
   - [ ] ศัตรูโจมตีช้าลง (0.7x)
   - [ ] คำถัดไปควรง่ายขึ้น
   - [ ] Console log: `State changed to Recovery`

---

### Test Scenario 3: Burst State 🎉
**วัตถุประสงค์**: ทดสอบว่า state เปลี่ยนเป็น Burst เมื่อ streak ยาว

**วิธีทำ**:
1. Load **Level 1**
2. พิมพ์คำให้ดีติดกัน **3 ครั้ง** (Burst trigger)
3. **ตรวจสอบ**:
   - [ ] Timer ลดลงมาก (5s → 4.25s = 15% reduction)
   - [ ] Monster attacks: 1.5x faster (challenge mode!)
   - [ ] Console log: `State changed to Burst`
   - [ ] หลังจาก 1-2 คำ ควรกลับมา Normal

---

### Test Scenario 4: Normal State 🎮
**วัตถุประสงค์**: ทดสอบว่า state ยังคง Normal เมื่อ performance ปกติ

**วิธีทำ**:
1. Load **Level 1**
2. พิมพ์คำ **ปกติ**: บางคำดี บางคำผิด
3. **ตรวจสอบ**:
   - [ ] Timer ไม่เปลี่ยน (ยังเป็น 5s)
   - [ ] Monster ที่อัตราปกติ
   - [ ] Console log: ไม่มี "State changed"

---

## 🎛️ ขั้นตอนที่ 3: ปรับ Parameters

### วิธี 1: ปรับในเกม (ง่ายที่สุด)

1. เปิด Level scene
2. Select GameObject ที่มี Typer
3. ใน Inspector ดูหัวข้อ "Dynamic Pacing AI"
4. แก้ไขค่า:

| Parameter | ค่าปัจจุบัน | ดีขึ้น (Easy) | ท้าทายมากขึ้น (Hard) |
|-----------|-----------|------------|------------------|
| Pressure WPM Threshold | 30 | 40 | 20 |
| Recovery Mistake Threshold | 0.3 | 0.4 | 0.2 |
| Timer Reduce % | 0.1 (10%) | 0.05 (5%) | 0.15 (15%) |
| Timer Increase % | 0.15 (15%) | 0.25 (25%) | 0.1 (10%) |
| Burst Required Streak | 3 | 5 | 2 |

---

### วิธี 2: ปรับทั้งหมดพร้อมกัน (Script)

ถ้าต้องการปรับให้แบบ Easy Mode ทั่วทั้งเกม:

```csharp
// ใน DynamicPacingAI Inspector

// EASY MODE (ผ่อนเกม)
pressureWpmThreshold = 40f;
recoveryMistakeThreshold = 0.4f;
timerReducePercentPressure = 0.05f;
timerIncreasePercentRecovery = 0.25f;
burstRequiredStreak = 5;
```

หรือ Hard Mode:

```csharp
// HARD MODE (ท้าทายมากขึ้น)
pressureWpmThreshold = 20f;
recoveryMistakeThreshold = 0.2f;
timerReducePercentPressure = 0.15f;
timerIncreasePercentRecovery = 0.1f;
burstRequiredStreak = 2;
```

---

## 🐛 การแก้ปัญหา

### ❌ ปัญหา: DynamicPacingAI ไม่ทำงาน

**ตรวจสอบ**:
1. เปิด Console (Ctrl+Shift+C)
2. หา log ที่มี "DynamicPacingAI"
3. ถ้าไม่เห็น:
   - [ ] ตรวจว่า Component ติดบน Typer GameObject หรือไม่
   - [ ] ตรวจว่า TypingStrategyProfiler ถูก assign หรือไม่

---

### ❌ ปัญหา: State ไม่เปลี่ยน

**ตรวจสอบ**:
1. เล่นเกมและทำให้ perfect streak (3 คำติดกัน)
2. ดู Console ว่ามี log อะไรไหม
3. ถ้าเห็น `Good: 3, Mistakes: 0` แต่ไม่เห็น `State changed`:
   - [ ] ตรวจ UpdateState() method ทำงานหรือไม่
   - [ ] ตรวจ currentState ค่า

---

### ❌ ปัญหา: Timer ไม่ลดลง/เพิ่มขึ้น

**ตรวจสอบ**:
1. ระบบจะปรับ `Typer.countdownTime` value
2. ถ้าไม่เห็นการเปลี่ยน:
   - [ ] ตรวจว่า OnWordResult() ถูกเรียกหรือไม่ (Console log)
   - [ ] ตรวจว่า state มีการเปลี่ยนหรือไม่
   - [ ] ตรวจว่า ApplyTimerAdjustment() ถูกเรียกหรือไม่

---

## 📈 ผลลัพธ์ที่คาดหวัง

### หลังการติดตั้งและเทส

**ผู้เล่นเก่ง** (WPM สูง, accuracy 90%+):
- ✅ Pressure state ตามตัวบ่อยขึ้น
- ✅ Timer ลดลง → ท้าทายมากขึ้น
- ✅ ศัตรูโจมตีเร็ว

**ผู้เล่นเบื้องต้น** (WPM ต่ำ, accuracy 60-70%):
- ✅ Recovery state ตามตัว
- ✅ Timer เพิ่มขึ้น → ง่ายขึ้น
- ✅ ศัตรูโจมตีช้า

**ผลรวม**:
- ✅ เกมปรับตัวให้พอดีกับฝีมือของผู้เล่นแต่ละคน
- ✅ ไม่เบื่อเมื่อเล่านานๆ
- ✅ ส่งเสริมการเรียนรู้

---

## 🎯 Quick Start Checklist

- [ ] 1. เปิด Unity Editor
- [ ] 2. ไปที่ Tools > Typing Legends > Setup All Level Scenes
- [ ] 3. รอให้ setup เสร็จ
- [ ] 4. เปิด Level 1
- [ ] 5. Play ในเกม
- [ ] 6. ดู Console ว่ามี DynamicPacingAI logs หรือไม่
- [ ] 7. ถ้าต้องการปรับ ไปที่ Inspector และแก้ parameter
- [ ] 8. Play again
- [ ] ✅ Done!

