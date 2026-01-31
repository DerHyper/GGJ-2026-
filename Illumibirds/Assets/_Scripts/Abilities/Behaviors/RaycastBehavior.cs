using GAS.Core;
using UnityEngine;

namespace GAS.Abilities.Behaviors
{
    /// <summary>
    /// Example ability behavior that performs a raycast attack (like a light beam).
    /// </summary>
    [System.Serializable]
    public class RaycastBehavior : IAbilityBehavior
    {
        public float Range = 10f;
        public GameObject BeamVFXPrefab;
        public LayerMask TargetLayers = ~0;
        public bool Is2D = true;

        public void OnActivate(AbilityInstance ability, AbilitySystemComponent owner)
        {
            // Spawn beam VFX
            if (BeamVFXPrefab != null)
            {
                Object.Instantiate(BeamVFXPrefab, owner.transform.position, owner.transform.rotation);
            }

            // Perform raycast
            if (Is2D)
            {
                PerformRaycast2D(ability, owner);
            }
            else
            {
                PerformRaycast3D(ability, owner);
            }
        }

        private void PerformRaycast2D(AbilityInstance ability, AbilitySystemComponent owner)
        {
            var hits = Physics2D.RaycastAll(owner.transform.position, owner.transform.right, Range, TargetLayers);

            foreach (var hit in hits)
            {
                if (hit.collider.TryGetComponent<AbilitySystemComponent>(out var target) && target != owner)
                {
                    ApplyEffectsToTarget(ability, owner, target);
                }
            }
        }

        private void PerformRaycast3D(AbilityInstance ability, AbilitySystemComponent owner)
        {
            var hits = Physics.RaycastAll(owner.transform.position, owner.transform.forward, Range, TargetLayers);

            foreach (var hit in hits)
            {
                if (hit.collider.TryGetComponent<AbilitySystemComponent>(out var target) && target != owner)
                {
                    ApplyEffectsToTarget(ability, owner, target);
                }
            }
        }

        private void ApplyEffectsToTarget(AbilityInstance ability, AbilitySystemComponent owner, AbilitySystemComponent target)
        {
            foreach (var effect in ability.Definition.ApplyToTargetEffects)
            {
                owner.ApplyEffectToTarget(effect, target);
            }
        }

        public void OnTick(AbilityInstance ability, AbilitySystemComponent owner, float deltaTime) { }

        public void OnEnd(AbilityInstance ability, AbilitySystemComponent owner) { }

        public bool CanActivate(AbilityInstance ability, AbilitySystemComponent owner) => true;
    }
}
