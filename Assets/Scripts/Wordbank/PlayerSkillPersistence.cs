using UnityEngine;

[DefaultExecutionOrder(-10000)]
public sealed class PlayerSkillPersistence : MonoBehaviour
{
    private static bool created;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (created) return;
        created = true;

        PlayerSkillState.LoadFromPrefs();

        var go = new GameObject("PlayerSkillPersistence");
        DontDestroyOnLoad(go);
        go.AddComponent<PlayerSkillPersistence>();
    }

    private void OnApplicationPause(bool pause)
    {
        if (!pause) return;
        PlayerSkillState.SaveToPrefs();
        PlayerSkillState.FlushPrefsToDisk();
    }

    private void OnApplicationQuit()
    {
        PlayerSkillState.SaveToPrefs();
        PlayerSkillState.FlushPrefsToDisk();
    }
}
