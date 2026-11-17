using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class TopDownRigid : MonoBehaviour
{
    #region Inspector
    [Header("MoveConfig")]
    [SerializeField] float moveSpeed = 3;
    [SerializeField] float maxMoveSpeed;
    [SerializeField] float bodyRotationSpeed = 10;
    [Header("GravityConfig")]
    [SerializeField] float jumpHeight = 2;
    [SerializeField] float gravity = 9.81f;
    [SerializeField] float terminalVelocity = 50;

    [Header("GroundLayerConfig")]
    [SerializeField] LayerMask layer;
    #endregion

    Rigidbody rb;

    #region PlayerMove
    PlayerInput input;
    InputAction move, jump, attack;
    InputActionMap playerMoveMap;
    CapsuleCollider cc;
    GameObject cam;
    Vector3 moveDir, lookDir;
    float currentMoveSpeed;
    float verticalVelocity;
    float attackCoolTime;
    float maxCoolTime;
    //Animator anim, camAnim;
    #endregion

    #region PlayerSelect
    InputAction rotate;
    InputActionMap playerSelectMap;
    float y;
    #endregion

    void Awake()
    {
        input = GetComponent<PlayerInput>();
        playerMoveMap = input.actions.FindActionMap("Player");
        playerSelectMap = input.actions.FindActionMap("Select");
        #region PlayerMove_ActionMap
        move = input.actions["Player/Move"];
        jump = input.actions["Player/Jump"];
        attack = input.actions["Player/Attack"];
        #endregion
        #region PlayerSelect_ActionMap
        rotate = input.actions["Select/Rotate"];
        #endregion

        rb = GetComponent<Rigidbody>();

        cc = GetComponent<CapsuleCollider>();

        cam = Camera.main.gameObject;

        //anim = GetComponentInChildren<Animator>();
        //camAnim = cam.GetComponent<Animator>();
    }

    #region InputSystem
    void OnEnable()
    {
        jump.performed += OnJump;
        attack.performed += OnAttack;
    }
    void OnDisable()
    {
        jump.performed -= OnJump;
        attack.performed -= OnAttack;
    }
    //PlayerInputAction
    void OnJump(InputAction.CallbackContext context)
    {
        if (!Physics.CheckSphere(transform.position + new Vector3(0, cc.radius - 0.01f, 0), cc.radius, layer, QueryTriggerInteraction.Ignore)) return;
        verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * -gravity);
    }
    void OnAttack(InputAction.CallbackContext context)
    {
        //
    }
    #endregion

    void Update()
    {
        if (input.currentActionMap == playerMoveMap)
        {
            GetCameraDirection();
            Move();
            RotationBody(); //ボディの回転
            //SetAnimation();
        }
        else if (input.currentActionMap == playerSelectMap)
        {
            y -= rotate.ReadValue<float>();
            transform.rotation = Quaternion.Euler(0, y, 0);
        }
    }

    #region Calculation
    void GetCameraDirection()
    {
        Vector3 camForward = cam.transform.forward; camForward.y = 0f; camForward.Normalize();
        Vector3 camRight = cam.transform.right; camRight.y = 0f; camRight.Normalize();
        Vector2 input_Move = move.ReadValue<Vector2>();
        moveDir = camForward * input_Move.y + camRight * input_Move.x;
    }
    void Move()
    {
        //use to Direction,Speed[x],and Time.deltaTime
        //rb.linearVelocity = ;
    }
    void RotationBody()
    {
        if (moveDir == Vector3.zero) return;
        Quaternion targetRotation = Quaternion.LookRotation(moveDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, bodyRotationSpeed * Time.deltaTime);
    }
    /*
    void SetAnimation()
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
    }
    */
    #endregion

    public void SetBodyRotation(float angle)
    {
        y = angle;
    }

    void OnDrawGizmosSelected()
    {
        if (cc == null) return;
        Gizmos.color = Physics.CheckSphere(transform.position + new Vector3(0, cc.radius - 0.01f, 0), cc.radius, layer, QueryTriggerInteraction.Ignore) ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position + new Vector3(0, cc.radius, 0), cc.radius);
    }
}