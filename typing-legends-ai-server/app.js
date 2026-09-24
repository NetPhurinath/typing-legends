import express from "express";

const systemInstruction = "คุณสร้างคำสำหรับเกมพิมพ์ภาษาไทยธีมรามเกียรติ์ สร้างคำไทยที่สะกดถูกต้อง 10 คำไม่ซ้ำกัน คำละ 2-32 ตัวอักษร ใช้เฉพาะอักษรไทย สระ และวรรณยุกต์ ห้ามเว้นวรรค ตัวเลข สัญลักษณ์ หรือแท็ก เลือกความยากตาม difficultyTier (0 ง่ายที่สุด ถึง 9 ยากที่สุด) ถ้าไม่มีให้ใช้ currentLevel (1-10) accuracy เป็นเปอร์เซ็นต์ averageTime เป็นวินาทีต่อคำ mistakes คืออักษรที่ผู้เล่นมักพิมพ์ผิด เน้นฝึกอักษรเหล่านี้โดยไม่เกินความยากที่กำหนด หาก sampleCount เป็น 0 ยังไม่มีสถิติจริง ห้ามตีความว่า averageTime 0 แปลว่าพิมพ์เร็ว ให้เริ่มด้วยคำตามระดับ ข้อมูล sourceLevel และ attempts คือพฤติกรรมตลอดด่านที่เพิ่งจบ currentLevel คือด่านถัดไปที่ต้องสร้างคำ วิเคราะห์ทุกคำทั้ง completed timeout skipped ความถี่อักษรผิด mistakeCounts เวลาเริ่มพิมพ์ firstInputDelay และช่วงห่างแป้น averageKeyInterval เพื่อเลือกคำฝึกสำหรับด่านถัดไป ค่า firstInputDelay -1 หมายถึงไม่ได้เริ่มพิมพ์ ส่งคำทั้ง 10 คำในฟิลด์ words ตาม JSON schema ที่กำหนด ไม่มีคำอธิบาย";

const wordBatchSchema = {
  type: "object",
  properties: {
    words: {
      type: "array",
      minItems: 10,
      maxItems: 10,
      items: { type: "string", description: "คำภาษาไทยหนึ่งคำ ความยาว 2-32 ตัวอักษร" }
    }
  },
  required: ["words"],
  additionalProperties: false
};

// Gemini sometimes answers 500/503 while overloaded; a short retry usually succeeds.
async function withRetry(delaysMs, call) {
  for (let attempt = 0; ; attempt++) {
    try { return await call(); }
    catch (error) {
      const status = Number(error.status || error.statusCode || error.code) || 0;
      if (attempt >= delaysMs.length || (status !== 500 && status !== 503)) throw error;
      await new Promise(resolve => setTimeout(resolve, delaysMs[attempt]));
    }
  }
}

export function createApp({ client, model = "gemini-3.6-flash", retryDelaysMs = [1000, 3000] } = {}) {
  const app = express();
  app.use(express.json({ limit: "2mb" }));
  app.get("/", (_req, res) => res.send("Typing Legends AI Server is running"));
  app.post("/api/create-practice", async (req, res) => {
    if (!client) return res.status(503).json({ success: false, words: "", error: "ยังไม่ได้ใส่ GEMINI_API_KEY ในไฟล์ .env กรุณาใส่ key แล้วเปิดเซิร์ฟเวอร์ใหม่" });
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
      const response = await withRetry(retryDelaysMs, () => client.models.generateContent({
        model,
        contents: JSON.stringify({ accuracy: stats.accuracy, averageTime: stats.averageTime, mistakes: stats.mistakes, currentLevel: stats.currentLevel,
          ...(stats.difficultyTier !== undefined ? { difficultyTier: stats.difficultyTier } : {}),
          ...(stats.sampleCount !== undefined ? { sampleCount: stats.sampleCount } : {}), ...stage }),
        config: {
          systemInstruction,
          temperature: 0.4,
          maxOutputTokens: 256,
          thinkingConfig: { thinkingBudget: 0 },
          responseMimeType: "application/json",
          responseJsonSchema: wordBatchSchema
        }
      }));
      const text = response.text?.trim();
      if (!text) return res.status(502).json({ success: false, words: "", error: "โมเดลไม่ได้ส่งข้อความกลับมา กรุณาลองใหม่" });
      let generated;
      try { generated = JSON.parse(text); }
      catch { return res.status(502).json({ success: false, words: "", error: "โมเดลส่งชุดคำไม่ตรงรูปแบบ เกมจะใช้คำสำรอง" }); }
      const batch = Array.isArray(generated?.words)
        ? generated.words.map(word => typeof word === "string" ? word.trim().normalize("NFC") : word)
        : [];
      if (batch.length !== 10 || new Set(batch).size !== 10 ||
          !batch.every(word => word.length >= 2 && word.length <= 32 && /^[\u0E01-\u0E2E\u0E40-\u0E44][\u0E01-\u0E3A\u0E40-\u0E4E]+$/u.test(word))) {
        return res.status(502).json({ success: false, words: "", error: "โมเดลส่งชุดคำไม่ตรงรูปแบบ เกมจะใช้คำสำรอง" });
      }
      res.json({ success: true, words: batch.join(",") });
    } catch (error) {
      // Do not log complete SDK errors because request details may be attached.
      const status = Number(error.status || error.statusCode || error.code) || 0;
      const code = String(error.error?.status || error.code || "unknown");
      console.error("Gemini request failed:", status || "network", code);
      const message = status === 400 && /API_KEY_INVALID|key/i.test(String(error.message)) ? "Gemini API key ไม่ถูกต้อง ตรวจสอบ GEMINI_API_KEY แล้วเปิดเซิร์ฟเวอร์ใหม่" :
        status === 401 || status === 403 ? "Gemini API key ไม่มีสิทธิ์ใช้งาน ตรวจสอบ key และเปิด Gemini API ในโปรเจกต์ Google" :
        status === 429 ? "Gemini ใช้โควตาครบหรือเรียกถี่เกินไป (429) ตรวจสอบ Usage/Rate limits ใน Google AI Studio แล้วลองใหม่" :
        status === 404 ? "ไม่พบโมเดล Gemini ที่ตั้งไว้ ตรวจสอบ GEMINI_MODEL" :
        status === 503 ? "Gemini มีผู้ใช้งานมากเกินไปชั่วคราว (503) กรุณาลองใหม่อีกครั้ง" :
        "เรียก Gemini ไม่สำเร็จ ตรวจสอบการเชื่อมต่อแล้วลองใหม่";
      res.status(502).json({ success: false, words: "", error: message });
    }
  });
  app.use((error, _req, res, _next) => {
    res.status(error.status === 413 ? 413 : 400).json({ success: false, words: "", error: "JSON ไม่ถูกต้องหรือมีขนาดใหญ่เกินไป" });
  });
  return app;
}

