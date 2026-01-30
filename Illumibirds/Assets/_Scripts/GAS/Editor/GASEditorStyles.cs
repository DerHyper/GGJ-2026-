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
        private static GUIStyle _toggleButtonStyle;
        private static GUIStyle _toggleButtonActiveStyle;
        private static GUIStyle _miniLabelStyle;
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

            _toggleButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                padding = new RectOffset(12, 12, 8, 8),
                margin = new RectOffset(2, 2, 2, 2)
            };

            _toggleButtonActiveStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(12, 12, 8, 8),
                margin = new RectOffset(2, 2, 2, 2)
            };

            _miniLabelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.6f, 0.6f, 0.6f) }
            };

            _initialized = true;
        }

        public static void DrawHeader(string title)
        {
            InitStyles();

            EditorGUILayout.Space(5);

            var rect = EditorGUILayout.GetControlRect(false, 2);
            EditorGUI.DrawRect(rect, new Color(0.3f, 0.6f, 1f, 0.8f));

            EditorGUILayout.LabelField(title, _headerStyle);

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

        /// <summary>
        /// Draw toggle buttons in a row. Returns the index of the selected button.
        /// </summary>
        public static int DrawToggleButtons(int selected, string[] labels, Color[] colors = null)
        {
            InitStyles();

            EditorGUILayout.BeginHorizontal();

            for (int i = 0; i < labels.Length; i++)
            {
                bool isSelected = i == selected;

                if (colors != null && i < colors.Length)
                    GUI.backgroundColor = isSelected ? colors[i] : Color.white;
                else if (isSelected)
                    GUI.backgroundColor = new Color(0.4f, 0.7f, 1f);

                var style = isSelected ? _toggleButtonActiveStyle : _toggleButtonStyle;

                if (GUILayout.Button(labels[i], style, GUILayout.Height(30)))
                {
                    selected = i;
                }
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            return selected;
        }

        /// <summary>
        /// Draw a compact field row with label and value.
        /// </summary>
        public static void DrawCompactRow(string label, Action drawField)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(80));
            drawField?.Invoke();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw a short hint text.
        /// </summary>
        public static void DrawHint(string hint)
        {
            InitStyles();
            EditorGUILayout.LabelField(hint, _miniLabelStyle);
        }

        /// <summary>
        /// Draw a preview box with custom color.
        /// </summary>
        public static void DrawPreview(string text, Color color)
        {
            GUI.color = color;
            EditorGUILayout.LabelField(text, EditorStyles.helpBox);
            GUI.color = Color.white;
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
