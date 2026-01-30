using System.Collections.Generic;
using GAS.Core;
using GAS.Effects;
using UnityEngine;

namespace GAS.Pickups
{
    /// <summary>
    /// An area that continuously applies effects to entities inside it.
    /// Use for: fire, poison gas, healing zones, buff areas, etc.
    /// </summary>
    public class DamageZone : MonoBehaviour
    {
        [Header("Effect")]
        [Tooltip("Effect applied to entities in this zone")]
        [SerializeField] private GameplayEffectDefinition _effectToApply;

        [Header("Timing")]
        [Tooltip("How often to apply the effect (seconds)")]
        [SerializeField] private float _applicationInterval = 1f;

        [Tooltip("Apply immediately on enter?")]
        [SerializeField] private bool _applyOnEnter = true;

        [Header("Filters")]
        [Tooltip("Only affect objects with these tags (leave empty for all)")]
        [SerializeField] private List<string> _affectedTags = new();

        private Dictionary<AbilitySystemComponent, float> _entitiesInZone = new();

        private void Update()
        {
            // Apply effect periodically to all entities in zone
            var toRemove = new List<AbilitySystemComponent>();

            foreach (var kvp in _entitiesInZone)
            {
                var asc = kvp.Key;

                // Check if still valid
                if (asc == null)
                {
                    toRemove.Add(asc);
                    continue;
                }

                // Update timer
                _entitiesInZone[asc] -= Time.deltaTime;

                if (_entitiesInZone[asc] <= 0)
                {
                    ApplyEffect(asc);
                    _entitiesInZone[asc] = _applicationInterval;
                }
            }

            foreach (var asc in toRemove)
            {
                _entitiesInZone.Remove(asc);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            HandleEnter(other.gameObject);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            HandleExit(other.gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            HandleEnter(other.gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            HandleExit(other.gameObject);
        }

        private void HandleEnter(GameObject obj)
        {
            // Check tag filter
            if (_affectedTags.Count > 0 && !_affectedTags.Contains(obj.tag))
                return;

            var asc = obj.GetComponent<AbilitySystemComponent>();
            if (asc == null) return;

            if (!_entitiesInZone.ContainsKey(asc))
            {
                _entitiesInZone[asc] = _applyOnEnter ? 0f : _applicationInterval;

                if (_applyOnEnter)
                {
                    ApplyEffect(asc);
                    _entitiesInZone[asc] = _applicationInterval;
                }
            }
        }

        private void HandleExit(GameObject obj)
        {
            var asc = obj.GetComponent<AbilitySystemComponent>();
            if (asc != null)
            {
                _entitiesInZone.Remove(asc);
            }
        }

        private void ApplyEffect(AbilitySystemComponent target)
        {
            if (_effectToApply != null)
            {
                target.ApplyEffectToSelf(_effectToApply, this);
            }
        }

#if UNITY_EDITOR
        [Header("Gizmo")]
        [SerializeField] private Color _gizmoColor = new Color(1f, 0.3f, 0.3f, 0.3f);

        private void OnDrawGizmos()
        {
            DrawZoneGizmo(false);
        }

        private void OnDrawGizmosSelected()
        {
            DrawZoneGizmo(true);

            // Draw effect name and interval
            if (_effectToApply != null)
            {
                var label = $"⚠️ {_effectToApply.name.Replace("Effect_", "")}\nEvery {_applicationInterval}s";
                UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, label);
            }
        }

        private void DrawZoneGizmo(bool selected)
        {
            var col2D = GetComponent<Collider2D>();
            var col3D = GetComponent<Collider>();

            Gizmos.color = selected ? _gizmoColor : new Color(_gizmoColor.r, _gizmoColor.g, _gizmoColor.b, _gizmoColor.a * 0.5f);

            if (col2D != null)
            {
                // Draw based on collider type
                if (col2D is BoxCollider2D box)
                {
                    var size = new Vector3(box.size.x * transform.lossyScale.x, box.size.y * transform.lossyScale.y, 0.1f);
                    var center = transform.position + (Vector3)box.offset;
                    Gizmos.DrawCube(center, size);
                    Gizmos.color = new Color(_gizmoColor.r, _gizmoColor.g, _gizmoColor.b, 1f);
                    Gizmos.DrawWireCube(center, size);
                }
                else if (col2D is CircleCollider2D circle)
                {
                    var radius = circle.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
                    var center = transform.position + (Vector3)circle.offset;
                    DrawCircle(center, radius, 32);
                    Gizmos.DrawWireSphere(center, radius);
                }
                else
                {
                    Gizmos.DrawCube(col2D.bounds.center, col2D.bounds.size);
                }
            }
            else if (col3D != null)
            {
                Gizmos.DrawCube(col3D.bounds.center, col3D.bounds.size);
                Gizmos.color = new Color(_gizmoColor.r, _gizmoColor.g, _gizmoColor.b, 1f);
                Gizmos.DrawWireCube(col3D.bounds.center, col3D.bounds.size);
            }
            else
            {
                // Default: draw a small cube
                Gizmos.DrawCube(transform.position, Vector3.one);
            }

            // Draw warning icon
            Gizmos.DrawIcon(transform.position, "console.warnicon", true);
        }

        private void DrawCircle(Vector3 center, float radius, int segments)
        {
            var prevPoint = center + new Vector3(radius, 0, 0);
            for (int i = 1; i <= segments; i++)
            {
                float angle = i * 2 * Mathf.PI / segments;
                var newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
            }
        }
#endif
    }
}
