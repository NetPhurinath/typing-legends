using UnityEngine;

public class GameWinScreen : GameOverScreen
{
    public new void Show(int points)
    {
        base.Show(points, true);
    }
}
