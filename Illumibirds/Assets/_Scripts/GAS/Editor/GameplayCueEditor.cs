#if UNITY_EDITOR
using GAS.Cues;
using UnityEditor;
using UnityEngine;

namespace GAS.Editor
{
    [CustomEditor(typeof(GameplayCue))]
    public class GameplayCueEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Header
            GASEditorStyles.DrawHeader("🎬 Gameplay Cue");

            EditorGUILayout.HelpBox(
                "Cues are VFX/SFX wrappers for abilities and effects.\n\n" +
                "Use cues to:\n" +
                "• Play particle effects on hit\n" +
                "• Play sound effects\n" +
                "• Spawn visual indicators",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Draw default inspector for the rest
            DrawDefaultInspector();
        }
    }
}
#endif
