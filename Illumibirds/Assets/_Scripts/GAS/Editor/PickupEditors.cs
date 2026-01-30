#if UNITY_EDITOR
using GAS.Pickups;
using UnityEditor;
using UnityEngine;

namespace GAS.Editor
{
    [CustomEditor(typeof(Pickup))]
    public class PickupEditor : UnityEditor.Editor
    {
        private Pickup _target;
        private bool _showSettings = false;

        private static readonly string[] TypeLabels = { "💊 Effect", "⚔️ Ability" };
        private static readonly Color[] TypeColors =
        {
            new Color(0.3f, 1f, 0.5f),
            new Color(1f, 0.8f, 0.3f)
        };

        private void OnEnable()
        {
            _target = (Pickup)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GASEditorStyles.DrawHeader("📦 PICKUP");

            // Type toggle
            var typeProp = serializedObject.FindProperty("_pickupType");
            typeProp.enumValueIndex = GASEditorStyles.DrawToggleButtons(typeProp.enumValueIndex, TypeLabels, TypeColors);

            EditorGUILayout.Space(10);

            // Conditional fields based on type
            if (_target.Type == PickupType.Effect)
            {
                GASEditorStyles.DrawSection("", () =>
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_effects"), new GUIContent("Effects"));
                });
            }
            else
            {
                GASEditorStyles.DrawSection("", () =>
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_ability"), new GUIContent("Ability"));
                });
            }

            // Settings foldout
            EditorGUILayout.Space(5);
            _showSettings = EditorGUILayout.Foldout(_showSettings, "▶ Settings...", true);

            if (_showSettings)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(serializedObject.FindProperty("_destroyOnPickup"), new GUIContent("Destroy on pickup"));

                if (!_target.DestroyOnPickup)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_respawnTime"), GUIContent.none, GUILayout.Width(50));
                    EditorGUILayout.LabelField("sec respawn (0=never)", GUILayout.Width(120));
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("_visualRoot"), new GUIContent("Visual Root"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_gizmoRadius"), new GUIContent("Gizmo Radius"));

                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(DamageZone))]
    public class DamageZoneEditor : UnityEditor.Editor
    {
        private bool _showSettings = false;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GASEditorStyles.DrawHeader("🔥 DAMAGE ZONE");
            GASEditorStyles.DrawHint("Applies effects repeatedly while inside");

            EditorGUILayout.Space(5);

            GASEditorStyles.DrawSection("", () =>
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_effectToApply"), new GUIContent("Effect"));

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Every", GUILayout.Width(40));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_applicationInterval"), GUIContent.none, GUILayout.Width(50));
                EditorGUILayout.LabelField("sec", GUILayout.Width(25));
                EditorGUILayout.EndHorizontal();
            });

            _showSettings = EditorGUILayout.Foldout(_showSettings, "▶ Settings...", true);

            if (_showSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_applyOnEnter"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_affectedTags"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_gizmoColor"));
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
