using UnityEngine;

public class GameSettingManager : MonoBehaviour
{
    int gameRound;

    void Awake()
    {
        gameRound = 0;
    }

    public void SetRound(int value)
    {
        gameRound += value;
    }
}