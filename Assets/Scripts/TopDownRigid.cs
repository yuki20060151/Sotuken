using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class TopDownRigid : MonoBehaviour
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

    Rigidbody rb;

    #region プレイヤー用
    PlayerInput input;
    InputAction move, jump, attack;
    InputActionMap playerMap;
    CapsuleCollider cc;
    GameObject cam;
    Vector3 moveDir, lookDir;
    float currentMoveSpeed;
    float verticalVelocity; //プレイヤーの縦移動速度
    float attackCoolTime;
    float maxCoolTime;
    //Animator anim, camAnim;
    #endregion

    #region セレクト用
    InputAction rotate;
    InputActionMap selectMap;
    float y;
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
        rotate = input.actions["Select/Rotate"];
        #endregion

        rb = GetComponent<Rigidbody>();

        cc = GetComponent<CapsuleCollider>();
        //playerStandCenter = cc.center.y;
        //playerStandHeight = cc.height;
        currentMoveSpeed = walkSpeed;

        //anim = GetComponentInChildren<Animator>();
        //camAnim = cam.GetComponent<Animator>();
    }

    #region 入力システム
    void OnEnable()
    {
        //プレイヤー
        jump.performed += OnJump;
        attack.performed += OnAttack;
    }
    void OnDisable()
    {
        //プレイヤー
        jump.performed -= OnJump;
        attack.performed -= OnAttack;
    }
    //プレイヤー入力
    void OnJump(InputAction.CallbackContext context)
    {
        if (!Physics.CheckSphere(transform.position + new Vector3(0, cc.radius - 0.01f, 0), cc.radius, layer, QueryTriggerInteraction.Ignore)) return;
        verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * -gravity);
    }
    void OnAttack(InputAction.CallbackContext context)
    {
        attackCoolTime -= Time.deltaTime;
        if (attackCoolTime > 0) return;
        attackCoolTime = maxCoolTime;
        print("アタック!!");

    }
    #endregion

    /*
    void Update()
    {
        CameraControl(); //カメラの移動、操作
        GetCameraDirection(); //カメラの向き取得、プレイヤーの移動
        RotationBody(); //ボディの回転
        if (smoothTime < 0.1f && Physics.CheckSphere(transform.position + new Vector3(0, cc.radius, 0), cc.radius, layer, QueryTriggerInteraction.Ignore))
        {
            smoothTime = 0.1f;
        }
    }

    #region 計算処理
    void CameraControl()
    {
        camPos.transform.position = Vector3.SmoothDamp
        (
            camPos.transform.position,
            transform.position + new Vector3(0, cc.height, 0),
            ref camVelocity,
            smoothTime
        );

        Vector2 input_Look = input.Player.Look.ReadValue<Vector2>() * sensitivity * Time.deltaTime;
        mx -= input_Look.y; my += input_Look.x;
        mx = Mathf.Clamp(mx, -verticalAngleLimit, verticalAngleLimit);
        camPos.transform.rotation = Quaternion.Euler(mx, my, 0);
    }
    void GetCameraDirection()
    {
        Vector3 camForward = camPos.transform.forward; camForward.y = 0f; camForward.Normalize();
        Vector3 camRight = camPos.transform.right; camRight.y = 0f; camRight.Normalize();
        Vector2 input_Move = input.Player.Move.ReadValue<Vector2>();
        moveDir = camForward * input_Move.y + camRight * input_Move.x;
        transform.Translate(moveDir * currentMoveSpeed * Time.deltaTime, Space.World);
    }
    void RotationBody()
    {
        Vector3 camForward = camPos.transform.forward; camForward.y = 0f; camForward.Normalize();
        Quaternion targetRotation = Quaternion.LookRotation(camForward);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
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
    //#endregion
    /*
    void OnDrawGizmosSelected()
    {
        if (cc == null) return;
        Gizmos.color = Physics.CheckSphere(transform.position + new Vector3(0, cc.radius, 0), cc.radius, layer, QueryTriggerInteraction.Ignore) ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position + new Vector3(0, cc.radius, 0), cc.radius);

        if (!isCrouch) return;
        Gizmos.color = Physics.CheckSphere
        (
            transform.position + new Vector3(0, playerStandHeight - cc.radius, 0),
            cc.radius,
            cannotStandLayer,
            QueryTriggerInteraction.Ignore
        ) ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position + new Vector3(0, playerStandHeight - cc.radius, 0), cc.radius);
    }
    */
}