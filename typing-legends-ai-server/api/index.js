import { GoogleGenAI } from "@google/genai";
import { createApp } from "../app.js";

// Vercel entry point: env vars come from the Vercel project settings, not .env.
const apiKey = process.env.GEMINI_API_KEY?.trim();
const client = apiKey ? new GoogleGenAI({ apiKey }) : null;
export default createApp({ client, model: process.env.GEMINI_MODEL || "gemini-3.6-flash" });
