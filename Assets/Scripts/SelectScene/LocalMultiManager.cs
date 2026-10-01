using UnityEngine;
using UnityEngine.InputSystem;

public class LocalMultiManager : MonoBehaviour
{
    public Transform[] spawnPoints;

    PlayerInputManager manager;

    void Awake()
    {
        manager = GetComponent<PlayerInputManager>();
        DontDestroyOnLoad(this);
    }

    private void OnEnable()
    {
        manager.onPlayerJoined += HandlePlayerJoined;
        manager.onPlayerLeft += HandlePlayerLeft;
    }
    private void OnDisable()
    {
        manager.onPlayerJoined -= HandlePlayerJoined;
        manager.onPlayerLeft -= HandlePlayerLeft;
    }
    private void HandlePlayerJoined(PlayerInput playerInput)
    {
        Debug.Log($"プレイヤー {playerInput.playerIndex} が参加しました。使用デバイス: {playerInput.devices[0].displayName}");
        if (playerInput.TryGetComponent<CharacterController>(out var cc))
        {
            cc.enabled = false;
            playerInput.transform.position = spawnPoints[playerInput.playerIndex].position;
            cc.enabled = true;
        }
        else
        {
            playerInput.transform.position = spawnPoints[playerInput.playerIndex].position;
        }
    }
    private void HandlePlayerLeft(PlayerInput playerInput)
    {
        Debug.Log($"プレイヤー {playerInput.playerIndex} が離脱しました。");
    }
}