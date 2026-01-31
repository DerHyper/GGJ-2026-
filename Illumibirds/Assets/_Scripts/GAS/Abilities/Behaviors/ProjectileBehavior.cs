using GAS.Core;
using UnityEngine;

namespace GAS.Abilities.Behaviors
{
    /// <summary>
    /// Example ability behavior that spawns a projectile.
    /// </summary>
    [System.Serializable]
    public class ProjectileBehavior : IAbilityBehavior
    {   
        public GameObject ProjectilePrefab;
        public float Speed = 10f;
        public float Lifetime = 5f;
        public Vector3 SpawnOffset = Vector3.zero;

        public void OnActivate(AbilityInstance ability, AbilitySystemComponent owner)
        {
            if (ProjectilePrefab == null) return;

            var spawnPos = owner.transform.position + owner.transform.TransformDirection(SpawnOffset);
            var projectile = Object.Instantiate(ProjectilePrefab, spawnPos, owner.transform.rotation);
            

            var rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = owner.transform.right * Speed;
            }

            if (Lifetime > 0f)
            {
                Object.Destroy(projectile, Lifetime);
            }
        }

        public void OnTick(AbilityInstance ability, AbilitySystemComponent owner, float deltaTime) { }

        public void OnEnd(AbilityInstance ability, AbilitySystemComponent owner) { }

        public bool CanActivate(AbilityInstance ability, AbilitySystemComponent owner) => true;
    }
}
