using System.Collections.Generic;
using GAS.Abilities;
using GAS.Core;
using GAS.Effects;
using UnityEngine;

namespace GAS.Pickups
{
    public enum PickupType
    {
        Effect,
        Ability
    }

    /// <summary>
    /// Unified pickup component that can grant either effects or abilities.
    /// </summary>
    public class Pickup : MonoBehaviour
    {
        [SerializeField] private PickupType _pickupType = PickupType.Effect;

        [SerializeField] private List<GameplayEffectDefinition> _effects = new();
        [SerializeField] private AbilityDefinition _ability;

        [SerializeField] private bool _destroyOnPickup = true;
        [SerializeField] private float _respawnTime = 0f;

        [SerializeField] private GameObject _visualRoot;

        private bool _isAvailable = true;
        private HashSet<AbilitySystemComponent> _pickedUpBy = new();

        public PickupType Type => _pickupType;
        public List<GameplayEffectDefinition> Effects => _effects;
        public AbilityDefinition Ability => _ability;
        public bool DestroyOnPickup => _destroyOnPickup;
        public float RespawnTime => _respawnTime;

        private void OnTriggerEnter2D(Collider2D other) => TryPickup(other.gameObject);
        private void OnTriggerEnter(Collider other) => TryPickup(other.gameObject);

        private void TryPickup(GameObject picker)
        {
            if (!_isAvailable) return;

            var asc = picker.GetComponent<AbilitySystemComponent>();
            if (asc == null) return;

            if (_pickedUpBy.Contains(asc)) return;

            // Apply based on type
            if (_pickupType == PickupType.Effect)
            {
                foreach (var effect in _effects)
                {
                    if (effect != null)
                        asc.ApplyEffectToSelf(effect, this);
                }
            }
            else if (_pickupType == PickupType.Ability)
            {
                if (_ability != null)
                    asc.GrantAbility(_ability);
            }

            _pickedUpBy.Add(asc);

            if (_destroyOnPickup)
            {
                Destroy(gameObject);
            }
            else if (_respawnTime > 0f)
            {
                StartCoroutine(RespawnAfterDelay());
            }
            else
            {
                _isAvailable = false;
                SetVisualActive(false);
            }
        }

        private System.Collections.IEnumerator RespawnAfterDelay()
        {
            _isAvailable = false;
            SetVisualActive(false);

            yield return new WaitForSeconds(_respawnTime);

            _isAvailable = true;
            _pickedUpBy.Clear();
            SetVisualActive(true);
        }

        private void SetVisualActive(bool active)
        {
            if (_visualRoot != null)
                _visualRoot.SetActive(active);
        }

#if UNITY_EDITOR
        [Header("Gizmo")]
        [SerializeField] private float _gizmoRadius = 0.5f;

        private void OnDrawGizmos()
        {
            var color = _pickupType == PickupType.Effect
                ? new Color(0.2f, 1f, 0.2f, 0.5f)
                : new Color(1f, 0.8f, 0.2f, 0.5f);

            Gizmos.color = color;
            Gizmos.DrawWireSphere(transform.position, _gizmoRadius);
        }

        private void OnDrawGizmosSelected()
        {
            var color = _pickupType == PickupType.Effect
                ? new Color(0.2f, 1f, 0.2f, 0.5f)
                : new Color(1f, 0.8f, 0.2f, 0.5f);

            Gizmos.color = color;
            Gizmos.DrawSphere(transform.position, _gizmoRadius);

            // Draw label
            string label = _pickupType == PickupType.Effect
                ? "💊 " + (_effects.Count > 0 && _effects[0] != null ? _effects[0].name.Replace("Effect_", "") : "?")
                : "⚔️ " + (_ability != null ? _ability.name.Replace("Ability_", "") : "?");

            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.8f, label);
        }
#endif
    }
}
