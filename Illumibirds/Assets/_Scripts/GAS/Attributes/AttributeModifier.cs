using System;

namespace GAS.Attributes
{
    public enum ModifierOperation
    {
        Add,
        Multiply,
        Override
    }

    /// <summary>
    /// A modifier that affects an attribute's value.
    /// </summary>
    [Serializable]
    public struct AttributeModifier
    {
        public ModifierOperation Operation;
        public float Value;
        public object Source; // Reference to what applied this modifier (for removal)

        public AttributeModifier(ModifierOperation operation, float value, object source = null)
        {
            Operation = operation;
            Value = value;
            Source = source;
        }
    }
}
