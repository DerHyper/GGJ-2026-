using GAS.Attributes;
using GAS.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Examples.UI
{
    /// <summary>
    /// Simple health bar that reads from an AbilitySystemComponent.
    /// Works for both player and enemies.
    /// </summary>
    public class HealthBar : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("The entity to show health for. If null, will search on parent.")]
        [SerializeField] private AbilitySystemComponent _target;

        [Header("Attributes")]
        [SerializeField] private AttributeDefinition _healthAttr;
        [SerializeField] private AttributeDefinition _maxHealthAttr;

        [Header("UI Elements")]
        [SerializeField] private Image _fillImage;
        [SerializeField] private Slider _slider;

        [Header("Settings")]
        [SerializeField] private bool _hideWhenFull = false;
        [SerializeField] private Gradient _colorGradient;

        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();

            if (_target == null)
            {
                _target = GetComponentInParent<AbilitySystemComponent>();
            }
        }

        private void Update()
        {
            if (_target == null || _healthAttr == null) return;

            float health = _target.GetAttributeValue(_healthAttr);
            float maxHealth = _maxHealthAttr != null ? _target.GetAttributeValue(_maxHealthAttr) : 100f;
            float percent = maxHealth > 0 ? health / maxHealth : 0f;

            // Update fill
            if (_fillImage != null)
            {
                _fillImage.fillAmount = percent;

                if (_colorGradient != null)
                {
                    _fillImage.color = _colorGradient.Evaluate(percent);
                }
            }

            // Update slider
            if (_slider != null)
            {
                _slider.value = percent;
            }

            // Hide when full
            if (_hideWhenFull && _canvasGroup != null)
            {
                _canvasGroup.alpha = percent >= 1f ? 0f : 1f;
            }
        }
    }
}
