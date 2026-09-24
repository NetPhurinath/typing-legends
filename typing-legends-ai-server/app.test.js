import test from "node:test";
import assert from "node:assert/strict";
import { createApp } from "./app.js";

const stats = { accuracy: 72, averageTime: 4.8, mistakes: ["ฤ", "วรรณยุกต์"], currentLevel: 4 };
const validWords = "ฤดู,ทฤษฎี,สัมฤทธิ์,ประดิษฐ์,ปรากฏ,กฤษณา,พฤษภาคม,สร้างสรรค์,มหัศจรรย์,วิจารณ์";
const validResponse = JSON.stringify({ words: validWords.split(",") });

async function withServer(options, run) {
  const server = createApp({ retryDelaysMs: [], ...options }).listen(0, "127.0.0.1");
  await new Promise(resolve => server.once("listening", resolve));
  try { await run(`http://127.0.0.1:${server.address().port}`); }
  finally { await new Promise(resolve => server.close(resolve)); }
}

const post = (base, body) => fetch(`${base}/api/create-practice`, {
  method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(body)
});

function geminiClient({ text = validResponse, error, inspect } = {}) {
  return { models: { generateContent: async input => {
    if (inspect) inspect(input);
    if (error) throw error;
    return { text };
  } } };
}

test("health works without credentials; generation explains missing Gemini key", async () => {
  await withServer({}, async base => {
    assert.equal(await (await fetch(base)).text(), "Typing Legends AI Server is running");
    const response = await post(base, stats);
    assert.equal(response.status, 503);
    assert.match((await response.json()).error, /GEMINI_API_KEY/);
  });
});

test("Thai stats and Gemini 3.6 Flash reach the SDK; UTF-8 words reach Unity", async () => {
  const client = geminiClient({ inspect: input => {
    assert.equal(input.model, "gemini-3.6-flash");
    assert.deepEqual(JSON.parse(input.contents), stats);
    assert.equal(input.config.thinkingConfig.thinkingBudget, 0);
    assert.equal(input.config.responseMimeType, "application/json");
    assert.equal(input.config.responseJsonSchema.properties.words.minItems, 10);
    assert.equal(input.config.responseJsonSchema.properties.words.maxItems, 10);
  } });
  await withServer({ client }, async base => {
    assert.deepEqual(await (await post(base, stats)).json(), { success: true, words: validWords });
    assert.equal((await post(base, { ...stats, accuracy: 101 })).status, 400);
    const invalid = await fetch(`${base}/api/create-practice`, { method: "POST", headers: { "Content-Type": "application/json" }, body: "{" });
    assert.equal(invalid.status, 400);
  });
});

test("Gemini errors are readable and never expose SDK details", async () => {
  for (const [status, expected] of [[401, /ไม่มีสิทธิ์/], [403, /ไม่มีสิทธิ์/], [429, /โควตา/], [404, /GEMINI_MODEL/], [503, /ลองใหม่/], [500, /เรียก Gemini ไม่สำเร็จ/]]) {
    const client = geminiClient({ error: Object.assign(new Error("secret-value"), { status }) });
    await withServer({ client }, async base => {
      const response = await post(base, stats);
      assert.equal(response.status, 502);
      const body = await response.text();
      assert.match(body, expected);
      assert.ok(!body.includes("secret-value"));
    });
  }
});

test("game difficulty and real sample count reach Gemini", async () => {
  const gameStats = { ...stats, difficultyTier: 2, sampleCount: 8 };
  const client = geminiClient({ inspect: input => assert.deepEqual(JSON.parse(input.contents), gameStats) });
  await withServer({ client }, async base => {
    assert.equal((await post(base, gameStats)).status, 200);
    assert.equal((await post(base, { ...gameStats, difficultyTier: 10 })).status, 400);
  });
});

test("invalid or duplicate model words never enter the game", async () => {
  for (const text of [
    JSON.stringify({ words: ["<b>คำ</b>", "น้ำ"] }),
    JSON.stringify({ words: Array(10).fill("น้ำ") }),
    "Here are ten words",
    ""
  ]) {
    await withServer({ client: geminiClient({ text }) }, async base => {
      const response = await post(base, stats);
      assert.equal(response.status, 502);
      assert.equal((await response.json()).success, false);
    });
  }
});

test("whole-stage history beyond 20 attempts is forwarded for the next stage", async () => {
  const attempt = { word: "ฤดู", outcome: "completed", seconds: 4, correctCharacters: 3, mistakes: 1, keyPresses: 4, firstInputDelay: 0.5, averageKeyInterval: 0.7 };
  const body = { ...stats, currentLevel: 2, sourceLevel: 1, difficultyTier: 1, sampleCount: 35,
    completedWords: 35, timedOutWords: 0, skippedWords: 0, totalKeyPresses: 140,
    attempts: Array.from({ length: 35 }, () => ({ ...attempt })), mistakeCounts: [{ character: "ฤ", count: 35 }] };
  const client = geminiClient({ inspect: input => assert.deepEqual(JSON.parse(input.contents), body) });
  await withServer({ client }, async base => {
    assert.equal((await post(base, body)).status, 200);
    assert.equal((await post(base, { ...body, sampleCount: 34 })).status, 400);
    assert.equal((await post(base, { ...body, currentLevel: 1 })).status, 400);
  });
});

test("temporary Gemini overload is retried before failing", async () => {
  let calls = 0;
  const client = { models: { generateContent: async () => {
    if (++calls < 3) throw Object.assign(new Error("overloaded"), { status: 503 });
    return { text: validResponse };
  } } };
  await withServer({ client, retryDelaysMs: [0, 0] }, async base => {
    assert.deepEqual(await (await post(base, stats)).json(), { success: true, words: validWords });
    assert.equal(calls, 3);
  });
});
