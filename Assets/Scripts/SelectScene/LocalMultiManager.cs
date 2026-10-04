using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class LocalMultiManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] GameObject[] selectUIs;
    [SerializeField] TMPro.TMP_Text timerText;
    [SerializeField] float completeTime = 3;
    [Header("カメラ")]
    [SerializeField] GameObject cam;
    [SerializeField] Vector3 targetPosition;
    [SerializeField] float speed = 20;
    [Header("スポーン位置")]
    [SerializeField] Transform[] spawnPoints;
    [Header("準備完了タイマー")]

    PlayerInputManager manager;
    PlayerInput[] players = new PlayerInput[4];//参加しているプレイヤー
    List<bool> playersReady = new List<bool>();
    bool?[] nullableReady = new bool?[] { null, null, null, null };
    float defaultCompleteTime;
    bool moveRuleDisplay;
    string initialWord;

    void Awake()
    {
        initialWord = timerText.text;
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
        int index = 0;
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null) continue;
            players[i] = playerInput;
            index = i;
            break;
        }
        nullableReady[index] = false;
        selectUIs[index].SetActive(true);
        players[index].uiInputModule = selectUIs[index].GetComponent<InputSystemUIInputModule>();

        Debug.Log($"{index + 1}列目に参加しました。使用デバイス: {playerInput.devices[0].displayName}");
        if (playerInput.TryGetComponent<CharacterController>(out var cc))
        {
            playerInput.SwitchCurrentActionMap("Select");
            DontDestroyOnLoad(playerInput.gameObject);
            cc.enabled = false;
            playerInput.transform.position = spawnPoints[index].position;
            cc.enabled = true;
        }
        else
        {
            playerInput.transform.position = spawnPoints[playerInput.playerIndex].position;
        }
    }
    private void HandlePlayerLeft(PlayerInput playerInput)
    {
        int index = System.Array.IndexOf(players, playerInput);
        Debug.Log($"{index + 1}列目のプレイヤーが離脱しました。");
        players[index] = null;
        nullableReady[index] = null;
        //1FおいてからMultiplayerEventSystemを非アクティブにする(退出ボタン処理中にEventSystemがnullになるのを防ぐ)
        StartCoroutine(DeactivateSelectUINextFrame(index));
        /*
        if (players.Count == 0) return;
        foreach (PlayerInput input in players)
        {
            CharacterController cc = input.GetComponent<CharacterController>();
            cc.enabled = false;
            input.transform.position = spawnPoints[players.IndexOf(input)].position;
            cc.enabled = true;
        }
        */
    }
    IEnumerator DeactivateSelectUINextFrame(int index)
    {
        yield return null;
        selectUIs[index].SetActive(false);
    }
    #endregion

    void Update()
    {
        if (playersReady.Count == 0 || moveRuleDisplay) return;
        foreach (bool ready in playersReady)
        {
            if (!ready)
            {
                if (timerText.text != initialWord) timerText.text = initialWord;
                if (completeTime < defaultCompleteTime) completeTime = defaultCompleteTime;
                return;
            }
        }

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
                if (player == null) continue;
                player.DeactivateInput();
                DontDestroyOnLoad(player.gameObject);
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
            if (player == null) continue;
            //player.ActivateInput(); //次回の変更点
        }
    }

    #region ボタン操作
    public void OnLeftButton(int playerIndex)
    {
        Destroy(players[playerIndex].gameObject);
    }
    public void OnAccessoryButton()
    {

    }
    #endregion

    public void SetReady()
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