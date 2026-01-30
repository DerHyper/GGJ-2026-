using System;
using System.Collections.Generic;
using UnityEngine;

namespace GAS.Attributes
{
    /// <summary>
    /// ScriptableObject defining the initial attribute values for an entity.
    /// </summary>
    [CreateAssetMenu(menuName = "GAS/Attribute Set Definition", fileName = "AttributeSet_")]
    public class AttributeSetDefinition : ScriptableObject
    {
        [Serializable]
        public struct AttributeInitialValue
        {
            public AttributeDefinition Attribute;
            public float InitialValue;
        }

        [Tooltip("List of attributes and their initial values")]
        public List<AttributeInitialValue> Attributes = new();
    }
}
