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

        public bool piercingBullet = false;

        public LayerMask hitLayer;

        public void OnActivate(AbilityInstance ability, AbilitySystemComponent owner)
        {
            if (ProjectilePrefab == null) return;

            var spawnPos =  owner.transform.GetComponentInChildren<HitboxParentMarker>().transform.position;
            Vector2 dir = spawnPos - owner.transform.position;
            dir.Normalize();


            Projectile projectile = Object.Instantiate(ProjectilePrefab, spawnPos, owner.transform.rotation).GetComponent<Projectile>();
            projectile.Initiate(Speed, Lifetime, dir, ability, owner, hitLayer, piercingBullet);

        }

        public void OnTick(AbilityInstance ability, AbilitySystemComponent owner, float deltaTime) { }

        public void OnEnd(AbilityInstance ability, AbilitySystemComponent owner) { }

        public bool CanActivate(AbilityInstance ability, AbilitySystemComponent owner) => true;
    }
}
