using System;
using System.Collections.Generic;
using UnityEngine;

namespace GAS.Attributes
{
    /// <summary>
    /// Runtime component managing all attributes for an entity.
    /// </summary>
    [Serializable]
    public class AttributeSet
    {
        private readonly Dictionary<AttributeDefinition, Attribute> _attributes = new();

        public event Action<Attribute, float, float> OnAttributeChanged;

        public IReadOnlyDictionary<AttributeDefinition, Attribute> Attributes => _attributes;

        public void Initialize(AttributeSetDefinition definition)
        {
            if (definition == null) return;

            foreach (var attrInit in definition.Attributes)
            {
                if (attrInit.Attribute == null) continue;
                AddAttribute(attrInit.Attribute, attrInit.InitialValue);
            }
        }

        public Attribute AddAttribute(AttributeDefinition definition, float baseValue)
        {
            if (definition == null) return null;

            if (_attributes.TryGetValue(definition, out var existing))
            {
                return existing;
            }

            var attribute = new Attribute(definition, baseValue);
            attribute.OnValueChanged += HandleAttributeChanged;
            _attributes[definition] = attribute;
            return attribute;
        }

        public Attribute GetAttribute(AttributeDefinition definition)
        {
            if (definition == null) return null;
            _attributes.TryGetValue(definition, out var attribute);
            return attribute;
        }

        public float GetAttributeValue(AttributeDefinition definition)
        {
            var attribute = GetAttribute(definition);
            return attribute?.CurrentValue ?? 0f;
        }

        public float GetAttributeBaseValue(AttributeDefinition definition)
        {
            var attribute = GetAttribute(definition);
            return attribute?.BaseValue ?? 0f;
        }

        public void SetAttributeBaseValue(AttributeDefinition definition, float value)
        {
            var attribute = GetAttribute(definition);
            if (attribute != null)
            {
                attribute.BaseValue = value;
            }
        }

        public void AddModifier(AttributeDefinition definition, AttributeModifier modifier)
        {
            var attribute = GetAttribute(definition);
            attribute?.AddModifier(modifier);
        }

        public void RemoveModifier(AttributeDefinition definition, AttributeModifier modifier)
        {
            var attribute = GetAttribute(definition);
            attribute?.RemoveModifier(modifier);
        }

        public void RemoveModifiersFromSource(object source)
        {
            foreach (var attribute in _attributes.Values)
            {
                attribute.RemoveModifiersFromSource(source);
            }
        }

        public bool HasAttribute(AttributeDefinition definition)
        {
            return definition != null && _attributes.ContainsKey(definition);
        }

        private void HandleAttributeChanged(Attribute attribute, float oldValue, float newValue)
        {
            OnAttributeChanged?.Invoke(attribute, oldValue, newValue);
        }
    }
}
