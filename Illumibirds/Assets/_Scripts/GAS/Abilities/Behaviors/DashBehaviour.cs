using System.Collections;
using GAS.Abilities;
using GAS.Core;
using UnityEngine;

namespace GAS
{
    public class DashBehaviour : IAbilityBehavior
    {
        bool IAbilityBehavior.CanActivate(AbilityInstance ability, AbilitySystemComponent owner)
        {
            return true;
        }

        void IAbilityBehavior.OnActivate(AbilityInstance ability, AbilitySystemComponent owner)
        {
            Rigidbody2D rb = owner.GetComponent<Rigidbody2D>();
            Vector2 direction = owner.transform.right.normalized;

            Debug.Log($"Dashing to: {direction}");

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
}
