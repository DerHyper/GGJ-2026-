using System.Collections.Generic;
using GAS.Core;
using GAS.Effects;
using UnityEngine;

namespace GAS.Pickups
{
    /// <summary>
    /// World pickup that applies effects on collision.
    /// Use for health pickups, buffs, damage zones, etc.
    /// </summary>
    public class EffectPickup : MonoBehaviour
    {
        [Header("Effects to Apply")]
        [Tooltip("Effects applied when picked up")]
        [SerializeField]
        private List<GameplayEffectDefinition> _effectsToApply = new();

        [Header("Settings")]
        [Tooltip("Destroy this object after pickup")]
        [SerializeField]
        private bool _destroyOnPickup = true;

        [Tooltip("Time before respawning (0 = no respawn, only if not destroyed)")]
        [SerializeField]
        private float _respawnTime = 0f;

        [Tooltip("Can be picked up multiple times (for damage zones)")]
        [SerializeField]
        private bool _allowMultiplePickups = false;

        [Header("Visual")]
        [Tooltip("Object to hide when picked up (optional)")]
        [SerializeField]
        private GameObject _visualRoot;

        private bool _isAvailable = true;
        private HashSet<AbilitySystemComponent> _pickedUpBy = new();

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryPickup(other.gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            TryPickup(other.gameObject);
        }

        private void TryPickup(GameObject picker)
        {
            if (!_isAvailable) return;

            var asc = picker.GetComponent<AbilitySystemComponent>();
            if (asc == null) return;

            // Check if already picked up by this entity
            if (!_allowMultiplePickups && _pickedUpBy.Contains(asc)) return;

            // Apply all effects
            foreach (var effect in _effectsToApply)
            {
                if (effect != null)
                {
                    asc.ApplyEffectToSelf(effect, this);
                }
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
            else if (!_allowMultiplePickups)
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
            {
                _visualRoot.SetActive(active);
            }
        }

#if UNITY_EDITOR
        [Header("Gizmo")]
        [SerializeField] private Color _gizmoColor = new Color(0.2f, 1f, 0.2f, 0.5f);
        [SerializeField] private float _gizmoRadius = 0.5f;

        private void OnDrawGizmos()
        {
            Gizmos.color = _gizmoColor;
            Gizmos.DrawWireSphere(transform.position, _gizmoRadius);

            // Draw icon
            Gizmos.DrawIcon(transform.position, "d_Prefab Icon", true);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _gizmoColor;
            Gizmos.DrawSphere(transform.position, _gizmoRadius);

            // Draw collider bounds if present
            var col2D = GetComponent<Collider2D>();
            if (col2D != null)
            {
                Gizmos.color = new Color(_gizmoColor.r, _gizmoColor.g, _gizmoColor.b, 0.3f);
                Gizmos.DrawCube(col2D.bounds.center, col2D.bounds.size);
            }

            // Draw effect names
            if (_effectsToApply != null && _effectsToApply.Count > 0)
            {
                var label = "";
                foreach (var effect in _effectsToApply)
                {
                    if (effect != null)
                        label += effect.name.Replace("Effect_", "") + "\n";
                }
                UnityEditor.Handles.Label(transform.position + Vector3.up * 0.8f, label.TrimEnd());
            }
        }
#endif
    }
}
