using UnityEngine;
using UnityEngine.Accessibility;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    Rigidbody2D rb;
    InputSystem_Actions inputActions;
    [SerializeField] float moveSpeed = 7;
    [SerializeField] Transform render;
    bool canMove = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputActions = new();
    }

    void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Attack.performed += OnAttack;
        inputActions.Player.Look.performed += OnLook;
    }

    void OnDisable()
    {
        inputActions.Disable();
        inputActions.Player.Attack.performed -= OnAttack;
        inputActions.Player.Look.performed -= OnLook;
    }

    void Update()
    {
        if (canMove) DoMove();
    }

    void DoMove()
    {
        Vector2 moveVector = inputActions.Player.Move.ReadValue<Vector2>().normalized;
        float multiplier = 1;
        rb.linearVelocity = moveVector * moveSpeed * multiplier;
        // Debug.Log($"Vector: {moveVector}");
    }

    void OnLook(InputAction.CallbackContext ctx)
    {
        Vector2 lookVector = inputActions.Player.Look.ReadValue<Vector2>().normalized;
        Debug.Log($"Look at: {lookVector}");

        if (lookVector.sqrMagnitude > 0.1f)
        {
            float angle = Mathf.Atan2(lookVector.y, lookVector.x) * Mathf.Rad2Deg;
            render.rotation = Quaternion.Euler(0, 0, angle);

        }
    }
    
    void OnAttack(InputAction.CallbackContext ctx)
    {
        Debug.Log("DO ATTACK");
    }

    public void ToggleControl(bool _canMove)
    {
        canMove = _canMove;
    }
}
