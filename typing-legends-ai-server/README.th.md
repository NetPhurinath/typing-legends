# Unity → Node.js → Gemini 3.6 Flash

เซิร์ฟเวอร์นี้ใช้ Google Gemini API รุ่น `gemini-3.6-flash` สร้างคำสำหรับเกม Typing Legends
Unity ยังเรียก `POST http://127.0.0.1:3000/api/create-practice` เหมือนเดิม จึงไม่ต้องแก้ URL ในฉาก

## การทำงานในเกม

1. ระหว่างเล่น ระบบเก็บพฤติกรรมทุกคำในด่าน โดยยังใช้ Wordbank ที่เตรียมไว้สำหรับด่านนั้น
2. เมื่อชนะ ระบบส่งพฤติกรรมทั้งด่านไป Gemini เพียงหนึ่งครั้ง
3. Gemini สร้างคำไทย 10 คำสำหรับด่านถัดไป
4. หากผู้เล่นกด Next ก่อน Gemini ตอบ ด่านถัดไปจะรอคำก่อนเริ่มจับเวลา
5. คำชุดเดียวกันจะถูกวนใช้ตลอดด่านถัดไป ไม่มีการเรียก API เพิ่มกลางด่าน
6. ถ้า Gemini ใช้งานไม่ได้ ด่านถัดไปใช้ Wordbank เดิมและเกมยังเล่นต่อได้

ข้อมูลที่ส่งประกอบด้วยผล completed/timeout/skipped ของทุกคำ เวลา การกดแป้น ความแม่นยำ
อักษรที่ผิดและจำนวนครั้ง first-input delay ช่วงห่างระหว่างแป้น ด่านต้นทาง ด่านเป้าหมาย และระดับความยาก

## ส่วนที่ต้องทำเองครั้งเดียว

1. เข้า [Google AI Studio API Keys](https://aistudio.google.com/api-keys) และล็อกอิน
2. สร้าง Gemini API key สำหรับโปรเจกต์ของคุณ
3. เปิดไฟล์ `.env` ในโฟลเดอร์นี้ แล้วใส่คีย์หลังเครื่องหมายเท่ากับ:

```env
GEMINI_API_KEY=วางคีย์ตรงนี้
GEMINI_MODEL=gemini-3.6-flash
PORT=3000
```

บรรทัด `OPENAI_...` เก่าที่อาจยังอยู่ใน `.env` จะไม่ถูกอ่านและลบภายหลังได้ อย่าส่ง API key ในแชตหรือใส่ Git

4. ถ้าเซิร์ฟเวอร์เก่าเปิดอยู่ ให้กด `Ctrl+C` แล้วดับเบิลคลิก `start-server.cmd` ใหม่
5. ต้องเห็นข้อความ:

```text
Server running at http://localhost:3000
AI provider: Google Gemini (gemini-3.6-flash)
```

## ตรวจการทำงาน

เปิด http://localhost:3000/ ต้องเห็น `Typing Legends AI Server is running`
หน้านี้ไม่เรียก Gemini และใช้ตรวจว่า Node.js เปิดอยู่เท่านั้น

ทดสอบ API โดยใช้ Thunder Client หรือไฟล์ `test-request.http` หรือ PowerShell:

```powershell
powershell -ExecutionPolicy Bypass -File .\test-api.ps1
```

ทดสอบภายในโดยไม่เสียโควตา Gemini:

```powershell
npm test
```

## ข้อผิดพลาดที่พบบ่อย

- `GEMINI_API_KEY`: ยังไม่ได้ใส่คีย์ ให้แก้ `.env` และเปิดเซิร์ฟเวอร์ใหม่
- `401` หรือ `403`: คีย์ไม่ถูกต้อง ไม่มีสิทธิ์ หรือยังไม่ได้เปิด Gemini API ใน Google project
- `429`: โควตาหมดหรือเรียกถี่เกินไป ตรวจ Dashboard > Usage/Rate limits ใน Google AI Studio
- ไม่พบโมเดล: ตรวจว่า `GEMINI_MODEL=gemini-3.6-flash`
- Unity เชื่อมต่อไม่ได้: ตรวจว่า `start-server.cmd` ยังเปิดอยู่และพอร์ต 3000 ว่าง

เซิร์ฟเวอร์รับเฉพาะ `127.0.0.1` สำหรับเกมและ Unity Editor บนเครื่องเดียวกัน

อ้างอิง: [Gemini API keys](https://ai.google.dev/gemini-api/docs/api-key), [Gemini 3.6 Flash](https://ai.google.dev/gemini-api/docs/models/gemini-3.6-flash)
