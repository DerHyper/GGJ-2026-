using GAS.Abilities;
using GAS.Attributes;
using GAS.Core;
using GAS.Effects;
using UnityEngine;
using UnityEngine.UIElements;

namespace Examples.Enemies
{

    [RequireComponent(typeof(AbilitySystemComponent))]
    public class EnemyBase : MonoBehaviour
    {
        [Header("Attributes (drag from Assets/Data/GAS/Attributes)")]
        [SerializeField] protected AttributeDefinition _healthAttr;
        [SerializeField] protected AttributeDefinition _maxHealthAttr;
        [SerializeField] protected AttributeDefinition _damageAttr;
        [SerializeField] protected AttributeDefinition _attackSpeedAttr;
        [SerializeField] protected AttributeDefinition _rangeAttr;

        [SerializeField] protected AttributeDefinition _minimumAttackDistance;


        [Header("Combat")]
        public AbilityDefinition abilityToUse;
        [SerializeField] protected GameplayEffectDefinition _attackEffect;
        [SerializeField] protected float _attackCooldown = 1f;

        [Header("Detection")]
        // [SerializeField] protected float _detectionRange = 10f;
        [SerializeField] protected string PLAYERTAG = "Player";

        // Components
        public AbilitySystemComponent _asc;
        protected Rigidbody2D _rb;

        // State
        public bool _isDead;
        public float _attackTimer;
        public Transform _target;

        public EnemyState currentState;
        EnemyState startingState = new AttackState();

        protected virtual void Awake()
        {
            _asc = GetComponent<AbilitySystemComponent>();
            _rb = GetComponent<Rigidbody2D>();
            ChangeState(startingState);
        }

        protected virtual void OnEnable()
        {
            _asc.OnAttributeChanged += HandleAttributeChanged;
        }

        protected virtual void OnDisable()
        {
            _asc.OnAttributeChanged -= HandleAttributeChanged;
        }

        protected virtual void OnDestroy()
        {
            if (_asc != null)
            {
                _asc.OnAttributeChanged -= HandleAttributeChanged;
            }
        }

        protected virtual void Update()
        {
            FindTarget();

            if (_isDead) return;

            UpdateAttackCooldown();
            currentState.OnUpdate(this.gameObject);
        }

        public void ChangeState(EnemyState newState)
        {
            Debug.Log($"{name} changing State to: {newState}");
            currentState = newState;

            currentState.OnStart(this.gameObject);
        }


        #region Targeting

        protected virtual void FindTarget()
        {
            // Simple: find player by tag or layer
            if (_target == null)
            {
                var player = GameObject.FindGameObjectWithTag(PLAYERTAG);
                if (player != null)
                {
                    _target = player.transform;
                }
                else
                {
                    Debug.LogWarning("No Player found");
                }
            }
        }

        public bool TargetIsInRange()
        {
            if (_target == null) return false;
            float range = _rangeAttr != null ? _asc.GetAttributeValue(_rangeAttr) : 2f;
            return Vector2.Distance(transform.position, _target.position) <= range;
        }

        #endregion

        #region Combat

        protected virtual void UpdateAttackCooldown()
        {
            if (_attackTimer > 0)
            {
                // Attack speed affects cooldown (higher = faster)
                float attackSpeed = _attackSpeedAttr != null ? _asc.GetAttributeValue(_attackSpeedAttr) : 1f;
                _attackTimer -= Time.deltaTime * attackSpeed;
            }
        }

        public virtual bool CanAttack()
        {
            return _attackTimer <= 0 && !_isDead && _target != null;
        }

        public virtual bool IsTooCloseToTarget()
        {
            float distance = Vector2.Distance(_target.position, transform.position);
            return distance < _asc.GetAttributeValue(_minimumAttackDistance);
        }

        public void ResetTimer()
        {
            _attackTimer = _attackCooldown;
        }

        #endregion

        #region Health & Death

        protected virtual void HandleAttributeChanged(Attribute attribute, float oldValue, float newValue)
        {
            if (attribute.Definition == _healthAttr && newValue <= 0 && !_isDead)
            {
                Die();
            }

            // Clamp health to max
            if (attribute.Definition == _healthAttr && _maxHealthAttr != null)
            {
                float maxHealth = _asc.GetAttributeValue(_maxHealthAttr);
                if (newValue > maxHealth)
                {
                    attribute.BaseValue = maxHealth;
                }
            }
        }

        protected virtual void Die()
        {
            _isDead = true;
            Debug.Log($"{name} died!");

            // Stop movement
            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
            }

            // TODO: Drop loot, play death animation, destroy after delay
            Destroy(gameObject, 1f);
        }

        #endregion
    }
}
