using System.Collections.Generic;
using System.Linq;
using GAS.Abilities;
using GAS.Core;
using GAS.Tags;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class MeleeHitBox : MonoBehaviour
{
    AbilitySystemComponent owner;
    AbilityInstance ability;
    LayerMask hitLayer;
    [SerializeField] GameplayTag dodgeTag;

    bool isActive = false;
    List<AbilitySystemComponent> alreadyHitTargets = new();

    public void Initiate(float timeToLive, AbilityInstance _ability, AbilitySystemComponent _owner, LayerMask _hitLayer)
    {
        ability = _ability;
        owner = _owner;
        hitLayer = _hitLayer;

        isActive = true;

        alreadyHitTargets.Clear();

        if (timeToLive > 0f)
        {
            Destroy(gameObject, timeToLive);
        }
        else
        {
            Debug.LogWarning("Hitbox will be active forever - check TimeToLive");
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive) return;

        if (!((hitLayer.value & (1 << collision.gameObject.layer)) != 0))
        {
            return;
        }

        if (collision.TryGetComponent<AbilitySystemComponent>(out AbilitySystemComponent _asc))
        {
            // if(collision.TryGetComponent<PlayerController>(out PlayerController player))
            // {
            //     if(player.is)    
            // }

            if (alreadyHitTargets.Contains(_asc)) return;


            alreadyHitTargets.Add(_asc);

            if (!_asc.OwnedTags.Tags.Contains(dodgeTag))
            {
                ApplyEffectsToTarget(owner, _asc);
                Debug.Log($"Damage dealt to {_asc.transform.name}");   
            }
            else
            {
                Debug.Log($"NO Damage dealt to {_asc.transform.name} because OF DODGE");   
            }

        }
    }


    private void ApplyEffectsToTarget(AbilitySystemComponent owner, AbilitySystemComponent target)
    {
        foreach (var effect in ability.Definition.ApplyToTargetEffects)
        {
            owner.ApplyEffectToTarget(effect, target);
        }
    }
}