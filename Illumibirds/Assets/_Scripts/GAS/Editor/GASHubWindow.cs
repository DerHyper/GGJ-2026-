#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using GAS.Abilities;
using GAS.Attributes;
using GAS.Cues;
using GAS.Effects;
using GAS.Tags;
using UnityEditor;
using UnityEngine;

namespace GAS.Editor
{
    public class GASHubWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private string _searchFilter = "";
        private int _browseCategory = 0;

        private readonly string[] _categories = { "All", "Attributes", "Effects", "Abilities", "Tags", "Cues", "Attr Sets" };

        [MenuItem("Window/GAS/Hub %#g")]
        public static void ShowWindow()
        {
            var window = GetWindow<GASHubWindow>("GAS Hub");
            window.minSize = new Vector2(450, 500);
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            DrawHeader();
            DrawQuickCreate();
            DrawBrowser();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("🎮 Gameplay Ability System", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Ctrl+Shift+G to open this window", EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
        }

        private void DrawQuickCreate()
        {
            GASEditorStyles.DrawSection("⚡ Quick Create", () =>
            {
                EditorGUILayout.HelpBox("Click to create a new asset. The inspector will guide you through setup.", MessageType.None);

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("📊\nAttribute", GUILayout.Height(50)))
                    CreateAsset<AttributeDefinition>("Attr_New", "Attributes");

                if (GUILayout.Button("⚡\nEffect", GUILayout.Height(50)))
                    CreateAsset<GameplayEffectDefinition>("Effect_New", "Effects");

                if (GUILayout.Button("⚔️\nAbility", GUILayout.Height(50)))
                    CreateAsset<AbilityDefinition>("Ability_New", "Abilities");

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("🏷️\nTag", GUILayout.Height(50)))
                    CreateAsset<GameplayTag>("Tag_New", "Tags");

                if (GUILayout.Button("📦\nAttr Set", GUILayout.Height(50)))
                    CreateAsset<AttributeSetDefinition>("AttributeSet_New", "AttributeSets");

                if (GUILayout.Button("🎬\nCue", GUILayout.Height(50)))
                    CreateAsset<GameplayCue>("Cue_New", "Cues");

                GUILayout.EndHorizontal();
            });
        }

        private void DrawBrowser()
        {
            GASEditorStyles.DrawSection("📁 Browse Assets", () =>
            {
                // Search
                GUILayout.BeginHorizontal();
                GUILayout.Label("🔍", GUILayout.Width(20));
                _searchFilter = GUILayout.TextField(_searchFilter);
                if (GUILayout.Button("✕", GUILayout.Width(25)))
                {
                    _searchFilter = "";
                    GUI.FocusControl(null);
                }
                GUILayout.EndHorizontal();

                EditorGUILayout.Space(5);

                // Category filter
                _browseCategory = GUILayout.Toolbar(_browseCategory, _categories);

                EditorGUILayout.Space(5);

                // Asset list
                DrawAssetList();
            });
        }

        private void DrawAssetList()
        {
            var allAssets = new List<(ScriptableObject asset, string type, string icon)>();

            if (_browseCategory == 0 || _browseCategory == 1)
                foreach (var a in FindAssets<AttributeDefinition>())
                    allAssets.Add((a, "Attribute", "📊"));

            if (_browseCategory == 0 || _browseCategory == 2)
                foreach (var a in FindAssets<GameplayEffectDefinition>())
                    allAssets.Add((a, "Effect", "⚡"));

            if (_browseCategory == 0 || _browseCategory == 3)
                foreach (var a in FindAssets<AbilityDefinition>())
                    allAssets.Add((a, "Ability", "⚔️"));

            if (_browseCategory == 0 || _browseCategory == 4)
                foreach (var a in FindAssets<GameplayTag>())
                    allAssets.Add((a, "Tag", "🏷️"));

            if (_browseCategory == 0 || _browseCategory == 5)
                foreach (var a in FindAssets<GameplayCue>())
                    allAssets.Add((a, "Cue", "🎬"));

            if (_browseCategory == 0 || _browseCategory == 6)
                foreach (var a in FindAssets<AttributeSetDefinition>())
                    allAssets.Add((a, "AttrSet", "📦"));

            // Filter
            if (!string.IsNullOrEmpty(_searchFilter))
            {
                allAssets = allAssets
                    .Where(a => a.asset.name.ToLower().Contains(_searchFilter.ToLower()))
                    .ToList();
            }

            if (allAssets.Count == 0)
            {
                EditorGUILayout.HelpBox("No assets found. Use Quick Create above!", MessageType.Info);
                return;
            }

            // Show count
            EditorGUILayout.LabelField($"Found {allAssets.Count} asset(s)", EditorStyles.miniLabel);

            // List assets
            foreach (var (asset, type, icon) in allAssets)
            {
                GUILayout.BeginHorizontal(EditorStyles.helpBox);

                GUILayout.Label(icon, GUILayout.Width(25));

                if (GUILayout.Button(asset.name, EditorStyles.linkLabel))
                {
                    Selection.activeObject = asset;
                    EditorGUIUtility.PingObject(asset);
                }

                GUILayout.FlexibleSpace();

                GUI.color = new Color(0.7f, 0.7f, 0.7f);
                GUILayout.Label(type, EditorStyles.miniLabel, GUILayout.Width(60));
                GUI.color = Color.white;

                if (GUILayout.Button("Select", GUILayout.Width(50)))
                {
                    Selection.activeObject = asset;
                    EditorGUIUtility.PingObject(asset);
                }

                GUILayout.EndHorizontal();
            }
        }

        private void CreateAsset<T>(string defaultName, string subfolder) where T : ScriptableObject
        {
            var basePath = "Assets/Data/GAS";
            var folderPath = $"{basePath}/{subfolder}";

            // Ensure folders exist
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder(basePath))
                AssetDatabase.CreateFolder("Assets/Data", "GAS");
            if (!AssetDatabase.IsValidFolder(folderPath))
                AssetDatabase.CreateFolder(basePath, subfolder);

            var asset = CreateInstance<T>();
            var path = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{defaultName}.asset");

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);

            // Focus the inspector
            EditorApplication.delayCall += () =>
            {
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = asset;
            };
        }

        private List<T> FindAssets<T>() where T : ScriptableObject
        {
            return AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(a => a != null)
                .OrderBy(a => a.name)
                .ToList();
        }
    }
}
#endif
