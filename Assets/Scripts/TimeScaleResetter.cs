using UnityEngine;
using UnityEngine.SceneManagement;

public static class TimeScaleResetter
{
    private static bool isHooked;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        ResetTimeScale();

        if (isHooked) return;
        SceneManager.sceneLoaded += OnSceneLoaded;
        isHooked = true;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetTimeScale();
    }

    private static void ResetTimeScale()
    {
        // Prevent stuck paused state after UI popups / scene loads.
        Time.timeScale = 1f;
    }
}
