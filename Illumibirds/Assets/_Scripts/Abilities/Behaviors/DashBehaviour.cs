using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GAS.Abilities;
using GAS.Core;
using GAS.Tags;
using UnityEditor.Callbacks;
using UnityEngine;

namespace GAS
{
    [System.Serializable]
    public class DashBehaviour : IAbilityBehavior
    {
        [SerializeField] float dashDuration, dashPower;
        [SerializeField] GameplayTag dodgeTag;

        bool IAbilityBehavior.CanActivate(AbilityInstance ability, AbilitySystemComponent owner)
        {
            return true;
        }

        void IAbilityBehavior.OnActivate(AbilityInstance ability, AbilitySystemComponent owner)
        {
            Rigidbody2D rb = owner.GetComponent<Rigidbody2D>();
            Vector2 direction = owner.transform.right.normalized;

            Debug.Log($"Dashing to: {direction}");

            owner.StartCoroutine(DashCoroutine(rb, direction, dashDuration, dashPower));

        }


        IEnumerator DashCoroutine(Rigidbody2D _rb, Vector2 direction, float duration, float power)
        {
            _rb.linearVelocity = direction * power;

            ToggleMovementIfIsPlayer(_rb, false);
            ToggleDodgeTagIfIsPlayer(_rb, true);

            yield return new WaitForSeconds(duration);

            _rb.linearVelocity = Vector2.zero;

            ToggleMovementIfIsPlayer(_rb, true);
            ToggleDodgeTagIfIsPlayer(_rb, false);


        }

        void ToggleMovementIfIsPlayer(Rigidbody2D _rb, bool active)
        {
            if (_rb.TryGetComponent<PlayerController>(out PlayerController player))
            {
                player.ToggleControl(active);
            }


        }

        void ToggleDodgeTagIfIsPlayer(Rigidbody2D _rb, bool active)
        {
            if (_rb.TryGetComponent<PlayerController>(out PlayerController player))
            {
                AbilitySystemComponent asc = _rb.GetComponent<AbilitySystemComponent>();
                if (active) asc.AddTag(dodgeTag);
                else if (asc.OwnedTags.Tags.Contains(dodgeTag))
                {
                    asc.RemoveTag(dodgeTag);
                }

                Debug.Log($"Dodging set to: {active}");
            }


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
