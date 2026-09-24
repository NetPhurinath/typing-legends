import { config } from "dotenv";
import { fileURLToPath } from "node:url";
import { GoogleGenAI } from "@google/genai";
import { createApp } from "./app.js";

config({ path: fileURLToPath(new URL(".env", import.meta.url)), quiet: true });
const apiKey = process.env.GEMINI_API_KEY?.trim();
const client = apiKey ? new GoogleGenAI({ apiKey }) : null;
const port = Number(process.env.PORT || 3000);
const app = createApp({ client, model: process.env.GEMINI_MODEL || "gemini-3.6-flash" });
app.listen(port, "127.0.0.1", () => {
  console.log(`Server running at http://localhost:${port}`);
  console.log(`AI provider: Google Gemini (${process.env.GEMINI_MODEL || "gemini-3.6-flash"})`);
  if (!client) console.log("GEMINI_API_KEY is empty. Add it to .env and restart to enable AI requests.");
}).on("error", error => {
  console.error("Server could not start:", error.code);
  process.exitCode = 1;
});
