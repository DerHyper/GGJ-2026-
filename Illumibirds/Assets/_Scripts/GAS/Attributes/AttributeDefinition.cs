using UnityEngine;

namespace GAS.Attributes
{
    /// <summary>
    /// ScriptableObject defining an attribute type (e.g., Health, Damage, AttackSpeed).
    /// </summary>
    [CreateAssetMenu(menuName = "GAS/Attribute Definition", fileName = "Attr_")]
    public class AttributeDefinition : ScriptableObject
    {
        [Tooltip("Display name for the attribute")]
        public string AttributeName;

        [Tooltip("Minimum allowed value for this attribute")]
        public float MinValue = 0f;

        [Tooltip("Maximum allowed value for this attribute")]
        public float MaxValue = float.MaxValue;
    }
}
