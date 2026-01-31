using System.Linq;
using GAS.Abilities;
using GAS.Attributes;
using GAS.Core;
using GAS.Tags;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    AbilitySystemComponent owner;
    AbilityInstance ability;

    LayerMask hitLayer;
    [SerializeField] GameplayTag dodgeTag;

    bool piercing = false;

    public void Initiate(float moveSpeed, float timeToLive, Vector2 direction, AbilityInstance _ability, AbilitySystemComponent _owner, LayerMask _hitLayer, bool piercing)
    {
        ability = _ability;
        owner = _owner;
        hitLayer = _hitLayer;


        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * moveSpeed;
        }

        if (timeToLive > 0f)
        {
            Destroy(gameObject, timeToLive);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        //CHECK FOR HITLAYER
        if (!((hitLayer.value & (1 << collision.gameObject.layer)) != 0))
        {
            return;
        }

        if (collision.TryGetComponent<AbilitySystemComponent>(out AbilitySystemComponent _asc))
        {

            if (!_asc.OwnedTags.Tags.Contains(dodgeTag))
            {
                ApplyEffectsToTarget(owner, _asc);
                Debug.Log($"Damage dealt to {_asc.transform.name}");   
            }
            else
            {
                Debug.Log($"NO Damage dealt to {_asc.transform.name} because OF DODGE");   
            }
            
            if(!piercing) Destroy(gameObject);
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
