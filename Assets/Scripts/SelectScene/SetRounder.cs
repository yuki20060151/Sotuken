using UnityEngine;

public class SetRounder : MonoBehaviour, IDamageable
{
    [SerializeField] bool roundUp = true;

    GameSettingManager gameSettingManager;

    void Awake()
    {
        gameSettingManager = FindAnyObjectByType<GameSettingManager>();
    }

    public void Action()
    {
        if (roundUp)
        {
            gameSettingManager.SetRound(1);
        }
        else
        {
            gameSettingManager.SetRound(-1);
        }
    }
}