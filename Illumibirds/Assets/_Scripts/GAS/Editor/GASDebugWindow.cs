#if UNITY_EDITOR
using System.Linq;
using GAS.Abilities;
using GAS.Core;
using GAS.Effects;
using UnityEditor;
using UnityEngine;

namespace GAS.Editor
{
    /// <summary>
    /// Runtime debug window for inspecting GAS state on selected entities.
    /// </summary>
    public class GASDebugWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private AbilitySystemComponent _selectedASC;
        private bool _showAttributes = true;
        private bool _showEffects = true;
        private bool _showAbilities = true;
        private bool _showTags = true;
        private bool _autoRefresh = true;

        [MenuItem("Window/GAS/Debug %#d")] // Ctrl+Shift+D
        public static void ShowWindow()
        {
            var window = GetWindow<GASDebugWindow>("GAS Debug");
            window.minSize = new Vector2(350, 400);
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }

        private void OnPlayModeChanged(PlayModeStateChange state)
        {
            Repaint();
        }

        private void OnInspectorUpdate()
        {
            if (_autoRefresh && Application.isPlaying)
            {
                Repaint();
            }
        }

        private void OnSelectionChange()
        {
            if (Selection.activeGameObject != null)
            {
                var asc = Selection.activeGameObject.GetComponent<AbilitySystemComponent>();
                if (asc != null)
                {
                    _selectedASC = asc;
                }
            }
            Repaint();
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to debug GAS runtime state.", MessageType.Info);
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            DrawTargetSelector();

            if (_selectedASC == null)
            {
                EditorGUILayout.HelpBox("Select a GameObject with AbilitySystemComponent, or choose from the dropdown above.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.Space(5);
                DrawAttributes();
                DrawActiveEffects();
                DrawAbilities();
                DrawTags();
                DrawTestActions();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            _autoRefresh = GUILayout.Toggle(_autoRefresh, "Auto Refresh", EditorStyles.toolbarButton);

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                Repaint();
            }

            GUILayout.FlexibleSpace();

            if (_selectedASC != null)
            {
                GUI.color = new Color(0.7f, 1f, 0.7f);
                GUILayout.Label($"● {_selectedASC.gameObject.name}", EditorStyles.toolbarButton);
                GUI.color = Color.white;
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawTargetSelector()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Target:", GUILayout.Width(50));

            // Find all ASCs in scene
            var allASCs = FindObjectsOfType<AbilitySystemComponent>();
            var names = allASCs.Select(a => a.gameObject.name).ToArray();
            var currentIndex = System.Array.IndexOf(allASCs, _selectedASC);

            var newIndex = EditorGUILayout.Popup(currentIndex, names);
            if (newIndex >= 0 && newIndex < allASCs.Length)
            {
                _selectedASC = allASCs[newIndex];
            }

            if (GUILayout.Button("Select", GUILayout.Width(50)))
            {
                if (_selectedASC != null)
                {
                    Selection.activeGameObject = _selectedASC.gameObject;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawAttributes()
        {
            _showAttributes = EditorGUILayout.Foldout(_showAttributes, $"📊 Attributes ({_selectedASC.Attributes.Attributes.Count})", true);
            if (!_showAttributes) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            foreach (var kvp in _selectedASC.Attributes.Attributes)
            {
                var attr = kvp.Value;
                var def = kvp.Key;

                EditorGUILayout.BeginHorizontal();

                // Name
                EditorGUILayout.LabelField(def.name, GUILayout.Width(120));

                // Value bar
                float percent = def.MaxValue > def.MinValue
                    ? (attr.CurrentValue - def.MinValue) / (def.MaxValue - def.MinValue)
                    : 1f;
                var barRect = EditorGUILayout.GetControlRect(GUILayout.Width(100));
                EditorGUI.ProgressBar(barRect, Mathf.Clamp01(percent), "");

                // Values
                GUI.color = attr.CurrentValue != attr.BaseValue ? Color.yellow : Color.white;
                EditorGUILayout.LabelField($"{attr.CurrentValue:F1}", GUILayout.Width(50));
                GUI.color = Color.white;

                // Base value if different
                if (!Mathf.Approximately(attr.CurrentValue, attr.BaseValue))
                {
                    GUI.color = new Color(0.6f, 0.6f, 0.6f);
                    EditorGUILayout.LabelField($"(base: {attr.BaseValue:F1})", EditorStyles.miniLabel, GUILayout.Width(80));
                    GUI.color = Color.white;
                }

                // Modifier count
                if (attr.Modifiers.Count > 0)
                {
                    GUI.color = Color.cyan;
                    EditorGUILayout.LabelField($"[{attr.Modifiers.Count} mod]", EditorStyles.miniLabel, GUILayout.Width(50));
                    GUI.color = Color.white;
                }

                EditorGUILayout.EndHorizontal();
            }

            if (_selectedASC.Attributes.Attributes.Count == 0)
            {
                EditorGUILayout.LabelField("No attributes initialized", EditorStyles.miniLabel);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawActiveEffects()
        {
            _showEffects = EditorGUILayout.Foldout(_showEffects, $"⚡ Active Effects ({_selectedASC.ActiveEffects.Count})", true);
            if (!_showEffects) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (_selectedASC.ActiveEffects.Count == 0)
            {
                EditorGUILayout.LabelField("No active effects", EditorStyles.miniLabel);
            }

            foreach (var effect in _selectedASC.ActiveEffects)
            {
                EditorGUILayout.BeginHorizontal();

                // Icon based on type
                var icon = effect.Definition.DurationType switch
                {
                    EffectDurationType.Duration => "⏱️",
                    EffectDurationType.Infinite => "∞",
                    _ => "⚡"
                };

                EditorGUILayout.LabelField(icon, GUILayout.Width(20));
                EditorGUILayout.LabelField(effect.Definition.name, GUILayout.Width(150));

                // Duration bar for timed effects
                if (effect.Definition.DurationType == EffectDurationType.Duration)
                {
                    float percent = effect.RemainingDuration / effect.Definition.Duration;
                    var barRect = EditorGUILayout.GetControlRect(GUILayout.Width(60));
                    EditorGUI.ProgressBar(barRect, percent, "");
                    EditorGUILayout.LabelField($"{effect.RemainingDuration:F1}s", EditorStyles.miniLabel, GUILayout.Width(40));
                }

                // Stack count
                if (effect.StackCount > 1)
                {
                    GUI.color = Color.yellow;
                    EditorGUILayout.LabelField($"x{effect.StackCount}", EditorStyles.miniLabel, GUILayout.Width(30));
                    GUI.color = Color.white;
                }

                // Remove button
                GUI.color = new Color(1f, 0.5f, 0.5f);
                if (GUILayout.Button("✕", GUILayout.Width(20)))
                {
                    _selectedASC.RemoveEffect(effect);
                }
                GUI.color = Color.white;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawAbilities()
        {
            _showAbilities = EditorGUILayout.Foldout(_showAbilities, $"⚔️ Abilities ({_selectedASC.GrantedAbilities.Count})", true);
            if (!_showAbilities) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (_selectedASC.GrantedAbilities.Count == 0)
            {
                EditorGUILayout.LabelField("No abilities granted", EditorStyles.miniLabel);
            }

            foreach (var kvp in _selectedASC.GrantedAbilities)
            {
                var ability = kvp.Value;
                var def = kvp.Key;

                EditorGUILayout.BeginHorizontal();

                // Status icon
                string icon;
                if (ability.IsActive)
                {
                    icon = "▶️";
                    GUI.color = Color.green;
                }
                else if (ability.IsOnCooldown)
                {
                    icon = "⏳";
                    GUI.color = Color.gray;
                }
                else
                {
                    icon = "✓";
                    GUI.color = Color.white;
                }

                EditorGUILayout.LabelField(icon, GUILayout.Width(20));
                EditorGUILayout.LabelField(def.name, GUILayout.Width(150));
                GUI.color = Color.white;

                // Cooldown bar
                if (ability.IsOnCooldown)
                {
                    float percent = ability.GetCooldownPercent();
                    var barRect = EditorGUILayout.GetControlRect(GUILayout.Width(60));
                    EditorGUI.ProgressBar(barRect, percent, "");
                    EditorGUILayout.LabelField($"{ability.CooldownRemaining:F1}s", EditorStyles.miniLabel, GUILayout.Width(40));
                }
                else
                {
                    GUILayout.Space(104);
                }

                // Activate button
                GUI.enabled = _selectedASC.CanActivateAbility(ability);
                if (GUILayout.Button("Use", GUILayout.Width(40)))
                {
                    _selectedASC.TryActivateAbility(ability);
                }
                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawTags()
        {
            _showTags = EditorGUILayout.Foldout(_showTags, $"🏷️ Tags ({_selectedASC.OwnedTags.Count})", true);
            if (!_showTags) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (_selectedASC.OwnedTags.Count == 0)
            {
                EditorGUILayout.LabelField("No tags", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                foreach (var tag in _selectedASC.OwnedTags.Tags)
                {
                    GUI.color = new Color(0.8f, 0.9f, 1f);
                    GUILayout.Label(tag.name, EditorStyles.helpBox);
                    GUI.color = Color.white;
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawTestActions()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("🧪 Test Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Damage -10"))
            {
                var healthAttr = _selectedASC.Attributes.Attributes.Keys
                    .FirstOrDefault(a => a.name.Contains("Health") && !a.name.Contains("Max"));
                if (healthAttr != null)
                {
                    var attr = _selectedASC.GetAttribute(healthAttr);
                    attr.BaseValue -= 10;
                }
            }

            if (GUILayout.Button("Heal +10"))
            {
                var healthAttr = _selectedASC.Attributes.Attributes.Keys
                    .FirstOrDefault(a => a.name.Contains("Health") && !a.name.Contains("Max"));
                if (healthAttr != null)
                {
                    var attr = _selectedASC.GetAttribute(healthAttr);
                    attr.BaseValue += 10;
                }
            }

            if (GUILayout.Button("Full Heal"))
            {
                var healthAttr = _selectedASC.Attributes.Attributes.Keys
                    .FirstOrDefault(a => a.name.Contains("Health") && !a.name.Contains("Max"));
                var maxHealthAttr = _selectedASC.Attributes.Attributes.Keys
                    .FirstOrDefault(a => a.name.Contains("MaxHealth"));

                if (healthAttr != null)
                {
                    var attr = _selectedASC.GetAttribute(healthAttr);
                    float max = maxHealthAttr != null ? _selectedASC.GetAttributeValue(maxHealthAttr) : 100f;
                    attr.BaseValue = max;
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Clear Effects"))
            {
                var effects = _selectedASC.ActiveEffects.ToList();
                foreach (var effect in effects)
                {
                    _selectedASC.RemoveEffect(effect);
                }
            }

            if (GUILayout.Button("Clear Tags"))
            {
                _selectedASC.OwnedTags.Clear();
            }

            if (GUILayout.Button("Log State"))
            {
                LogFullState();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void LogFullState()
        {
            var log = $"=== GAS Debug: {_selectedASC.gameObject.name} ===\n";

            log += "\n[Attributes]\n";
            foreach (var kvp in _selectedASC.Attributes.Attributes)
            {
                var attr = kvp.Value;
                log += $"  {kvp.Key.name}: {attr.CurrentValue:F1} (base: {attr.BaseValue:F1}, mods: {attr.Modifiers.Count})\n";
            }

            log += "\n[Active Effects]\n";
            foreach (var effect in _selectedASC.ActiveEffects)
            {
                log += $"  {effect.Definition.name} - {effect.Definition.DurationType}";
                if (effect.Definition.DurationType == EffectDurationType.Duration)
                    log += $" ({effect.RemainingDuration:F1}s remaining)";
                if (effect.StackCount > 1)
                    log += $" x{effect.StackCount}";
                log += "\n";
            }

            log += "\n[Abilities]\n";
            foreach (var kvp in _selectedASC.GrantedAbilities)
            {
                var ability = kvp.Value;
                var status = ability.IsActive ? "ACTIVE" : (ability.IsOnCooldown ? $"CD {ability.CooldownRemaining:F1}s" : "Ready");
                log += $"  {kvp.Key.name}: {status}\n";
            }

            log += "\n[Tags]\n";
            foreach (var tag in _selectedASC.OwnedTags.Tags)
            {
                log += $"  {tag.name}\n";
            }

            Debug.Log(log);
        }
    }
}
#endif
