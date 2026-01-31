using GAS.Attributes;
using GAS.Core;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] AttributeDefinition _healthAttr;
    [SerializeField] AttributeModifier healthAttrModifier;
    public void Initiate(float moveSpeed, float timeToLive, Vector2 direction)
    {
        Debug.LogWarning("MUST BE IMPLEMENTED");
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<AbilitySystemComponent>(out AbilitySystemComponent _asc))
        {
            //DO DAMAGE HERE
            Debug.Log($"Projectile hit: {collision.name}");
            _asc.GetAttribute(_healthAttr).AddModifier(healthAttrModifier);
        }
    }
}
