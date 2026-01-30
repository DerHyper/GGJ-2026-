using GAS.Abilities;
using GAS.Core;
using UnityEngine;

namespace GAS.Pickups
{
    /// <summary>
    /// World pickup that grants an ability on collision.
    /// </summary>
    public class AbilityPickup : MonoBehaviour
    {
        [Header("Ability")]
        [SerializeField]
        private AbilityDefinition _abilityToGrant;

        [Header("Settings")]
        [SerializeField]
        private bool _destroyOnPickup = true;

        [SerializeField]
        private float _respawnTime = 0f;

        [Header("Visual")]
        [SerializeField]
        private GameObject _visualRoot;

        private bool _isAvailable = true;

        public AbilityDefinition AbilityToGrant => _abilityToGrant;

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

            // Grant the ability
            asc.GrantAbility(_abilityToGrant);

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
                if (_visualRoot != null)
                {
                    _visualRoot.SetActive(false);
                }
            }
        }

        private System.Collections.IEnumerator RespawnAfterDelay()
        {
            _isAvailable = false;
            if (_visualRoot != null)
            {
                _visualRoot.SetActive(false);
            }

            yield return new WaitForSeconds(_respawnTime);

            _isAvailable = true;
            if (_visualRoot != null)
            {
                _visualRoot.SetActive(true);
            }
        }

#if UNITY_EDITOR
        [Header("Gizmo")]
        [SerializeField] private Color _gizmoColor = new Color(1f, 0.8f, 0.2f, 0.5f);
        [SerializeField] private float _gizmoRadius = 0.5f;

        private void OnDrawGizmos()
        {
            Gizmos.color = _gizmoColor;
            Gizmos.DrawWireSphere(transform.position, _gizmoRadius);

            // Draw star icon for ability
            Gizmos.DrawIcon(transform.position, "d_Favorite Icon", true);
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

            // Draw ability name
            if (_abilityToGrant != null)
            {
                var label = "⚔️ " + _abilityToGrant.name.Replace("Ability_", "");
                UnityEditor.Handles.Label(transform.position + Vector3.up * 0.8f, label);
            }
        }
#endif
    }
}
