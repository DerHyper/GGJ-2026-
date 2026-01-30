#if UNITY_EDITOR
using System.Linq;
using GAS.Attributes;
using GAS.Effects;
using GAS.Tags;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace GAS.Editor
{
    [CustomEditor(typeof(GameplayEffectDefinition))]
    public class GameplayEffectDefinitionEditor : UnityEditor.Editor
    {
        private GameplayEffectDefinition _target;
        private ReorderableList _modifiersList;
        private ReorderableList _grantedTagsList;
        private bool _showAdvanced = false;

        private void OnEnable()
        {
            _target = (GameplayEffectDefinition)target;
            SetupModifiersList();
            SetupGrantedTagsList();
        }

        private void SetupModifiersList()
        {
            _modifiersList = new ReorderableList(serializedObject,
                serializedObject.FindProperty("Modifiers"), true, true, true, true);

            _modifiersList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Attribute Modifiers (what this effect changes)");
            };

            _modifiersList.elementHeightCallback = index => EditorGUIUtility.singleLineHeight * 3 + 10;

            _modifiersList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = _modifiersList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                var lineHeight = EditorGUIUtility.singleLineHeight + 2;

                var attrRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
                var opRect = new Rect(rect.x, rect.y + lineHeight, rect.width * 0.5f - 5, EditorGUIUtility.singleLineHeight);
                var magRect = new Rect(rect.x + rect.width * 0.5f + 5, rect.y + lineHeight, rect.width * 0.5f - 5, EditorGUIUtility.singleLineHeight);
                var previewRect = new Rect(rect.x, rect.y + lineHeight * 2, rect.width, EditorGUIUtility.singleLineHeight);

                EditorGUI.PropertyField(attrRect, element.FindPropertyRelative("TargetAttribute"), new GUIContent("Target Attribute"));
                EditorGUI.PropertyField(opRect, element.FindPropertyRelative("Operation"), GUIContent.none);
                EditorGUI.PropertyField(magRect, element.FindPropertyRelative("Magnitude"), GUIContent.none);

                // Preview
                var attr = element.FindPropertyRelative("TargetAttribute").objectReferenceValue as AttributeDefinition;
                var op = (ModifierOperation)element.FindPropertyRelative("Operation").enumValueIndex;
                var mag = element.FindPropertyRelative("Magnitude").floatValue;

                var attrName = attr != null ? attr.name : "???";
                var preview = op switch
                {
                    ModifierOperation.Add => $"→ {attrName} {(mag >= 0 ? "+" : "")}{mag}",
                    ModifierOperation.Multiply => $"→ {attrName} ×{mag}",
                    ModifierOperation.Override => $"→ {attrName} = {mag}",
                    _ => ""
                };

                GUI.color = new Color(0.7f, 0.85f, 1f);
                EditorGUI.LabelField(previewRect, preview, EditorStyles.helpBox);
                GUI.color = Color.white;
            };
        }

        private void SetupGrantedTagsList()
        {
            _grantedTagsList = new ReorderableList(serializedObject,
                serializedObject.FindProperty("GrantedTags"), true, true, true, true);

            _grantedTagsList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Tags granted while effect is active");
            };

            _grantedTagsList.elementHeight = EditorGUIUtility.singleLineHeight + 4;

            _grantedTagsList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = _grantedTagsList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                EditorGUI.PropertyField(rect, element, GUIContent.none);
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Header
            GASEditorStyles.DrawHeader("⚡ Gameplay Effect");

            // Quick type indicator
            var typeColor = _target.DurationType switch
            {
                EffectDurationType.Instant => new Color(1f, 0.8f, 0.3f),
                EffectDurationType.Duration => new Color(0.3f, 0.8f, 1f),
                EffectDurationType.Infinite => new Color(0.8f, 0.5f, 1f),
                _ => Color.white
            };
            var typeLabel = _target.DurationType switch
            {
                EffectDurationType.Instant => "⚡ INSTANT - Applies once immediately",
                EffectDurationType.Duration => $"⏱️ DURATION - Lasts {_target.Duration} seconds",
                EffectDurationType.Infinite => "∞ INFINITE - Lasts until removed",
                _ => ""
            };

            GUI.backgroundColor = typeColor;
            EditorGUILayout.HelpBox(typeLabel, MessageType.None);
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(10);

            // Duration Section
            GASEditorStyles.DrawSection("⏱️ Duration Type", () =>
            {
                EditorGUILayout.HelpBox(
                    "• INSTANT: Apply once and done (damage, heal)\n" +
                    "• DURATION: Temporary effect for X seconds (buffs)\n" +
                    "• INFINITE: Stays until manually removed (passives)",
                    MessageType.None);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("DurationType"));

                if (_target.DurationType == EffectDurationType.Duration)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Duration"),
                        new GUIContent("Duration (seconds)"));

                    if (_target.Duration <= 0)
                    {
                        EditorGUILayout.HelpBox("Duration must be greater than 0!", MessageType.Error);
                    }
                }
            });

            // Modifiers Section
            GASEditorStyles.DrawSection("📊 Modifiers", () =>
            {
                EditorGUILayout.HelpBox(
                    "What attributes does this effect change?\n\n" +
                    "Operations:\n" +
                    "• ADD: +10 damage, -20 health (use negative for damage!)\n" +
                    "• MULTIPLY: ×1.5 = +50% speed, ×0.5 = -50% speed\n" +
                    "• OVERRIDE: Set to exact value, ignores other modifiers",
                    MessageType.None);

                _modifiersList.DoLayoutList();

                if (_target.Modifiers.Count == 0)
                {
                    EditorGUILayout.HelpBox("Add at least one modifier - what should this effect do?", MessageType.Warning);
                }
            });

            // Tags Section
            GASEditorStyles.DrawSection("🏷️ Tags", () =>
            {
                EditorGUILayout.HelpBox(
                    "Tags are used for:\n" +
                    "• Granted Tags: Added while effect is active (e.g., 'Burning', 'Slowed')\n" +
                    "• Required Tags: Target must have these for effect to apply\n" +
                    "• Blocked Tags: Effect won't apply if target has these",
                    MessageType.None);

                _grantedTagsList.DoLayoutList();
            });

            // Advanced Section
            _showAdvanced = EditorGUILayout.Foldout(_showAdvanced, "⚙️ Advanced Settings", true);
            if (_showAdvanced)
            {
                EditorGUI.indentLevel++;

                GASEditorStyles.DrawSection("Tag Requirements", () =>
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ApplicationRequiredTags"),
                        new GUIContent("Required Tags", "Target must have ALL of these tags"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ApplicationBlockedTags"),
                        new GUIContent("Blocked Tags", "Effect won't apply if target has ANY of these"));
                });

                GASEditorStyles.DrawSection("Stacking", () =>
                {
                    EditorGUILayout.HelpBox(
                        "Can multiple instances of this effect exist on the same target?\n" +
                        "Example: Poison that stacks 5 times for more damage.",
                        MessageType.None);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("CanStack"));
                    if (_target.CanStack)
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("MaxStacks"),
                            new GUIContent("Max Stacks", "0 = unlimited"));
                    }
                });

                GASEditorStyles.DrawSection("Periodic", () =>
                {
                    EditorGUILayout.HelpBox(
                        "Periodic effects re-apply their modifiers every X seconds.\n" +
                        "Great for damage-over-time (DoT) effects!",
                        MessageType.None);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("IsPeriodic"),
                        new GUIContent("Is Periodic", "Apply modifiers repeatedly?"));

                    if (_target.IsPeriodic)
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("Period"),
                            new GUIContent("Period (seconds)", "Time between each application"));

                        if (_target.Period <= 0)
                        {
                            EditorGUILayout.HelpBox("Period must be greater than 0!", MessageType.Error);
                        }

                        // DoT preview
                        if (_target.DurationType == EffectDurationType.Duration && _target.Period > 0)
                        {
                            var ticks = Mathf.FloorToInt(_target.Duration / _target.Period);
                            EditorGUILayout.LabelField($"Will apply {ticks} times over {_target.Duration}s", EditorStyles.helpBox);
                        }
                    }
                });

                EditorGUI.indentLevel--;
            }

            // Summary
            EditorGUILayout.Space(10);
            DrawSummary();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSummary()
        {
            GASEditorStyles.DrawSection("📋 Summary", () =>
            {
                var summary = "";

                // Duration
                summary += _target.DurationType switch
                {
                    EffectDurationType.Instant => "Applies instantly",
                    EffectDurationType.Duration => $"Lasts {_target.Duration}s",
                    EffectDurationType.Infinite => "Lasts until removed",
                    _ => ""
                };

                // Modifiers
                if (_target.Modifiers.Count > 0)
                {
                    summary += "\n\nChanges:";
                    foreach (var mod in _target.Modifiers.Where(m => m.TargetAttribute != null))
                    {
                        var sign = mod.Operation == ModifierOperation.Multiply ? "×" :
                                  (mod.Magnitude >= 0 ? "+" : "");
                        summary += $"\n  • {mod.TargetAttribute.name}: {sign}{mod.Magnitude}";
                    }
                }

                // Periodic
                if (_target.IsPeriodic && _target.Period > 0)
                {
                    summary += $"\n\nRepeats every {_target.Period}s";
                }

                // Tags
                if (_target.GrantedTags.Count > 0)
                {
                    summary += $"\n\nGrants tags: {string.Join(", ", _target.GrantedTags.Where(t => t != null).Select(t => t.name))}";
                }

                GUI.color = new Color(0.85f, 0.95f, 0.85f);
                EditorGUILayout.LabelField(summary, EditorStyles.helpBox);
                GUI.color = Color.white;
            });
        }
    }
}
#endif
