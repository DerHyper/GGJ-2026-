using System.Collections.Generic;
using GAS.Abilities;
using GAS.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Examples.Player
{
    /// <summary>
    /// Bridges input to ability activation on the player's AbilitySystemComponent.
    /// </summary>
    public class PlayerAbilityInput : MonoBehaviour
    {
        [System.Serializable]
        public struct AbilityBinding
        {
            public InputAction InputAction;
            public AbilityDefinition Ability;
        }

        [SerializeField]
        private AbilitySystemComponent _abilitySystemComponent;

        [SerializeField]
        private List<AbilityBinding> _abilityBindings = new();

        private void Awake()
        {
            if (_abilitySystemComponent == null)
            {
                _abilitySystemComponent = GetComponent<AbilitySystemComponent>();
            }
        }

        private void OnEnable()
        {
            foreach (var binding in _abilityBindings)
            {
                if (binding.InputAction != null)
                {
                    binding.InputAction.Enable();
                    binding.InputAction.performed += OnAbilityInputPerformed;
                    binding.InputAction.canceled += OnAbilityInputCanceled;
                }
            }
        }

        private void OnDisable()
        {
            foreach (var binding in _abilityBindings)
            {
                if (binding.InputAction != null)
                {
                    binding.InputAction.performed -= OnAbilityInputPerformed;
                    binding.InputAction.canceled -= OnAbilityInputCanceled;
                    binding.InputAction.Disable();
                }
            }
        }

        private void OnAbilityInputPerformed(InputAction.CallbackContext context)
        {
            var ability = GetAbilityForAction(context.action);
            if (ability != null && _abilitySystemComponent != null)
            {
                _abilitySystemComponent.TryActivateAbility(ability);
            }
        }

        private void OnAbilityInputCanceled(InputAction.CallbackContext context)
        {
            var ability = GetAbilityForAction(context.action);
            if (ability != null && ability.IsChanneled && _abilitySystemComponent != null)
            {
                var instance = _abilitySystemComponent.GetAbilityInstance(ability);
                if (instance != null && instance.IsActive)
                {
                    _abilitySystemComponent.CancelAbility(instance);
                }
            }
        }

        private AbilityDefinition GetAbilityForAction(InputAction action)
        {
            foreach (var binding in _abilityBindings)
            {
                if (binding.InputAction == action)
                {
                    return binding.Ability;
                }
            }
            return null;
        }

        public void BindAbility(InputAction action, AbilityDefinition ability)
        {
            // Remove existing binding for this action
            _abilityBindings.RemoveAll(b => b.InputAction == action);

            var binding = new AbilityBinding
            {
                InputAction = action,
                Ability = ability
            };
            _abilityBindings.Add(binding);

            action.Enable();
            action.performed += OnAbilityInputPerformed;
            action.canceled += OnAbilityInputCanceled;
        }

        public void UnbindAbility(InputAction action)
        {
            action.performed -= OnAbilityInputPerformed;
            action.canceled -= OnAbilityInputCanceled;
            _abilityBindings.RemoveAll(b => b.InputAction == action);
        }
    }
}
