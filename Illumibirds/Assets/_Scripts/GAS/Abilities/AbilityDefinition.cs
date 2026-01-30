using System.Collections.Generic;
using GAS.Attributes;
using GAS.Effects;
using GAS.Tags;
using UnityEngine;

namespace GAS.Abilities
{
    /// <summary>
    /// ScriptableObject template for an ability.
    /// </summary>
    [CreateAssetMenu(menuName = "GAS/Ability", fileName = "Ability_")]
    public class AbilityDefinition : ScriptableObject
    {
        [Header("Basic Info")]
        public string AbilityName;
        public Sprite Icon;

        [Header("Cost")]
        [Tooltip("Attribute consumed when using this ability")]
        public AttributeDefinition CostAttribute;

        [Tooltip("Amount of the attribute consumed")]
        public float CostAmount;

        [Header("Cooldown")]
        [Tooltip("Time in seconds before the ability can be used again")]
        public float Cooldown;

        [Header("Tags")]
        [Tooltip("Tags granted while this ability is active")]
        public List<GameplayTag> ActivationGrantedTags = new();

        [Tooltip("Tags required to activate this ability")]
        public List<GameplayTag> ActivationRequiredTags = new();

        [Tooltip("Tags that block activation of this ability")]
        public List<GameplayTag> ActivationBlockedTags = new();

        [Tooltip("Tags this ability grants to the owner while on cooldown")]
        public List<GameplayTag> CooldownTags = new();

        [Header("Effects")]
        [Tooltip("Effects applied to the owner when this ability activates")]
        public List<GameplayEffectDefinition> ApplyToSelfEffects = new();

        [Tooltip("Effects applied to the target when this ability activates")]
        public List<GameplayEffectDefinition> ApplyToTargetEffects = new();

        [Header("Behavior")]
        [Tooltip("The logic that executes when this ability is used")]
        [SerializeReference]
        public IAbilityBehavior Behavior;

        [Header("Settings")]
        [Tooltip("Is this a channeled ability that stays active over time?")]
        public bool IsChanneled;

        [Tooltip("Duration of the channel (0 = until manually cancelled)")]
        public float ChannelDuration;
    }
}
