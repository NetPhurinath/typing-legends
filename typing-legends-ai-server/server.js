import { config } from "dotenv";
import { fileURLToPath } from "node:url";
import OpenAI from "openai";
import { createApp } from "./app.js";

config({ path: fileURLToPath(new URL(".env", import.meta.url)), quiet: true });
const apiKey = process.env.OPENAI_API_KEY?.trim();
const client = apiKey ? new OpenAI({ apiKey, timeout: 60000, maxRetries: 0 }) : null;
const port = Number(process.env.PORT || 3000);
const app = createApp({ client, model: process.env.OPENAI_MODEL || "gpt-5.6-luna" });
app.listen(port, "127.0.0.1", () => {
  console.log(`Server running at http://localhost:${port}`);
  if (!client) console.log("OPENAI_API_KEY is empty. Add it to .env and restart to enable AI requests.");
}).on("error", error => {
  console.error("Server could not start:", error.code);
  process.exitCode = 1;
});
