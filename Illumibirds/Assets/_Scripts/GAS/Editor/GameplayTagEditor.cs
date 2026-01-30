#if UNITY_EDITOR
using GAS.Tags;
using UnityEditor;
using UnityEngine;

namespace GAS.Editor
{
    [CustomEditor(typeof(GameplayTag))]
    public class GameplayTagEditor : UnityEditor.Editor
    {
        private GameplayTag _target;

        private void OnEnable()
        {
            _target = (GameplayTag)target;
        }

        public override void OnInspectorGUI()
        {
            // Header
            GASEditorStyles.DrawHeader("🏷️ Gameplay Tag");

            EditorGUILayout.HelpBox(
                "Tags are used to categorize and filter.\n\n" +
                "Common uses:\n" +
                "• Block abilities when Stunned/Dead\n" +
                "• Mark status effects (Burning, Poisoned)\n" +
                "• Categorize abilities (Attack, Movement, Buff)",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Parent tag
            GASEditorStyles.DrawSection("Hierarchy", () =>
            {
                EditorGUILayout.HelpBox(
                    "Tags can have parents for hierarchical matching.\n\n" +
                    "Example: If 'Stunned' has parent 'Status',\n" +
                    "checking for 'Status' will also match 'Stunned'.",
                    MessageType.None);

                _target.Parent = (GameplayTag)EditorGUILayout.ObjectField(
                    new GUIContent("Parent Tag", "Optional parent for hierarchy"),
                    _target.Parent, typeof(GameplayTag), false);

                // Show hierarchy
                if (_target.Parent != null)
                {
                    EditorGUILayout.Space(5);
                    var hierarchy = _target.name;
                    var current = _target.Parent;
                    while (current != null)
                    {
                        hierarchy = current.name + " → " + hierarchy;
                        current = current.Parent;
                    }
                    EditorGUILayout.LabelField("Hierarchy: " + hierarchy, EditorStyles.helpBox);
                }
            });

            // Naming convention
            GASEditorStyles.DrawSection("💡 Naming Convention", () =>
            {
                EditorGUILayout.LabelField("Recommended naming patterns:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("• Tag_Status_Stunned", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• Tag_Status_Burning", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• Tag_Ability_Attack", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• Tag_Ability_Movement", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• Tag_Cooldown_Fireball", EditorStyles.miniLabel);
            });

            // Usage examples
            GASEditorStyles.DrawSection("📖 Where to use this tag", () =>
            {
                EditorGUILayout.LabelField("In Effects:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("  • Granted Tags: Add while effect is active", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("  • Required Tags: Effect only applies if target has tag", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("  • Blocked Tags: Effect won't apply if target has tag", EditorStyles.miniLabel);

                EditorGUILayout.Space(5);

                EditorGUILayout.LabelField("In Abilities:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("  • Blocked Tags: Can't activate while tag is present", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("  • Required Tags: Must have tag to activate", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("  • Granted Tags: Add while ability is active", EditorStyles.miniLabel);
            });

            if (GUI.changed)
            {
                EditorUtility.SetDirty(_target);
            }
        }
    }
}
#endif
