using GAS.Attributes;
using GAS.Core;
using GAS.Effects;
using UnityEngine;

namespace Examples.Enemies
{
    /// <summary>
    /// Base class for enemies using the Gameplay Ability System.
    /// Different enemy types can use different AttributeSetDefinitions.
    /// </summary>
    [RequireComponent(typeof(AbilitySystemComponent))]
    public class EnemyBase : MonoBehaviour
    {
        [Header("Attributes (drag from Assets/Data/GAS/Attributes)")]
        [SerializeField] protected AttributeDefinition _healthAttr;
        [SerializeField] protected AttributeDefinition _maxHealthAttr;
        [SerializeField] protected AttributeDefinition _damageAttr;
        [SerializeField] protected AttributeDefinition _attackSpeedAttr;
        [SerializeField] protected AttributeDefinition _rangeAttr;

        [Header("Combat")]
        [SerializeField] protected GameplayEffectDefinition _attackEffect;
        [SerializeField] protected float _attackCooldown = 1f;

        [Header("Detection")]
        // [SerializeField] protected float _detectionRange = 10f;
        [SerializeField] protected string PLAYERTAG = "Player";

        // Components
        protected AbilitySystemComponent _asc;
        protected Rigidbody2D _rb;

        // State
        protected bool _isDead;
        protected float _attackTimer;
        protected Transform _target;

        // Public accessors
        // public float Health => _asc.GetAttributeValue(_healthAttr);
        // public float MaxHealth => _maxHealthAttr != null ? _asc.GetAttributeValue(_maxHealthAttr) : Health;
        // public float HealthPercent => MaxHealth > 0 ? Health / MaxHealth : 0;
        // public bool IsDead => _isDead;
        // public AbilitySystemComponent AbilitySystem => _asc;

        protected virtual void Awake()
        {
            _asc = GetComponent<AbilitySystemComponent>();
            _rb = GetComponent<Rigidbody2D>();
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
            UpdateBehavior();
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

        protected bool TargetIsInRange()
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

        protected virtual bool CanAttack()
        {
            return _attackTimer <= 0 && !_isDead && _target != null;
        }

        protected virtual void TryAttack()
        {
            if (!CanAttack()) return;

            PerformAttack();
            _attackTimer = _attackCooldown;
        }

        protected virtual void PerformAttack()
        {
            if (_target == null) return;

            AbilitySystemComponent targetASC = _target.GetComponent<AbilitySystemComponent>();
            if (targetASC == null) return;

            //CHANGE FOR HITBOX OR PROJECTILE
            if (_attackEffect != null)
            {
                _asc.ApplyEffectToTarget(_attackEffect, targetASC);
            }

            Debug.Log($"{name} attacked {_target.name}");
        }

        #endregion

        #region Behavior (override in subclasses)

        protected virtual void UpdateBehavior()
        {
            // Default: attack if in range
            if (TargetIsInRange())
            {
                TryAttack();
            }
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
