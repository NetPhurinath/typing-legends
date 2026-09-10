import express from "express";

export function createApp({ client, model = "gpt-5.6-luna" } = {}) {
  const app = express();
  app.use(express.json({ limit: "2mb" }));
  app.get("/", (_req, res) => res.send("Typing Legends AI Server is running"));
  app.post("/api/create-practice", async (req, res) => {
    if (!client) return res.status(503).json({ success: false, words: "", error: "ยังไม่ได้ใส่ OPENAI_API_KEY ในไฟล์ .env กรุณาใส่ key แล้วเปิดเซิร์ฟเวอร์ใหม่" });
    const stats = req.body;
    if (!stats || !Number.isFinite(stats.accuracy) || stats.accuracy < 0 || stats.accuracy > 100 ||
        !Number.isFinite(stats.averageTime) || stats.averageTime < 0 ||
        !Number.isInteger(stats.currentLevel) || stats.currentLevel < 1 ||
        !Array.isArray(stats.mistakes) || stats.mistakes.length > 100 ||
        !stats.mistakes.every(value => typeof value === "string" && value.length <= 100) ||
        (stats.difficultyTier !== undefined && (!Number.isInteger(stats.difficultyTier) || stats.difficultyTier < 0 || stats.difficultyTier > 9)) ||
        (stats.sampleCount !== undefined && (!Number.isInteger(stats.sampleCount) || stats.sampleCount < 0 || stats.sampleCount > 10000))) {
      return res.status(400).json({ success: false, words: "", error: "ข้อมูลผู้เล่นไม่ถูกต้อง" });
    }
    let stage = {};
    if (stats.sourceLevel !== undefined) {
      const nonnegative = value => Number.isFinite(value) && value >= 0;
      const integer = value => Number.isSafeInteger(value) && value >= 0;
      if (!Number.isInteger(stats.sourceLevel) || stats.sourceLevel < 1 || stats.currentLevel !== stats.sourceLevel + 1 ||
          !Array.isArray(stats.attempts) || stats.attempts.length !== stats.sampleCount ||
          !stats.attempts.every(a => a && typeof a.word === "string" && a.word.length <= 100 &&
            ["completed", "timeout", "skipped"].includes(a.outcome) && nonnegative(a.seconds) &&
            integer(a.correctCharacters) && integer(a.mistakes) && integer(a.keyPresses) &&
            Number.isFinite(a.firstInputDelay) && a.firstInputDelay >= -1 && nonnegative(a.averageKeyInterval)) ||
          !Array.isArray(stats.mistakeCounts) || stats.mistakeCounts.length > 100 ||
          !stats.mistakeCounts.every(m => m && typeof m.character === "string" && m.character.length <= 2 && integer(m.count)) ||
          ![stats.completedWords, stats.timedOutWords, stats.skippedWords, stats.totalKeyPresses].every(integer)) {
        return res.status(400).json({ success: false, words: "", error: "ข้อมูลพฤติกรรมทั้งด่านไม่ถูกต้อง" });
      }
      stage = { sourceLevel: stats.sourceLevel, completedWords: stats.completedWords, timedOutWords: stats.timedOutWords,
        skippedWords: stats.skippedWords, totalKeyPresses: stats.totalKeyPresses,
        attempts: stats.attempts.map(({ word, outcome, seconds, correctCharacters, mistakes, keyPresses, firstInputDelay, averageKeyInterval }) =>
          ({ word, outcome, seconds, correctCharacters, mistakes, keyPresses, firstInputDelay, averageKeyInterval })),
        mistakeCounts: stats.mistakeCounts.map(({ character, count }) => ({ character, count })) };
    }
    try {
      const response = await client.responses.create({
        model,
        instructions: "คุณสร้างคำสำหรับเกมพิมพ์ภาษาไทยธีมรามเกียรติ์ สร้างคำไทยที่สะกดถูกต้อง 10 คำไม่ซ้ำกัน คำละ 2-32 ตัวอักษร ใช้เฉพาะอักษรไทย สระ และวรรณยุกต์ ห้ามเว้นวรรค ตัวเลข สัญลักษณ์ หรือแท็ก เลือกความยากตาม difficultyTier (0 ง่ายที่สุด ถึง 9 ยากที่สุด) ถ้าไม่มีให้ใช้ currentLevel (1-10) accuracy เป็นเปอร์เซ็นต์ averageTime เป็นวินาทีต่อคำ mistakes คืออักษรที่ผู้เล่นมักพิมพ์ผิด เน้นฝึกอักษรเหล่านี้โดยไม่เกินความยากที่กำหนด หาก sampleCount เป็น 0 ยังไม่มีสถิติจริง ห้ามตีความว่า averageTime 0 แปลว่าพิมพ์เร็ว ให้เริ่มด้วยคำตามระดับ ข้อมูล sourceLevel และ attempts คือพฤติกรรมตลอดด่านที่เพิ่งจบ currentLevel คือด่านถัดไปที่ต้องสร้างคำ วิเคราะห์ทุกคำทั้ง completed timeout skipped ความถี่อักษรผิด mistakeCounts เวลาเริ่มพิมพ์ firstInputDelay และช่วงห่างแป้น averageKeyInterval เพื่อเลือกคำฝึกสำหรับด่านถัดไป ค่า firstInputDelay -1 หมายถึงไม่ได้เริ่มพิมพ์ ตอบเป็นคำคั่นด้วยจุลภาคเท่านั้น ไม่มีคำอธิบาย",
        input: JSON.stringify({ accuracy: stats.accuracy, averageTime: stats.averageTime, mistakes: stats.mistakes, currentLevel: stats.currentLevel,
          ...(stats.difficultyTier !== undefined ? { difficultyTier: stats.difficultyTier } : {}),
          ...(stats.sampleCount !== undefined ? { sampleCount: stats.sampleCount } : {}), ...stage })
      });
      const words = response.output_text?.trim();
      if (!words) return res.status(502).json({ success: false, words: "", error: "โมเดลไม่ได้ส่งข้อความกลับมา กรุณาลองใหม่" });
      const batch = words.split(/[,，\r\n]+/).map(word => word.trim().normalize("NFC"));
      if (batch.length !== 10 || new Set(batch).size !== 10 ||
          !batch.every(word => word.length >= 2 && word.length <= 32 && /^[\u0E01-\u0E2E\u0E40-\u0E44][\u0E01-\u0E3A\u0E40-\u0E4E]+$/u.test(word))) {
        return res.status(502).json({ success: false, words: "", error: "โมเดลส่งชุดคำไม่ตรงรูปแบบ เกมจะใช้คำสำรอง" });
      }
      res.json({ success: true, words: batch.join(",") });
    } catch (error) {
      // Do not log full SDK errors: they can contain request details.
      console.error("OpenAI request failed:", error.status || "network", error.code || "unknown");
      const code = error.code || error.error?.code;
      const quotaExceeded = code === "insufficient_quota" || code === "credit_balance_exhausted" ||
        error.type === "insufficient_quota" || error.error?.type === "insufficient_quota";
      const message = error.status === 401 ? "API key ไม่ถูกต้อง ตรวจสอบ .env แล้วเปิดเซิร์ฟเวอร์ใหม่" :
        error.status === 429 && quotaExceeded ? "เครดิต API ไม่เพียงพอหรือใช้โควตาครบแล้ว (429 insufficient_quota) ตรวจสอบ Billing และ Limits ขององค์กรที่สร้าง API key การรอแล้วกดซ้ำอย่างเดียวไม่แก้สาเหตุนี้" :
        error.status === 429 && code === "rate_limit_exceeded" ? "เรียก API เกินอัตราที่อนุญาต (429 rate_limit_exceeded) รอสักครู่แล้วลองใหม่ หรือตรวจสอบ Limits ของโมเดล" :
        error.status === 429 ? "OpenAI ตอบ 429 แต่ไม่ได้ระบุสาเหตุที่รู้จัก ตรวจสอบ Billing และ Limits" :
        error.status === 404 ? "บัญชีนี้เข้าถึงโมเดลที่ตั้งไว้ไม่ได้ ตรวจสอบ OPENAI_MODEL" :
        "เรียก OpenAI ไม่สำเร็จ ตรวจสอบการเชื่อมต่อแล้วลองใหม่";
      res.status(502).json({ success: false, words: "", error: message });
    }
  });
  app.use((error, _req, res, _next) => {
    res.status(error.status === 413 ? 413 : 400).json({ success: false, words: "", error: "JSON ไม่ถูกต้องหรือมีขนาดใหญ่เกินไป" });
  });
  return app;
}

