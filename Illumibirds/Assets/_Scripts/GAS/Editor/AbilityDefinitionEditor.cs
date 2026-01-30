#if UNITY_EDITOR
using System;
using System.Linq;
using GAS.Abilities;
using GAS.Attributes;
using GAS.Effects;
using GAS.Tags;
using UnityEditor;
using UnityEngine;

namespace GAS.Editor
{
    [CustomEditor(typeof(AbilityDefinition))]
    public class AbilityDefinitionEditor : UnityEditor.Editor
    {
        private AbilityDefinition _target;
        private bool _showAdvanced = false;
        private bool _useCost = false;

        // Behavior types cache
        private static Type[] _behaviorTypes;
        private static string[] _behaviorNames;

        private void OnEnable()
        {
            _target = (AbilityDefinition)target;
            _useCost = _target.CostAttribute != null;
            CacheBehaviorTypes();
        }

        private static void CacheBehaviorTypes()
        {
            if (_behaviorTypes != null) return;

            _behaviorTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Array.Empty<Type>(); }
                })
                .Where(t => typeof(IAbilityBehavior).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToArray();

            _behaviorNames = new string[_behaviorTypes.Length + 1];
            _behaviorNames[0] = "(None)";
            for (int i = 0; i < _behaviorTypes.Length; i++)
            {
                _behaviorNames[i + 1] = _behaviorTypes[i].Name;
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GASEditorStyles.DrawHeader("⚔️ ABILITY");

            // Name
            EditorGUILayout.PropertyField(serializedObject.FindProperty("AbilityName"), new GUIContent("Name"));

            EditorGUILayout.Space(10);

            // Cost (conditional)
            GASEditorStyles.DrawSection("💰 COST", () =>
            {
                _useCost = EditorGUILayout.Toggle("Uses resource", _useCost);

                if (_useCost)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("CostAttribute"), GUIContent.none);
                    EditorGUILayout.LabelField("-", GUILayout.Width(10));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("CostAmount"), GUIContent.none, GUILayout.Width(50));
                    EditorGUILayout.LabelField("per use", GUILayout.Width(50));
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    serializedObject.FindProperty("CostAttribute").objectReferenceValue = null;
                }
            });

            // Cooldown
            GASEditorStyles.DrawSection("⏱️ COOLDOWN", () =>
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Cooldown"), GUIContent.none, GUILayout.Width(60));
                EditorGUILayout.LabelField("seconds", GUILayout.Width(55));
                EditorGUILayout.EndHorizontal();
            });

            // Effects
            GASEditorStyles.DrawSection("⚡ EFFECTS", () =>
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("On Target:", GUILayout.Width(70));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ApplyToTargetEffects"), GUIContent.none);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("On Self:", GUILayout.Width(70));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ApplyToSelfEffects"), GUIContent.none);
                EditorGUILayout.EndHorizontal();
            });

            // Behavior
            GASEditorStyles.DrawSection("🎮 BEHAVIOR", () =>
            {
                DrawBehaviorSelector();
            });

            // Advanced foldout
            EditorGUILayout.Space(5);
            _showAdvanced = EditorGUILayout.Foldout(_showAdvanced, "▶ Advanced...", true);

            if (_showAdvanced)
            {
                EditorGUI.indentLevel++;

                // Icon
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Icon"));

                // Channeling
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("IsChanneled"), new GUIContent("Channeled"));
                if (_target.IsChanneled)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ChannelDuration"), GUIContent.none, GUILayout.Width(50));
                    EditorGUILayout.LabelField("sec (0=manual)", GUILayout.Width(80));
                }
                EditorGUILayout.EndHorizontal();

                // Tags
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Tags", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ActivationRequiredTags"), new GUIContent("Required"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ActivationBlockedTags"), new GUIContent("Blocked by"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ActivationGrantedTags"), new GUIContent("Grants while active"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CooldownTags"), new GUIContent("Grants on cooldown"));

                EditorGUI.indentLevel--;
            }

            // Preview summary
            EditorGUILayout.Space(10);
            DrawPreview();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawBehaviorSelector()
        {
            var behaviorProp = serializedObject.FindProperty("Behavior");

            // Find current type index
            int currentIndex = 0;
            if (_target.Behavior != null)
            {
                var currentType = _target.Behavior.GetType();
                for (int i = 0; i < _behaviorTypes.Length; i++)
                {
                    if (_behaviorTypes[i] == currentType)
                    {
                        currentIndex = i + 1;
                        break;
                    }
                }
            }

            // Type dropdown
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Type:", GUILayout.Width(40));
            int newIndex = EditorGUILayout.Popup(currentIndex, _behaviorNames);
            EditorGUILayout.EndHorizontal();

            // Handle type change
            if (newIndex != currentIndex)
            {
                if (newIndex == 0)
                {
                    behaviorProp.managedReferenceValue = null;
                }
                else
                {
                    var newType = _behaviorTypes[newIndex - 1];
                    behaviorProp.managedReferenceValue = Activator.CreateInstance(newType);
                }
            }

            // Draw behavior properties if one is selected
            if (_target.Behavior != null)
            {
                EditorGUILayout.Space(5);

                // Draw all visible children of the behavior
                var iterator = behaviorProp.Copy();
                var endProp = behaviorProp.GetEndProperty();
                iterator.NextVisible(true); // Enter the first child

                while (!SerializedProperty.EqualContents(iterator, endProp))
                {
                    EditorGUILayout.PropertyField(iterator, true);
                    if (!iterator.NextVisible(false))
                        break;
                }
            }
        }

        private void DrawPreview()
        {
            var summary = "";

            // Cost
            if (_target.CostAttribute != null && _target.CostAmount > 0)
                summary += $"💰 {_target.CostAmount} {_target.CostAttribute.name}";
            else
                summary += "💰 Free";

            // Cooldown
            if (_target.Cooldown > 0)
                summary += $"  ⏱️ {_target.Cooldown}s";

            // Effects count
            var targetCount = _target.ApplyToTargetEffects.Count(e => e != null);
            var selfCount = _target.ApplyToSelfEffects.Count(e => e != null);
            if (targetCount > 0 || selfCount > 0)
            {
                summary += "  ⚡";
                if (targetCount > 0) summary += $" {targetCount}→target";
                if (selfCount > 0) summary += $" {selfCount}→self";
            }

            // Behavior
            if (_target.Behavior != null)
                summary += $"  🎮 {_target.Behavior.GetType().Name}";

            GASEditorStyles.DrawPreview(summary, new Color(0.85f, 0.95f, 0.85f));
        }
    }
}
#endif
