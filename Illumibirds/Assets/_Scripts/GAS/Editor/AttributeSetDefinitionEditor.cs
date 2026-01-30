#if UNITY_EDITOR
using GAS.Attributes;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace GAS.Editor
{
    [CustomEditor(typeof(AttributeSetDefinition))]
    public class AttributeSetDefinitionEditor : UnityEditor.Editor
    {
        private AttributeSetDefinition _target;
        private ReorderableList _attributesList;

        private void OnEnable()
        {
            _target = (AttributeSetDefinition)target;
            SetupAttributesList();
        }

        private void SetupAttributesList()
        {
            _attributesList = new ReorderableList(serializedObject,
                serializedObject.FindProperty("Attributes"), true, true, true, true);

            _attributesList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Attributes & Initial Values");
            };

            _attributesList.elementHeightCallback = index => EditorGUIUtility.singleLineHeight * 2 + 8;

            _attributesList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = _attributesList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                var lineHeight = EditorGUIUtility.singleLineHeight + 2;

                var attrRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
                var valueRect = new Rect(rect.x, rect.y + lineHeight, rect.width, EditorGUIUtility.singleLineHeight);

                EditorGUI.PropertyField(attrRect, element.FindPropertyRelative("Attribute"), new GUIContent("Attribute"));

                var attr = element.FindPropertyRelative("Attribute").objectReferenceValue as AttributeDefinition;
                var valueProp = element.FindPropertyRelative("InitialValue");

                if (attr != null)
                {
                    // Show slider with min/max from the attribute definition
                    var newValue = EditorGUI.Slider(valueRect, "Initial Value", valueProp.floatValue, attr.MinValue, attr.MaxValue);
                    valueProp.floatValue = newValue;
                }
                else
                {
                    EditorGUI.PropertyField(valueRect, valueProp, new GUIContent("Initial Value"));
                }
            };

            _attributesList.onAddCallback = list =>
            {
                var index = list.serializedProperty.arraySize;
                list.serializedProperty.arraySize++;
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("Attribute").objectReferenceValue = null;
                element.FindPropertyRelative("InitialValue").floatValue = 0;
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Header
            GASEditorStyles.DrawHeader("📦 Attribute Set Definition");

            EditorGUILayout.HelpBox(
                "An Attribute Set defines the starting stats for an entity.\n\n" +
                "How to use:\n" +
                "1. Add all attributes this entity should have\n" +
                "2. Set their initial values\n" +
                "3. Assign this to an AbilitySystemComponent",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Attributes list
            GASEditorStyles.DrawSection("📊 Attributes", () =>
            {
                if (_target.Attributes.Count == 0)
                {
                    EditorGUILayout.HelpBox(
                        "No attributes defined! Add attributes like:\n" +
                        "• Health (100)\n" +
                        "• Stamina (100)\n" +
                        "• Damage (10)\n" +
                        "• AttackSpeed (1.0)",
                        MessageType.Warning);
                }

                _attributesList.DoLayoutList();
            });

            // Quick add section
            GASEditorStyles.DrawSection("⚡ Quick Add", () =>
            {
                EditorGUILayout.HelpBox("Drag & drop AttributeDefinitions here to add them", MessageType.None);

                var dropArea = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
                GUI.Box(dropArea, "Drop Attributes Here", EditorStyles.helpBox);

                var evt = Event.current;
                if (dropArea.Contains(evt.mousePosition))
                {
                    if (evt.type == EventType.DragUpdated)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        evt.Use();
                    }
                    else if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        foreach (var obj in DragAndDrop.objectReferences)
                        {
                            if (obj is AttributeDefinition attr)
                            {
                                // Check if already exists
                                bool exists = false;
                                foreach (var existing in _target.Attributes)
                                {
                                    if (existing.Attribute == attr)
                                    {
                                        exists = true;
                                        break;
                                    }
                                }

                                if (!exists)
                                {
                                    _target.Attributes.Add(new AttributeSetDefinition.AttributeInitialValue
                                    {
                                        Attribute = attr,
                                        InitialValue = (attr.MinValue + attr.MaxValue) / 2f
                                    });
                                    EditorUtility.SetDirty(_target);
                                }
                            }
                        }
                        evt.Use();
                    }
                }
            });

            // Summary
            if (_target.Attributes.Count > 0)
            {
                EditorGUILayout.Space(10);
                GASEditorStyles.DrawSection("📋 Summary", () =>
                {
                    var summary = "Starting stats:\n";
                    foreach (var attr in _target.Attributes)
                    {
                        if (attr.Attribute != null)
                        {
                            summary += $"  • {attr.Attribute.name}: {attr.InitialValue}\n";
                        }
                    }

                    GUI.color = new Color(0.85f, 0.95f, 0.85f);
                    EditorGUILayout.LabelField(summary.TrimEnd(), EditorStyles.helpBox);
                    GUI.color = Color.white;
                });
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
