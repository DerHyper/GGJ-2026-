using GAS.Attributes;
using GAS.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Examples.Player
{

    /// <summary>
    /// Player controller that integrates with the Gameplay Ability System.
    /// Requires: AbilitySystemComponent on the same GameObject.
    /// </summary>
    [RequireComponent(typeof(AbilitySystemComponent))]
    public class PlayerControllerExample : MonoBehaviour
    {
        [Header("Movement")] [SerializeField] private float _baseMoveSpeed = 5f;

        [Header("Attributes (drag from Assets/Data/GAS/Attributes)")] [SerializeField]
        private AttributeDefinition _healthAttr;

        [SerializeField] private AttributeDefinition _maxHealthAttr;
        [SerializeField] private AttributeDefinition _staminaAttr;
        [SerializeField] private AttributeDefinition _maxStaminaAttr;
        [SerializeField] private AttributeDefinition _attackSpeedAttr;

        [Header("Stamina Regen")] [SerializeField]
        private float _staminaRegenRate = 10f; // per second

        [SerializeField] private float _staminaRegenDelay = 1f; // delay after using stamina

        // Components
        private AbilitySystemComponent _asc;
        private Rigidbody2D _rb;

        // Input
        private Vector2 _moveInput;

        // State
        private float _staminaRegenTimer;
        private bool _isDead;

        // Public accessors for UI
        public float Health => _asc.GetAttributeValue(_healthAttr);
        public float MaxHealth => _asc.GetAttributeValue(_maxHealthAttr);
        public float Stamina => _asc.GetAttributeValue(_staminaAttr);
        public float MaxStamina => _asc.GetAttributeValue(_maxStaminaAttr);
        public float HealthPercent => MaxHealth > 0 ? Health / MaxHealth : 0;
        public float StaminaPercent => MaxStamina > 0 ? Stamina / MaxStamina : 0;
        public bool IsDead => _isDead;

        public AbilitySystemComponent AbilitySystem => _asc;

        private void Awake()
        {
            _asc = GetComponent<AbilitySystemComponent>();
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            // Subscribe to attribute changes
            _asc.OnAttributeChanged += HandleAttributeChanged;
        }

        private void OnDestroy()
        {
            if (_asc != null)
            {
                _asc.OnAttributeChanged -= HandleAttributeChanged;
            }
        }

        private void Update()
        {
            if (_isDead) return;

            HandleStaminaRegen();
        }

        private void FixedUpdate()
        {
            if (_isDead) return;

            HandleMovement();
        }

        #region Input (call these from InputSystem or your input handler)

        public void OnMove(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
        }

        // Alternative: call directly with Vector2
        public void SetMoveInput(Vector2 input)
        {
            _moveInput = input;
        }

        #endregion

        #region Movement

        private void HandleMovement()
        {
            if (_rb == null || _moveInput == Vector2.zero) return;

            // Base speed (could also be an attribute if you want speed buffs)
            float speed = _baseMoveSpeed;

            Vector2 velocity = _moveInput.normalized * speed;
            _rb.linearVelocity = velocity;
        }

        #endregion

        #region Stamina

        /// <summary>
        /// Try to consume stamina. Returns true if successful.
        /// Use this for abilities, sprinting, etc.
        /// </summary>
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

            // Wait for delay
            if (_staminaRegenTimer > 0)
            {
                _staminaRegenTimer -= Time.deltaTime;
                return;
            }

            // Regen stamina
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
            _moveInput = Vector2.zero;

            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
            }

            Debug.Log("Player died!");

            // TODO: Trigger death animation, game over screen, etc.
        }

        /// <summary>
        /// Heal the player by a flat amount.
        /// </summary>
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
}