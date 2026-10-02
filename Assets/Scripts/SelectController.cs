using UnityEngine;
using UnityEngine.InputSystem;

public class SelectController : MonoBehaviour
{
    PlayerInput input;
    InputAction look, select, left, ready;

    void Awake()
    {
        input = GetComponent<PlayerInput>();
        #region 入力取得
        look = input.actions["Look"];
        select = input.actions["Select"];
        left = input.actions["Left"];
        ready = input.actions["Ready"];
        #endregion
    }

    #region 入力システム
    void OnEnable()
    {
        select.performed += OnSelect;
        left.performed += OnLeft;
        ready.performed += OnReady;
    }
    void OnDisable()
    {
        select.performed -= OnSelect;
        left.performed -= OnLeft;
        ready.performed -= OnReady;
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
        print("準備完了");
    }
    #endregion

}
