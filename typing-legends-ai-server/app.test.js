import test from "node:test";
import assert from "node:assert/strict";
import { createApp } from "./app.js";

const stats = { accuracy: 72, averageTime: 4.8, mistakes: ["ฤ", "วรรณยุกต์"], currentLevel: 4 };
async function withServer(options, run) {
  const server = createApp(options).listen(0, "127.0.0.1");
  await new Promise(resolve => server.once("listening", resolve));
  try { await run(`http://127.0.0.1:${server.address().port}`); }
  finally { await new Promise(resolve => server.close(resolve)); }
}
const post = (base, body) => fetch(`${base}/api/create-practice`, { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(body) });

test("429 distinguishes exhausted credit, quota, throttling and unknown causes", async () => {
  for (const [code, type, expected] of [
    ["credit_balance_exhausted", "insufficient_quota", /เครดิต API ไม่เพียงพอ/],
    ["insufficient_quota", undefined, /เครดิต API ไม่เพียงพอ/],
    ["rate_limit_exceeded", undefined, /เรียก API เกินอัตรา/],
    [undefined, undefined, /ไม่ได้ระบุสาเหตุ/]
  ]) {
    const client = { responses: { create: async () => { throw Object.assign(new Error("private details"), { status: 429, code, type }); } } };
    await withServer({ client }, async base => {
      const response = await post(base, stats);
      assert.equal(response.status, 502);
      const body = await response.json();
      assert.equal(body.success, false);
      assert.match(body.error, expected);
      assert.ok(!body.error.includes("private details"));
    });
  }
});

test("health works without credentials; generation explains missing key", async () => {
  await withServer({}, async base => {
    assert.equal(await (await fetch(base)).text(), "Typing Legends AI Server is running");
    const response = await post(base, stats);
    assert.equal(response.status, 503);
    assert.match((await response.json()).error, /OPENAI_API_KEY/);
  });
});
test("Thai stats and model pass through SDK; UTF-8 words reach Unity contract", async () => {
  const words = "ฤดู,ทฤษฎี,สัมฤทธิ์,ประดิษฐ์,ปรากฏ,กฤษณา,พฤษภาคม,สร้างสรรค์,มหัศจรรย์,วิจารณ์";
  const client = { responses: { create: async input => {
    assert.equal(input.model, "gpt-5.6-luna");
    assert.deepEqual(JSON.parse(input.input), stats);
    return { output_text: words };
  } } };
  await withServer({ client }, async base => {
    assert.deepEqual(await (await post(base, stats)).json(), { success: true, words });
    assert.equal((await post(base, { ...stats, accuracy: 101 })).status, 400);
    const invalid = await fetch(`${base}/api/create-practice`, { method: "POST", headers: { "Content-Type": "application/json" }, body: "{" });
    assert.equal(invalid.status, 400);
  });
});
test("upstream errors are readable and do not expose credentials", async () => {
  const client = { responses: { create: async () => { throw Object.assign(new Error("secret-value"), { status: 401 }); } } };
  await withServer({ client }, async base => {
    const response = await post(base, stats);
    assert.equal(response.status, 502);
    const body = await response.text();
    assert.match(body, /API key/);
    assert.ok(!body.includes("secret-value"));
  });
});

test("game difficulty and real sample count reach the model", async () => {
  const gameStats = { ...stats, difficultyTier: 2, sampleCount: 8 };
  const client = { responses: { create: async input => {
    assert.deepEqual(JSON.parse(input.input), gameStats);
    return { output_text: "ฤดู,ฤทธิ์,พฤกษา,ทฤษฎี,พฤศจิกายน,น้ำ,ข้าว,เก้าอี้,มื้อ,เสื้อ" };
  } } };
  await withServer({ client }, async base => {
    assert.equal((await post(base, gameStats)).status, 200);
    assert.equal((await post(base, { ...gameStats, difficultyTier: 10 })).status, 400);
  });
});

test("invalid or duplicate model words never enter the game", async () => {
  for (const output_text of ["<b>คำ</b>,น้ำ", "น้ำ,น้ำ,น้ำ,น้ำ,น้ำ,น้ำ,น้ำ,น้ำ,น้ำ,น้ำ", "Here are ten words", ""]) {
    const client = { responses: { create: async () => ({ output_text }) } };
    await withServer({ client }, async base => {
      const response = await post(base, stats);
      assert.equal(response.status, 502);
      assert.equal((await response.json()).success, false);
    });
  }
});

test("whole-stage history beyond 20 attempts is forwarded, with next-stage target", async () => {
  const attempt = { word: "ฤดู", outcome: "completed", seconds: 4, correctCharacters: 3, mistakes: 1, keyPresses: 4, firstInputDelay: 0.5, averageKeyInterval: 0.7 };
  const body = { ...stats, currentLevel: 2, sourceLevel: 1, difficultyTier: 1, sampleCount: 35,
    completedWords: 35, timedOutWords: 0, skippedWords: 0, totalKeyPresses: 140,
    attempts: Array.from({ length: 35 }, () => ({ ...attempt })), mistakeCounts: [{ character: "ฤ", count: 35 }] };
  const client = { responses: { create: async input => {
    assert.deepEqual(JSON.parse(input.input), body);
    return { output_text: "ฤดู,ฤทธิ์,พฤกษา,ทฤษฎี,พฤศจิกายน,น้ำ,ข้าว,เก้าอี้,มื้อ,เสื้อ" };
  } } };
  await withServer({ client }, async base => {
    assert.equal((await post(base, body)).status, 200);
    assert.equal((await post(base, { ...body, sampleCount: 34 })).status, 400);
    assert.equal((await post(base, { ...body, currentLevel: 1 })).status, 400);
  });
});
