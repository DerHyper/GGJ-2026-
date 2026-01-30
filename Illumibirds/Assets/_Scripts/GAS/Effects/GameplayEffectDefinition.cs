using System;
using System.Collections.Generic;
using GAS.Attributes;
using GAS.Tags;
using UnityEngine;

namespace GAS.Effects
{
    /// <summary>
    /// ScriptableObject template for a gameplay effect.
    /// </summary>
    [CreateAssetMenu(menuName = "GAS/Gameplay Effect", fileName = "Effect_")]
    public class GameplayEffectDefinition : ScriptableObject
    {
        [Header("Duration")]
        [Tooltip("How long this effect lasts")]
        public EffectDurationType DurationType = EffectDurationType.Instant;

        [Tooltip("Duration in seconds (only used if DurationType is Duration)")]
        public float Duration = 0f;

        [Header("Modifiers")]
        [Tooltip("Attribute modifiers applied by this effect")]
        public List<EffectAttributeModifier> Modifiers = new();

        [Header("Tags")]
        [Tooltip("Tags granted while this effect is active")]
        public List<GameplayTag> GrantedTags = new();

        [Tooltip("Tags required for this effect to be applied")]
        public List<GameplayTag> ApplicationRequiredTags = new();

        [Tooltip("Tags that block this effect from being applied")]
        public List<GameplayTag> ApplicationBlockedTags = new();

        [Header("Stacking")]
        [Tooltip("Can this effect stack with itself?")]
        public bool CanStack = false;

        [Tooltip("Maximum number of stacks (0 = unlimited)")]
        public int MaxStacks = 0;

        [Header("Periodic")]
        [Tooltip("Apply the effect periodically (only for Duration/Infinite effects)")]
        public bool IsPeriodic = false;

        [Tooltip("Period interval in seconds")]
        public float Period = 1f;
    }

    [Serializable]
    public struct EffectAttributeModifier
    {
        public AttributeDefinition TargetAttribute;
        public ModifierOperation Operation;
        public float Magnitude;
    }
}
