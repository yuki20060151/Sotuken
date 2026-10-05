using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class LocalMultiManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] GameObject[] selectUIs;
    [SerializeField] GameObject[] readyImage;
    [SerializeField] TMPro.TMP_Text timerText;
    [SerializeField] float completeTime = 3;

    [Header("カメラ")]
    [SerializeField] GameObject cam;
    [SerializeField] Vector3 targetPosition;
    [SerializeField] float speed = 20;

    [Header("スポーン位置")]
    [SerializeField] Transform[] spawnPoints;

    PlayerInputManager manager;
    PlayerInput[] players = new PlayerInput[4];//参加しているプレイヤー
    bool?[] nullableReady = new bool?[] { null, null, null, null };
    float defaultCompleteTime;
    bool moveRuleDisplay;
    string initialWord;

    void Awake()
    {
        initialWord = timerText.text;
        manager = GetComponent<PlayerInputManager>();
        manager.EnableJoining();
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
        if (moveRuleDisplay) return;
        int nullCount = 0;
        foreach (bool? ready in nullableReady)
        {
            if (ready == null)
            {
                nullCount++;
                if (nullCount == 4) return;
                continue;
            }
            else if (ready == false)
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
            manager.DisableJoining(); //途中参加を許可しない
            moveRuleDisplay = true;
            timerText.gameObject.SetActive(false);
            foreach (PlayerInput player in players)
            {
                if (player == null) continue;
                player.DeactivateInput();
                DontDestroyOnLoad(player.gameObject);
                selectUIs[System.Array.IndexOf(players, player)].SetActive(false);
            }
            StartCoroutine(OnMoveRuleDisplay());
            //決まったらプレイヤー情報を別のDontDestroyOnLoadクラスに送る
        }
    }
    IEnumerator OnMoveRuleDisplay()
    {
        while (!(Vector3.Distance(cam.transform.position, targetPosition) < 0.01f))
        {
            cam.transform.position = Vector3.MoveTowards(cam.transform.position, targetPosition, speed * Time.deltaTime);
            cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, Quaternion.Euler(13, 0, 0), speed / 2 * Time.deltaTime);
            yield return null;
        }
        cam.transform.position = targetPosition;

        foreach (PlayerInput player in players)
        {
            if (player == null) continue;
            player.SwitchCurrentActionMap("Player");
            player.ActivateInput();
        }
    }

    #region ボタン操作
    public void OnLeftButton(int playerIndex)
    {
        Destroy(players[playerIndex].gameObject);
    }
    public void OnAccessoryButton()
    {
        //各アクセサリー画面を開く
    }
    public void OnReadyButton(int playerIndex)
    {
        if (nullableReady[playerIndex] == null) return;
        nullableReady[playerIndex] = !nullableReady[playerIndex];

        if (readyImage[playerIndex].TryGetComponent(out UnityEngine.UI.Image image))
        {
            bool imageReady = nullableReady[playerIndex] ?? false;
            image.color = imageReady ? Color.green : Color.red;
        }
    }
    #endregion

    public PlayerInput GetPlayer(int num)
    {
        if (players[num] == null)
        {
            Debug.LogWarning("その番号のプレイヤーはいません");
            return null;
        }
        return players[num];
    }
}