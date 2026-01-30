using System.Collections.Generic;
using GAS.Attributes;
using UnityEngine;

namespace GAS.Effects
{
    /// <summary>
    /// Runtime instance of an active gameplay effect.
    /// </summary>
    public class GameplayEffectInstance
    {
        public GameplayEffectDefinition Definition { get; }
        public object Source { get; }
        public float StartTime { get; }
        public float RemainingDuration { get; private set; }
        public int StackCount { get; private set; } = 1;
        public bool IsExpired { get; private set; }

        private float _periodicTimer;
        private readonly List<AttributeModifier> _appliedModifiers = new();

        public IReadOnlyList<AttributeModifier> AppliedModifiers => _appliedModifiers;

        public GameplayEffectInstance(GameplayEffectDefinition definition, object source)
        {
            Definition = definition;
            Source = source;
            StartTime = Time.time;
            RemainingDuration = definition.Duration;
            _periodicTimer = 0f;
        }

        public void Tick(float deltaTime, System.Action onPeriodElapsed)
        {
            if (IsExpired) return;

            // Handle duration
            if (Definition.DurationType == EffectDurationType.Duration)
            {
                RemainingDuration -= deltaTime;
                if (RemainingDuration <= 0f)
                {
                    IsExpired = true;
                    return;
                }
            }

            // Handle periodic effects
            if (Definition.IsPeriodic && Definition.Period > 0f)
            {
                _periodicTimer += deltaTime;
                while (_periodicTimer >= Definition.Period)
                {
                    _periodicTimer -= Definition.Period;
                    onPeriodElapsed?.Invoke();
                }
            }
        }

        public bool TryAddStack()
        {
            if (!Definition.CanStack) return false;
            if (Definition.MaxStacks > 0 && StackCount >= Definition.MaxStacks) return false;

            StackCount++;

            // Refresh duration on stack
            if (Definition.DurationType == EffectDurationType.Duration)
            {
                RemainingDuration = Definition.Duration;
            }

            return true;
        }

        public void RemoveStack()
        {
            StackCount--;
            if (StackCount <= 0)
            {
                IsExpired = true;
            }
        }

        public void TrackModifier(AttributeModifier modifier)
        {
            _appliedModifiers.Add(modifier);
        }

        public void Expire()
        {
            IsExpired = true;
        }
    }
}
