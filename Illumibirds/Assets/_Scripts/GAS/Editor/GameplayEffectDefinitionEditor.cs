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
        private bool _showAdvanced = false;

        private static readonly string[] TypeLabels = { "⚡ Instant", "⏱️ Duration", "∞ Infinite" };
        private static readonly Color[] TypeColors =
        {
            new Color(1f, 0.8f, 0.3f),
            new Color(0.3f, 0.8f, 1f),
            new Color(0.8f, 0.5f, 1f)
        };

        private void OnEnable()
        {
            _target = (GameplayEffectDefinition)target;
            SetupModifiersList();
        }

        private void SetupModifiersList()
        {
            _modifiersList = new ReorderableList(serializedObject,
                serializedObject.FindProperty("Modifiers"), true, false, true, true);

            _modifiersList.elementHeightCallback = index => EditorGUIUtility.singleLineHeight + 6;

            _modifiersList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = _modifiersList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 3;
                rect.height = EditorGUIUtility.singleLineHeight;

                float spacing = 5;
                float attrWidth = rect.width * 0.4f;
                float opWidth = 60;
                float magWidth = rect.width - attrWidth - opWidth - spacing * 2;

                var attrRect = new Rect(rect.x, rect.y, attrWidth, rect.height);
                var opRect = new Rect(rect.x + attrWidth + spacing, rect.y, opWidth, rect.height);
                var magRect = new Rect(rect.x + attrWidth + opWidth + spacing * 2, rect.y, magWidth, rect.height);

                EditorGUI.PropertyField(attrRect, element.FindPropertyRelative("TargetAttribute"), GUIContent.none);
                EditorGUI.PropertyField(opRect, element.FindPropertyRelative("Operation"), GUIContent.none);
                EditorGUI.PropertyField(magRect, element.FindPropertyRelative("Magnitude"), GUIContent.none);
            };

            _modifiersList.onAddCallback = list =>
            {
                list.serializedProperty.arraySize++;
                var newElement = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
                newElement.FindPropertyRelative("Operation").enumValueIndex = 0;
                newElement.FindPropertyRelative("Magnitude").floatValue = 0;
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GASEditorStyles.DrawHeader("⚡ EFFECT");

            // Type toggle buttons
            var typeProp = serializedObject.FindProperty("DurationType");
            typeProp.enumValueIndex = GASEditorStyles.DrawToggleButtons(typeProp.enumValueIndex, TypeLabels, TypeColors);

            // Duration field (only if Duration type)
            if (_target.DurationType == EffectDurationType.Duration)
            {
                EditorGUILayout.Space(5);
                GASEditorStyles.DrawCompactRow("⏱️ Duration:", () =>
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Duration"), GUIContent.none);
                    EditorGUILayout.LabelField("sec", GUILayout.Width(25));
                });

                if (_target.Duration <= 0)
                {
                    EditorGUILayout.HelpBox("Must be > 0", MessageType.Error);
                }
            }

            EditorGUILayout.Space(10);

            // Modifiers section
            GASEditorStyles.DrawSection("📊 WHAT CHANGES?", () =>
            {
                _modifiersList.DoLayoutList();

                // Preview
                if (_target.Modifiers.Count > 0)
                {
                    var preview = string.Join(" | ", _target.Modifiers
                        .Where(m => m.TargetAttribute != null)
                        .Select(m =>
                        {
                            var sign = m.Operation == ModifierOperation.Multiply ? "×" : (m.Magnitude >= 0 ? "+" : "");
                            return $"{m.TargetAttribute.name} {sign}{m.Magnitude}";
                        }));

                    if (!string.IsNullOrEmpty(preview))
                        GASEditorStyles.DrawPreview(preview, new Color(0.7f, 0.85f, 1f));
                }
            });

            // Advanced foldout
            EditorGUILayout.Space(5);
            _showAdvanced = EditorGUILayout.Foldout(_showAdvanced, "▶ Advanced...", true);

            if (_showAdvanced)
            {
                EditorGUI.indentLevel++;

                // Periodic
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("IsPeriodic"), new GUIContent("Periodic"));
                if (_target.IsPeriodic)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Period"), GUIContent.none, GUILayout.Width(50));
                    EditorGUILayout.LabelField("sec", GUILayout.Width(25));
                }
                EditorGUILayout.EndHorizontal();

                if (_target.IsPeriodic && _target.DurationType == EffectDurationType.Duration && _target.Period > 0)
                {
                    var ticks = Mathf.FloorToInt(_target.Duration / _target.Period);
                    GASEditorStyles.DrawHint($"Applies {ticks}× over {_target.Duration}s");
                }

                // Stacking
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CanStack"), new GUIContent("Can Stack"));
                if (_target.CanStack)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("MaxStacks"), GUIContent.none, GUILayout.Width(50));
                    EditorGUILayout.LabelField("max", GUILayout.Width(25));
                }
                EditorGUILayout.EndHorizontal();

                // Tags
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Tags", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("GrantedTags"), new GUIContent("Grants"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ApplicationRequiredTags"), new GUIContent("Requires"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ApplicationBlockedTags"), new GUIContent("Blocked by"));

                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
