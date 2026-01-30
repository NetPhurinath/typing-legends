using UnityEngine;

public static class ScoreKeeper
{
    private const string LastScoreKey = "LastScore";

    public static int LastScore { get; private set; }

    static ScoreKeeper()
    {
        LastScore = PlayerPrefs.GetInt(LastScoreKey, 0);
    }

    public static void Set(int score)
    {
        if (score < 0) score = 0;
        LastScore = score;
        PlayerPrefs.SetInt(LastScoreKey, score);
        PlayerPrefs.Save();
    }

    public static void Reset()
    {
        Set(0);
    }
}
