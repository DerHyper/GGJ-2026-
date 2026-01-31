using System.Collections;
using GAS.Abilities;
using GAS.Core;
using UnityEngine;

namespace GAS
{
    [System.Serializable]
    public class DashBehaviour : IAbilityBehavior
    {
        [SerializeField] float dashDuration, dashPower;
        bool IAbilityBehavior.CanActivate(AbilityInstance ability, AbilitySystemComponent owner)
        {
            return true;
        }

        void IAbilityBehavior.OnActivate(AbilityInstance ability, AbilitySystemComponent owner)
        {
            Rigidbody2D rb = owner.GetComponent<Rigidbody2D>();
            Vector2 direction = owner.transform.right.normalized;

            Debug.Log($"Dashing to: {direction}");
            
            owner.StartCoroutine(DashCoroutine(rb,direction, dashDuration, dashPower));

        }


        IEnumerator DashCoroutine(Rigidbody2D _rb, Vector2 direction, float duration, float power)
        {
            _rb.linearVelocity = direction * power;

            if(_rb.TryGetComponent<PlayerController>(out PlayerController player))
            {
                player.ToggleControl(false);
            }

            yield return new WaitForSeconds(duration);

            _rb.linearVelocity = Vector2.zero;

            if(_rb.TryGetComponent<PlayerController>(out player))
            {
                player.ToggleControl(true);
            }


        }

        void IAbilityBehavior.OnEnd(AbilityInstance ability, AbilitySystemComponent owner)
        {
            //  if(owner.TryGetComponent<PlayerController>(out PlayerController player))
            // {
            //     player.ToggleControl(true);
            // }

            // Rigidbody2D rb = owner.GetComponent<Rigidbody2D>();
            // rb.linearVelocity = Vector2.zero;

            return;
        }

        void IAbilityBehavior.OnTick(AbilityInstance ability, AbilitySystemComponent owner, float deltaTime)
        {
            return;
        }
    }
}
