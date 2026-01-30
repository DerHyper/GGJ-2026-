#if UNITY_EDITOR
using System.Linq;
using GAS.Abilities;
using GAS.Attributes;
using GAS.Effects;
using GAS.Tags;
using UnityEditor;
using UnityEngine;

namespace GAS.Editor
{
    [CustomEditor(typeof(AbilityDefinition))]
    public class AbilityDefinitionEditor : UnityEditor.Editor
    {
        private AbilityDefinition _target;
        private bool _showTagsSection = false;
        private bool _showEffectsSection = true;
        private bool _showBehaviorSection = true;

        private void OnEnable()
        {
            _target = (AbilityDefinition)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Header
            GASEditorStyles.DrawHeader("⚔️ Ability Definition");

            EditorGUILayout.HelpBox(
                "An Ability is an action the player/enemy can activate.\n" +
                "• Has a cost (consumes an attribute like Stamina)\n" +
                "• Has a cooldown (time before reuse)\n" +
                "• Applies Effects to self and/or targets\n" +
                "• Uses a Behavior for custom logic",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Basic Info
            GASEditorStyles.DrawSection("📝 Basic Info", () =>
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AbilityName"),
                    new GUIContent("Ability Name", "Display name (e.g., 'Fireball', 'Dash')"));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("Icon"),
                    new GUIContent("Icon", "Sprite for UI display"));

                if (string.IsNullOrEmpty(_target.AbilityName))
                {
                    EditorGUILayout.HelpBox("Give this ability a name!", MessageType.Warning);
                }
            });

            // Cost & Cooldown
            GASEditorStyles.DrawSection("💰 Cost & Cooldown", () =>
            {
                EditorGUILayout.HelpBox(
                    "Cost: Which attribute to consume (e.g., Stamina, Mana)\n" +
                    "Cooldown: Seconds before ability can be used again",
                    MessageType.None);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("CostAttribute"),
                    new GUIContent("Cost Attribute", "Leave empty for no cost"));

                if (_target.CostAttribute != null)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("CostAmount"),
                        new GUIContent("Cost Amount", "How much to consume"));

                    if (_target.CostAmount <= 0)
                    {
                        EditorGUILayout.HelpBox("Cost amount should be > 0, or remove the Cost Attribute", MessageType.Warning);
                    }
                }

                EditorGUILayout.Space(5);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("Cooldown"),
                    new GUIContent("Cooldown (seconds)", "0 = no cooldown"));

                // Preview
                var costStr = _target.CostAttribute != null ? $"Costs {_target.CostAmount} {_target.CostAttribute.name}" : "Free";
                var cdStr = _target.Cooldown > 0 ? $"{_target.Cooldown}s cooldown" : "No cooldown";
                EditorGUILayout.LabelField($"→ {costStr}, {cdStr}", EditorStyles.helpBox);
            });

            // Effects
            _showEffectsSection = EditorGUILayout.Foldout(_showEffectsSection, "⚡ Effects", true);
            if (_showEffectsSection)
            {
                GASEditorStyles.DrawSection("", () =>
                {
                    EditorGUILayout.HelpBox(
                        "Effects applied when this ability activates:\n" +
                        "• Self Effects: Applied to the caster (buffs, self-heal)\n" +
                        "• Target Effects: Applied to targets hit by the ability",
                        MessageType.None);

                    EditorGUILayout.LabelField("Apply to SELF (caster):", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ApplyToSelfEffects"), GUIContent.none);

                    EditorGUILayout.Space(5);

                    EditorGUILayout.LabelField("Apply to TARGET:", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ApplyToTargetEffects"), GUIContent.none);

                    if (_target.ApplyToSelfEffects.Count == 0 && _target.ApplyToTargetEffects.Count == 0 && _target.Behavior == null)
                    {
                        EditorGUILayout.HelpBox("This ability does nothing! Add effects or a behavior.", MessageType.Warning);
                    }
                });
            }

            // Behavior
            _showBehaviorSection = EditorGUILayout.Foldout(_showBehaviorSection, "🎮 Behavior", true);
            if (_showBehaviorSection)
            {
                GASEditorStyles.DrawSection("", () =>
                {
                    EditorGUILayout.HelpBox(
                        "Behavior defines WHAT the ability does (spawn projectile, raycast, etc.)\n\n" +
                        "Built-in behaviors:\n" +
                        "• ProjectileBehavior - Spawns and launches a projectile\n" +
                        "• RaycastBehavior - Instant raycast attack (light beam)\n" +
                        "• AreaOfEffectBehavior - Hits all targets in radius",
                        MessageType.None);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Behavior"),
                        new GUIContent("Behavior", "The logic that runs when ability activates"));

                    if (_target.Behavior != null)
                    {
                        EditorGUILayout.LabelField($"Using: {_target.Behavior.GetType().Name}", EditorStyles.helpBox);
                    }
                });
            }

            // Channeling
            GASEditorStyles.DrawSection("⏳ Channeling", () =>
            {
                EditorGUILayout.HelpBox(
                    "Channeled abilities stay active over time (e.g., charging an attack, beam weapon).\n" +
                    "Non-channeled abilities activate once and end immediately.",
                    MessageType.None);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("IsChanneled"),
                    new GUIContent("Is Channeled", "Does this ability stay active over time?"));

                if (_target.IsChanneled)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ChannelDuration"),
                        new GUIContent("Channel Duration", "0 = until manually cancelled"));
                }
            });

            // Tags
            _showTagsSection = EditorGUILayout.Foldout(_showTagsSection, "🏷️ Tags (Advanced)", true);
            if (_showTagsSection)
            {
                GASEditorStyles.DrawSection("", () =>
                {
                    EditorGUILayout.HelpBox(
                        "Tags control when abilities can/cannot be used:\n" +
                        "• Required: Must have these tags to activate\n" +
                        "• Blocked: Cannot activate if have these tags\n" +
                        "• Granted: Added while ability is active\n" +
                        "• Cooldown: Added while on cooldown",
                        MessageType.None);

                    EditorGUILayout.LabelField("Required Tags (must have all):", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ActivationRequiredTags"), GUIContent.none);

                    EditorGUILayout.LabelField("Blocked Tags (cannot have any):", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ActivationBlockedTags"), GUIContent.none);

                    EditorGUILayout.Space(5);

                    EditorGUILayout.LabelField("Granted while active:", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ActivationGrantedTags"), GUIContent.none);

                    EditorGUILayout.LabelField("Granted while on cooldown:", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("CooldownTags"), GUIContent.none);
                });
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
                var summary = $"「{(_target.AbilityName ?? "Unnamed")}」\n";

                // Cost
                if (_target.CostAttribute != null && _target.CostAmount > 0)
                    summary += $"\n💰 Costs {_target.CostAmount} {_target.CostAttribute.name}";
                else
                    summary += "\n💰 Free to use";

                // Cooldown
                if (_target.Cooldown > 0)
                    summary += $"\n⏱️ {_target.Cooldown}s cooldown";

                // Effects
                if (_target.ApplyToSelfEffects.Count > 0)
                    summary += $"\n✨ Self: {string.Join(", ", _target.ApplyToSelfEffects.Where(e => e != null).Select(e => e.name))}";
                if (_target.ApplyToTargetEffects.Count > 0)
                    summary += $"\n🎯 Target: {string.Join(", ", _target.ApplyToTargetEffects.Where(e => e != null).Select(e => e.name))}";

                // Behavior
                if (_target.Behavior != null)
                    summary += $"\n🎮 Behavior: {_target.Behavior.GetType().Name}";

                // Channeled
                if (_target.IsChanneled)
                    summary += _target.ChannelDuration > 0 ? $"\n⏳ Channeled ({_target.ChannelDuration}s)" : "\n⏳ Channeled (until cancelled)";

                // Blocked by
                if (_target.ActivationBlockedTags.Count > 0)
                    summary += $"\n🚫 Blocked by: {string.Join(", ", _target.ActivationBlockedTags.Where(t => t != null).Select(t => t.name))}";

                GUI.color = new Color(0.85f, 0.95f, 0.85f);
                EditorGUILayout.LabelField(summary, EditorStyles.helpBox);
                GUI.color = Color.white;
            });
        }
    }
}
#endif
