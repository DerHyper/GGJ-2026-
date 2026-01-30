using GAS.Core;
using UnityEngine;

namespace GAS.Abilities.Behaviors
{
    /// <summary>
    /// Example ability behavior that affects all targets in a radius.
    /// </summary>
    [System.Serializable]
    public class AreaOfEffectBehavior : IAbilityBehavior
    {
        public float Radius = 5f;
        public GameObject AOEVFXPrefab;
        public LayerMask TargetLayers = ~0;
        public bool IncludeSelf = false;

        public void OnActivate(AbilityInstance ability, AbilitySystemComponent owner)
        {
            // Spawn AOE VFX
            if (AOEVFXPrefab != null)
            {
                var vfx = Object.Instantiate(AOEVFXPrefab, owner.transform.position, Quaternion.identity);
                vfx.transform.localScale = Vector3.one * Radius * 2f;
            }

            // Find all targets in radius
            var colliders = Physics2D.OverlapCircleAll(owner.transform.position, Radius, TargetLayers);

            foreach (var col in colliders)
            {
                if (col.TryGetComponent<AbilitySystemComponent>(out var target))
                {
                    if (!IncludeSelf && target == owner) continue;

                    foreach (var effect in ability.Definition.ApplyToTargetEffects)
                    {
                        owner.ApplyEffectToTarget(effect, target);
                    }
                }
            }
        }

        public void OnTick(AbilityInstance ability, AbilitySystemComponent owner, float deltaTime) { }

        public void OnEnd(AbilityInstance ability, AbilitySystemComponent owner) { }

        public bool CanActivate(AbilityInstance ability, AbilitySystemComponent owner) => true;
    }
}
