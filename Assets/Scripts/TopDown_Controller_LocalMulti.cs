using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(CharacterController))]
public class TopDown_Controller_LocalMulti : MonoBehaviour
{
    #region インスペクター設定
    [Header("移動設定")]
    [SerializeField] float walkSpeed = 3;
    [SerializeField] float bodyRotationSpeed = 10;
    [Header("重力設定")]
    [SerializeField] float jumpHeight = 2;
    [SerializeField] float gravity = 9.81f;
    [SerializeField] float terminalVelocity = 50;

    [Header("地面判定")]
    [SerializeField] LayerMask layer;
    #endregion

    #region パブリック変数

    #endregion

    #region プライベート変数
    PlayerInput input;
    InputAction move, jump, attack;
    InputActionMap playerMap;
    CharacterController cc;
    GameObject cam;
    Vector3 moveDir, lookDir;
    float currentMoveSpeed;
    float verticalVelocity; //プレイヤーの縦移動速度
    //Animator anim, camAnim;
    #endregion

    #region セレクト用
    InputAction look, select, left, ready;
    InputActionMap selectMap;
    float y;
    bool isReady;
    #endregion

    void Awake()
    {
        input = GetComponent<PlayerInput>();
        playerMap = input.actions.FindActionMap("Player");
        selectMap = input.actions.FindActionMap("Select");
        #region Player入力取得
        move = input.actions["Player/Move"];
        jump = input.actions["Player/Jump"];
        attack = input.actions["Player/Attack"];
        #endregion
        #region Select入力取得
        look = input.actions["Select/Look"];
        select = input.actions["Select/Select"];
        left = input.actions["Select/Left"];
        ready = input.actions["Select/Ready"];
        #endregion

        cc = GetComponent<CharacterController>();
        currentMoveSpeed = walkSpeed;

        cam = Camera.main.gameObject;

        //anim = GetComponentInChildren<Animator>();
        //camAnim = cam.GetComponent<Animator>();
    }

    #region 入力システム
    void OnEnable()
    {
        jump.performed += OnJump;
        attack.performed += OnAttack;

        select.performed += OnSelect;
        left.performed += OnLeft;
        ready.performed += OnReady;
    }
    void OnDisable()
    {
        jump.performed -= OnJump;
        attack.performed -= OnAttack;

        select.performed -= OnSelect;
        left.performed -= OnLeft;
        ready.performed -= OnReady;
    }
    void OnJump(InputAction.CallbackContext context)
    {
        if (!Physics.CheckSphere(transform.position + new Vector3(0, cc.radius - 0.01f, 0), cc.radius, layer, QueryTriggerInteraction.Ignore)) return;
        verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * -gravity);
    }
    void OnAttack(InputAction.CallbackContext context)
    {
        print("アタック!!");
    }

    void OnSelect(InputAction.CallbackContext context)
    {
        //boolで選択したら値が0になるまで無効化
    }
    void OnLeft(InputAction.CallbackContext context)
    {
        Destroy(gameObject);
    }
    void OnReady(InputAction.CallbackContext context)
    {
        isReady = !isReady;
        print(isReady ? "準備完了" : "キャンセル");
        FindAnyObjectByType<LocalMultiManager>().SetReady(input, isReady);
    }
    #endregion

    void Update()
    {
        if (input.currentActionMap == playerMap)
        {
            GetCameraDirection(); //カメラの向き取得、プレイヤーの移動
            RotationBody(); //ボディの回転

            //SetAnimation();
        }
        SetGravity(); //重力計算

        if (input.currentActionMap == selectMap)
        {
            y += look.ReadValue<float>();
            transform.rotation = Quaternion.Euler(0, -y, 0);
        }
    }

    #region 計算処理
    void GetCameraDirection()
    {
        Vector3 camForward = cam.transform.forward; camForward.y = 0f; camForward.Normalize();
        Vector3 camRight = cam.transform.right; camRight.y = 0f; camRight.Normalize();
        Vector2 input_Move = move.ReadValue<Vector2>();
        moveDir = camForward * input_Move.y + camRight * input_Move.x;
        if (moveDir != Vector3.zero)
            lookDir = moveDir;
        cc.Move(moveDir * currentMoveSpeed * Time.deltaTime);
    }
    void RotationBody()
    {
        if (lookDir == Vector3.zero) return;
        Quaternion targetRotation = Quaternion.LookRotation(lookDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, bodyRotationSpeed * Time.deltaTime);
    }
    void SetGravity()
    {
        if (Physics.CheckSphere(transform.position + new Vector3(0, cc.radius - 0.01f, 0), cc.radius, layer, QueryTriggerInteraction.Ignore) && verticalVelocity < 0)
        {
            verticalVelocity = -1f; //しっかり地面に着けるために微小な重力をかける
        }
        else
        {
            verticalVelocity += -gravity * Time.deltaTime;
            verticalVelocity = verticalVelocity < -terminalVelocity ? -terminalVelocity : verticalVelocity;
        }
        cc.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);
    }
    /*void SetAnimation()
    {
        if (anim != null)
        {
            float x = moveInput.x * (isSprint ? 2 : 1);
            float y = moveInput.y * (isSprint ? 2 : 1);
            anim.SetFloat("SpeedX", x);
            anim.SetFloat("SpeedY", y);
            anim.SetBool("IsCrouch", isCrouch);
        }

        if (camAnim != null)
        {
            camAnim.SetBool("IsMove", moveInput.x != 0 || moveInput.y != 0);
            camAnim.SetFloat("BobSpeed", isSprint ? 2 : 1);
        }
    }*/
    #endregion

    void OnDrawGizmosSelected()
    {
        if (cc == null) return;
        Gizmos.color = Physics.CheckSphere(transform.position + new Vector3(0, cc.radius - 0.01f, 0), cc.radius, layer, QueryTriggerInteraction.Ignore) ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position + new Vector3(0, cc.radius, 0), cc.radius);
    }
}

/*
PlayerInputコンポーネントを用いる方式、InputActionからC#クラスを生成する必要はない。
ローカルマルチプレイ専用であり、ソロ・オンラインには向かない。
*/