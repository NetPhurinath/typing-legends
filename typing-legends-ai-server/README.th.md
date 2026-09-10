# ทดสอบ Unity → Node.js → OpenAI

เชื่อมเกมจริงแล้ว: ด่านที่มี Typer จะใช้คำจาก API อัตโนมัติ และยังมีฉาก AIPracticeTest สำหรับตรวจเซิร์ฟเวอร์

## เล่นในด่านจริง

เปิดเซิร์ฟเวอร์ แล้วเปิด `Assets/Resources/Scenes/Level 1.unity` หรือด่านอื่น กด Play และเล่นตามปกติ
ไม่ต้องกดปุ่มสร้างคำในฉากทดสอบก่อน เกมจะขอครั้งละ 10 คำเมื่อเริ่มเล่น/คิวเหลือน้อย
ระหว่างรอ ใช้ Wordbank เดิมก่อน เมื่อคำ API พร้อมจะใช้ตั้งแต่คำถัดไป ไม่เปลี่ยนคำกลางการพิมพ์
Console จะแสดง `รับชุดคำ API สำหรับด่าน ...` และ `ใช้คำจาก API ในเกม: ...`

ระบบส่งสถิติสูงสุด 20 คำล่าสุด: เปอร์เซ็นต์ตัวอักษรถูก, เวลาเฉลี่ยต่อคำ, อักษรที่ควรพิมพ์แต่พิมพ์ผิด,
ด่าน และระดับความยาก 0-9 จาก TypeMasterAI (ถ้าด่านใช้ Wordbank โดยตรง ใช้หมายเลขด่านลบหนึ่ง)
ตอนยังไม่มีผลการเล่น sampleCount จะเป็น 0 ให้โมเดลเริ่มตามระดับ
TypeMasterAI, PlayerSkillState, StrategyProfiler และ DynamicPacingAI ยังรับผลการเล่นตามเดิม

คำ API อยู่ในคิวหน่วยความจำของ `ApiWordbank` ไม่เขียนทับคลังคำเดิม และไม่เก็บข้ามด่าน
ขอห่างอย่างน้อย 30 วินาที เมื่อผิดพลาดพัก 60 วินาทีแล้วลองใหม่เมื่อเกมต้องการคำ
คำผิดรูปแบบหรือ API ล่มจะใช้คลังคำเดิม เกมไม่หยุดและไม่เสียพลังเพราะรอเครือข่าย
หากต้องการกลับไปใช้คลังเดิมทั้งหมด ปิด `Use Api Words` บน Typer ใน Inspector

API endpoint ตั้งได้บน ApiWordbank; โดยปกติ Typer จะเพิ่มคอมโพเนนต์นี้ให้อัตโนมัติเมื่อเริ่มเล่น
หากต้องการเปลี่ยน endpoint ถาวร ให้เพิ่ม ApiWordbank บน GameObject เดียวกับ Typer แล้วแก้ Endpoint ก่อนกด Play

## ส่วนที่คุณต้องทำเอง

1. ล็อกอิน https://platform.openai.com/ แล้วตั้งค่า Billing/เครดิตสำหรับ API
2. สร้าง API key ที่ https://platform.openai.com/api-keys ชื่อ `typing-legends-development`
3. เปิด `.env` ในโฟลเดอร์นี้ ใส่ key หลัง `OPENAI_API_KEY=` แล้วบันทึก ไม่ต้องส่ง key ในแชต
4. ดับเบิลคลิก `start-server.cmd` แล้วปล่อยหน้าต่างนั้นเปิดไว้ ถ้าเคยเปิดเซิร์ฟเวอร์แล้ว ให้ Ctrl+C และเปิดใหม่หลังแก้ `.env`
5. ใน Unity เปิด `Assets/Resources/Scenes/AIPracticeTest.unity` กด Play แล้วกด **สร้างชุดฝึกด้วย AI**
6. เปิด `Window > General > Console` เพื่อดู `คำที่ AI สร้าง: ...`

ถ้าฉากยังไม่ปรากฏหลัง Unity คอมไพล์ ให้ใช้เมนู `Tools > Typing Legends > Create AI Practice Test Scene` เพื่อสร้างฉาก (ไม่เขียนทับฉากที่มีอยู่)

## ตรวจเซิร์ฟเวอร์

เปิด http://localhost:3000/ ต้องเห็น `Typing Legends AI Server is running`
หน้านี้ตรวจได้โดยไม่ต้องมี API key และไม่ได้เรียกโมเดล

ทดสอบ API ด้วย PowerShell ในโฟลเดอร์นี้:

```powershell
powershell -ExecutionPolicy Bypass -File .\test-api.ps1
```

หรือใช้ Thunder Client: POST `http://localhost:3000/api/create-practice`, Body > JSON แล้วคัดลอก JSON จาก `test-request.http`

## ไฟล์ที่เตรียมไว้

- `server.js`, `app.js`: เซิร์ฟเวอร์และ endpoint
- `.env`: key ว่างไว้ให้กรอก; `.gitignore` ป้องกัน key และ node_modules เข้า Git
- `.env.example`: ตัวอย่างสำหรับย้ายเครื่อง
- `start-server.cmd`: เปิดเซิร์ฟเวอร์
- `test-request.http`, `test-api.ps1`: คำขอทดสอบ
- `app.test.js`: ทดสอบในเครื่องด้วย `npm test` ใช้คำตอบจำลอง ไม่เสียค่า API
- `../Assets/Scripts/AIPracticeClient.cs`: ส่งข้อมูลตัวอย่างตามขั้นตอน และพิมพ์ผลใน Console
- `../Assets/Editor/AIPracticeTestSetup.cs`: สร้างฉากพร้อม AI Manager, ปุ่ม TMP ภาษาไทย และ On Click

## ข้อผิดพลาดที่อาจเห็นก่อนตั้งค่าบัญชี

- `503 / OPENAI_API_KEY`: ยังไม่ได้ใส่ key ให้กรอก `.env` แล้วเปิดใหม่
- ข้อความ `API key ไม่ถูกต้อง`: ตรวจ key
- `429`: ตรวจเครดิต/โควตา หรือรอแล้วลองใหม่
- ไม่พบโมเดล: ตรวจสิทธิ์บัญชีและ `OPENAI_MODEL` แล้วเปิดเซิร์ฟเวอร์ใหม่
- เชื่อมต่อไม่ได้: ตรวจว่า `start-server.cmd` ยังเปิดอยู่ และพอร์ต 3000 ว่าง

เซิร์ฟเวอร์รับการเชื่อมต่อเฉพาะเครื่องนี้ (127.0.0.1) สำหรับ Unity Editor/เกมบนเครื่องเดียวกัน ยังไม่ได้เตรียมสำหรับ WebGL หรือเครื่องผู้เล่นอื่น

อ้างอิง: [OpenAI quickstart](https://developers.openai.com/api/docs/quickstart), [gpt-5.6-luna](https://developers.openai.com/api/docs/models/gpt-5.6-luna)
