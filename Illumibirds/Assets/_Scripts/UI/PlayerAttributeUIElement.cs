using GAS.Attributes;
using UnityEngine;
using GAS.Core;
using UnityEngine.UI;

public class PlayerAttributeUIElement : MonoBehaviour
{
    [SerializeField] AttributeDefinition attrToDisplayDefinition, attrMaxDefinition;
    [SerializeField] Image fillImage;
    AbilitySystemComponent abilitySystem;

    Attribute maxValue;
    Attribute attrToDisplay;

    void Start()
    {
        Initiate();
    }

    void Initiate()
    {
        abilitySystem = FindFirstObjectByType<PlayerController>(FindObjectsInactive.Exclude).GetComponent<AbilitySystemComponent>();

        maxValue = abilitySystem.GetAttribute(attrMaxDefinition);
        attrToDisplay = abilitySystem.GetAttribute(attrToDisplayDefinition);

        attrToDisplay.OnValueChanged += UpdateUI;
        UpdateUI(attrToDisplay, 0 , abilitySystem.GetAttributeValue(attrToDisplayDefinition));
    }


    void UpdateUI(Attribute attr, float oldValue, float newValue)
    {
        float _fillAmount = newValue / maxValue.CurrentValue;
        fillImage.fillAmount = _fillAmount;
    }
}
