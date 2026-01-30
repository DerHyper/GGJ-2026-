using System;
using System.Collections.Generic;
using UnityEngine;

namespace GAS.Attributes
{
    /// <summary>
    /// Runtime instance of a single attribute with modifiers.
    /// </summary>
    [Serializable]
    public class Attribute
    {
        [SerializeField]
        private AttributeDefinition _definition;

        [SerializeField]
        private float _baseValue;

        private float _currentValue;
        private readonly List<AttributeModifier> _modifiers = new();
        private bool _isDirty = true;

        public AttributeDefinition Definition => _definition;
        public float BaseValue
        {
            get => _baseValue;
            set
            {
                if (Mathf.Approximately(_baseValue, value)) return;
                _baseValue = value;
                _isDirty = true;
                RecalculateIfDirty();
            }
        }

        public float CurrentValue
        {
            get
            {
                RecalculateIfDirty();
                return _currentValue;
            }
        }

        public IReadOnlyList<AttributeModifier> Modifiers => _modifiers;

        public event Action<Attribute, float, float> OnValueChanged; // attribute, oldValue, newValue

        public Attribute(AttributeDefinition definition, float baseValue)
        {
            _definition = definition;
            _baseValue = baseValue;
            _isDirty = true;
            RecalculateIfDirty();
        }

        public void AddModifier(AttributeModifier modifier)
        {
            _modifiers.Add(modifier);
            _isDirty = true;
            RecalculateIfDirty();
        }

        public void RemoveModifier(AttributeModifier modifier)
        {
            _modifiers.Remove(modifier);
            _isDirty = true;
            RecalculateIfDirty();
        }

        public void RemoveModifiersFromSource(object source)
        {
            _modifiers.RemoveAll(m => m.Source == source);
            _isDirty = true;
            RecalculateIfDirty();
        }

        public void ClearModifiers()
        {
            _modifiers.Clear();
            _isDirty = true;
            RecalculateIfDirty();
        }

        private void RecalculateIfDirty()
        {
            if (!_isDirty) return;
            _isDirty = false;

            float oldValue = _currentValue;
            float newValue = CalculateValue();

            if (!Mathf.Approximately(oldValue, newValue))
            {
                _currentValue = newValue;
                OnValueChanged?.Invoke(this, oldValue, newValue);
            }
        }

        private float CalculateValue()
        {
            // Check for override first
            foreach (var mod in _modifiers)
            {
                if (mod.Operation == ModifierOperation.Override)
                {
                    return Clamp(mod.Value);
                }
            }

            // Calculate: (Base + SumOfAdds) * ProductOfMultipliers
            float addSum = 0f;
            float multiplyProduct = 1f;

            foreach (var mod in _modifiers)
            {
                switch (mod.Operation)
                {
                    case ModifierOperation.Add:
                        addSum += mod.Value;
                        break;
                    case ModifierOperation.Multiply:
                        multiplyProduct *= mod.Value;
                        break;
                }
            }

            float result = (_baseValue + addSum) * multiplyProduct;
            return Clamp(result);
        }

        private float Clamp(float value)
        {
            if (_definition == null) return value;
            return Mathf.Clamp(value, _definition.MinValue, _definition.MaxValue);
        }
    }
}
