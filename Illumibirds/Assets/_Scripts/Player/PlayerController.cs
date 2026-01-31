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

    [Header("Attribute Values")]
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

        _asc.OnAttributeChanged += HandleAttributeChanged;
    }

    void OnDisable()
    {
        inputActions.Disable();
        inputActions.Player.Attack.performed -= OnAttack;
        inputActions.Player.Look.performed -= OnLook;

        if (_asc != null)
        {
            _asc.OnAttributeChanged -= HandleAttributeChanged;
        }
    }

    void Update()
    {
        if (_isDead) return;

        HandleStaminaRegen();
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

        if (lookVector.sqrMagnitude > 0.1f)
        {
            float angle = Mathf.Atan2(lookVector.y, lookVector.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

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


    #region Stamina

    public bool TryConsumeStamina(float amount)
    {
        if (_staminaAttr == null) return true; // No stamina system

        float current = _asc.GetAttributeValue(_staminaAttr);
        if (current < amount) return false;

        // Consume stamina
        var attr = _asc.GetAttribute(_staminaAttr);
        attr.BaseValue -= amount;

        // Reset regen delay
        _staminaRegenTimer = _staminaRegenDelay;

        return true;
    }

    private void HandleStaminaRegen()
    {
        if (_staminaAttr == null || _maxStaminaAttr == null) return;

        if (_staminaRegenTimer > 0)
        {
            _staminaRegenTimer -= Time.deltaTime;
            return;
        }

        float current = _asc.GetAttributeValue(_staminaAttr);
        float max = _asc.GetAttributeValue(_maxStaminaAttr);

        if (current < max)
        {
            var attr = _asc.GetAttribute(_staminaAttr);
            attr.BaseValue = Mathf.Min(attr.BaseValue + _staminaRegenRate * Time.deltaTime, max);
        }
    }

    #endregion

    #region Health & Death

    private void HandleAttributeChanged(Attribute attribute, float oldValue, float newValue)
    {
        // Check for death
        if (attribute.Definition == _healthAttr && newValue <= 0 && !_isDead)
        {
            Die();
        }

        // Clamp health to max health
        if (attribute.Definition == _healthAttr && _maxHealthAttr != null)
        {
            float maxHealth = _asc.GetAttributeValue(_maxHealthAttr);
            if (newValue > maxHealth)
            {
                attribute.BaseValue = maxHealth;
            }
        }

        // Clamp stamina to max stamina
        if (attribute.Definition == _staminaAttr && _maxStaminaAttr != null)
        {
            float maxStamina = _asc.GetAttributeValue(_maxStaminaAttr);
            if (newValue > maxStamina)
            {
                attribute.BaseValue = maxStamina;
            }
        }
    }

    private void Die()
    {
        _isDead = true;
        Debug.Log("PLAYER DIE");
        rb.linearVelocity = Vector2.zero;
        // TODO: Trigger death animation, game over screen, etc.
    }

    public void Heal(float amount)
    {
        if (_healthAttr == null || _isDead) return;

        var attr = _asc.GetAttribute(_healthAttr);
        float maxHealth = _maxHealthAttr != null ? _asc.GetAttributeValue(_maxHealthAttr) : float.MaxValue;
        attr.BaseValue = Mathf.Min(attr.BaseValue + amount, maxHealth);
    }

    /// <summary>
    /// Deal damage to the player directly (bypassing effects).
    /// Prefer using Effects for damage when possible.
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (_healthAttr == null || _isDead) return;

        var attr = _asc.GetAttribute(_healthAttr);
        attr.BaseValue -= amount;
    }

    #endregion

}
