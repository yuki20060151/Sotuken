using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LocalMultiManager : MonoBehaviour
{
    [Header("カメラ")]
    [SerializeField] GameObject cam;
    [SerializeField] Vector3 targetPosition;
    [SerializeField] float speed = 20;
    [Header("スポーン位置")]
    [SerializeField] Transform[] spawnPoints;
    [Header("準備完了タイマー")]
    [SerializeField] TMPro.TMP_Text timerText;
    [SerializeField] float completeTime = 3;

    PlayerInputManager manager;
    List<PlayerInput> players = new List<PlayerInput>();//参加しているプレイヤー
    List<bool> playersReady = new List<bool>();
    float defaultCompleteTime;
    bool moveRuleDisplay;

    void Awake()
    {
        manager = GetComponent<PlayerInputManager>();
        defaultCompleteTime = completeTime + 1;
    }

    #region 入力システム
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
        playersReady.Add(false);
        Debug.Log($"プレイヤー:{players.IndexOf(playerInput) + 1}Pが参加しました。使用デバイス: {playerInput.devices[0].displayName}");
        if (playerInput.TryGetComponent<CharacterController>(out var cc))
        {
            playerInput.SwitchCurrentActionMap("Select");
            DontDestroyOnLoad(playerInput.gameObject);
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
        int index = players.IndexOf(playerInput);
        Debug.Log($"プレイヤー:{index + 1}Pが離脱しました。");
        players.RemoveAt(index);
        playersReady.RemoveAt(index);
        foreach (PlayerInput input in players)
        {
            CharacterController cc = input.GetComponent<CharacterController>();
            cc.enabled = false;
            input.transform.position = spawnPoints[players.IndexOf(input)].position;
            cc.enabled = true;
        }
    }
    #endregion

    void Update()
    {
        if (playersReady.Count == 0 || moveRuleDisplay) return;
        foreach (bool ready in playersReady)
        {
            if (!ready)
            {
                if (timerText.gameObject.activeSelf) timerText.gameObject.SetActive(false);
                if (completeTime < defaultCompleteTime) completeTime = defaultCompleteTime;
                return;
            }
        }

        if (!timerText.gameObject.activeSelf) timerText.gameObject.SetActive(true);
        if (completeTime > 0)
        {
            completeTime -= Time.deltaTime;
            int remainTime = Mathf.FloorToInt(completeTime);
            timerText.text = remainTime.ToString();
        }
        if (completeTime <= 0)
        {
            moveRuleDisplay = true;
            timerText.gameObject.SetActive(false);
            foreach (PlayerInput player in players)
            {
                player.DeactivateInput();
            }
            StartCoroutine(OnMoveRuleDisplay());
            //決まり際にプレイヤー情報を別のDontDestroyOnLoadクラスに送る
        }
    }
    IEnumerator OnMoveRuleDisplay()
    {
        while (!(Vector3.Distance(cam.transform.position, targetPosition) < 0.01f))
        {
            cam.transform.position = Vector3.MoveTowards(cam.transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
        cam.transform.position = targetPosition;

        foreach (PlayerInput player in players)
        {
            player.ActivateInput(); //次回の変更点
        }
    }

    public void SetReady(PlayerInput playerInput, bool isReady)
    {
        foreach (PlayerInput player in players)
        {
            if (player == playerInput)
            {
                playersReady[players.IndexOf(player)] = isReady;
                return;
            }
        }
    }
    public PlayerInput GetPlayer(int num)
    {
        return players[num];
    }
    /*
    manager.EnableJoining()
    manager.DisableJoining()
    */
}