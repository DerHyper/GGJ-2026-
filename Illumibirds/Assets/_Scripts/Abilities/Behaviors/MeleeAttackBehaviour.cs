using UnityEngine;
using GAS.Abilities;
using GAS.Core;
using System;

[System.Serializable]
public class MeleeAttackBehaviour : IAbilityBehavior
{
    [SerializeField] float Lifetime = 0.1f;
    // [SerializeField] bool SingleHit = true;
    [SerializeField] MeleeHitBox hitBoxPrefab;
    public LayerMask hitLayer;
    // [SerializeField] Vector3 SpawnOffset = Vector3.zero;
    bool IAbilityBehavior.CanActivate(AbilityInstance ability, AbilitySystemComponent owner)
    {
        return true;
    }

    void IAbilityBehavior.OnActivate(AbilityInstance ability, AbilitySystemComponent owner)
    {
        Debug.Log("MELEE");
        if (hitBoxPrefab == null) return;

        //   owner.transform.position + owner.transform.TransformDirection(SpawnOffset);
        var spawnPos = owner.transform.GetComponentInChildren<HitboxParentMarker>().transform.position;


        MeleeHitBox hitbox = UnityEngine.Object.Instantiate(hitBoxPrefab, spawnPos, owner.transform.rotation).GetComponent<MeleeHitBox>();

        hitbox.Initiate(Lifetime, ability, owner, hitLayer);
    }

    void IAbilityBehavior.OnEnd(AbilityInstance ability, AbilitySystemComponent owner)
    {
        return;
    }

    void IAbilityBehavior.OnTick(AbilityInstance ability, AbilitySystemComponent owner, float deltaTime)
    {
        return;
    }

}