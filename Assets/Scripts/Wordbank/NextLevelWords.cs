using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

// The request outlives the winning scene even if Next is pressed immediately.
public class NextLevelWords : MonoBehaviour
{
    private static NextLevelWords instance;
    private readonly Dictionary<int, string[]> batches = new Dictionary<int, string[]>();
    private readonly HashSet<int> pending = new HashSet<int>();
    [Serializable] private class Reply { public bool success; public string words; public string error; }

    public static bool IsPending(int level) => instance != null && instance.pending.Contains(level);
    public static string[] GetBatch(int level) => instance != null && instance.batches.TryGetValue(level, out var words)
        ? (string[])words.Clone() : Array.Empty<string>();

    public static void Prepare(string endpoint, ApiWordbank.Stats stats)
    {
        if (instance == null)
        {
            instance = new GameObject("Next Level AI Words").AddComponent<NextLevelWords>();
            DontDestroyOnLoad(instance.gameObject);
        }
        if (instance.pending.Contains(stats.currentLevel)) return;
        instance.batches.Remove(stats.currentLevel);
        instance.pending.Add(stats.currentLevel);
        instance.StartCoroutine(instance.Fetch(endpoint, stats));
    }

    private IEnumerator Fetch(string endpoint, ApiWordbank.Stats stats)
    {
        int target = stats.currentLevel;
        Debug.Log("จบด่าน " + stats.sourceLevel + ": ส่งพฤติกรรม " + stats.sampleCount + " คำ เพื่อสร้างคำด่าน " + target);
        try
        {
            using (var request = new UnityWebRequest(endpoint, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(stats)));
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = 75;
                yield return request.SendWebRequest();
                Reply reply = null;
                try { reply = JsonUtility.FromJson<Reply>(request.downloadHandler.text); }
                catch (ArgumentException) { }
                var words = ApiWordbank.ParseWords(reply?.words);
                if (request.result == UnityWebRequest.Result.Success && reply != null && reply.success && words.Length == 10)
                {
                    batches[target] = words;
                    Debug.Log("เตรียมคำด่าน " + target + " สำเร็จ: " + string.Join(",", words));
                }
                else Debug.LogWarning("ด่าน " + target + " จะใช้ Wordbank เดิม: " + (reply?.error ?? request.error ?? "ชุดคำไม่ครบ"));
            }
        }
        finally { pending.Remove(target); }
    }

    private void OnDestroy() { if (instance == this) instance = null; }
}
