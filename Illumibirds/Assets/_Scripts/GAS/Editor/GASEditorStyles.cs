#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace GAS.Editor
{
    /// <summary>
    /// Shared styles and drawing utilities for GAS editors.
    /// </summary>
    public static class GASEditorStyles
    {
        private static GUIStyle _headerStyle;
        private static GUIStyle _sectionStyle;
        private static bool _initialized;

        private static void InitStyles()
        {
            if (_initialized) return;

            _headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(0, 0, 5, 10)
            };

            _sectionStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(0, 0, 5, 5)
            };

            _initialized = true;
        }

        public static void DrawHeader(string title)
        {
            InitStyles();

            EditorGUILayout.Space(5);

            // Draw colored bar
            var rect = EditorGUILayout.GetControlRect(false, 2);
            EditorGUI.DrawRect(rect, new Color(0.3f, 0.6f, 1f, 0.8f));

            EditorGUILayout.LabelField(title, _headerStyle);

            // Another colored bar
            rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, new Color(0.3f, 0.6f, 1f, 0.3f));

            EditorGUILayout.Space(5);
        }

        public static void DrawSection(string title, Action drawContent)
        {
            InitStyles();

            EditorGUILayout.BeginVertical(_sectionStyle);

            if (!string.IsNullOrEmpty(title))
            {
                EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
                EditorGUILayout.Space(3);
            }

            drawContent?.Invoke();

            EditorGUILayout.EndVertical();
        }

        public static void DrawInfoBox(string message)
        {
            GUI.color = new Color(0.8f, 0.9f, 1f);
            EditorGUILayout.HelpBox(message, MessageType.Info);
            GUI.color = Color.white;
        }

        public static void DrawWarningBox(string message)
        {
            GUI.color = new Color(1f, 0.95f, 0.8f);
            EditorGUILayout.HelpBox(message, MessageType.Warning);
            GUI.color = Color.white;
        }

        public static void DrawSuccessBox(string message)
        {
            GUI.color = new Color(0.8f, 1f, 0.85f);
            EditorGUILayout.LabelField(message, EditorStyles.helpBox);
            GUI.color = Color.white;
        }
    }
}
#endif
