using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LocalMultiManager : MonoBehaviour
{
    public Transform[] spawnPoints;

    PlayerInputManager manager;
    //参加しているプレイヤー
    List<PlayerInput> players = new List<PlayerInput>();

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
        players.Add(playerInput);
        Debug.Log($"プレイヤー:{players.IndexOf(playerInput) + 1}Pが参加しました。使用デバイス: {playerInput.devices[0].displayName}");
        if (playerInput.TryGetComponent<CharacterController>(out var cc))
        {
            playerInput.SwitchCurrentActionMap("Select");
            cc.enabled = false;
            playerInput.transform.position = spawnPoints[players.IndexOf(playerInput)].position;
            cc.enabled = true;
        }
        else
        {
            playerInput.transform.position = spawnPoints[playerInput.playerIndex].position;
        }
    }
    private void HandlePlayerLeft(PlayerInput playerInput)
    {
        Debug.Log($"プレイヤー:{players.IndexOf(playerInput) + 1}Pが離脱しました。");
        players.RemoveAt(players.IndexOf(playerInput));
        foreach (PlayerInput input in players)
        {
            CharacterController cc = input.GetComponent<CharacterController>();
            cc.enabled = false;
            input.transform.position = spawnPoints[players.IndexOf(input)].position;
            cc.enabled = true;
        }
    }
}