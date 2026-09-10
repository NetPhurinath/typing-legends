using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class AIPracticeClient : MonoBehaviour
{
    [SerializeField] private string endpoint = "http://127.0.0.1:3000/api/create-practice";
    private bool isRequesting;

    [Serializable]
    private class PlayerStats
    {
        public int accuracy = 72;
        public float averageTime = 4.8f;
        public string[] mistakes = { "ฤ", "วรรณยุกต์" };
        public int currentLevel = 4;
    }

    [Serializable]
    private class AIResponse
    {
        public bool success;
        public string words;
        public string error;
    }

    public void RequestPracticeWords()
    {
        if (isRequesting || !isActiveAndEnabled) return;
        StartCoroutine(SendRequest());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isRequesting = false;
    }

    private IEnumerator SendRequest()
    {
        isRequesting = true;
        try
        {
            byte[] body = Encoding.UTF8.GetBytes(JsonUtility.ToJson(new PlayerStats()));
            using (var request = new UnityWebRequest(endpoint, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(body);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.timeout = 75;
                request.SetRequestHeader("Content-Type", "application/json");
                Debug.Log("กำลังขอชุดคำจาก AI", this);
                yield return request.SendWebRequest();

                AIResponse result = null;
                if (!string.IsNullOrWhiteSpace(request.downloadHandler.text))
                {
                    try { result = JsonUtility.FromJson<AIResponse>(request.downloadHandler.text); }
                    catch (ArgumentException) { /* Report invalid JSON below. */ }
                }
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("เชื่อมต่อไม่สำเร็จ: " +
                        (!string.IsNullOrEmpty(result?.error) ? result.error : request.error + " — ตรวจสอบว่าเปิดเซิร์ฟเวอร์อยู่"), this);
                    yield break;
                }
                if (result != null && result.success && !string.IsNullOrWhiteSpace(result.words))
                    Debug.Log("คำที่ AI สร้าง: " + result.words, this);
                else
                    Debug.LogError("AI ทำงานไม่สำเร็จ: " + (result?.error ?? "รูปแบบคำตอบไม่ถูกต้อง"), this);
            }
        }
        finally { isRequesting = false; }
    }
}
