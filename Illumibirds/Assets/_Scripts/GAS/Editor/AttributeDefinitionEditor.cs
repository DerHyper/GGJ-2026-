#if UNITY_EDITOR
using GAS.Attributes;
using UnityEditor;
using UnityEngine;

namespace GAS.Editor
{
    [CustomEditor(typeof(AttributeDefinition))]
    public class AttributeDefinitionEditor : UnityEditor.Editor
    {
        private AttributeDefinition _target;

        private void OnEnable()
        {
            _target = (AttributeDefinition)target;
        }

        public override void OnInspectorGUI()
        {
            // Header
            GASEditorStyles.DrawHeader("📊 Attribute Definition");

            EditorGUILayout.HelpBox(
                "An Attribute is a stat like Health, Damage, or Speed.\n" +
                "• Create one AttributeDefinition per stat type\n" +
                "• Add to an AttributeSet to use on entities\n" +
                "• Effects modify attributes at runtime",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Fields
            GASEditorStyles.DrawSection("Basic Info", () =>
            {
                _target.AttributeName = EditorGUILayout.TextField(
                    new GUIContent("Display Name", "The name shown in UI (e.g., 'Health', 'Attack Speed')"),
                    _target.AttributeName);

                if (string.IsNullOrEmpty(_target.AttributeName))
                {
                    EditorGUILayout.HelpBox("Give this attribute a display name!", MessageType.Warning);
                }
            });

            GASEditorStyles.DrawSection("Value Limits", () =>
            {
                EditorGUILayout.HelpBox(
                    "These are HARD limits. The attribute value will always be clamped to this range.\n" +
                    "For dynamic limits (e.g., MaxHealth that can be buffed), create a separate attribute.",
                    MessageType.None);

                _target.MinValue = EditorGUILayout.FloatField(
                    new GUIContent("Min Value", "Minimum possible value (usually 0 for Health)"),
                    _target.MinValue);

                _target.MaxValue = EditorGUILayout.FloatField(
                    new GUIContent("Max Value", "Maximum possible value (use float.MaxValue for unlimited)"),
                    _target.MaxValue);

                if (_target.MinValue > _target.MaxValue)
                {
                    EditorGUILayout.HelpBox("Min Value cannot be greater than Max Value!", MessageType.Error);
                }

                // Preview
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Preview:", EditorStyles.boldLabel);
                var preview = $"{_target.AttributeName}: [{_target.MinValue} → {_target.MaxValue}]";
                EditorGUILayout.LabelField(preview, EditorStyles.helpBox);
            });

            // Examples
            GASEditorStyles.DrawSection("💡 Common Examples", () =>
            {
                EditorGUILayout.LabelField("• Health: Min=0, Max=100 (or your max HP)", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• Damage: Min=0, Max=9999 (no real limit)", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• AttackSpeed: Min=0.1, Max=5 (multiplier)", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• Stamina: Min=0, Max=100", EditorStyles.miniLabel);
            });

            if (GUI.changed)
            {
                EditorUtility.SetDirty(_target);
            }
        }
    }
}
#endif
