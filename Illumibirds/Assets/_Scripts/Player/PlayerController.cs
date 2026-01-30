using GAS.Abilities;
using GAS.Attributes;
using GAS.Core;
using UnityEngine;
using UnityEngine.Accessibility;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(AbilitySystemComponent))]
public class PlayerController : MonoBehaviour
{
    [Header("Attributes (drag from Assets/Data/GAS/Attributes)")]
    [SerializeField]
    private AttributeDefinition _healthAttr;

    [SerializeField] private AttributeDefinition _maxHealthAttr;
    [SerializeField] private AttributeDefinition _staminaAttr;
    [SerializeField] private AttributeDefinition _maxStaminaAttr;
    [SerializeField] private AttributeDefinition _attackSpeedAttr;
    [SerializeField] private AttributeDefinition _moveSpeedAttr;

    [Header("Stamina Regen")]
    [SerializeField]
    private float _staminaRegenRate = 10f; // per second

    [SerializeField] private float _staminaRegenDelay = 1f; // delay after using stamina


    private float _staminaRegenTimer;
    private bool _isDead;

    // Public accessors for UI
    private AbilitySystemComponent _asc;
    public float Health => _asc.GetAttributeValue(_healthAttr);
    public float MaxHealth => _asc.GetAttributeValue(_maxHealthAttr);
    public float Stamina => _asc.GetAttributeValue(_staminaAttr);
    public float MaxStamina => _asc.GetAttributeValue(_maxStaminaAttr);

    public float MoveSpeed => _asc.GetAttributeValue(_moveSpeedAttr);

    public float HealthPercent => MaxHealth > 0 ? Health / MaxHealth : 0;
    public float StaminaPercent => MaxStamina > 0 ? Stamina / MaxStamina : 0;
    public bool IsDead => _isDead;



    Rigidbody2D rb;
    InputSystem_Actions inputActions;
    [SerializeField] Transform render;
    bool canMove = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        _asc = GetComponent<AbilitySystemComponent>();
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
        rb.linearVelocity = moveVector * MoveSpeed * multiplier;
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
