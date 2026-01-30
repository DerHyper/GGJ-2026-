using System;
using System.Collections.Generic;
using GAS.Abilities;
using GAS.Attributes;
using GAS.Effects;
using GAS.Tags;
using UnityEngine;

namespace GAS.Core
{
    /// <summary>
    /// Central component that manages attributes, effects, and abilities for an entity.
    /// Attach this to any GameObject that should participate in the GAS.
    /// </summary>
    public class AbilitySystemComponent : MonoBehaviour
    {
        [Header("Initialization")]
        [SerializeField]
        private AttributeSetDefinition _attributeSetDefinition;

        [SerializeField]
        private List<AbilityDefinition> _startingAbilities = new();

        [Header("Debug")]
        [SerializeField] private bool _debugLogEffects = false;
        [SerializeField] private bool _debugLogAbilities = false;
        [SerializeField] private bool _debugLogAttributes = false;

        // Runtime state
        private readonly AttributeSet _attributeSet = new();
        private readonly GameplayTagContainer _ownedTags = new();
        private readonly List<GameplayEffectInstance> _activeEffects = new();
        private readonly Dictionary<AbilityDefinition, AbilityInstance> _grantedAbilities = new();

        // Public accessors
        public AttributeSet Attributes => _attributeSet;
        public GameplayTagContainer OwnedTags => _ownedTags;
        public IReadOnlyList<GameplayEffectInstance> ActiveEffects => _activeEffects;
        public IReadOnlyDictionary<AbilityDefinition, AbilityInstance> GrantedAbilities => _grantedAbilities;

        // Events
        public event Action<Attributes.Attribute, float, float> OnAttributeChanged;
        public event Action<GameplayEffectInstance> OnEffectApplied;
        public event Action<GameplayEffectInstance> OnEffectRemoved;
        public event Action<AbilityInstance> OnAbilityGranted;
        public event Action<AbilityInstance> OnAbilityRemoved;
        public event Action<AbilityInstance> OnAbilityActivated;
        public event Action<AbilityInstance> OnAbilityEnded;

        private void Awake()
        {
            // Initialize attributes
            _attributeSet.Initialize(_attributeSetDefinition);
            _attributeSet.OnAttributeChanged += HandleAttributeChanged;

            // Grant starting abilities
            foreach (var abilityDef in _startingAbilities)
            {
                GrantAbility(abilityDef);
            }
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            // Update effects
            UpdateEffects(deltaTime);

            // Update abilities
            UpdateAbilities(deltaTime);
        }

        private void OnDestroy()
        {
            _attributeSet.OnAttributeChanged -= HandleAttributeChanged;
        }

        #region Attributes

        public float GetAttributeValue(AttributeDefinition definition)
        {
            return _attributeSet.GetAttributeValue(definition);
        }

        public float GetAttributeBaseValue(AttributeDefinition definition)
        {
            return _attributeSet.GetAttributeBaseValue(definition);
        }

        public void SetAttributeBaseValue(AttributeDefinition definition, float value)
        {
            _attributeSet.SetAttributeBaseValue(definition, value);
        }

        public Attributes.Attribute GetAttribute(AttributeDefinition definition)
        {
            return _attributeSet.GetAttribute(definition);
        }

        private void HandleAttributeChanged(Attributes.Attribute attribute, float oldValue, float newValue)
        {
            if (_debugLogAttributes)
            {
                var delta = newValue - oldValue;
                var sign = delta >= 0 ? "+" : "";
                Debug.Log($"[GAS] {name} | {attribute.Definition.name}: {oldValue:F1} → {newValue:F1} ({sign}{delta:F1})", this);
            }
            OnAttributeChanged?.Invoke(attribute, oldValue, newValue);
        }

        #endregion

        #region Effects

        public bool CanApplyEffect(GameplayEffectDefinition effectDef)
        {
            if (effectDef == null) return false;

            // Check required tags
            if (effectDef.ApplicationRequiredTags.Count > 0)
            {
                if (!_ownedTags.HasAllTags(effectDef.ApplicationRequiredTags))
                    return false;
            }

            // Check blocked tags
            if (effectDef.ApplicationBlockedTags.Count > 0)
            {
                if (_ownedTags.HasAnyTag(effectDef.ApplicationBlockedTags))
                    return false;
            }

            return true;
        }

        public GameplayEffectInstance ApplyEffectToSelf(GameplayEffectDefinition effectDef, object source = null)
        {
            if (!CanApplyEffect(effectDef)) return null;

            // Check for stacking
            if (effectDef.CanStack)
            {
                var existingEffect = FindActiveEffect(effectDef);
                if (existingEffect != null && existingEffect.TryAddStack())
                {
                    return existingEffect;
                }
            }

            var instance = new GameplayEffectInstance(effectDef, source ?? this);

            // Apply modifiers
            ApplyEffectModifiers(instance);

            // Grant tags
            foreach (var tag in effectDef.GrantedTags)
            {
                _ownedTags.AddTag(tag);
            }

            // Handle instant effects
            if (effectDef.DurationType == EffectDurationType.Instant)
            {
                // Instant effects apply once and are done
                // Modifiers were already applied directly to base values
                if (_debugLogEffects)
                    Debug.Log($"[GAS] {name} | Effect Applied (Instant): {effectDef.name}", this);
                OnEffectApplied?.Invoke(instance);
                return instance;
            }

            // Add to active effects for duration/infinite
            _activeEffects.Add(instance);
            if (_debugLogEffects)
                Debug.Log($"[GAS] {name} | Effect Applied ({effectDef.DurationType}): {effectDef.name}", this);
            OnEffectApplied?.Invoke(instance);

            return instance;
        }

        public void ApplyEffectToTarget(GameplayEffectDefinition effectDef, AbilitySystemComponent target)
        {
            if (target == null || effectDef == null) return;
            target.ApplyEffectToSelf(effectDef, this);
        }

        public void RemoveEffect(GameplayEffectInstance instance)
        {
            if (instance == null || !_activeEffects.Contains(instance)) return;

            // Remove modifiers
            _attributeSet.RemoveModifiersFromSource(instance);

            // Remove granted tags
            foreach (var tag in instance.Definition.GrantedTags)
            {
                _ownedTags.RemoveTag(tag);
            }

            _activeEffects.Remove(instance);
            if (_debugLogEffects)
                Debug.Log($"[GAS] {name} | Effect Removed: {instance.Definition.name}", this);
            OnEffectRemoved?.Invoke(instance);
        }

        public void RemoveEffectsByDefinition(GameplayEffectDefinition definition)
        {
            var toRemove = new List<GameplayEffectInstance>();
            foreach (var effect in _activeEffects)
            {
                if (effect.Definition == definition)
                {
                    toRemove.Add(effect);
                }
            }

            foreach (var effect in toRemove)
            {
                RemoveEffect(effect);
            }
        }

        public void RemoveEffectsBySource(object source)
        {
            var toRemove = new List<GameplayEffectInstance>();
            foreach (var effect in _activeEffects)
            {
                if (effect.Source == source)
                {
                    toRemove.Add(effect);
                }
            }

            foreach (var effect in toRemove)
            {
                RemoveEffect(effect);
            }
        }

        private GameplayEffectInstance FindActiveEffect(GameplayEffectDefinition definition)
        {
            foreach (var effect in _activeEffects)
            {
                if (effect.Definition == definition)
                {
                    return effect;
                }
            }
            return null;
        }

        private void ApplyEffectModifiers(GameplayEffectInstance instance)
        {
            foreach (var modDef in instance.Definition.Modifiers)
            {
                if (modDef.TargetAttribute == null) continue;

                if (instance.Definition.DurationType == EffectDurationType.Instant)
                {
                    // Instant effects modify base value directly
                    var attribute = _attributeSet.GetAttribute(modDef.TargetAttribute);
                    if (attribute != null)
                    {
                        switch (modDef.Operation)
                        {
                            case ModifierOperation.Add:
                                attribute.BaseValue += modDef.Magnitude;
                                break;
                            case ModifierOperation.Multiply:
                                attribute.BaseValue *= modDef.Magnitude;
                                break;
                            case ModifierOperation.Override:
                                attribute.BaseValue = modDef.Magnitude;
                                break;
                        }
                    }
                }
                else
                {
                    // Duration/Infinite effects add modifiers
                    var modifier = new AttributeModifier(modDef.Operation, modDef.Magnitude, instance);
                    _attributeSet.AddModifier(modDef.TargetAttribute, modifier);
                    instance.TrackModifier(modifier);
                }
            }
        }

        private void UpdateEffects(float deltaTime)
        {
            var expiredEffects = new List<GameplayEffectInstance>();

            foreach (var effect in _activeEffects)
            {
                effect.Tick(deltaTime, () => ApplyPeriodicEffect(effect));

                if (effect.IsExpired)
                {
                    expiredEffects.Add(effect);
                }
            }

            foreach (var expired in expiredEffects)
            {
                RemoveEffect(expired);
            }
        }

        private void ApplyPeriodicEffect(GameplayEffectInstance effect)
        {
            // Re-apply instant modifiers for periodic effects
            foreach (var modDef in effect.Definition.Modifiers)
            {
                if (modDef.TargetAttribute == null) continue;

                var attribute = _attributeSet.GetAttribute(modDef.TargetAttribute);
                if (attribute == null) continue;

                switch (modDef.Operation)
                {
                    case ModifierOperation.Add:
                        attribute.BaseValue += modDef.Magnitude;
                        break;
                    case ModifierOperation.Multiply:
                        attribute.BaseValue *= modDef.Magnitude;
                        break;
                    case ModifierOperation.Override:
                        attribute.BaseValue = modDef.Magnitude;
                        break;
                }
            }
        }

        #endregion

        #region Abilities

        public AbilityInstance GrantAbility(AbilityDefinition definition)
        {
            if (definition == null) return null;

            if (_grantedAbilities.TryGetValue(definition, out var existing))
            {
                return existing;
            }

            var instance = new AbilityInstance(definition);
            _grantedAbilities[definition] = instance;
            if (_debugLogAbilities)
                Debug.Log($"[GAS] {name} | Ability Granted: {definition.name}", this);
            OnAbilityGranted?.Invoke(instance);

            return instance;
        }

        public void RemoveAbility(AbilityDefinition definition)
        {
            if (definition == null) return;

            if (_grantedAbilities.TryGetValue(definition, out var instance))
            {
                if (instance.IsActive)
                {
                    EndAbility(instance);
                }

                _grantedAbilities.Remove(definition);
                OnAbilityRemoved?.Invoke(instance);
            }
        }

        public bool HasAbility(AbilityDefinition definition)
        {
            return definition != null && _grantedAbilities.ContainsKey(definition);
        }

        public AbilityInstance GetAbilityInstance(AbilityDefinition definition)
        {
            if (definition == null) return null;
            _grantedAbilities.TryGetValue(definition, out var instance);
            return instance;
        }

        public bool CanActivateAbility(AbilityDefinition definition)
        {
            if (definition == null) return false;

            if (!_grantedAbilities.TryGetValue(definition, out var instance))
                return false;

            return CanActivateAbility(instance);
        }

        public bool CanActivateAbility(AbilityInstance instance)
        {
            if (instance == null) return false;

            var def = instance.Definition;

            // Check cooldown
            if (instance.IsOnCooldown) return false;

            // Check if already active (for non-channeled abilities)
            if (instance.IsActive && !def.IsChanneled) return false;

            // Check cost
            if (def.CostAttribute != null && def.CostAmount > 0)
            {
                float currentValue = GetAttributeValue(def.CostAttribute);
                if (currentValue < def.CostAmount) return false;
            }

            // Check required tags
            if (def.ActivationRequiredTags.Count > 0)
            {
                if (!_ownedTags.HasAllTags(def.ActivationRequiredTags))
                    return false;
            }

            // Check blocked tags
            if (def.ActivationBlockedTags.Count > 0)
            {
                if (_ownedTags.HasAnyTag(def.ActivationBlockedTags))
                    return false;
            }

            // Check behavior-specific conditions
            if (def.Behavior != null && !def.Behavior.CanActivate(instance, this))
                return false;

            return true;
        }

        public bool TryActivateAbility(AbilityDefinition definition)
        {
            if (!_grantedAbilities.TryGetValue(definition, out var instance))
                return false;

            return TryActivateAbility(instance);
        }

        public bool TryActivateAbility(AbilityInstance instance)
        {
            if (!CanActivateAbility(instance)) return false;

            var def = instance.Definition;

            // Pay cost
            if (def.CostAttribute != null && def.CostAmount > 0)
            {
                var attribute = GetAttribute(def.CostAttribute);
                if (attribute != null)
                {
                    attribute.BaseValue -= def.CostAmount;
                }
            }

            // Activate
            instance.Activate();

            // Grant activation tags
            foreach (var tag in def.ActivationGrantedTags)
            {
                _ownedTags.AddTag(tag);
            }

            // Apply self effects
            foreach (var effectDef in def.ApplyToSelfEffects)
            {
                ApplyEffectToSelf(effectDef, instance);
            }

            // Execute behavior
            def.Behavior?.OnActivate(instance, this);

            if (_debugLogAbilities)
                Debug.Log($"[GAS] {name} | Ability Activated: {def.name}", this);
            OnAbilityActivated?.Invoke(instance);

            // For non-channeled abilities, end immediately after activation
            if (!def.IsChanneled)
            {
                EndAbility(instance);
            }

            return true;
        }

        public void EndAbility(AbilityInstance instance)
        {
            if (instance == null || !instance.IsActive) return;

            var def = instance.Definition;

            // Call behavior end
            def.Behavior?.OnEnd(instance, this);

            // Remove activation tags
            foreach (var tag in def.ActivationGrantedTags)
            {
                _ownedTags.RemoveTag(tag);
            }

            // Start cooldown
            instance.StartCooldown();
            instance.Deactivate();

            // Add cooldown tags
            foreach (var tag in def.CooldownTags)
            {
                _ownedTags.AddTag(tag);
            }

            if (_debugLogAbilities)
                Debug.Log($"[GAS] {name} | Ability Ended: {def.name} (CD: {def.Cooldown}s)", this);
            OnAbilityEnded?.Invoke(instance);
        }

        public void CancelAbility(AbilityInstance instance)
        {
            EndAbility(instance);
        }

        private void UpdateAbilities(float deltaTime)
        {
            foreach (var kvp in _grantedAbilities)
            {
                var instance = kvp.Value;
                var def = instance.Definition;
                bool wasOnCooldown = instance.IsOnCooldown;

                instance.Tick(deltaTime);

                // Remove cooldown tags when cooldown ends
                if (wasOnCooldown && !instance.IsOnCooldown)
                {
                    foreach (var tag in def.CooldownTags)
                    {
                        _ownedTags.RemoveTag(tag);
                    }
                }

                // Tick channeled abilities
                if (instance.IsActive && def.IsChanneled)
                {
                    def.Behavior?.OnTick(instance, this, deltaTime);

                    // Check channel duration
                    if (def.ChannelDuration > 0 && instance.ActiveDuration >= def.ChannelDuration)
                    {
                        EndAbility(instance);
                    }
                }
            }
        }

        #endregion

        #region Tags

        public void AddTag(GameplayTag tag)
        {
            _ownedTags.AddTag(tag);
        }

        public void RemoveTag(GameplayTag tag)
        {
            _ownedTags.RemoveTag(tag);
        }

        public bool HasTag(GameplayTag tag)
        {
            return _ownedTags.HasTag(tag);
        }

        #endregion
    }
}
